﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using LinFu.AOP.Interfaces;

namespace LinFu.AOP.Cecil
{
    /// <summary>
    /// Represents the information associated with 
    /// a single method call.
    /// </summary>
    [Serializable]
    public class InvocationInfo : IInvocationInfo
    {
        private readonly object[] _arguments;
        private readonly Type[] _parameterTypes;
        private readonly Type _returnType;
        private readonly StackTrace _stackTrace;
        private readonly object _target;
        private readonly MethodBase _targetMethod;
        private readonly Type[] _typeArguments;

        /// <summary>
        /// Initializes the <see cref="InvocationInfo"/> instance.
        /// </summary>
        /// <param name="target">The target instance currently being called.</param>
        /// <param name="targetMethod">The method currently being called.</param>
        /// <param name="stackTrace"> The <see cref="StackTrace"/> associated with the method call when the call was made.</param>
        /// <param name="parameterTypes">The parameter types for the current target method.</param>
        /// <param name="typeArguments">
        /// If the <see cref="TargetMethod"/> method is a generic method, 
        /// this will hold the generic type arguments used to construct the
        /// method.
        /// </param>
        /// <param name="returnType">The return type of the target method.</param>
        /// <param name="arguments">The arguments used in the method call.</param>
        public InvocationInfo(object target, MethodBase targetMethod,
                              StackTrace stackTrace, Type[] parameterTypes,
                              Type[] typeArguments, Type returnType, object[] arguments)
        {
            _target = target;
            _targetMethod = targetMethod;
            _stackTrace = stackTrace;
            _parameterTypes = parameterTypes;
            _typeArguments = typeArguments;
            _arguments = arguments;
            _returnType = returnType;
        }

        /// <summary>
        /// This is the actual calling method that invoked the <see cref="TargetMethod"/>.
        /// </summary>
        public MethodBase CallingMethod
        {
            get
            {
                StackFrame frame = _stackTrace.GetFrame(0);

                return frame.GetMethod();
            }
        }

        #region IInvocationInfo Members

        /// <summary>
        /// The target instance currently being called.
        /// </summary>
        /// <remarks>This typically is a reference to a proxy object.</remarks>
        public object Target
        {
            get { return _target; }
        }

        /// <summary>
        /// The method currently being called.
        /// </summary>
        public MethodBase TargetMethod
        {
            get { return _targetMethod; }
        }

        /// <summary>
        /// The <see cref="StackTrace"/> associated
        /// with the method call when the call was made.
        /// </summary>
        public StackTrace StackTrace
        {
            get { return _stackTrace; }
        }

        /// <summary>
        /// The return type of the <see cref="TargetMethod"/>.
        /// </summary>
        public Type ReturnType
        {
            get { return _returnType; }
        }

        /// <summary>
        /// The parameter types for the current target method.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This could be very useful in cases where the actual target method
        /// is based on a generic type definition. In such cases, 
        /// the <see cref="IInvocationInfo"/> instance needs to be able
        /// to describe the actual parameter types being used by the
        /// current generic type instantiation. This property helps
        /// users determine which parameter types are actually being used
        /// at the time of the method call.
        /// </para>
        /// </remarks>
        public Type[] ParameterTypes
        {
            get { return _parameterTypes; }
        }

        /// <summary>
        /// If the <see cref="TargetMethod"/> method is a generic method, 
        /// this will hold the generic type arguments used to construct the
        /// method.
        /// </summary>
        public Type[] TypeArguments
        {
            get { return _typeArguments; }
        }

        /// <summary>
        /// The arguments used in the method call.
        /// </summary>
        public object[] Arguments
        {
            get { return _arguments; }
        }

        #endregion

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var writer = new StringWriter();
            MethodBase targetMethod = TargetMethod;

            writer.Write("{0}.{1}(", targetMethod.DeclaringType, targetMethod.Name);

            var arguments = new Queue<object>(Arguments);
            while (arguments.Count > 0)
            {
                object argument = arguments.Dequeue();

                if (argument is string)
                    argument = string.Format("\"{0}\"", argument);

                writer.Write(argument);

                if (arguments.Count > 0)
                    writer.Write(", ");
            }

            writer.WriteLine(")");

            return writer.ToString();
        }
    }
}