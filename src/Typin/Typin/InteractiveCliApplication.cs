﻿namespace Typin
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;
    using Typin.AutoCompletion;
    using Typin.Console;
    using Typin.Input;
    using Typin.Input.Resolvers;
    using Typin.Internal;
    using Typin.Schemas;

    /// <summary>
    /// Command line application facade.
    /// </summary>
    public sealed class InteractiveCliApplication : CliApplication
    {
        private readonly ConsoleColor _promptForeground;
        private readonly ConsoleColor _commandForeground;
        private readonly AutoCompleteInput? _autoCompleteInput;

        /// <summary>
        /// Initializes an instance of <see cref="InteractiveCliApplication"/>.
        /// </summary>
        public InteractiveCliApplication(IServiceProvider serviceProvider,
                                         CliContext cliContext,
                                         ConsoleColor promptForeground,
                                         ConsoleColor commandForeground,
                                         HashSet<ShortcutDefinition> userDefinedShortcut) :
            base(serviceProvider, cliContext)
        {
            _promptForeground = promptForeground;
            _commandForeground = commandForeground;

            if (cliContext.Configuration.IsAdvancedInputAllowed)
            {
                _autoCompleteInput = new AutoCompleteInput(cliContext.Console, userDefinedShortcut)
                {
                    AutoCompletionHandler = new AutoCompletionHandler(cliContext),
                };

                _autoCompleteInput.History.IsEnabled = true;
                cliContext.InputHistory = _autoCompleteInput.History;
            }
        }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        protected override async Task<int> ParseInput(IReadOnlyList<string> commandLineArguments,
                                                      RootSchema root)
        {
            CommandInput input = CommandInputResolver.Parse(commandLineArguments, root.GetCommandNames());
            CliContext.Input = input;

            if (input.IsInteractiveDirectiveSpecified)
            {
                CliContext.IsInteractiveMode = true;

                // we don't want to run default command for e.g. `[interactive]` but we want to run if there is sth else
                if (!input.IsDefaultCommandOrEmpty)
                    await ExecuteCommand();

                await RunInteractivelyAsync(root);

                return ExitCodes.Success; // called after Ctrl+C
            }

            return await ExecuteCommand();
        }

        [ExcludeFromCodeCoverage]
        private async Task RunInteractivelyAsync(RootSchema root)
        {
            IConsole console = CliContext.Console;
            string executableName = CliContext.Metadata.ExecutableName;

            do
            {
                string[]? commandLineArguments = GetInput(console, executableName);

                if (commandLineArguments is null)
                {
                    console.ResetColor();
                    return;
                }

                CommandInput input = CommandInputResolver.Parse(commandLineArguments, root.GetCommandNames());
                CliContext.Input = input; //TODO maybe refactor with some clever IDisposable class

                await ExecuteCommand();
                console.ResetColor();
            }
            while (!console.GetCancellationToken().IsCancellationRequested);
        }

        /// <summary>
        /// Gets user input and returns arguments or null if cancelled.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private string[]? GetInput(IConsole console, string executableName)
        {
            string[] arguments;
            string? line = string.Empty; // Can be null when Ctrl+C is pressed to close the app.
            do
            {
                // Print prompt
                console.WithForegroundColor(_promptForeground, () =>
                {
                    console.Output.Write(executableName);
                });

                if (!string.IsNullOrWhiteSpace(CliContext.Scope))
                {
                    console.WithForegroundColor(ConsoleColor.Cyan, () =>
                    {
                        console.Output.Write(' ');
                        console.Output.Write(CliContext.Scope);
                    });
                }

                console.WithForegroundColor(_promptForeground, () =>
                {
                    console.Output.Write("> ");
                });

                // Read user input
                console.WithForegroundColor(_commandForeground, () =>
                {
                    if (_autoCompleteInput is null)
                        line = console.Input.ReadLine();
                    else
                        line = _autoCompleteInput.ReadLine();
                });

                if (line is null)
                    return null;

                if (string.IsNullOrWhiteSpace(CliContext.Scope)) // handle unscoped command input
                {
                    arguments = CommandLineSplitter.Split(line)
                                                   .Where(x => !string.IsNullOrWhiteSpace(x))
                                                   .ToArray();
                }
                else // handle scoped command input
                {
                    List<string> tmp = CommandLineSplitter.Split(line)
                                                          .Where(x => !string.IsNullOrWhiteSpace(x))
                                                          .ToList();

                    int lastDirective = tmp.FindLastIndex(x => x.StartsWith('[') && x.EndsWith(']'));
                    tmp.Insert(lastDirective + 1, CliContext.Scope);

                    arguments = tmp.ToArray();
                }

            } while (string.IsNullOrWhiteSpace(line)); // retry on empty line

            console.ForegroundColor = ConsoleColor.Gray;

            return arguments;
        }
    }
}
