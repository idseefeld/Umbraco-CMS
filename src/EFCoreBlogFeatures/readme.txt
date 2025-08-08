# This file is used to document the steps to create the initial migration for the BlogDbContext in the EFCoreBlogFeatures project.

dotnet ef migrations add initialCreateBlogComments -s src/Umbraco.Web.UI -p src/EFCoreBlogFeatures.PostgreSQL --context BlogDbContext -- --provider Npgsql

dotnet ef migrations add initialCreateBlogComments -s src/Umbraco.Web.UI -p src/EFCoreBlogFeatures.SqlServer -c BlogDbContext -- --provider Microsoft.Data.SqlClient

dotnet ef migrations add initialCreateBlogComments -s src/Umbraco.Web.UI -p src/EFCoreBlogFeatures.SQLite -c BlogDbContext -- --provider Microsoft.Data.Sqlite

# remove with

dotnet ef migrations remove -s src/Umbraco.Web.UI -p src/EFCoreBlogFeatures.PostgreSQL --context BlogDbContext


dotnet ef migrations remove -s src/Umbraco.Web.UI -p src/EFCoreBlogFeatures.SqlServer --context BlogDbContext


dotnet ef migrations remove -s src/Umbraco.Web.UI -p src/EFCoreBlogFeatures.SQLite --context BlogDbContext
