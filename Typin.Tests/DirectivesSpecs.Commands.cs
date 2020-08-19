﻿namespace Typin.Tests
{
    using System.Threading.Tasks;
    using Typin.Attributes;
    using Typin.Console;

    public partial class DirectivesSpecs
    {
        [Command("cmd")]
        private class NamedCommand : ICommand
        {
            public ValueTask ExecuteAsync(IConsole console)
            {
                return default;
            }
        }
    }
}