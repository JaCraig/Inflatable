/*
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

using Inflatable.LinqExpression.OrderBy.Enums;
using System.Reflection;

namespace Inflatable.LinqExpression.OrderBy
{
    /// <summary>
    /// Order by clause
    /// </summary>
    public class OrderByClause
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OrderByClause"/> class.
        /// </summary>
        /// <param name="order">The order.</param>
        /// <param name="property">The property.</param>
        /// <param name="direction">The direction.</param>
        public OrderByClause(int order, PropertyInfo property, Direction direction)
        {
            Order = order;
            Property = property;
            Direction = direction;
        }

        /// <summary>
        /// Gets the direction.
        /// </summary>
        /// <value>The direction.</value>
        public Direction Direction { get; }

        /// <summary>
        /// Gets the order.
        /// </summary>
        /// <value>The order.</value>
        public int Order { get; }

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <value>The property.</value>
        public PropertyInfo Property { get; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return Property.Name + (Direction == Direction.Ascending ? "" : "DESC");
        }
    }
}