# Doc Automation Portal

Portal web para **gobernar y generar MVPs de pases a producción** a partir de templates configurables. En lugar de escribir a mano cada ticket Jira con los pasos de instalación, los administradores definen templates con variables dinámicas y los operadores generan el documento completo (listo para pegar en Jira) con un par de clicks.

## ¿Qué problema resuelve?

En la operación diaria, cada pase a producción (instalación de una web en IIS, actualización de un microservicio, cambio de certificado, etc.) requiere crear un ticket MVP en Jira con instrucciones muy específicas: rutas, comandos PowerShell, verificaciones, plan de reversión, tablas con credenciales, etc.

Escribir todo eso a mano, ticket por ticket, es:

- **Lento** — horas por cada pase
- **Propenso a errores** — un comando mal copiado puede tumbar producción
- **Inconsistente** — cada operador escribe el mismo paso de forma distinta
- **Difícil de mantener** — si un procedimiento cambia, hay que actualizarlo en cientos de tickets futuros

Este portal:

- ✅ **Templates reusables** con descripciones HTML ricas (tablas, código, callouts) y variables `{{dinámicas}}`
- ✅ **Editor WYSIWYG** estilo Jira por cada paso (colores, títulos, listas, links, source view)
- ✅ **Generación instantánea** del MVP con valores sustituidos a partir de un formulario dinámico
- ✅ **Soporte para verificaciones con acciones correctivas** anidadas (el clásico "si falla X, entonces hacer Y")
- ✅ **Secciones de Post-Pase y Reversión** estandarizadas por template
- ✅ **Export a HTML** listo para pegar en Jira o descargar como archivo

## Arquitectura

### Stack

