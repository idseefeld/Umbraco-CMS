# PostgreSQL Provider for Umbraco

## Install PostggreSQL Database
1. Download and install PostgreSQL from the [official website](https://www.postgresql.org/download/) or especially for [Windwos](https://www.postgresql.org/download/windows/).
1. Create a new database for Umbraco using the PostgreSQL command line or a GUI tool like pgAdmin. Follow my [tutorial video](https://youtu.be/6ruTSbTdzSk).
1. Start debugging this solution [F5]

## Known Issues
1. Can not create User Group
1. If not trusting databse certificate "SSL Mode" is set to `VerifyCA`. ToDo: "SSL Mode" should be configurable in appsettings. Read details: https://www.npgsql.org/doc/security.html?tabs=tabid-1
