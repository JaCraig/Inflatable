using System;
using System.ComponentModel.DataAnnotations;

namespace TestApp.Models
{
    /// <summary>
    /// Represents a complex data model with a date, identifier, and string value.
    /// </summary>
    public class ComplexClass2 : IComplexClass2
    {
        /// <summary>
        /// Gets or sets the date value associated with this instance.
        /// </summary>
        public DateTime DateValue { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for this instance.
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// Gets or sets the string value.
        /// </summary>
        [MaxLength]
        public string StringVal { get; set; }
    }
}