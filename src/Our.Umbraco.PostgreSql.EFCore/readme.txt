# open terminal from Solution context menu and execute the following command to create the initial migration for OpenIdDict V5
# Make sure you have the correct path to your Umbraco project in the -s parameter
# Make sure you have the correct connection string in appsettings.json of your Umbraco project:
# "ConnectionStrings": {
#    "umbracoDbDSN": "Host=[SERVER (e.g.: localhost)];Port=[PORT (default: 5433)];Database=[DATABASE_NAME];Username=[DATABASE_USER];Password=[DATABASE_PASSWORD];",
#    "umbracoDbDSN_ProviderName": "Npgsql"
#  }

# Tip
# The -- token directs dotnet ef to treat everything that follows as an argument
# and not try to parse them as options.
# Any extra arguments not used by dotnet ef are forwarded to the app.

dotnet ef migrations add InitialCreate -s src/Umbraco.Web.UI -p src/Our.Umbraco.PostgreSql.EFCore -c UmbracoDbContext
# -- --provider Npgsql

dotnet ef migrations add AddOpenIddict -s src/Umbraco.Web.UI -p src/Our.Umbraco.PostgreSql.EFCore -c UmbracoDbContext
# -- --provider Npgsql

dotnet ef migrations add UpdateOpenIddictToV5 -s src/Umbraco.Web.UI -p src/Our.Umbraco.PostgreSql.EFCore -c UmbracoDbContext
# -- --provider Npgsql

dotnet ef migrations add UpdateOpenIddictToV7 -s src/Umbraco.Web.UI -p src/Our.Umbraco.PostgreSql.EFCore -c UmbracoDbContext
# -- --provider Npgsql

# remove
dotnet ef migrations remove -s src/Umbraco.Web.UI -p src/Our.Umbraco.PostgreSql.EFCore -c UmbracoDbContext

# test
dotnet ef migrations add Test -s src/Umbraco.Web.UI -p src/Our.Umbraco.PostgreSql.EFCore -c UmbracoDbContext
