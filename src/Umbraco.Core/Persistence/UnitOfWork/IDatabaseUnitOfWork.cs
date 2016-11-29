namespace Umbraco.Core.Persistence.UnitOfWork
{
	/// <summary>
	/// Represents a persistence unit of work for working with a database.
	/// </summary>
	public interface IDatabaseUnitOfWork : IUnitOfWork
	{
        /// <summary>
        /// Gets the database context.
        /// </summary>
        DatabaseContext DatabaseContext { get; }

        /// <summary>
        /// Gets the current database instance.
        /// </summary>
		UmbracoDatabase Database { get; }

        /// <summary>
        /// Read-locks some lock objects.
        /// </summary>
        /// <param name="lockIds">The lock object identifiers.</param>
	    void ReadLock(params int[] lockIds);

        /// <summary>
        /// Write-locks some lock objects.
        /// </summary>
        /// <param name="lockIds">The lock object identifiers.</param>
	    void WriteLock(params int[] lockIds);
    }
}