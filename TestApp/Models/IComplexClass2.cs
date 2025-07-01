using System;

namespace TestApp.Models
{
    /// <summary>
    /// Represents a complex model with a date value.
    /// </summary>
    public interface IComplexClass2 : IModel
    {
        /// <summary>
        /// Gets or sets the date value associated with the model.
        /// </summary>
        DateTime DateValue { get; set; }
    }
}