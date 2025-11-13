namespace Our.Umbraco.PostgreSql
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
        ///     PostgreSQL dbprovider name.
        /// </summary>
        public const string DbProvider = "Npgsql";
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

        public const string NoAutoIncrementTableNames = "cmsContentNu,cmsContentType2ContentType,cmsContentTypeAllowedContentType,cmsDocumentType,cmsMember,cmsMember2MemberGroup,cmsTagRelationship,umbracoAccess,umbracoAccessRule,umbracoContent,umbracoContentSchedule,umbracoContentVersionCleanupPolicy,umbracoDataType,umbracoDocument,umbracoDocumentVersion,umbracoKeyValue,umbracoLock,umbracoLongRunningOperation,umbracoMediaVersion,umbracoRedirectUrl,umbracoUser2ClientId,umbracoUser2NodeNotify,umbracoUser2UserGroup,umbracoUserData,umbracoUserGroup2App,umbracoUserGroup2Language,umbracoUserLogin,umbracoWebhook2ContentTypeKeys,umbracoWebhook2Events,umbracoWebhook2Headers";
                    
    }
}
