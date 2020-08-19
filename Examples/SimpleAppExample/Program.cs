﻿namespace SimpleAppExample
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using SimpleAppExample.Commands;
    using Typin;
    using Typin.Directives;

    public static class Program
    {
        private static readonly string[] Arguments = { "--str", "hello world", "-i", "13", "-b" };

        public static async Task<int> Main()
        {
            return await new CliApplicationBuilder().AddCommand(typeof(TypinBenchmarkCommand))
                                                    .AddDirective<DebugDirective>()
                                                    .AddDirective<PreviewDirective>()
                                                    .Build()
                                                    .RunAsync(Arguments, new Dictionary<string, string>());
        }
    }
}