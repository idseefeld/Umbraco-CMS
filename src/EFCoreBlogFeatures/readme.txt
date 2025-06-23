# This file is used to document the steps to create the initial migration for the BlogContext in the EFCoreBlogFeatures project.

dotnet ef migrations add initialCreateBolgComments -s src/Umbraco.Web.UI -p src/EFCoreBlogFeatures --context BlogContext --no-build --verbose

# remove with

dotnet ef migrations remove -s src/Umbraco.Web.UI -p src/EFCoreBlogFeatures --context BlogContext
