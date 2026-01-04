namespace Our.Umbraco.PostgreSql
{
    /// <summary>
    ///     Constants related to PostgreSQL.
    /// </summary>
    public static class Constants
    {
        public const string Configuration = "PostgreSqlOptions";
        /// <summary>
        ///     PostgreSQL provider name.
        /// </summary>
        public const string ProviderName = "Npgsql2";

        /// <summary>
        ///     PostgreSQL dbprovider name.
        /// </summary>
        public const string DbProvider = "Npgsql2";
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
        /// </summary>U
        public const string UmbracoDefaultDatabaseName = "Umbraco";

        /// <summary>
        /// Specifies the default maximum number of retry attempts for test operations.
        /// </summary>
        public const int TestRetryMax = 10; // default is 10
    }
}
