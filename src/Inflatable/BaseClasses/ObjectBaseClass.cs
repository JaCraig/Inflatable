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

using Inflatable.Interfaces;
using Mirage.Generators;
using System;
using Valkyrie;

namespace Inflatable.BaseClasses
{
    /// <summary>
    /// Object base class helper. This is not required but automatically sets up basic functions and
    /// properties to simplify things a bit.
    /// </summary>
    /// <typeparam name="TObjectType">Object type (must be the child object type)</typeparam>
    /// <typeparam name="TIDType">ID type</typeparam>
    public abstract class ObjectBaseClass<TObjectType, TIDType> : IComparable, IComparable<TObjectType>, IObject<TIDType>
        where TObjectType : ObjectBaseClass<TObjectType, TIDType>, new()
        where TIDType : IComparable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        protected ObjectBaseClass()
        {
            ID = default!;
            Active = true;
            DateCreated = DateTime.Now;
            DateModified = DateTime.Now;
        }

        /// <summary>
        /// Is the object active?
        /// </summary>
        [BoolGenerator]
        public bool Active { get; set; }

        /// <summary>
        /// Date object was created
        /// </summary>
        [Between("1/1/1900", "1/1/2100")]
        [DateTimeGenerator("1/1/1900", "1/1/2100")]
        public DateTime DateCreated { get; set; }

        /// <summary>
        /// Date last modified
        /// </summary>
        [Between("1/1/1900", "1/1/2100")]
        [DateTimeGenerator("1/1/1900", "1/1/2100")]
        public DateTime DateModified { get; set; }

        /// <summary>
        /// ID for the object
        /// </summary>
        public TIDType ID { get; set; }

        /// <summary>
        /// != operator
        /// </summary>
        /// <param name="first">First item</param>
        /// <param name="second">Second item</param>
        /// <returns>returns true if they are not equal, false otherwise</returns>
        public static bool operator !=(ObjectBaseClass<TObjectType, TIDType> first, ObjectBaseClass<TObjectType, TIDType> second)
        {
            return !(first == second);
        }

        /// <summary>
        /// The &lt; operator
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>True if the first item is less than the second, false otherwise</returns>
        public static bool operator <(ObjectBaseClass<TObjectType, TIDType> left, ObjectBaseClass<TObjectType, TIDType> right)
        {
            return !ReferenceEquals(left, right) && !(left is null) && !(right is null) && left.CompareTo(right) < 0;
        }

        /// <summary>
        /// Implements the operator &lt;=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator <=(ObjectBaseClass<TObjectType, TIDType> left, ObjectBaseClass<TObjectType, TIDType> right)
        {
            return ReferenceEquals(left, right) || (!(left is null) && !(right is null) && left.CompareTo(right) <= 0);
        }

        /// <summary>
        /// The == operator
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>true if the first and second item are the same, false otherwise</returns>
        public static bool operator ==(ObjectBaseClass<TObjectType, TIDType> left, ObjectBaseClass<TObjectType, TIDType> right)
        {
            return ReferenceEquals(left, right) || (!(left is null) && !(right is null) && left.CompareTo(right) == 0);
        }

        /// <summary>
        /// The &gt; operator
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>True if the first item is greater than the second, false otherwise</returns>
        public static bool operator >(ObjectBaseClass<TObjectType, TIDType> left, ObjectBaseClass<TObjectType, TIDType> right)
        {
            return !ReferenceEquals(left, right) && !(left is null) && !(right is null) && left.CompareTo(right) > 0;
        }

        /// <summary>
        /// Implements the operator &gt;=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator >=(ObjectBaseClass<TObjectType, TIDType> left, ObjectBaseClass<TObjectType, TIDType> right)
        {
            return ReferenceEquals(left, right) || (!(left is null) && !(right is null) && left.CompareTo(right) >= 0);
        }

        /// <summary>
        /// Compares the object to another object
        /// </summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns>0 if they are equal, -1 if this is smaller, 1 if it is larger</returns>
        public int CompareTo(object? obj)
        {
            return obj is ObjectBaseClass<TObjectType, TIDType> objectBaseClass ? CompareTo(objectBaseClass) : -1;
        }

        /// <summary>
        /// Compares the object to another object
        /// </summary>
        /// <param name="other">Object to compare to</param>
        /// <returns>0 if they are equal, -1 if this is smaller, 1 if it is larger</returns>
        public virtual int CompareTo(TObjectType? other)
        {
            if (other is null)
                return -1;
            if (other.ID.CompareTo(default(TIDType)!) == 0 && ID.CompareTo(default(TIDType)!) == 0)
                return 0;
            return other.ID.CompareTo(ID);
        }

        /// <summary>
        /// Determines if two items are equal
        /// </summary>
        /// <param name="obj">The object to compare this to</param>
        /// <returns>true if they are the same, false otherwise</returns>
        public override bool Equals(object? obj)
        {
            return CompareTo(obj) == 0;
        }

        /// <summary>
        /// Returns the hash of this item
        /// </summary>
        /// <returns>the int hash of the item</returns>
        public override int GetHashCode() => ID.GetHashCode();

        /// <summary>
        /// Sets up the object for saving purposes
        /// </summary>
        public virtual void SetupObject() => DateModified = DateTime.Now;
    }
}