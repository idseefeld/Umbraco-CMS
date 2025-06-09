namespace IdSeefeld.Umbraco.Cms.Persistence.PostgreSql
{
    /// <summary>
    ///     Constants related to PostgreSQL.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        ///     PostgreSQL provider name.
        /// </summary>
        public const string ProviderName = "Npgsql";

        /// <summary>
        /// PostgreSQL name
        /// </summary>
        public const string Name = "PostgreSql";

        /// <summary>
        /// Connect to default db to create new one.
        /// </summary>
        public const string PostgreSqlDefaultDatabase = "postgres";

        /// <summary>
        /// Default database name for Umbraco.
        /// </summary>
        public const string UmbracoDefaultDatabaseName = "umbraco";
    }
}
