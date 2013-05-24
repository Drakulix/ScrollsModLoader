﻿using System;

namespace LinFu.IoC.Interfaces
{
    /// <summary>
    /// Represents a class that keeps track of all the disposable objects 
    /// created within a service container and disposes them when 
    /// the scope itself has been disposed.
    /// </summary>
    public interface IScope : IDisposable
    {
    }
}