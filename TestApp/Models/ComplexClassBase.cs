namespace TestApp.Models
{
    /// <summary>
    /// Represents the abstract base class for complex classes, implementing <see cref="ICompexClass"/>.
    /// </summary>
    public abstract class ComplexClassBase : ICompexClass
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ComplexClassBase"/> class.
        /// </summary>
        protected ComplexClassBase()
        {
        }

        /// <summary>
        /// Gets or sets the unique identifier for the model.
        /// </summary>
        public long ID { get; set; }

        /// <summary>
        /// Gets or sets the secondary integer value.
        /// </summary>
        public int Value2 { get; set; }

        /// <summary>
        /// Gets or sets the third value associated with the complex class.
        /// </summary>
        public long Value3 { get; set; }
    }
}