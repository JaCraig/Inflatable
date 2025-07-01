using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TestApp.Models
{
    /// <summary>
    /// Represents a complex class that inherits from <see cref="ComplexClassBase"/>.
    /// </summary>
    public class ComplexClass : ComplexClassBase
    {
        /// <summary>
        /// Gets or sets the collection of related <see cref="IComplexClass2"/> instances.
        /// </summary>
        public virtual IList<IComplexClass2> ManyToOneProperty { get; set; } = [];

        /// <summary>
        /// Gets or sets the value for this complex class. Maximum length is 400 characters.
        /// </summary>
        [MaxLength(400)]
        public string Value1 { get; set; }
    }
}