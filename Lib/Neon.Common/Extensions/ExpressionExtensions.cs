// -----------------------------------------------------------------------------
// FILE:	    ExpressionExtensions.cs
// CONTRIBUTOR: NEONFORGE Team
// COPYRIGHT:   Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License").
// You may not use this file except in compliance with the License.
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
using System.Linq;
using System.Linq.Expressions;

namespace Neon.Common.Extensions
{
    /// <summary>
    /// Provides extension methods for working with <see cref="Expression"/> objects.
    /// </summary>
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Combines two expressions using the logical OR operator.
        /// </summary>
        /// <typeparam name="T">The type of the parameter in the expressions.</typeparam>
        /// <param name="expression1">The first expression.</param>
        /// <param name="expression2">The second expression.</param>
        /// <returns>The combined expression.</returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> expression1, Expression<Func<T, bool>> expression2)
        {
            InvocationExpression invokedExpression = Expression.Invoke(expression2, expression1.Parameters.Cast<Expression>());

            return Expression.Lambda<Func<T, bool>>(Expression.Or(expression1.Body, invokedExpression), expression1.Parameters);
        }

        /// <summary>
        /// Combines two expressions using the logical OR assignment operator.
        /// </summary>
        /// <typeparam name="T">The type of the parameter in the expressions.</typeparam>
        /// <param name="expression1">The first expression.</param>
        /// <param name="expression2">The second expression.</param>
        /// <returns>The combined expression.</returns>
        public static Expression<Func<T, bool>> OrAssign<T>(this Expression<Func<T, bool>> expression1, Expression<Func<T, bool>> expression2)
        {
            InvocationExpression invokedExpression = Expression.Invoke(expression2, expression1.Parameters.Cast<Expression>());

            return Expression.Lambda<Func<T, bool>>(Expression.OrAssign(expression1.Body, invokedExpression), expression1.Parameters);
        }

        /// <summary>
        /// Combines two expressions using the logical ORElse operator.
        /// </summary>
        /// <typeparam name="T">The type of the parameter in the expressions.</typeparam>
        /// <param name="expression1">The first expression.</param>
        /// <param name="expression2">The second expression.</param>
        /// <returns>The combined expression.</returns>
        public static Expression<Func<T, bool>> OrElse<T>(this Expression<Func<T, bool>> expression1, Expression<Func<T, bool>> expression2)
        {
            InvocationExpression invokedExpression = Expression.Invoke(expression2, expression1.Parameters.Cast<Expression>());

            return Expression.Lambda<Func<T, bool>>(Expression.OrElse(expression1.Body, invokedExpression), expression1.Parameters);
        }

        /// <summary>
        /// Combines two expressions using the logical Exclusive OR operator.
        /// </summary>
        /// <typeparam name="T">The type of the parameter in the expressions.</typeparam>
        /// <param name="expression1">The first expression.</param>
        /// <param name="expression2">The second expression.</param>
        /// <returns>The combined expression.</returns>
        public static Expression<Func<T, bool>> ExclusiveOr<T>(this Expression<Func<T, bool>> expression1, Expression<Func<T, bool>> expression2)
        {
            InvocationExpression invokedExpression = Expression.Invoke(expression2, expression1.Parameters.Cast<Expression>());

            return Expression.Lambda<Func<T, bool>>(Expression.ExclusiveOr(expression1.Body, invokedExpression), expression1.Parameters);
        }

        /// <summary>
        /// Combines two expressions using the logical Exclusive OR assignment operator.
        /// </summary>
        /// <typeparam name="T">The type of the parameter in the expressions.</typeparam>
        /// <param name="expression1">The first expression.</param>
        /// <param name="expression2">The second expression.</param>
        /// <returns>The combined expression.</returns>
        public static Expression<Func<T, bool>> ExclusiveOrAssign<T>(this Expression<Func<T, bool>> expression1, Expression<Func<T, bool>> expression2)
        {
            InvocationExpression invokedExpression = Expression.Invoke(expression2, expression1.Parameters.Cast<Expression>());

            return Expression.Lambda<Func<T, bool>>(Expression.ExclusiveOrAssign(expression1.Body, invokedExpression), expression1.Parameters);
        }

        /// <summary>
        /// Combines two expressions using the logical AND operator.
        /// </summary>
        /// <typeparam name="T">The type of the parameter in the expressions.</typeparam>
        /// <param name="expression1">The first expression.</param>
        /// <param name="expression2">The second expression.</param>
        /// <returns>The combined expression.</returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> expression1, Expression<Func<T, bool>> expression2)
        {
            InvocationExpression invokedExpression = Expression.Invoke(expression2, expression1.Parameters.Cast<Expression>());

            return Expression.Lambda<Func<T, bool>>(Expression.And(expression1.Body, invokedExpression), expression1.Parameters);
        }

        /// <summary>
        /// Combines two expressions using the logical ANDAlso operator.
        /// </summary>
        /// <typeparam name="T">The type of the parameter in the expressions.</typeparam>
        /// <param name="expression1">The first expression.</param>
        /// <param name="expression2">The second expression.</param>
        /// <returns>The combined expression.</returns>
        public static Expression<Func<T, bool>> AndAlso<T>(this Expression<Func<T, bool>> expression1, Expression<Func<T, bool>> expression2)
        {
            InvocationExpression invokedExpression = Expression.Invoke(expression2, expression1.Parameters.Cast<Expression>());

            return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(expression1.Body, invokedExpression), expression1.Parameters);
        }

        /// <summary>
        /// Combines two expressions using the logical AND assignment operator.
        /// </summary>
        /// <typeparam name="T">The type of the parameter in the expressions.</typeparam>
        /// <param name="expression1">The first expression.</param>
        /// <param name="expression2">The second expression.</param>
        /// <returns>The combined expression.</returns>
        public static Expression<Func<T, bool>> AndAssign<T>(this Expression<Func<T, bool>> expression1, Expression<Func<T, bool>> expression2)
        {
            InvocationExpression invokedExpression = Expression.Invoke(expression2, expression1.Parameters.Cast<Expression>());

            return Expression.Lambda<Func<T, bool>>(Expression.AndAssign(expression1.Body, invokedExpression), expression1.Parameters);
        }
    }
}
