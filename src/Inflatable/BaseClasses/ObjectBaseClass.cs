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
using Inflatable.Interfaces;
using Mirage.Generators;
using System;
using System.Collections.Generic;
using System.Linq;
using Valkyrie;

namespace Inflatable.BaseClasses
{
    /// <summary>
    /// Object base class helper. This is not required but automatically sets up basic functions and
    /// properties to simplify things a bit.
    /// </summary>
    /// <typeparam name="IDType">ID type</typeparam>
    /// <typeparam name="ObjectType">Object type (must be the child object type)</typeparam>
    public abstract class ObjectBaseClass<ObjectType, IDType> : IComparable, IComparable<ObjectType>, IObject<IDType>
        where ObjectType : ObjectBaseClass<ObjectType, IDType>, new()
        where IDType : IComparable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        protected ObjectBaseClass()
        {
            Active = true;
            DateCreated = DateTime.Now;
            DateModified = DateTime.Now;
        }

        /// <summary>
        /// Is the object active?
        /// </summary>
        [BoolGenerator]
        public virtual bool Active { get; set; }

        /// <summary>
        /// Date object was created
        /// </summary>
        [Between("1/1/1900", "1/1/2100")]
        [DateTimeGenerator("1/1/1900", "1/1/2100")]
        public virtual DateTime DateCreated { get; set; }

        /// <summary>
        /// Date last modified
        /// </summary>
        [Between("1/1/1900", "1/1/2100")]
        [DateTimeGenerator("1/1/1900", "1/1/2100")]
        public virtual DateTime DateModified { get; set; }

        /// <summary>
        /// ID for the object
        /// </summary>
        public virtual IDType ID { get; set; }

        /// <summary>
        /// Loads the items based on type
        /// </summary>
        /// <param name="Params">Parameters used to specify what to load</param>
        /// <returns>All items that fit the specified query</returns>
        public static IEnumerable<ObjectType> All(params IParameter[] Params)
        {
            IEnumerable<ObjectType> instance = new List<ObjectType>();
            instance = QueryProvider.All<ObjectType>(Params);
            return instance;
        }

        /// <summary>
        /// Loads the items based on the criteria specified
        /// </summary>
        /// <param name="Command">Command to run</param>
        /// <param name="Type">Command type</param>
        /// <param name="ConnectionString">Connection string name</param>
        /// <param name="Params">Parameters used to specify what to load</param>
        /// <returns>The specified items</returns>
        public static IEnumerable<ObjectType> All(string Command, CommandType Type, string ConnectionString, params object[] Params)
        {
            IEnumerable<ObjectType> instance = new List<ObjectType>();
            instance = QueryProvider.All<ObjectType>(Command, Type, ConnectionString, Params);

            return instance;
        }

        /// <summary>
        /// Loads the item based on the criteria specified
        /// </summary>
        /// <param name="Params">Parameters used to specify what to load</param>
        /// <returns>The specified item</returns>
        public static ObjectType Any(params IParameter[] Params)
        {
            var instance = new ObjectType();
            instance = QueryProvider.Any<ObjectType>(Params);
            return instance;
        }

        /// <summary>
        /// Loads the item based on the criteria specified
        /// </summary>
        /// <param name="Command">Command to run</param>
        /// <param name="Type">Command type</param>
        /// <param name="ConnectionString">Connection string name</param>
        /// <param name="Params">Parameters used to specify what to load</param>
        /// <returns>The specified item</returns>
        public static ObjectType Any(string Command, CommandType Type, string ConnectionString, params object[] Params)
        {
            var instance = new ObjectType();
            instance = QueryProvider.Any<ObjectType>(Command, Type, ConnectionString, Params);
            return instance;
        }

        /// <summary>
        /// != operator
        /// </summary>
        /// <param name="first">First item</param>
        /// <param name="second">Second item</param>
        /// <returns>returns true if they are not equal, false otherwise</returns>
        public static bool operator !=(ObjectBaseClass<ObjectType, IDType> first, ObjectBaseClass<ObjectType, IDType> second)
        {
            return !(first == second);
        }

        /// <summary>
        /// The &lt; operator
        /// </summary>
        /// <param name="first">First item</param>
        /// <param name="second">Second item</param>
        /// <returns>True if the first item is less than the second, false otherwise</returns>
        public static bool operator <(ObjectBaseClass<ObjectType, IDType> first, ObjectBaseClass<ObjectType, IDType> second)
        {
            if (ReferenceEquals(first, second))
                return false;
            if ((object)first == null || (object)second == null)
                return false;
            return first.GetHashCode() < second.GetHashCode();
        }

