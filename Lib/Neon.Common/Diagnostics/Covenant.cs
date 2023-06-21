//-----------------------------------------------------------------------------
// FILE:        Covenant.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;

using Neon.Diagnostics;

// $todo(jefflill):
//
// This code is currently supporting only the documentation of Contract requirements
// but doesn't actually enforce anything.

namespace System.Diagnostics.Contracts
{
    /// <summary>
    /// A simple, lightweight, and inspired by the <see cref="Contract"/> class.
    /// </summary>
    /// <remarks>
    /// This class is intended to be a drop-in replacement for code contract assertions by simply
    /// searching and replacing <b>"Contract."</b> with "<see cref="Covenant"/>." in all source code.
    /// In my experience, code contracts slow down build times too much and often obsfucate 
    /// <c>async</c> methods such that they cannot be debugged effectively using the debugger.
    /// Code Contracts are also somewhat of a pain to configure as project propoerties.
    /// </remarks>
    public static class Covenant
    {
        private static Type[]   oneStringArg  = new Type[] { typeof(string) };
        private static Type[]   twoStringArgs = new Type[] { typeof(string), typeof(string) };

        /// <summary>
        /// Verifies a method pre-condition throwing a custom exception.
        /// </summary>
        /// <typeparam name="TException">Specifies exception to be thrown if the condition is <c>false</c>.</typeparam>
        /// <param name="condition">Specifies the condition to be tested.</param>
        /// <param name="arg1">Optionally specifies the first string argument to the exception constructor.</param>
        /// <param name="arg2">Optionally specifies the second optional string argument to the exception constructor.</param>
        /// <remarks>
        /// <para>
        /// This method throws a <typeparamref name="TException"/> instance when <paramref name="condition"/>
        /// is <c>false</c>.  Up to two string arguments may be passed to the exception constructor when an
        /// appropriate constructor exists, otherwise these arguments will be ignored.
        /// </para>
        /// </remarks>
        public static void Requires<TException>(bool condition, string arg1 = null, string arg2 = null)
            where TException : Exception, new()
        {
            if (condition)
            {
                return;
            }

            var exceptionType = typeof(TException);

            // Look for a constructor with two string parameters.

            var constructor = exceptionType.GetConstructor(twoStringArgs);

            if (constructor != null)
            {
                throw (Exception)constructor.Invoke(new object[] { arg1, arg2 });
            }

            // Look for a constructor with one string parameter.

            constructor = exceptionType.GetConstructor(oneStringArg);

            if (constructor != null)
            {
                throw (Exception)constructor.Invoke(new object[] { arg1 });
            }

            // Fall back to the default constructor.

            throw new TException();
        }

        /// <summary>
        /// Verifies a method pre-condition throwing a custom exception using lambda functions
        /// to obtain the messages to be included in the exception.  Use this override for better
        /// performance when the messages need to be formatted at runtime.
        /// </summary>
        /// <typeparam name="TException">Specifies exception to be thrown if the condition is <c>false</c>.</typeparam>
        /// <param name="condition">Specifies the condition to be tested.</param>
        /// <param name="arg1Func">
        /// Specifies a function that returns first string argument to the exception constructor.
        /// This may be <c>null</c>.
        /// </param>
        /// <param name="arg2Func">
        /// Optionally a function that returns the second optional string argument to the exception constructor.
        /// </param>
        /// <remarks>
        /// <para>
        /// This method throws a <typeparamref name="TException"/> instance when <paramref name="condition"/>
        /// is <c>false</c>.  Up to two string arguments may be passed to the exception constructor when an
        /// appropriate constructor exists, otherwise these arguments will be ignored.
        /// </para>
        /// </remarks>
        public static void Requires<TException>(bool condition, Func<string> arg1Func, Func<string> arg2Func = null)
            where TException : Exception, new()
        {
            if (condition)
            {
                return;
            }

            var exceptionType = typeof(TException);

            // Look for a constructor with two string parameters.

            var constructor = exceptionType.GetConstructor(twoStringArgs);

            if (constructor != null)
            {
                throw (Exception)constructor.Invoke(new object[] { arg1Func?.Invoke(), arg2Func?.Invoke() });
            }

            // Look for a constructor with one string parameter.

            constructor = exceptionType.GetConstructor(oneStringArg);

            if (constructor != null)
            {
                throw (Exception)constructor.Invoke(new object[] { arg1Func?.Invoke() });
            }

            // Fall back to the default constructor.

            throw new TException();
        }

        /// <summary>
        /// Asserts that a condition is <c>true</c>.
        /// </summary>
        /// <param name="condition">Specifies the condition to be tested.</param>
        /// <param name="message">Optionally spacifies the message to be included in the exception thrown.</param>
        /// <exception cref="AssertException">Thrown if <paramref name="condition"/> is <c>false</c>.</exception>
        public static void Assert(bool condition, string message = null)
        {
            if (!condition)
            {
                throw new AssertException(message);
            }
        }

        /// <summary>
        /// Asserts that a condition is <c>true</c>.  Use this override for better performance
        /// when the message needs to be formatted at runtime.
        /// </summary>
        /// <param name="condition">Specifies the condition to be tested.</param>
        /// <param name="messageFunc">
        /// Specifies a lambda function that returns the message to be included in the
        /// exception thrown.  This may be <c>null</c>.
        /// </param>
        public static void Assert(bool condition, Func<string> messageFunc)
        {
            if (!condition)
            {
                throw new AssertException(messageFunc?.Invoke());
            }
        }
    }
}
