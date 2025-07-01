namespace TestApp.Models
{
    /// <summary>
    /// Represents a complex model with an additional <see cref="Value3"/> property.
    /// </summary>
    public interface ICompexClass : IModel
    {
        /// <summary>
        /// Gets or sets the third value associated with the complex class.
        /// </summary>
        long Value3 { get; set; }
    }
}