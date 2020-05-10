using BigBook;

namespace Inflatable.QueryProvider.Interfaces
{
    /// <summary>
    /// Data query generator interface
    /// </summary>
    /// <typeparam name="TObject">The type of the object.</typeparam>
    /// <seealso cref="IQueryGenerator{TObject}"/>
    public interface IDataQueryGenerator<TObject> : IQueryGenerator<TObject>
        where TObject : class
    {
        /// <summary>
        /// Generates the query.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns>The resulting query</returns>
        IQuery[] GenerateQueries(Dynamo[] ids);
    }
}