| Capa               | Tecnología                                                                |
| ------------------ | ------------------------------------------------------------------------- |
| Backend + Frontend | [Blazor Server](https://learn.microsoft.com/aspnet/core/blazor/) (.NET 9) |
| UI Components      | [Radzen.Blazor](https://blazor.radzen.com/) (tema `standard-base`)        |
| Orquestación dev   | [.NET Aspire 9.3](https://learn.microsoft.com/dotnet/aspire/)             |
| Base de datos      | SQL Server 2022 (container en dev, instancia dedicada en prod)            |
| ORM                | Entity Framework Core 9                                                   |
| CQRS               | Mediador propio (sin MediatR — v12+ es comercial)                         |
| Validación         | FluentValidation                                                          |
| Hosting target     | IIS en Windows Server                                                     |

### Clean Architecture + Feature Folders

```
src/
├── DocAutomation.Domain/            # Entities, enums — sin dependencias externas
├── DocAutomation.Application/       # Use cases, CQRS, interfaces
│   ├── Common/
│   │   ├── Cqrs/                    # Mediador propio (ICommand, IQuery, IMediator)
│   │   ├── Result.cs
│   │   └── ValidationBehavior.cs
│   ├── Interfaces/                  # Cross-cutting (repos, UoW, renderer)
│   ├── Services/                    # Cross-cutting services
│   ├── Models/                      # Modelos cross-feature
│   └── Features/                    # ⭐ Bounded contexts
│       ├── Templates/
│       │   ├── Commands/            # Create, Update, Delete
│       │   ├── Queries/             # GetAll, GetById
│       │   └── Models/              # DTOs + StepDefinition
│       └── Deployments/
│           ├── Commands/            # Start, UpdateStepStatus, Complete
│           ├── Queries/             # GetAll, GetById
│           └── Models/              # DTOs
│
├── DocAutomation.Infrastructure/    # EF Core, repositorios, migraciones, seeder
├── DocAutomation.Web/               # Blazor Server (Razor Components)
│   └── Components/
│       ├── Layout/
│       ├── Pages/
│       │   ├── Admin/Templates/
│       │   └── Deployments/
│       └── Shared/                  # StepsEditor, StepEditCard, TemplateForm, ConfirmDialog
│
├── DocAutomation.AppHost/           # .NET Aspire — orquesta SQL Server + Web
└── DocAutomation.ServiceDefaults/   # Aspire service defaults (OpenTelemetry, health, etc.)
```

### CQRS propio

Por diseño este proyecto **no usa MediatR** (la versión 12+ requiere licencia comercial). Se implementa un mediador propio con separación explícita entre commands y queries:

```csharp
// Command — muta estado
public record CreateTemplateCommand(...) : ICommand<Guid>;

public class CreateTemplateCommandHandler(...) : ICommandHandler<CreateTemplateCommand, Guid>
{
    public Task<Guid> Handle(CreateTemplateCommand cmd, CancellationToken ct) { ... }
}

// Query — solo lectura
public record GetTemplateByIdQuery(Guid Id) : IQuery<TemplateDto?>;

public class GetTemplateByIdQueryHandler(...) : IQueryHandler<GetTemplateByIdQuery, TemplateDto?>
{
    public Task<TemplateDto?> Handle(GetTemplateByIdQuery q, CancellationToken ct) { ... }
}
```

El [`Mediator`](src/DocAutomation.Application/Common/Cqrs/Mediator.cs) usa el patrón de wrapper tipado (`RequestHandlerWrapperImpl<TRequest, TResponse>`) cacheado en un `ConcurrentDictionary` para dispatch type-safe sin reflection después de la primera llamada. Los `IPipelineBehavior<,>` permiten agregar validación, logging o transacciones cross-cutting sin tocar los handlers.

## Prerequisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/) / [OrbStack](https://orbstack.dev/) / Podman — para el container de SQL Server que levanta Aspire
- Editor: [Visual Studio 2022 17.12+](https://visualstudio.microsoft.com/), [Rider 2024.3+](https://www.jetbrains.com/rider/), o [VS Code](https://code.visualstudio.com/) con C# Dev Kit
- Certificado HTTPS de desarrollo de .NET (se instala con `dotnet dev-certs https --trust` en macOS/Windows)

## Cómo correr el portal en desarrollo

```bash
# Clonar el repo
git clone <repo-url>
cd doc-automation

# Asegurarse que el certificado HTTPS de dev esté confiado
dotnet dev-certs https --trust

# Restaurar paquetes
dotnet restore doc-automation.slnx

# Correr todo vía Aspire (levanta SQL Server en container + aplica migraciones + seedea template + arranca el Web)
dotnet run --project src/DocAutomation.AppHost
```

Aspire abre el dashboard en `https://localhost:17xxx` — desde ahí vas al link del Web.

**Qué pasa al arrancar:**

1. Aspire levanta un container de SQL Server 2022 con volumen persistente
2. El Web espera a que SQL esté healthy
3. EF Core aplica migraciones pendientes automáticamente
4. El `TemplateSeeder` inserta/reactiva el template `iis-web-deploy` con contenido detallado
5. Se abre el portal en el browser

## Estructura de la base de datos

| Tabla               | Propósito                                                     |
| ------------------- | ------------------------------------------------------------- |
| `Templates`         | Definición del template con metadata + steps en JSON          |
| `TemplateInputs`    | Variables dinámicas (key, label, tipo, required) por template |
| `Deployments`       | Instancias generadas a partir de un template + input values   |
| `DeploymentSteps`   | Flat de pasos renderizados del deployment                     |
| `DeploymentHistory` | Histórico para trust badges (Fase 5)                          |

Ver [DocAutomationDbContext](src/DocAutomation.Infrastructure/Persistence/DocAutomationDbContext.cs) y las [Configurations](src/DocAutomation.Infrastructure/Persistence/Configurations/) para los detalles.

### Template JSON schema

Cada template tiene un `StepsJson` con tres secciones:

```json
{
  "steps": [
    {
      "order": 1,
      "title": "Ingresar al servidor {{servidor}}",
      "type": "action",
      "description": "<h3>Objetivo</h3><p>...</p><pre><code>PowerShell command</code></pre>"
    },
    {
      "order": 2,
      "title": "Verificar Hosting Bundle",
      "type": "verification",
      "description": "<p>...</p>",
      "on_fail": {
        "steps": [
          {
            "title": "Instalar Hosting Bundle",
            "type": "action",
            "description": "..."
          }
        ]
      }
    }
  ],
  "post_steps": [
    { "title": "Limpiar carpeta", "type": "action", "description": "..." }
  ],
  "reversion": [
    { "title": "Restaurar backup", "type": "action", "description": "..." }
  ]
}
```

Las descripciones son **HTML puro** (bold, italic, color, tablas, code blocks, callouts — todo lo que el RadzenHtmlEditor soporta). Los placeholders `{{variable}}` son reemplazados por los valores del formulario al generar cada pase.

## Flujos principales

### Admin — crear template

1. `/admin/templates` → **Nuevo Template**
2. Completar nombre, slug, descripción general
3. Definir las **variables del formulario** (servidor, ticket Jira, rama, etc.) con su tipo
4. En la pestaña **Pasos Principales**, agregar cada paso:
   - Título corto
   - Tipo: Acción o Verificación
   - **Descripción HTML rica** con el editor completo (colores, tablas, código, callouts)
   - Referencias a variables con `{{nombre_variable}}`
5. Para pasos de **Verificación**, agregar acciones correctivas anidadas (`on_fail`)
6. Completar **Post-Pase** y **Reversión** de la misma forma
7. Guardar

### Operador — generar MVP

1. `/deployments/new`
2. Seleccionar template del dropdown
3. Completar el formulario dinámico con los valores del pase actual
4. **Generar MVP**
5. El portal renderiza el documento con todas las variables sustituidas
6. **Copiar como HTML** → pegar en Jira, o **Descargar .html**

## Comandos útiles

```bash
# Build
dotnet build doc-automation.slnx

# Formatear código
dotnet format doc-automation.slnx

# Crear una nueva migración EF
dotnet ef migrations add <NombreMigracion> \
  --project src/DocAutomation.Infrastructure \
  --startup-project src/DocAutomation.Web \
  --output-dir Persistence/Migrations

# Remover la última migración (antes de aplicar)
dotnet ef migrations remove \
  --project src/DocAutomation.Infrastructure \
  --startup-project src/DocAutomation.Web
```

## CI / CD

Ver [`.github/workflows/`](.github/workflows/):

| Workflow      | Trigger                              | Acción                                                                                           |
| ------------- | ------------------------------------ | ------------------------------------------------------------------------------------------------ |
| `ci.yml`      | push / PR a `main` y `develop`       | Restore, build con `/warnaserror`, test, publica resultados                                      |
| `publish.yml` | push a `main`, release tag, o manual | Publish del Web para IIS (`win-x64`, framework-dependent), empaqueta en zip y sube como artifact |

### Dependabot

Configurado en [`.github/dependabot.yml`](.github/dependabot.yml) para actualizar semanalmente:

- NuGet packages (agrupados por familia: AspNetCore, EF Core, Aspire, Radzen)
- GitHub Actions

### Deploy manual a IIS

El workflow de publish genera un zip con todos los binarios listos para IIS. Para desplegarlo:

1. Descargar el artifact del workflow run
2. Descomprimir en el servidor Windows donde está el App Pool del site
3. Instalar el [.NET 9 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/9.0) en el servidor si no está
4. Asegurar que el `web.config` apunta a `DocAutomation.Web.dll` con `hostingModel="inprocess"`
5. Configurar la connection string de SQL Server vía variables de entorno o `appsettings.Production.json`

> ⚠️ En producción, la autenticación debería ser Windows Auth (IIS integrado). El portal todavía no tiene autenticación — está en la lista de próximos features.

## Roadmap

### ✅ Ya implementado

- [x] CRUD de templates con editor HTML rico por paso
- [x] Generación de MVP a partir de template + formulario dinámico
- [x] Renderización con sustitución de variables
- [x] Soporte para verificaciones con `on_fail` anidado
- [x] Secciones Main / Post-Pase / Reversión
- [x] Export como HTML / copy to clipboard
- [x] Orquestación de dev con .NET Aspire
- [x] Mediador propio con ICommand/IQuery explícitos
- [x] Feature folders por bounded context
- [x] Seeder de template IIS con contenido detallado

### 🔄 Próximos

- [ ] **Autenticación Windows** y roles (Admin / Operator / Viewer)
- [ ] **Audit trail** — quién hizo qué y cuándo
- [ ] **Búsqueda y categorías** de templates
- [ ] **Duplicar template** como base para uno nuevo
- [ ] **Versionado visual** con diff entre versiones
- [ ] **Tests unitarios** de handlers, validators y renderer
- [ ] **Export a PDF** con QuestPDF
- [ ] **Export a Word (.docx)** con OpenXML SDK
- [ ] **Integración Jira** — crear el MVP directamente vía REST API
- [ ] **Trust Badge system** — historial con badges verde/amarillo por frecuencia de uso

## Licencia

Proyecto interno — uso restringido a BCP.
