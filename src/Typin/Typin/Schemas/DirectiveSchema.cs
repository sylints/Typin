﻿namespace Typin.Schemas
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Text;
    using Typin.Attributes;
    using Typin.Internal.Extensions;

    /// <summary>
    /// Stores directive schema.
    /// </summary>
    public class DirectiveSchema
    {
        /// <summary>
        /// Directive type.
        /// </summary>
        public Type Type { get; }

        /// <summary>
        /// Directive name.
        /// All directives in an application must have different names.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Directive description, which is used in help text.
        /// </summary>
        public string? Description { get; }

        /// <summary>
        /// Whether directive can run only in interactive mode.
        /// </summary>
        public bool InteractiveModeOnly { get; }

        /// <summary>
        /// Initializes an instance of <see cref="DirectiveSchema"/>.
        /// </summary>
        internal DirectiveSchema(Type type,
                                 string name,
                                 string? description,
                                 bool interactiveModeOnly)
        {
            Type = type;
            Name = name;
            Description = description;
            InteractiveModeOnly = interactiveModeOnly;
        }

        internal string GetInternalDisplayString()
        {
            var buffer = new StringBuilder();

            // Type
            buffer.Append(Type.FullName);

            // Name
            buffer.Append(' ')
                  .Append('[')
                  .Append(Name)
                  .Append(']');

            return buffer.ToString();
        }

        /// <inheritdoc/>
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            return GetInternalDisplayString();
        }

        internal static bool IsDirectiveType(Type type)
        {
            return type.Implements(typeof(IDirective)) &&
                   type.IsDefined(typeof(DirectiveAttribute)) &&
                   !type.IsAbstract &&
                   !type.IsInterface;
        }
    }
}