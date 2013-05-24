﻿namespace LinFu.AOP.Interfaces
{
    /// <summary>
    /// Represents a type that can have its method body  implementations replaced at runtime.
    /// </summary>
    public interface IMethodReplacementHost
    {
        /// <summary>
        /// Gets or sets a value indicating the <see cref="IMethodReplacementProvider"/> that will be used to swap method body implementations at runtime.
        /// </summary>
        IMethodReplacementProvider MethodBodyReplacementProvider { get; set; }

        /// <summary>
        /// Gets or sets a value indicating the <see cref="IMethodReplacementProvider"/> that will be used to swap method body implementations at runtime.
        /// </summary>
        IMethodReplacementProvider MethodCallReplacementProvider { get; set; }
    }
}