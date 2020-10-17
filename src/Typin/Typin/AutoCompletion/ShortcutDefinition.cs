﻿namespace Typin.AutoCompletion
{
    using System;

    /// <summary>
    /// Console shortcut definition.
    /// </summary>
    public readonly struct ShortcutDefinition
    {
        /// <summary>
        /// Initializes an instance of <see cref="ShortcutDefinition"/>.
        /// </summary>
        public ShortcutDefinition(ConsoleKey key, Action action)
        {
            Key = key;
            Modifiers = 0;
            Action = action;
        }

        /// <summary>
        /// Initializes an instance of <see cref="ShortcutDefinition"/>.
        /// </summary>
        public ShortcutDefinition(ConsoleKey key, ConsoleModifiers modifiers, Action action)
        {
            Key = key;
            Modifiers = modifiers;
            Action = action;
        }

        /// <summary>
        /// A value that identifies the console key that was pressed.
        /// </summary>
        public ConsoleKey Key { get; }

        /// <summary>
        ///  A bitwise combination of the enumeration values that specifies one or more modifier keys pressed simultaneously with the console key.
        /// </summary>
        public ConsoleModifiers Modifiers { get; }

        /// <summary>
        /// An action associated with the shortcut.
        /// </summary>
        public Action Action { get; }

        /// <inheritdoc/>
        public static bool operator ==(ShortcutDefinition left, ShortcutDefinition right)
        {
            return left.Equals(right);
        }

        /// <inheritdoc/>
        public static bool operator !=(ShortcutDefinition left, ShortcutDefinition right)
        {
            return !left.Equals(right);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return obj is ShortcutDefinition sd && Key == sd.Key && Modifiers == sd.Modifiers;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return ((int)Key << 16) | (int)Modifiers;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            if (Modifiers == 0)
                return Key.ToString();

            return string.Concat(Modifiers, Key);
        }
    }
}