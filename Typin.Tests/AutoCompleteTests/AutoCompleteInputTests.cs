﻿namespace Typin.Tests.AutoCompleteTests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using FluentAssertions;
    using Typin.AutoCompletion;
    using Typin.Console;
    using Typin.Extensions;
    using Xunit;
    using static Typin.Extensions.ConsoleKeyInfoExtensions;

    public sealed class AutoCompleteInputTests : IDisposable
    {
        private readonly LinkedList<string> _history = new LinkedList<string>(new string[] { "dotnet run", "git init", "clear" });
        private readonly TestAutoCompleteHandler _autoCompleteHandler;
        private readonly string[] _completions;

        private readonly IConsole _console;
        private readonly MemoryStream stdIn;
        private readonly MemoryStream stdOut;
        private readonly MemoryStream stdErr;

        public AutoCompleteInputTests()
        {
            _autoCompleteHandler = new TestAutoCompleteHandler();
            _completions = _autoCompleteHandler.GetSuggestions("", 0);

            stdIn = new MemoryStream(Console.InputEncoding.GetBytes("input"));
            stdOut = new MemoryStream();
            stdErr = new MemoryStream();

            _console = new VirtualConsole(input: stdIn,
                                          output: stdOut,
                                          error: stdErr);
        }

        private KeyHandler GetKeyHandlerInstanceForTests(TestAutoCompleteHandler? _autoCompleteHandler = null)
        {
            // Arrange
            KeyHandler keyHandler = new KeyHandler(_console);

            "Hello".Select(c => c.ToConsoleKeyInfo())
                   .ToList()
                   .ForEach(keyHandler.Handle);

            return keyHandler;
        }

        [Fact]
        public void TestUpArrow()
        {
            // Arrange
            KeyHandler _keyHandler = GetKeyHandlerInstanceForTests();

            // Act
            // Assert
            _history.AsEnumerable().Reverse().ToList().ForEach((history) =>
            {
                _keyHandler.Handle(UpArrow);
                _keyHandler.Text.Should().Be(history);
            });
        }

        [Fact]
        public void TestDownArrow()
        {
            // Arrange
            KeyHandler _keyHandler = GetKeyHandlerInstanceForTests();

            // Act
            Enumerable.Repeat(UpArrow, _history.Count)
                    .ToList()
                    .ForEach(_keyHandler.Handle);

            // Assert
            _history.ToList().ForEach(history =>
            {
                _keyHandler.Text.Should().Be(history);
                _keyHandler.Handle(DownArrow);
            });
        }

        [Fact]
        public void TestTab()
        {
            // Arrange
            KeyHandler _keyHandler = GetKeyHandlerInstanceForTests();

            // Act
            _keyHandler.Handle(Tab);

            // Assert
            // Nothing happens when no auto complete handler is set
            _keyHandler.Text.Should().Be("Hello");

            // Arrange
            _keyHandler = GetKeyHandlerInstanceForTests(_autoCompleteHandler);

            // Act
            "Hi ".Select(c => c.ToConsoleKeyInfo()).ToList().ForEach(_keyHandler.Handle);

            // Assert
            _completions.ToList().ForEach(completion =>
            {
                _keyHandler.Handle(Tab);
                _keyHandler.Text.Should().Be($"HelloHi {completion}");
            });
        }

        [Fact]
        public void TestShiftTab()
        {
            // Arrange
            KeyHandler _keyHandler = GetKeyHandlerInstanceForTests();

            // Act
            _keyHandler.Handle(Tab);

            // Assert
            // Nothing happens when no auto complete handler is set
            _keyHandler.Text.Should().Be("Hello");

            // Arrange
            _keyHandler = GetKeyHandlerInstanceForTests(_autoCompleteHandler);

            // Act
            "Hi ".Select(c => c.ToConsoleKeyInfo()).ToList().ForEach(_keyHandler.Handle);

            // Bring up the first Autocomplete
            _keyHandler.Handle(Tab);

            // Assert
            _completions.Reverse().ToList().ForEach(completion =>
            {
                _keyHandler.Handle(ShiftTab);
                _keyHandler.Text.Should().Be($"HelloHi {completion}");
            });
        }

        [Fact]
        public void MoveCursorThenPreviousHistory()
        {
            // Arrange
            KeyHandler _keyHandler = GetKeyHandlerInstanceForTests();

            // Act
            _keyHandler.Handle(LeftArrow);
            _keyHandler.Handle(UpArrow);

            // Assert
            _keyHandler.Text.Should().Be("git init");
        }

        [Fact]
        public void PreviousThenMoveCursorThenNextHistory()
        {
            // Arrange
            KeyHandler _keyHandler = GetKeyHandlerInstanceForTests();

            // Act
            _keyHandler.Handle(UpArrow);

            // Assert
            _keyHandler.Text.Should().Be("git init");

            // Act
            _keyHandler.Handle(LeftArrow);
            _keyHandler.Handle(DownArrow);

            // Assert
            _keyHandler.Text.Should().Be("clear");
        }

        public void Dispose()
        {
            stdIn.Dispose();
            stdOut.Dispose();
            stdErr.Dispose();
        }
    }
}