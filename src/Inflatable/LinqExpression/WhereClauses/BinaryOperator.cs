﻿/*
Copyright 2017 James Craig

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using Inflatable.ClassMapper;
using Inflatable.Interfaces;
using Inflatable.LinqExpression.WhereClauses.Interfaces;
using SQLHelperDB.HelperClasses.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Inflatable.LinqExpression.WhereClauses
{
    /// <summary>
    /// Binary operator class
    /// </summary>
    /// <seealso cref="IOperator"/>
    public class BinaryOperator : IOperator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryOperator"/> class.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <param name="operatorType">Type of the operator.</param>
        /// <exception cref="ArgumentNullException">left or right</exception>
        public BinaryOperator(IOperator left, IOperator right, ExpressionType operatorType)
        {
            Operator = operatorType;
            if (left.IsNull)
            {
                (right, left) = (left, right);
            }
            Left = left ?? throw new ArgumentNullException(nameof(left));
            Right = right ?? throw new ArgumentNullException(nameof(right));
            Left.Parent = this;
            Right.Parent = this;
            TypeCode = _TypeCodeConverter[operatorType](this);
        }

        /// <summary>
        /// The converter
        /// </summary>
        private static readonly Dictionary<ExpressionType, string> _Converter = new()
        {
            [ExpressionType.And] = " AND ",
            [ExpressionType.Or] = " OR ",
            [ExpressionType.Equal] = " = ",
            [ExpressionType.NotEqual] = " <> ",
            [ExpressionType.LessThan] = " < ",
            [ExpressionType.LessThanOrEqual] = " <= ",
            [ExpressionType.GreaterThan] = " > ",
            [ExpressionType.GreaterThanOrEqual] = " >= ",
            [ExpressionType.OrElse] = " OR ",
            [ExpressionType.AndAlso] = " AND "
        };

        /// <summary>
        /// The negation converter
        /// </summary>
        private static readonly Dictionary<ExpressionType, ExpressionType> _NegationConverter = new()
        {
            [ExpressionType.And] = ExpressionType.Or,
            [ExpressionType.Or] = ExpressionType.And,
            [ExpressionType.Equal] = ExpressionType.NotEqual,
            [ExpressionType.NotEqual] = ExpressionType.Equal,
            [ExpressionType.LessThan] = ExpressionType.GreaterThanOrEqual,
            [ExpressionType.LessThanOrEqual] = ExpressionType.GreaterThan,
            [ExpressionType.GreaterThan] = ExpressionType.LessThanOrEqual,
            [ExpressionType.GreaterThanOrEqual] = ExpressionType.LessThan,
            [ExpressionType.OrElse] = ExpressionType.AndAlso,
            [ExpressionType.AndAlso] = ExpressionType.OrElse
        };

        /// <summary>
        /// The type code converter
        /// </summary>
        private static readonly Dictionary<ExpressionType, Func<BinaryOperator, Type>> _TypeCodeConverter = new()
        {
            [ExpressionType.And] = _ => typeof(bool),
            [ExpressionType.Or] = _ => typeof(bool),
            [ExpressionType.Equal] = _ => typeof(bool),
            [ExpressionType.NotEqual] = _ => typeof(bool),
            [ExpressionType.LessThan] = _ => typeof(bool),
            [ExpressionType.LessThanOrEqual] = _ => typeof(bool),
            [ExpressionType.GreaterThan] = _ => typeof(bool),
            [ExpressionType.GreaterThanOrEqual] = _ => typeof(bool),
            [ExpressionType.OrElse] = _ => typeof(bool),
            [ExpressionType.AndAlso] = _ => typeof(bool)
        };

        /// <summary>
        /// Gets a value indicating whether this instance is null.
        /// </summary>
        /// <value><c>true</c> if this instance is null; otherwise, <c>false</c>.</value>
        public bool IsNull { get; }

        /// <summary>
        /// Gets the left.
        /// </summary>
        /// <value>The left.</value>
        public IOperator Left { get; private set; }

        /// <summary>
        /// Gets the operator.
        /// </summary>
        /// <value>The operator.</value>
        public ExpressionType Operator { get; private set; }

        /// <summary>
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public IOperator? Parent { get; set; }

        /// <summary>
        /// Gets the right.
        /// </summary>
        /// <value>The right.</value>
        public IOperator Right { get; private set; }

        /// <summary>
        /// Gets the type code.
        /// </summary>
        /// <value>The type code.</value>
        public Type TypeCode { get; }

        /// <summary>
        /// Copies this instance.
        /// </summary>
        /// <returns>A copy of this instance.</returns>
        public IOperator Copy() => new BinaryOperator(Left.Copy(), Right.Copy(), Operator);

        /// <summary>
        /// Gets the parameters associated with the operator.
        /// </summary>
        /// <returns>A list of parameters associated with the operator.</returns>
        public List<IParameter> GetParameters()
        {
            var ReturnValue = new List<IParameter>();
            ReturnValue.AddRange(Left.GetParameters());
            ReturnValue.AddRange(Right.GetParameters());
            return ReturnValue;
        }

        /// <summary>
        /// Does a logical negation of the operator.
        /// </summary>
        /// <returns>The resulting operator.</returns>
        public IOperator LogicallyNegate()
        {
            Operator = _NegationConverter[Operator];
            Left = Left.LogicallyNegate();
            Right = Right.LogicallyNegate();
            return this;
        }

        /// <summary>
        /// Optimizes the operator based on the mapping source.
        /// </summary>
        /// <param name="mappingSource">The mapping source.</param>
        /// <returns></returns>
        public IOperator Optimize(IMappingSource mappingSource)
        {
            Left = Left.Optimize(mappingSource);
            Right = Right.Optimize(mappingSource);
            if (Left is null || Right is null)
            {
                return (Left ?? Right)!;
            }

            if (Operator == ExpressionType.And
                || Operator == ExpressionType.Or
                || Operator == ExpressionType.OrElse
                || Operator == ExpressionType.AndAlso)
            {
                if (Left.TypeCode != typeof(bool))
                {
                    return Right;
                }

                if (Right.TypeCode != typeof(bool))
                {
                    return Left;
                }
            }
            return this;
        }

        /// <summary>
        /// Sets the column names.
        /// </summary>
        /// <param name="mappingSource">The mapping source.</param>
        /// <param name="mapping">The mapping.</param>
        public void SetColumnNames(IMappingSource mappingSource, IMapping mapping)
        {
            Left?.SetColumnNames(mappingSource, mapping);
            Right?.SetColumnNames(mappingSource, mapping);
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string"/> that represents this instance.</returns>
        public override string ToString()
        {
            if (Operator == ExpressionType.Equal && Right.IsNull)
                return "(" + Left + " IS " + Right + ")";
            if (Operator == ExpressionType.NotEqual && Right.IsNull)
                return "(" + Left + " IS NOT " + Right + ")";
            return "(" + Left + _Converter[Operator] + Right + ")";
        }
    }
}