# GeoVise

GeoVise is a .NET solution with an ASP.NET Core web application, a Web API, and a shared data model. It provides account registration and login, a map interface with OpenStreetMap, editable/queryable GIS data flows, and optional GeoServer WMS layers.

## Features

- User registration and login through the API, with JWT authentication.
- A web interface for map viewing, GIS data entry, querying, and layer controls.
- PostgreSQL persistence through Entity Framework Core migrations.
- OpenStreetMap as the default base map.
- Optional WMS layers from GeoServer. Layer names are loaded from the API and are not hard-coded in the browser.

## Technology

- .NET 8 / ASP.NET Core
- PostgreSQL
- Entity Framework Core 9 and Npgsql
- OpenLayers (BSD 2-Clause)
- Bootstrap, jQuery, jsPanel, Font Awesome, and other browser libraries under `WebApp/wwwroot/lib` or loaded from their public CDNs

Some table definitions were inferred from application code rather than verified against an original database. See [docs/inferred-schema.md](./docs/inferred-schema.md) for schema details and assumptions.

## Requirements

- .NET 8 SDK
- PostgreSQL
- `dotnet-ef` 9.0.14

Install the EF Core command-line tool if needed:

```powershell
dotnet tool install --global dotnet-ef --version 9.0.14
```

## Local setup

1. Create a local PostgreSQL database (for example, `geovise_dev`) using your preferred PostgreSQL tool.
2. Configure the API's connection string and JWT signing key with User Secrets. Replace the placeholders locally; do not commit real values:

   ```powershell
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<LOCAL_POSTGRES_CONNECTION_STRING>" --project .\WebApi\WebApi.csproj
   $key = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(64))
   dotnet user-secrets set "Jwt:Key" $key --project .\WebApi\WebApi.csproj
   ```

   `Jwt:Issuer`, `Jwt:Audience`, and `Jwt:ExpirationMinutes` have non-secret development defaults in `WebApi/appsettings.json`. A JWT key must contain at least 32 UTF-8 bytes.

3. Apply the database migrations:

   ```powershell
   dotnet ef database update --project .\ModelClass\ModelClass.csproj --startup-project .\WebApi\WebApi.csproj --context MyDbContext
   ```

4. Start the API and web app in separate terminals from the solution directory:

   ```powershell
   dotnet run --project .\WebApi\WebApi.csproj
   ```

   ```powershell
   dotnet run --project .\WebApp\WebApp.csproj
   ```

   The launch profiles use API ports `https://localhost:7227` / `http://localhost:5097` and web app ports `https://localhost:7203` / `http://localhost:5078`. The web app's API address is configured by `Api:BaseUrl`.

## Creating the first local user

There are no default users or credentials. With the API running in Development, open its Swagger page at `https://localhost:7227/swagger` and call `POST /api/Token/Register` with a username and a locally chosen password. Registration requires a password between 12 and 256 characters. Then sign in to the web app with that account.

## Optional: GeoServer

GeoServer is optional. With `GeoServer:BaseUrl` empty, the map uses OpenStreetMap and does not add a GeoServer layer or request GeoServer layer names.

To enable WMS layers:

1. Publish the layers required by your local database and application in GeoServer. The app requests the user's layer names through the API; it does not define or guess a workspace or layer list in the browser. Ensure the names returned by the API resolve to published GeoServer WMS layers in the appropriate workspace.
2. Set `GeoServer:BaseUrl` to the GeoServer base URL ending in its GeoServer context path (for example, a local development URL). Set it in an ignored `WebApp/appsettings.Development.json` file or the `GeoServer__BaseUrl` environment variable. `WebApp/appsettings.Example.json` shows a local example.
3. If GeoServer is hosted on a different origin from the web app, configure GeoServer/server-side CORS to allow the web app origin. Do not put GeoServer credentials in tracked files; this integration uses the configured base URL and does not require credentials in the client.

## Build and test

```powershell
dotnet restore .\Geovise.sln
dotnet build .\Geovise.sln
dotnet test .\Geovise.sln
```

## Architecture

- `WebApp`: ASP.NET Core MVC interface; sends API requests to the configured API base URL.
- `WebApi`: API endpoints, registration/login, JWT creation, and database access.
- `ModelClass`: shared entities, EF Core context, and migrations.
- `WebApi.Tests`: API service tests.

## Third-party licenses

OpenLayers is distributed under the BSD 2-Clause license; its license text is in `WebApp/wwwroot/lib/ol/LICENSE`. Other vendored libraries retain their license files or upstream license headers where provided. Review the notices in `WebApp/wwwroot/lib` before redistribution.
