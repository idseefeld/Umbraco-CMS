# open project "Our.Umbraco.PostgreSql.EFCore" (this) in terminal and execute the following command to create the initial migration for OpenIdDict V5
# Make sure you have the correct path to your Umbraco project in the -s parameter
# Make sure you have the correct connection string in appsettings.json of your Umbraco project:
# "ConnectionStrings": {
#    "umbracoDbDSN": "Host=[SERVER (e.g.: localhost)];Port=[PORT (default: 5433)];Database=[DATABASE_NAME];Username=[DATABASE_USER];Password=[DATABASE_PASSWORD];",
#    "umbracoDbDSN_ProviderName": "Npgsql"
#  }

dotnet ef migrations add initialCreate -s ../Umbraco.Web.UI/ --context PostgreSqlDbContext
