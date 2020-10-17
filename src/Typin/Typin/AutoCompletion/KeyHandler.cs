﻿namespace Typin.AutoCompletion
{
    using System;
    using System.Collections.Generic;
    using Typin.Console;

    internal sealed class KeyHandler
    {
        private readonly IConsole _console;
        private readonly HashSet<ShortcutDefinition> _shortcuts;

        public delegate void UnhandledControlSequenceDetectedEventHandler(ref ConsoleKeyInfo keyInfo);
        public event UnhandledControlSequenceDetectedEventHandler? UnhandledControlSequenceDetected;

        public delegate void UnhandledKeyDetectedEventHandler(ref ConsoleKeyInfo keyInfo);
        public event UnhandledKeyDetectedEventHandler? UnhandledKeyDetected;

        public delegate void NewLineDetectedEventHandler();
        public event NewLineDetectedEventHandler? NewLineDetected;

        public delegate void InputModifiedEventHandler(ref ConsoleKeyInfo keyInfo);
        public event InputModifiedEventHandler? InputModified;

        /// <summary>
        /// Initializes an instance of <see cref="KeyHandler"/>.
        /// </summary>
        public KeyHandler(IConsole console,
                          HashSet<ShortcutDefinition> shortcuts)
        {
            _console = console;
            _shortcuts = shortcuts;
        }

        /// <summary>
        /// Handles key input.
        /// </summary>
        public void ReadKey()
        {
            ConsoleKeyInfo keyInfo = _console.ReadKey(true);

            ReadKey(keyInfo);
        }

        /// <summary>
        /// Handles key input.
        /// </summary>
        public void ReadKey(ConsoleKeyInfo keyInfo)
        {
            ConsoleModifiers modifiers = keyInfo.Modifiers;
            if (keyInfo.Key == ConsoleKey.Enter && modifiers == 0)
            {
                NewLineDetected?.Invoke();
                InputModified?.Invoke(ref keyInfo);

                return;
            }

            bool inputModified = false;
            var input = new ShortcutDefinition(keyInfo.Key, keyInfo.Modifiers, () => { });
            if (_shortcuts.TryGetValue(input, out ShortcutDefinition shortcutDefinition))
            {
                shortcutDefinition.Action.Invoke();
                inputModified = shortcutDefinition.ModifiesInput;
            }
            else if (modifiers.HasFlag(ConsoleModifiers.Control) && !modifiers.HasFlag(ConsoleModifiers.Alt))
            {
                UnhandledControlSequenceDetected?.Invoke(ref keyInfo);
                inputModified = true; //TODO refactor - this should not be there - having a KeyHandler instaed of only line handler is problematic
            }
            else
            {
                UnhandledKeyDetected?.Invoke(ref keyInfo);
                inputModified = true; //TODO refactor - this should not be there
            }

            if (inputModified)
                InputModified?.Invoke(ref keyInfo);
        }
    }
}