        /// <summary>
        /// The == operator
        /// </summary>
        /// <param name="first">First item</param>
        /// <param name="second">Second item</param>
        /// <returns>true if the first and second item are the same, false otherwise</returns>
        public static bool operator ==(ObjectBaseClass<ObjectType, IDType> first, ObjectBaseClass<ObjectType, IDType> second)
        {
            if (ReferenceEquals(first, second))
                return true;

            if ((object)first == null || (object)second == null)
                return false;

            return first.GetHashCode() == second.GetHashCode();
        }

        /// <summary>
        /// The &gt; operator
        /// </summary>
        /// <param name="first">First item</param>
        /// <param name="second">Second item</param>
        /// <returns>True if the first item is greater than the second, false otherwise</returns>
        public static bool operator >(ObjectBaseClass<ObjectType, IDType> first, ObjectBaseClass<ObjectType, IDType> second)
        {
            if (ReferenceEquals(first, second))
                return false;
            if ((object)first == null || (object)second == null)
                return false;
            return first.GetHashCode() > second.GetHashCode();
        }

        /// <summary>
        /// Gets the page count based on page size
        /// </summary>
        /// <param name="PageSize">Page size</param>
        /// <param name="Params">Parameters used to specify what to load</param>
        /// <returns>All items that fit the specified query</returns>
        public static int PageCount(int PageSize = 25, params IParameter[] Params)
        {
            return QueryProvider.PageCount<ObjectType>(PageSize, Params);
        }

        /// <summary>
        /// Loads the items based on type
        /// </summary>
        /// <param name="PageSize">Page size</param>
        /// <param name="CurrentPage">Current page (0 based)</param>
        /// <param name="OrderBy">The order by portion of the query</param>
        /// <param name="Params">Parameters used to specify what to load</param>
        /// <returns>All items that fit the specified query</returns>
        public static IEnumerable<ObjectType> Paged(int PageSize = 25, int CurrentPage = 0, string OrderBy = "", params IParameter[] Params)
        {
            IEnumerable<ObjectType> instance = new List<ObjectType>();
            instance = QueryProvider.Paged<ObjectType>(PageSize, CurrentPage, OrderBy, Params);
            return instance;
        }

        /// <summary>
        /// Saves a list of objects
        /// </summary>
        /// <param name="Objects">List of objects</param>
        public static void Save(IEnumerable<ObjectType> Objects)
        {
            if (Objects == null)
                return;
            Objects.ForEach(x => x.Save());
        }

        /// <summary>
        /// Compares the object to another object
        /// </summary>
        /// <param name="obj">Object to compare to</param>
        /// <returns>0 if they are equal, -1 if this is smaller, 1 if it is larger</returns>
        public int CompareTo(object obj)
        {
            if (obj is ObjectBaseClass<ObjectType, IDType>)
                return CompareTo((ObjectType)obj);
            return -1;
        }

        /// <summary>
        /// Compares the object to another object
        /// </summary>
        /// <param name="other">Object to compare to</param>
        /// <returns>0 if they are equal, -1 if this is smaller, 1 if it is larger</returns>
        public virtual int CompareTo(ObjectType other)
        {
            return other.ID.CompareTo(ID);
        }

        /// <summary>
        /// Deletes the item
        /// </summary>
        public virtual void Delete()
        {
            QueryProvider.Delete((ObjectType)this);
        }

        /// <summary>
        /// Determines if two items are equal
        /// </summary>
        /// <param name="obj">The object to compare this to</param>
        /// <returns>true if they are the same, false otherwise</returns>
        public override bool Equals(object obj)
        {
            var TempObject = obj as ObjectBaseClass<ObjectType, IDType>;
            if (ReferenceEquals(TempObject, null))
                return false;
            return TempObject.GetHashCode() == GetHashCode();
        }

        /// <summary>
        /// Returns the hash of this item
        /// </summary>
        /// <returns>the int hash of the item</returns>
        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }

        /// <summary>
        /// Saves the item (if it already exists, it updates the item. Otherwise it inserts the item)
        /// </summary>
        public virtual void Save()
        {
            SetupObject();
            this.Validate();
            QueryProvider.Save<ObjectType, IDType>((ObjectType)this);
        }

        /// <summary>
        /// Sets up the object for saving purposes
        /// </summary>
        public virtual void SetupObject()
        {
            DateModified = DateTime.Now;
        }
    }
}