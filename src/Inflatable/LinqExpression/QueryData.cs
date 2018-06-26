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

using BigBook;
using Inflatable.ClassMapper;
using Inflatable.LinqExpression.Interfaces;
using Inflatable.LinqExpression.OrderBy;
using Inflatable.LinqExpression.WhereClauses;
using SQLHelperDB.HelperClasses.Interfaces;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Inflatable.LinqExpression
{
    /// <summary>
    /// Query data holder
    /// </summary>
    /// <typeparam name="TObject">The type of the object.</typeparam>
    public class QueryData<TObject> : IQueryData
        where TObject : class
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryData{TObject}"/> class.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <exception cref="ArgumentNullException">source</exception>
        public QueryData(MappingSource source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            SelectValues = new List<PropertyInfo>();
            Parameters = new List<IParameter>();
            WhereClause = new WhereClause<TObject>(null);
            OrderByValues = new List<OrderByClause>();
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="QueryData{TObject}"/> is distinct.
        /// </summary>
        /// <value><c>true</c> if distinct; otherwise, <c>false</c>.</value>
        public bool Distinct { get; set; }

        /// <summary>
        /// Gets the type of the object.
        /// </summary>
        /// <value>The type of the object.</value>
        public Type ObjectType => typeof(TObject);

        /// <summary>
        /// Gets the order by values.
        /// </summary>
        /// <value>The order by values.</value>
        public IList<OrderByClause> OrderByValues { get; }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>The parameters.</value>
        public IList<IParameter> Parameters { get; }

        /// <summary>
        /// Gets the select values.
        /// </summary>
        /// <value>The select values.</value>
        public IList<PropertyInfo> SelectValues { get; }

        /// <summary>
        /// Gets the source.
        /// </summary>
        /// <value>The source.</value>
        public MappingSource Source { get; }

        /// <summary>
        /// Gets or sets the top.
        /// </summary>
        /// <value>The top.</value>
        public int Top { get; set; }

        /// <summary>
        /// Gets the where clause.
        /// </summary>
        /// <value>The where clause.</value>
        public WhereClause<TObject> WhereClause { get; }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String"/> that represents this instance.</returns>
        public override string ToString()
        {
            return $"SELECT {SelectValues.ToString(x => x.Name)} FROM {ObjectType.Name} {WhereClause} {(OrderByValues.Count > 0 ? "ORDER BY " + OrderByValues.ToString(x => x.ToString()) : "")}";
        }
    }
}