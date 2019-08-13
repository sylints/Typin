﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CliFx.Internal;
using CliFx.Models;
using CliFx.Services;

namespace CliFx
{
    /// <summary>
    /// Default implementation of <see cref="ICliApplicationBuilder"/>.
    /// </summary>
    public class CliApplicationBuilder : ICliApplicationBuilder
    {
        private readonly HashSet<Type> _commandTypes = new HashSet<Type>();

        private string _title;
        private string _executableName;
        private string _versionText;
        private IConsole _console;
        private ICommandFactory _commandFactory;

        /// <inheritdoc />
        public ICliApplicationBuilder WithCommand(Type commandType)
        {
            _commandTypes.Add(commandType);
            return this;
        }

        /// <inheritdoc />
        public ICliApplicationBuilder WithCommandsFrom(Assembly commandAssembly)
        {
            var commandTypes = commandAssembly.ExportedTypes.Where(t => t.Implements(typeof(ICommand)));

            foreach (var commandType in commandTypes)
                WithCommand(commandType);

            return this;
        }

        /// <inheritdoc />
        public ICliApplicationBuilder UseTitle(string title)
        {
            _title = title;
            return this;
        }

        /// <inheritdoc />
        public ICliApplicationBuilder UseExecutableName(string executableName)
        {
            _executableName = executableName;
            return this;
        }

        /// <inheritdoc />
        public ICliApplicationBuilder UseVersionText(string version)
        {
            _versionText = version;
            return this;
        }

        /// <inheritdoc />
        public ICliApplicationBuilder UseConsole(IConsole console)
        {
            _console = console;
            return this;
        }

        /// <inheritdoc />
        public ICliApplicationBuilder UseCommandFactory(ICommandFactory factory)
        {
            _commandFactory = factory;
            return this;
        }

        private void SetFallbackValues()
        {
            // Entry assembly is null in tests
            var entryAssembly = Assembly.GetEntryAssembly();

            if (_title.IsNullOrWhiteSpace())
                UseTitle(entryAssembly?.GetName().Name ?? "App");

            if (_executableName.IsNullOrWhiteSpace())
                UseExecutableName(Path.GetFileNameWithoutExtension(entryAssembly?.Location) ?? "app");

            if (_versionText.IsNullOrWhiteSpace())
                UseVersionText(entryAssembly?.GetName().Version.ToString() ?? "1.0");

            if (_console == null)
                UseConsole(new SystemConsole());

            if (_commandFactory == null)
                UseCommandFactory(new CommandFactory());
        }

        /// <inheritdoc />
        public ICliApplication Build()
        {
            // Use defaults for required parameters that were not configured
            SetFallbackValues();

            // Project parameters to expected types
            var metadata = new ApplicationMetadata(_title, _executableName, _versionText);
            var commandTypes = _commandTypes.ToArray();

            return new CliApplication(metadata, commandTypes,
                _console, new CommandInputParser(), new CommandSchemaResolver(),
                _commandFactory, new CommandInitializer(), new CommandHelpTextRenderer());
        }
    }
}