# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project state

ASP.NET Core Web API (.NET 10, `net10.0`) created from the Visual Studio "ASP.NET Core Web API" template with controllers. No login/auth functionality exists yet; the only endpoint is the template's `WeatherForecastController`. The template's `WeatherForecast.cs` model was removed, so **the project currently does not build** (CS0246 in `Controllers/WeatherForecastController.cs`) until that controller is deleted or the model is restored.

There is no test project yet.

## Commands

```
dotnet build                          # build (solution file: LoginWebApp.slnx)
dotnet run --launch-profile http      # http://localhost:5154
dotnet run --launch-profile https     # https://localhost:7255 + http://localhost:5154
docker build -t loginwebapp .         # container listens on 8080 (http) / 8081 (https)
```

`LoginWebApp.http` holds sample requests for the VS/VS Code REST client, targeting `http://localhost:5154`.

## Architecture

- `Program.cs`: minimal hosting setup. Registers controllers and OpenAPI (`AddOpenApi`). The OpenAPI document is mapped only in Development, at `/openapi/v1.json`; there is no Swagger UI. The pipeline is HTTPS redirection → `UseAuthorization` → `MapControllers`. `UseAuthorization` is present, but no authentication scheme is registered yet.
- `Controllers/`: attribute-routed `[ApiController]` classes in the `LoginWebApp.Controllers` namespace, routed by `[Route("[controller]")]`.
- Config: `appsettings.json` / `appsettings.Development.json`. User Secrets is enabled (`UserSecretsId` in the csproj), so put local secrets such as connection strings and signing keys there with `dotnet user-secrets`, not in appsettings.
- Nullable reference types and implicit usings are enabled.
