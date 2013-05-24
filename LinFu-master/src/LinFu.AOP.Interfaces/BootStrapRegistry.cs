﻿using System;
using System.Collections.Generic;
using LinFu.Reflection;

namespace LinFu.AOP.Interfaces
{
    /// <summary>
    /// Represents a registry class that bootstraps components into memory when the application starts.
    /// </summary>
    public sealed class BootStrapRegistry
    {
        private readonly IList<IBootStrappedComponent> _components = new List<IBootStrappedComponent>();

        private BootStrapRegistry()
        {
            Initialize();
        }

        /// <summary>
        /// Gets the value indicating the BootStrapRegistry instance.
        /// </summary>
        public static BootStrapRegistry Instance
        {
            get { return NestedLoader.Instance; }
        }

        /// <summary>
        /// Initializes the BootStrapRegistry.
        /// </summary>
        private void Initialize()
        {
            lock (_components)
            {
				string AppPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            	AppPath = AppPath.Replace("file:", "");
                _components.LoadFrom(AppPath, "*.dll");
                foreach (IBootStrappedComponent component in _components)
                {
                    try
                    {
                        component.Initialize();
                    }
                    catch (Exception ex)
                    {
                        string componentName = component != null ? component.GetType().Name : "(unknown)";
                        string message = string.Format("{0} Error: Unable to load component '{1}' - {2}",
                                                       GetType().FullName,
                                                       componentName, ex);

                        throw new BootstrapException(message, ex);
                    }
                }
            }
        }

        /// <summary>
        /// Returns the list of components that have been initialized by the bootstrapper.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<IBootStrappedComponent> GetComponents()
        {
            return _components;
        }

        #region Nested type: NestedLoader

        private class NestedLoader
        {
            internal static readonly BootStrapRegistry Instance;

            // Explicit static constructor to tell C# compiler
            // not to mark type as beforefieldinit
            static NestedLoader()
            {
                Instance = new BootStrapRegistry();
            }
        }

        #endregion
    }
}