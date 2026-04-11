# Doc Automation Portal

Portal web para **gobernar y generar MVPs de pases a producción** a partir de templates configurables. Los administradores definen templates con descripciones HTML ricas y variables dinámicas; los operadores generan el documento completo (listo para pegar en Jira) completando un formulario.

## Stack

| Capa               | Tecnología                      |
| ------------------ | ------------------------------- |
| Frontend + Backend | Blazor Server (.NET 9)          |
| UI                 | Radzen.Blazor (`standard-base`) |
| Orquestación dev   | .NET Aspire 9.3                 |
| Base de datos      | SQL Server 2022                 |
| ORM                | Entity Framework Core 9         |
| CQRS               | Mediador propio (sin MediatR)   |
| Validación         | FluentValidation                |
| Hosting            | IIS / Windows Server            |

## Estructura

```
src/
├── DocAutomation.Domain/            # Entidades, enums
├── DocAutomation.Application/
│   ├── Common/Cqrs/                 # ICommand, IQuery, IMediator, Mediator
│   ├── Interfaces/                  # Repos, UoW, renderer
│   ├── Services/                    # JsonValidator, TemplateRenderer
│   └── Features/                    # Feature folders por bounded context
│       ├── Templates/{Commands,Queries,Models}
│       └── Deployments/{Commands,Queries,Models}
├── DocAutomation.Infrastructure/    # EF Core, repos, migraciones, seeder
├── DocAutomation.Web/               # Blazor Server + Radzen
├── DocAutomation.AppHost/           # Aspire — orquesta SQL Server + Web
└── DocAutomation.ServiceDefaults/   # OpenTelemetry, health checks
```

## Cómo correr

**Prerequisitos:** .NET 9 SDK, Docker/OrbStack, certificado HTTPS de dev (`dotnet dev-certs https --trust`).

```bash
dotnet restore doc-automation.slnx
dotnet run --project src/DocAutomation.AppHost
```

Aspire abre el dashboard en `https://localhost:17xxx`. Al arrancar levanta SQL Server en container, aplica migraciones, seedea el template `iis-web-deploy` y abre el portal.

## Flujos

**Admin — crear template**

1. `/admin/templates` → Nuevo Template
2. Definir nombre, slug, variables del formulario y pasos con descripciones HTML ricas
3. Guardar (`Ctrl+S`)

**Operador — generar MVP**

1. `/deployments/new` → seleccionar template → completar formulario
2. Generar MVP → copiar como HTML o descargar `.html` → pegar en Jira

## Comandos útiles

```bash
dotnet build doc-automation.slnx
dotnet format doc-automation.slnx

# Nueva migración EF
dotnet ef migrations add <Nombre> \
  --project src/DocAutomation.Infrastructure \
  --startup-project src/DocAutomation.Web \
  --output-dir Persistence/Migrations
```

## CI/CD

| Workflow         | Trigger                      | Acción                                               |
| ---------------- | ---------------------------- | ---------------------------------------------------- |
| `ci.yml`         | push/PR a `main` y `develop` | Restore, build con `/warnaserror`, test              |
| `publish.yml`    | push a `main` o release tag  | Publish `win-x64` framework-dependent + zip artifact |
| `dependabot.yml` | semanal                      | NuGet (agrupado) y GitHub Actions                    |
