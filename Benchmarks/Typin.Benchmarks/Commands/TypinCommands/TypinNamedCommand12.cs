﻿namespace Typin.Benchmarks.Commands.TypinCommands
{
    using System.Threading.Tasks;
    using Typin.Attributes;
    using Typin.Console;

    [Command("named-command12")]
    public class TypinNamedCommand12 : ICommand
    {
        [CommandOption("str", 's')]
        public string? StrOption { get; set; }

        [CommandOption("int", 'i')]
        public int IntOption { get; set; }

        [CommandOption("bool", 'b')]
        public bool BoolOption { get; set; }

        public ValueTask ExecuteAsync(IConsole console)
        {
            return default;
        }
    }
}