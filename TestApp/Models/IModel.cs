namespace TestApp.Models
{
    /// <summary>
    /// Represents a model with a unique identifier.
    /// </summary>
    public interface IModel
    {
        /// <summary>
        /// Gets or sets the unique identifier for the model.
        /// </summary>
        long ID { get; set; }
    }
}