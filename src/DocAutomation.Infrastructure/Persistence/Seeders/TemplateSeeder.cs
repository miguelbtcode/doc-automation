using DocAutomation.Domain.Entities;
using DocAutomation.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace DocAutomation.Infrastructure.Persistence.Seeders;

public static class TemplateSeeder
{
    private const string Slug = "iis-web-deploy";
    private const string Name = "Instalación Web en IIS";
    private const string Description =
        "Pase a producción de una nueva versión de aplicación web hospedada en IIS. Incluye preparación, verificaciones, copia de archivos, validación post-deploy y plan de reversión.";

    /// <summary>
    /// Resetea por completo los datos del template (y todos sus deployments asociados)
    /// en cada arranque de la app, dejando el template recreado desde cero con 0 deployments.
    ///
    /// Solo debe usarse en Development. En producción, esto NUNCA debe ejecutarse —
    /// se llamaría desde Program.cs solo si IsDevelopment().
    /// </summary>
    public static async Task SeedAsync(DocAutomationDbContext context)
    {
        // 1. Borramos en orden inverso a las FK:
        //    DeploymentHistories → DeploymentSteps → Deployments → TemplateInputs → Templates
        //    Usamos ExecuteDeleteAsync para no pasar nada por el change tracker.
        await context.DeploymentHistories.ExecuteDeleteAsync();
        await context.DeploymentSteps.ExecuteDeleteAsync();
        await context.Deployments.ExecuteDeleteAsync();
        await context.TemplateInputs.ExecuteDeleteAsync();
        await context.Templates.IgnoreQueryFilters().ExecuteDeleteAsync();

        // 2. Insert fresco del template con todos sus inputs
        var template = new Template
        {
            Id = Guid.NewGuid(),
            Slug = Slug,
            IsActive = true,
            Name = Name,
            Description = Description,
            StepsJson = BuildStepsJson(),
            Version = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            CreatedBy = "system",
        };
        template.Inputs = BuildInputs(template.Id).ToList();

        context.Templates.Add(template);
        await context.SaveChangesAsync();
    }

    private static IEnumerable<TemplateInput> BuildInputs(Guid templateId)
    {
        yield return new TemplateInput
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            Key = "servidor",
            Label = "Servidor (hostname o IP)",
            InputType = InputType.Text,
            IsRequired = true,
            DisplayOrder = 1,
            HelpText = "Hostname o dirección IP del servidor Windows. Ej: SRVPRDWEB01",
        };
        yield return new TemplateInput
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            Key = "ticket_jira",
            Label = "Número de Ticket Jira (MVP)",
            InputType = InputType.Text,
            IsRequired = true,
            DisplayOrder = 2,
            HelpText = "Ejemplo: MVPLEGBCP-24567",
        };
        yield return new TemplateInput
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            Key = "rama_bitbucket",
            Label = "Rama Bitbucket",
            InputType = InputType.Text,
            IsRequired = true,
            DisplayOrder = 3,
            HelpText = "Nombre exacto de la rama a desplegar. Ej: release/v1.4.2",
        };
        yield return new TemplateInput
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            Key = "hosting_bundle_version",
            Label = "Versión esperada del Hosting Bundle",
            InputType = InputType.Text,
            IsRequired = true,
            DisplayOrder = 4,
            HelpText = "Ejemplo: 8.0.11 o 9.0.0",
        };
        yield return new TemplateInput
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            Key = "nombre_web",
            Label = "Nombre del Site IIS",
            InputType = InputType.Text,
            IsRequired = true,
            DisplayOrder = 5,
            HelpText = "Nombre exacto del site tal como aparece en IIS Manager.",
        };
        yield return new TemplateInput
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            Key = "app_pools",
            Label = "Application Pools",
            InputType = InputType.Text,
            IsRequired = true,
            DisplayOrder = 6,
            HelpText = "Separar con comas si hay más de uno.",
        };
        yield return new TemplateInput
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            Key = "ruta_fisica_web",
            Label = "Ruta física del site",
            InputType = InputType.Text,
            IsRequired = true,
            DisplayOrder = 7,
            HelpText = "Ejemplo: E:\\inetpub\\wwwroot\\MiApp",
        };
        yield return new TemplateInput
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            Key = "ruta_backup",
            Label = "Ruta para backup",
            InputType = InputType.Text,
            IsRequired = true,
            DisplayOrder = 8,
            HelpText = "Ejemplo: E:\\Backups",
        };
        yield return new TemplateInput
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            Key = "url_verificacion",
            Label = "URL de verificación",
            InputType = InputType.Text,
            IsRequired = true,
            DisplayOrder = 9,
            HelpText = "URL completa para verificar que el sitio responde tras el deploy.",
        };
        yield return new TemplateInput
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            Key = "usuario_instalador",
            Label = "Usuario de instalación",
            InputType = InputType.Text,
            IsRequired = true,
            DisplayOrder = 10,
            HelpText = "Cuenta de administración para conectarse al servidor.",
        };
    }

    private static string BuildStepsJson()
    {
        var doc = new
        {
            steps = new object[]
            {
                new
                {
                    order = 1,
                    title = "Ingresar al servidor con usuario administrador",
                    type = "action",
                    description = @"<h3>Objetivo</h3>
<p>Establecer conexión remota con el servidor de producción donde se realizará el pase. Esta conexión debe hacerse con un usuario con <strong>permisos de administrador local</strong> sobre el servidor y sobre IIS.</p>

<h3>Datos de conexión</h3>
<table>
  <thead>
    <tr><th>Parámetro</th><th>Valor</th></tr>
  </thead>
  <tbody>
    <tr><td><strong>Servidor</strong></td><td>{{servidor}}</td></tr>
    <tr><td><strong>Protocolo</strong></td><td>RDP (Escritorio Remoto) — puerto 3389</td></tr>
    <tr><td><strong>Usuario</strong></td><td>{{usuario_instalador}}</td></tr>
    <tr><td><strong>Password</strong></td><td>Solicitar al equipo de Servicio en horario hábil</td></tr>
  </tbody>
</table>

<h3>Procedimiento</h3>
<ol>
  <li>Abrir el cliente de <strong>Escritorio Remoto (mstsc.exe)</strong> en la PC del operador.</li>
  <li>Ingresar el hostname <code>{{servidor}}</code> en el campo <em>Equipo</em>.</li>
  <li>Click en <em>Conectar</em>. Al aparecer el prompt de credenciales, usar la cuenta <code>{{usuario_instalador}}</code>.</li>
  <li>Aceptar el certificado del servidor si aparece la advertencia.</li>
  <li>Una vez dentro del desktop remoto, <strong>verificar que el ambiente sea PRODUCCIÓN</strong> mirando el wallpaper o el banner del servidor.</li>
</ol>

<div data-panel-type=""warning""><p><strong>⚠ Importante:</strong> Si no tenés acceso al servidor o las credenciales fueron rotadas, solicitar elevación al equipo de Seguridad antes de iniciar el pase. <strong>NO</strong> pedir credenciales por canales no seguros (mail sin cifrar, chat público).</p></div>

<div data-panel-type=""note""><p><strong>💡 Nota:</strong> Dejá solo una sesión RDP abierta por servidor para evitar conflictos con otros operadores.</p></div>",
                },
                new
                {
                    order = 2,
                    title = "Crear carpeta de trabajo del pase en disco D",
                    type = "action",
                    description = @"<h3>Objetivo</h3>
<p>Crear un directorio temporal en el servidor donde se van a depositar los archivos de la nueva versión antes de ser promovidos al site de IIS. Esta carpeta sirve como área de staging y facilita la reversión si algo sale mal.</p>

<h3>Ruta a crear</h3>
<p><code>D:\{{ticket_jira}}</code></p>

<h3>Procedimiento</h3>
<ol>
  <li>Abrir <strong>PowerShell como Administrador</strong> (botón derecho → Ejecutar como administrador).</li>
  <li>Ejecutar el siguiente comando:</li>
</ol>
<pre><code>New-Item -Path ""D:\{{ticket_jira}}"" -ItemType Directory -Force</code></pre>
<ol start=""3"">
  <li>Verificar que la carpeta fue creada correctamente:</li>
</ol>
<pre><code>Test-Path ""D:\{{ticket_jira}}""
# Debe devolver: True</code></pre>

<div data-panel-type=""warning""><p><strong>💡 Convención:</strong> El nombre de la carpeta debe ser exactamente el <strong>número del ticket Jira</strong> para facilitar la trazabilidad. No usar espacios ni caracteres especiales.</p></div>",
                },
                new
                {
                    order = 3,
                    title = "Descargar archivos de Bitbucket a la carpeta de trabajo",
                    type = "action",
                    description = @"<h3>Objetivo</h3>
<p>Descargar el contenido de la rama específica del repositorio en Bitbucket a la carpeta de trabajo del servidor. Estos archivos incluyen el publish de la aplicación, instaladores auxiliares y scripts de configuración.</p>

<h3>Datos</h3>
<table>
  <thead>
    <tr><th>Parámetro</th><th>Valor</th></tr>
  </thead>
  <tbody>
    <tr><td><strong>Rama</strong></td><td>{{rama_bitbucket}}</td></tr>
    <tr><td><strong>Destino</strong></td><td><code>D:\{{ticket_jira}}</code></td></tr>
  </tbody>
</table>

<h3>Procedimiento (opción A — desde Bitbucket Web)</h3>
<ol>
  <li>Desde la PC del operador, navegar a Bitbucket y ubicar el repositorio.</li>
  <li>Cambiar a la rama <code>{{rama_bitbucket}}</code>.</li>
  <li>Click en <em>Download</em> → descargar el <code>.zip</code>.</li>
  <li>Copiar el zip al servidor vía portapapeles RDP o compartido de red.</li>
  <li>Extraer el zip en <code>D:\{{ticket_jira}}</code>.</li>
</ol>

<h3>Procedimiento (opción B — git clone directo en servidor)</h3>
<pre><code>cd D:\{{ticket_jira}}
git clone --branch {{rama_bitbucket}} --single-branch &lt;URL_REPO&gt; .</code></pre>

<h3>Verificación del contenido</h3>
<p>Una vez descargados los archivos, validar que estén presentes en la carpeta:</p>
<ul>
  <li>✔ Carpeta <code>publish</code> (o archivos <code>.dll</code>, <code>web.config</code>, <code>wwwroot</code>)</li>
  <li>✔ Instalador del Hosting Bundle (<code>dotnet-hosting-*.exe</code>) — <strong>sólo si aplica para esta versión</strong></li>
  <li>✔ Scripts SQL de migración (si el pase incluye cambios de base)</li>
  <li>✔ Archivo <code>README.md</code> o <code>CHANGELOG.md</code> con las notas del release</li>
</ul>

<div data-panel-type=""error""><p><strong>⚠ Importante:</strong> Validar que la rama coincide exactamente con la del ticket Jira. Un deploy con la rama incorrecta es el error más común y causa rollbacks innecesarios.</p></div>",
                },
                new
                {
                    order = 4,
                    title = "Verificar versión del .NET Hosting Bundle instalado",
                    type = "verification",
                    description = @"<h3>Objetivo</h3>
<p>Confirmar que el <strong>ASP.NET Core Runtime Hosting Bundle</strong> instalado en el servidor coincide con la versión requerida por la nueva aplicación. Si la versión es inferior a la esperada, la aplicación fallará al arrancar con error 500.30 o 500.19.</p>

<h3>Versión esperada</h3>
<p><code>{{hosting_bundle_version}}</code></p>

<h3>Procedimiento de verificación</h3>
<ol>
  <li>Desde PowerShell en el servidor, ejecutar:</li>
</ol>
<pre><code>dotnet --list-runtimes</code></pre>
<ol start=""2"">
  <li>Buscar en la salida la línea que comienza con <code>Microsoft.AspNetCore.App</code>. Ejemplo de salida esperada:</li>
</ol>
<pre><code>Microsoft.AspNetCore.App {{hosting_bundle_version}} [C:\Program Files\dotnet\shared\Microsoft.AspNetCore.App]
Microsoft.NETCore.App      {{hosting_bundle_version}} [C:\Program Files\dotnet\shared\Microsoft.NETCore.App]</code></pre>
<ol start=""3"">
  <li>Comparar con la versión esperada <code>{{hosting_bundle_version}}</code>.</li>
</ol>

<h3>Criterio de aprobación</h3>
<table>
  <thead>
    <tr><th>Resultado</th><th>Acción</th></tr>
  </thead>
  <tbody>
    <tr><td>✅ La versión instalada es <strong>igual o mayor</strong> a la esperada</td><td>Marcar como <strong>Pasa</strong> y continuar al paso 5</td></tr>
    <tr><td>❌ La versión instalada es <strong>menor</strong> o no está instalada</td><td>Marcar como <strong>Falla</strong> — se desplegarán los pasos correctivos</td></tr>
  </tbody>
</table>

<div data-panel-type=""error""><p><strong>⚠ Nota crítica:</strong> NO proceder con el pase si el Hosting Bundle no cumple el criterio. El sitio no responderá y tendrás que ejecutar la reversión.</p></div>",
                    on_fail = new
                    {
                        steps = new object[]
                        {
                            new
                            {
                                title = "Instalar el .NET Hosting Bundle requerido",
                                type = "action",
                                description = @"<h3>Objetivo</h3>
<p>Instalar la versión correcta del ASP.NET Core Hosting Bundle antes de continuar con el pase.</p>

<h3>Ubicación del instalador</h3>
<p>El instalador debe encontrarse en la carpeta de trabajo descargada en el paso 3:</p>
<p><code>D:\{{ticket_jira}}\dotnet-hosting-{{hosting_bundle_version}}-win.exe</code></p>

<div data-panel-type=""info""><p><strong>Si el instalador NO está en la carpeta:</strong> descargarlo de <a href=""https://dotnet.microsoft.com/download/dotnet"">dotnet.microsoft.com/download/dotnet</a> seleccionando <em>ASP.NET Core Runtime → Hosting Bundle → versión {{hosting_bundle_version}}</em>.</p></div>

<h3>Procedimiento de instalación</h3>
<ol>
  <li>Navegar a <code>D:\{{ticket_jira}}</code>.</li>
  <li>Click derecho en el instalador → <em>Ejecutar como administrador</em>.</li>
  <li>Aceptar los términos de licencia.</li>
  <li>Esperar que finalice la instalación (≈1-2 minutos).</li>
  <li>Al finalizar, <strong>NO reiniciar el servidor</strong>. Solo es necesario reiniciar el servicio de IIS:</li>
</ol>
<pre><code>net stop was /y
net start w3svc</code></pre>

<h3>Verificación post-instalación</h3>
<pre><code>dotnet --list-runtimes</code></pre>
<p>Confirmar que la versión <code>{{hosting_bundle_version}}</code> aparece en la lista. Luego volver al paso 4 y marcar como <strong>Pasa</strong>.</p>",
                            },
                        },
                    },
                },
                new
                {
                    order = 5,
                    title = "Generar backup del site actual antes de sobrescribir",
                    type = "action",
                    description = @"<h3>Objetivo</h3>
<p>Crear una copia completa del contenido actual del site en producción antes de aplicar la nueva versión. Este backup es la <strong>única fuente de recuperación</strong> en caso de necesitar reversión.</p>

<h3>Datos del backup</h3>
<table>
  <thead>
    <tr><th>Parámetro</th><th>Valor</th></tr>
  </thead>
  <tbody>
    <tr><td><strong>Origen</strong></td><td><code>{{ruta_fisica_web}}</code></td></tr>
    <tr><td><strong>Destino</strong></td><td><code>{{ruta_backup}}\{{ticket_jira}}_backup</code></td></tr>
    <tr><td><strong>Fecha</strong></td><td>Automática (timestamp en el nombre del backup)</td></tr>
  </tbody>
</table>

<h3>Procedimiento</h3>
<ol>
  <li>Abrir PowerShell como Administrador.</li>
  <li>Crear la carpeta destino del backup:</li>
</ol>
<pre><code>$backupPath = ""{{ruta_backup}}\{{ticket_jira}}_backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')""
New-Item -Path $backupPath -ItemType Directory -Force</code></pre>
<ol start=""3"">
  <li>Ejecutar <code>robocopy</code> para copiar todos los archivos preservando estructura y permisos:</li>
</ol>
<pre><code>robocopy ""{{ruta_fisica_web}}"" $backupPath /MIR /R:3 /W:5 /COPY:DAT /LOG:""$backupPath\backup.log""</code></pre>
<ol start=""4"">
  <li>Revisar el log de <code>robocopy</code>. El exit code debe ser <strong>0, 1 o 2</strong> (todos son estados OK).</li>
  <li>Guardar la ruta del backup en un lugar accesible (Notepad temporal o variable PowerShell) por si se necesita para reversión.</li>
</ol>

<h3>Tamaño estimado</h3>
<p>El backup debería ocupar aproximadamente lo mismo que el site actual. Verificar que hay espacio suficiente en <code>{{ruta_backup}}</code> antes de ejecutar.</p>

<div data-panel-type=""error""><p><strong>⚠ Importante:</strong> El backup es <strong>OBLIGATORIO</strong>. Si no se puede generar por falta de espacio o permisos, <strong>DETENER EL PASE</strong> y escalar al equipo de Infraestructura.</p></div>",
                },
                new
                {
                    order = 6,
                    title = "Detener Site IIS y Application Pools",
                    type = "action",
                    description = @"<h3>Objetivo</h3>
<p>Detener los recursos activos del site antes de reemplazar los archivos. Esto es necesario porque Windows bloquea los <code>.dll</code> en uso y el deploy fallaría con <em>access denied</em>.</p>

<h3>Recursos a detener</h3>
<table>
  <thead>
    <tr><th>Recurso</th><th>Nombre</th></tr>
  </thead>
  <tbody>
    <tr><td><strong>Site IIS</strong></td><td><code>{{nombre_web}}</code></td></tr>
    <tr><td><strong>Application Pools</strong></td><td><code>{{app_pools}}</code></td></tr>
  </tbody>
</table>

<h3>Procedimiento</h3>
<ol>
  <li>Importar el módulo de administración de IIS:</li>
</ol>
<pre><code>Import-Module WebAdministration</code></pre>
<ol start=""2"">
  <li>Detener primero el site:</li>
</ol>
<pre><code>Stop-Website -Name ""{{nombre_web}}""</code></pre>
<ol start=""3"">
  <li>Luego detener los application pools:</li>
</ol>
<pre><code>Stop-WebAppPool -Name ""{{app_pools}}""</code></pre>
<ol start=""4"">
  <li>Verificar que ambos estén en estado <code>Stopped</code>:</li>
</ol>
<pre><code>Get-Website -Name ""{{nombre_web}}"" | Select-Object Name, State
Get-WebAppPoolState -Name ""{{app_pools}}""</code></pre>

<h3>Criterio de éxito</h3>
<ul>
  <li>Site state: <code>Stopped</code></li>
  <li>App pool state: <code>Stopped</code></li>
</ul>

<div data-panel-type=""note""><p><strong>💡 Tip:</strong> A veces los application pools tardan unos segundos en detenerse por completo. Si el comando devuelve error de timeout, esperar 10 segundos y verificar el estado antes de continuar.</p></div>

<div data-panel-type=""warning""><p><strong>⚠ Si es un ambiente con múltiples nodos:</strong> Este paso debe ejecutarse en cada nodo del balanceador. Verificar con Infraestructura si aplica.</p></div>",
                },
                new
                {
                    order = 7,
                    title = "Copiar el publish de la nueva versión al site físico",
                    type = "action",
                    description = @"<h3>Objetivo</h3>
<p>Reemplazar el contenido del site en producción con los archivos de la nueva versión descargados previamente en la carpeta de trabajo.</p>

<h3>Rutas</h3>
<table>
  <thead>
    <tr><th>Parámetro</th><th>Valor</th></tr>
  </thead>
  <tbody>
    <tr><td><strong>Origen</strong></td><td><code>D:\{{ticket_jira}}\publish</code></td></tr>
    <tr><td><strong>Destino</strong></td><td><code>{{ruta_fisica_web}}</code></td></tr>
  </tbody>
</table>

<h3>Procedimiento</h3>
<ol>
  <li>Verificar que los application pools estén detenidos (paso 6 completado):</li>
</ol>
<pre><code>Get-WebAppPoolState -Name ""{{app_pools}}""</code></pre>
<ol start=""2"">
  <li>Ejecutar <code>robocopy</code> con el flag <code>/MIR</code> para copiar y eliminar archivos obsoletos:</li>
</ol>
<pre><code>robocopy ""D:\{{ticket_jira}}\publish"" ""{{ruta_fisica_web}}"" /MIR /R:3 /W:5 /COPY:DAT /LOG:""D:\{{ticket_jira}}\deploy.log""</code></pre>
<ol start=""3"">
  <li>Revisar el log de copia. El exit code esperado es <strong>0, 1, 2 o 3</strong> (todos son OK).</li>
  <li>Verificar que los archivos clave estén presentes:</li>
</ol>
<pre><code>Get-ChildItem ""{{ruta_fisica_web}}\web.config""
Get-ChildItem ""{{ruta_fisica_web}}\*.dll"" | Select-Object Name, LastWriteTime | Sort-Object LastWriteTime -Descending | Select-Object -First 5</code></pre>

<h3>Importante sobre el flag /MIR</h3>
<p>El flag <code>/MIR</code> (mirror) asegura que los archivos que existen en el destino pero <strong>no en el origen</strong> sean <strong>eliminados</strong>. Esto previene dejar DLLs viejas que puedan causar conflictos de versión.</p>

<div data-panel-type=""warning""><p><strong>⚠ Cuidado con appsettings.json:</strong> Si el servidor tiene un <code>appsettings.Production.json</code> local que NO viene en el publish, <code>/MIR</code> lo va a borrar. Verificar si aplica y si es así, respaldarlo antes y restaurarlo después del copy.</p></div>

<div data-panel-type=""info""><p><strong>💡 Archivos transformados:</strong> Si el ambiente usa Config Transform, validar que el <code>web.config</code> copiado tenga los settings correctos de producción.</p></div>",
                },
                new
                {
                    order = 8,
                    title = "Iniciar Site IIS y Application Pools",
                    type = "action",
                    description = @"<h3>Objetivo</h3>
<p>Levantar el site y los application pools para que la nueva versión comience a servir tráfico.</p>

<h3>Procedimiento</h3>
<ol>
  <li>Iniciar <strong>primero</strong> los application pools (deben estar arriba antes que el site):</li>
</ol>
<pre><code>Start-WebAppPool -Name ""{{app_pools}}""</code></pre>
<ol start=""2"">
  <li>Luego iniciar el site:</li>
</ol>
<pre><code>Start-Website -Name ""{{nombre_web}}""</code></pre>
<ol start=""3"">
  <li>Verificar el estado:</li>
</ol>
<pre><code>Get-Website -Name ""{{nombre_web}}"" | Select-Object Name, State
Get-WebAppPoolState -Name ""{{app_pools}}""</code></pre>

<h3>Criterio de éxito</h3>
<ul>
  <li>Site state: <code>Started</code></li>
  <li>App pool state: <code>Started</code></li>
  <li>No hay errores en el Event Viewer (<code>eventvwr.msc</code>) relacionados a IIS en los últimos minutos</li>
</ul>

<div data-panel-type=""info""><p><strong>💡 Warm-up:</strong> La primera request al site puede tardar 10-30 segundos porque ASP.NET Core compila vistas y carga el DI container. Eso es normal.</p></div>",
                },
                new
                {
                    order = 9,
                    title = "Verificar que el sitio responde correctamente",
                    type = "verification",
                    description = @"<h3>Objetivo</h3>
<p>Validar que la nueva versión está funcionando correctamente mediante pruebas de smoke test sobre la URL pública del sitio.</p>

<h3>URL a verificar</h3>
<p><strong><a href=""{{url_verificacion}}"">{{url_verificacion}}</a></strong></p>

<h3>Checks a validar</h3>
<table>
  <thead>
    <tr><th>#</th><th>Check</th><th>Criterio</th></tr>
  </thead>
  <tbody>
    <tr><td>1</td><td>HTTP Status Code</td><td>200 OK (no 500, 503, 502, 404)</td></tr>
    <tr><td>2</td><td>Carga visual</td><td>La página home carga completa sin errores</td></tr>
    <tr><td>3</td><td>Versión mostrada</td><td>Si la app muestra versión, corresponde a la nueva</td></tr>
    <tr><td>4</td><td>Event Log</td><td>No hay errores críticos en el log de aplicación del servidor en los últimos 5 minutos</td></tr>
    <tr><td>5</td><td>Endpoint de health</td><td>Si existe <code>/health</code> o <code>/ping</code>, responde 200</td></tr>
  </tbody>
</table>

<h3>Procedimiento</h3>
<ol>
  <li>Desde una máquina con acceso a la URL, abrir el navegador.</li>
  <li>Navegar a <code>{{url_verificacion}}</code>.</li>
  <li>Verificar visualmente que la página carga.</li>
  <li>Abrir DevTools (F12) → pestaña <em>Network</em> → filtrar por <em>Doc</em> → verificar que el primer request retornó <strong>200</strong>.</li>
  <li>Desde el servidor, verificar el Event Viewer:</li>
</ol>
<pre><code>Get-EventLog -LogName Application -Source ""IIS*"", ""ASP.NET*"" -Newest 20 -EntryType Error, Warning</code></pre>

<h3>Criterio de aprobación</h3>
<table>
  <thead>
    <tr><th>Resultado</th><th>Acción</th></tr>
  </thead>
  <tbody>
    <tr><td>✅ Todos los checks pasan</td><td>Marcar como <strong>Pasa</strong> y continuar al Post-Pase</td></tr>
    <tr><td>❌ Cualquier check falla</td><td>Marcar como <strong>Falla</strong> e iniciar inmediatamente la sección de <strong>Reversión</strong></td></tr>
  </tbody>
</table>

<div data-panel-type=""warning""><p><strong>⚠ NO INTENTAR ARREGLAR EN CALIENTE.</strong> Si el sitio no responde correctamente, ejecutar reversión y resolver el problema en ambiente pre-productivo. Intentar fixes en caliente multiplica el tiempo de downtime.</p></div>",
                    on_fail = new
                    {
                        steps = new object[]
                        {
                            new
                            {
                                title = "Iniciar proceso de Reversión inmediatamente",
                                type = "action",
                                description = @"<h3>Acción</h3>
<p>El sitio no respondió correctamente tras el pase. <strong>Proceder inmediatamente</strong> con la sección de <strong>Reversión</strong> de este documento para restaurar la versión anterior.</p>

<h3>Comunicación</h3>
<ol>
  <li>Notificar al canal de guardia y al TL del equipo.</li>
  <li>Indicar el ticket <code>{{ticket_jira}}</code> y el motivo del rollback.</li>
  <li>Ejecutar los pasos de la sección <strong>Reversión</strong> en orden estricto.</li>
  <li>Una vez completada la reversión, programar análisis post-mortem para identificar la causa raíz.</li>
</ol>

<div data-panel-type=""warning""><p><strong>⚠ Prioridad MÁXIMA:</strong> Un sitio caído en producción afecta directamente a clientes. Cada minuto cuenta.</p></div>",
                            },
                        },
                    },
                },
            },
            post_steps = new object[]
            {
                new
                {
                    order = 1,
                    title = "Validación extendida post-deploy (15 minutos de monitoreo)",
                    type = "action",
                    description = @"<h3>Objetivo</h3>
<p>Durante los primeros 15 minutos después del pase exitoso, mantener un monitoreo activo de la aplicación para detectar problemas que puedan aparecer bajo carga real.</p>

<h3>Qué monitorear</h3>
<ul>
  <li>📊 <strong>Dashboard de APM</strong> (New Relic, Dynatrace, App Insights): errores 5xx, latencia p95, throughput</li>
  <li>📊 <strong>Logs del servidor</strong>: buscar excepciones nuevas en los logs de aplicación</li>
  <li>📊 <strong>CPU y memoria del servidor</strong>: no deben subir más del 20% respecto al baseline</li>
  <li>📊 <strong>Event Viewer</strong>: sin errores nuevos de IIS o .NET Runtime</li>
</ul>

<h3>Comandos útiles en el servidor</h3>
<pre><code># Ver últimos errores de la aplicación
Get-EventLog -LogName Application -Source ""IIS*"", ""ASP.NET*"", "".NET Runtime"" -Newest 20 -After (Get-Date).AddMinutes(-15)

# Ver procesos w3wp (worker process de IIS)
Get-Process w3wp | Select-Object Id, CPU, WS, PagedMemorySize</code></pre>

<div data-panel-type=""warning""><p><strong>⚠ Si durante los 15 minutos aparece algún error significativo, ejecutar Reversión.</strong> Es mejor revertir temprano que tarde.</p></div>",
                },
                new
                {
                    order = 2,
                    title = "Eliminar carpeta de trabajo del servidor",
                    type = "action",
                    description = @"<h3>Objetivo</h3>
<p>Una vez confirmado que el pase fue exitoso y la aplicación está estable, liberar el espacio ocupado por la carpeta temporal en el disco D.</p>

<h3>Procedimiento</h3>
<ol>
  <li>Desde PowerShell como administrador:</li>
</ol>
<pre><code>Remove-Item -Path ""D:\{{ticket_jira}}"" -Recurse -Force</code></pre>
<ol start=""2"">
  <li>Verificar que la carpeta fue eliminada:</li>
</ol>
<pre><code>Test-Path ""D:\{{ticket_jira}}""
# Debe devolver: False</code></pre>

<div data-panel-type=""error""><p><strong>💡 Nota:</strong> El backup NO se elimina. Se mantiene durante al menos <strong>30 días</strong> en <code>{{ruta_backup}}</code> por si se necesita rollback retroactivo o análisis forense.</p></div>",
                },
                new
                {
                    order = 3,
                    title = "Documentar en el ticket Jira que el pase fue exitoso",
                    type = "action",
                    description = @"<h3>Objetivo</h3>
<p>Actualizar el ticket Jira <code>{{ticket_jira}}</code> con la evidencia del pase exitoso para cerrar el ciclo de vida del release.</p>

<h3>Información a documentar en el ticket</h3>
<ul>
  <li>✔ <strong>Fecha y hora exacta</strong> del pase (inicio y fin)</li>
  <li>✔ <strong>Versión desplegada</strong>: rama <code>{{rama_bitbucket}}</code></li>
  <li>✔ <strong>Servidor afectado</strong>: <code>{{servidor}}</code></li>
  <li>✔ <strong>Ruta del backup</strong>: <code>{{ruta_backup}}\{{ticket_jira}}_backup_*</code></li>
  <li>✔ <strong>Screenshot</strong> de la página funcionando post-deploy</li>
  <li>✔ <strong>Resultado del smoke test</strong></li>
</ul>

<h3>Transición del estado</h3>
<p>Mover el ticket Jira al estado <strong>Listo para Cerrar</strong> o <strong>Cerrado</strong> según el workflow del equipo.</p>",
                },
                new
                {
                    order = 4,
                    title = "Cerrar sesión RDP del servidor",
                    type = "action",
                    description = @"<h3>Objetivo</h3>
<p>Cerrar formalmente la sesión de Escritorio Remoto. Las sesiones abiertas sin uso son un riesgo de seguridad y consumen recursos del servidor.</p>

<h3>Procedimiento</h3>
<ol>
  <li><strong>NO</strong> usar solo la X del ventana de RDP (eso deja la sesión en estado <em>disconnected</em>, no la cierra).</li>
  <li>Dentro del servidor, abrir el menú <em>Inicio</em> → botón de usuario → <strong>Cerrar sesión</strong>.</li>
  <li>Alternativamente, desde PowerShell:</li>
</ol>
<pre><code>logoff</code></pre>

<div data-panel-type=""warning""><p><strong>⚠ Política de seguridad:</strong> No dejar sesiones RDP abiertas al finalizar el pase. Cualquier sesión abandonada puede ser usada por terceros si la PC del operador queda desatendida.</p></div>",
                },
            },
            reversion = new object[]
            {
                new
                {
                    order = 1,
                    title = "Detener Site IIS y Application Pools",
                    type = "action",
                    description = @"<h3>Objetivo</h3>
<p>Detener los recursos del site para permitir la restauración del backup sin bloqueos de archivos en uso.</p>

<h3>Procedimiento</h3>
<pre><code>Import-Module WebAdministration
Stop-Website -Name ""{{nombre_web}}""
Stop-WebAppPool -Name ""{{app_pools}}""</code></pre>
<p>Verificar el estado:</p>
<pre><code>Get-Website -Name ""{{nombre_web}}"" | Select-Object Name, State
Get-WebAppPoolState -Name ""{{app_pools}}""</code></pre>
<p>Ambos deben estar en <code>Stopped</code>.</p>",
                },
                new
                {
                    order = 2,
                    title = "Restaurar backup al site original",
                    type = "action",
                    description = @"<h3>Objetivo</h3>
<p>Copiar el contenido del backup (versión anterior estable) de vuelta al site en producción, sobrescribiendo los archivos de la nueva versión fallida.</p>

<h3>Procedimiento</h3>
<ol>
  <li>Identificar la carpeta de backup más reciente:</li>
</ol>
<pre><code>Get-ChildItem ""{{ruta_backup}}"" -Directory -Filter ""{{ticket_jira}}_backup_*"" |
  Sort-Object LastWriteTime -Descending |
  Select-Object -First 1 FullName</code></pre>
<ol start=""2"">
  <li>Restaurar con <code>robocopy</code> <code>/MIR</code>:</li>
</ol>
<pre><code>$latestBackup = (Get-ChildItem ""{{ruta_backup}}"" -Directory -Filter ""{{ticket_jira}}_backup_*"" | Sort-Object LastWriteTime -Descending | Select-Object -First 1).FullName
robocopy $latestBackup ""{{ruta_fisica_web}}"" /MIR /R:3 /W:5 /COPY:DAT /LOG:""D:\{{ticket_jira}}\rollback.log""</code></pre>
<ol start=""3"">
  <li>Verificar que los archivos se restauraron correctamente:</li>
</ol>
<pre><code>Get-ChildItem ""{{ruta_fisica_web}}\web.config"" | Select-Object Name, LastWriteTime</code></pre>

<div data-panel-type=""error""><p><strong>⚠ Si el backup no existe o está corrupto:</strong> ESCALAR INMEDIATAMENTE al equipo de Infraestructura. No intentar recuperación manual.</p></div>",
                },
                new
                {
                    order = 3,
                    title = "Iniciar Site IIS y Application Pools con versión anterior",
                    type = "action",
                    description = @"<h3>Objetivo</h3>
<p>Levantar el site con los archivos del backup restaurado para volver al estado estable previo al pase.</p>

<h3>Procedimiento</h3>
<pre><code>Start-WebAppPool -Name ""{{app_pools}}""
Start-Website -Name ""{{nombre_web}}""</code></pre>
<p>Verificar el estado:</p>
<pre><code>Get-Website -Name ""{{nombre_web}}"" | Select-Object Name, State
Get-WebAppPoolState -Name ""{{app_pools}}""</code></pre>
<p>Ambos deben estar en <code>Started</code>.</p>",
                },
                new
                {
                    order = 4,
                    title = "Verificar que el sitio responde con la versión anterior",
                    type = "verification",
                    description = @"<h3>Objetivo</h3>
<p>Validar que la reversión fue exitosa y el sitio está funcionando con la versión anterior.</p>

<h3>URL a verificar</h3>
<p><a href=""{{url_verificacion}}"">{{url_verificacion}}</a></p>

<h3>Checks</h3>
<ul>
  <li>✔ HTTP 200 OK</li>
  <li>✔ Carga visual correcta</li>
  <li>✔ Versión mostrada corresponde a la <strong>anterior</strong> (no la nueva fallida)</li>
  <li>✔ No hay errores en Event Viewer</li>
</ul>

<h3>Resultado esperado</h3>
<p>✅ Sitio estable con versión anterior → Reversión exitosa. Continuar con pasos 5 y 6.</p>
<p>❌ Sitio sigue sin responder → INCIDENTE CRÍTICO, escalar al paso siguiente.</p>",
                    on_fail = new
                    {
                        steps = new object[]
                        {
                            new
                            {
                                title = "Escalar como incidente CRÍTICO a Infraestructura y Guardia",
                                type = "action",
                                description = @"<h3>Situación</h3>
<p>La reversión falló: el sitio no responde ni con la versión anterior. Esto indica un problema más profundo (hardware, red, DNS, infraestructura).</p>

<h3>Acciones inmediatas</h3>
<ol>
  <li>📞 Llamar al canal de <strong>Guardia 24/7</strong> del equipo de Infraestructura.</li>
  <li>📞 Notificar al <strong>Gerente de Operaciones</strong> del área.</li>
  <li>📝 Crear incidente con prioridad <strong>P1 / Sev1</strong>.</li>
  <li>🔒 Dejar la sesión RDP abierta para que Infra pueda conectarse y diagnosticar.</li>
  <li>📋 Documentar todos los comandos ejecutados y los errores vistos.</li>
</ol>

<div data-panel-type=""warning""><p><strong>⚠ NO tocar más el servidor sin autorización de Infra.</strong> Cualquier acción adicional puede empeorar la situación o dificultar el diagnóstico.</p></div>",
                            },
                        },
                    },
                },
                new
                {
                    order = 5,
                    title = "Eliminar carpeta de trabajo del servidor",
                    type = "action",
                    description = @"<h3>Objetivo</h3>
<p>Limpiar la carpeta temporal del servidor una vez confirmada la reversión exitosa.</p>

<h3>Procedimiento</h3>
<pre><code>Remove-Item -Path ""D:\{{ticket_jira}}"" -Recurse -Force</code></pre>

<div data-panel-type=""note""><p><strong>💡 Nota:</strong> El backup en <code>{{ruta_backup}}</code> NO se elimina. Se mantiene para análisis post-mortem.</p></div>",
                },
                new
                {
                    order = 6,
                    title = "Documentar la reversión en el ticket Jira y programar post-mortem",
                    type = "action",
                    description = @"<h3>Objetivo</h3>
<p>Registrar en el ticket Jira <code>{{ticket_jira}}</code> que se ejecutó la reversión, con toda la evidencia necesaria para el análisis posterior.</p>

<h3>Información a documentar</h3>
<ul>
  <li>📌 <strong>Hora exacta</strong> del rollback (inicio y fin)</li>
  <li>📌 <strong>Motivo</strong> por el cual falló el pase (error observado)</li>
  <li>📌 <strong>Screenshots</strong> o logs del error</li>
  <li>📌 <strong>Estado actual</strong>: sitio operativo con versión anterior</li>
  <li>📌 <strong>Ruta del backup</strong> usado para la restauración</li>
</ul>

<h3>Post-mortem</h3>
<ol>
  <li>Cambiar el estado del ticket a <strong>Fallido / En análisis</strong>.</li>
  <li>Programar reunión de post-mortem en las próximas 24-48hs con:
    <ul>
      <li>TL del equipo de desarrollo</li>
      <li>Operador que ejecutó el pase</li>
      <li>Referente de Infraestructura</li>
      <li>QA si aplica</li>
    </ul>
  </li>
  <li>Identificar causa raíz y acciones preventivas.</li>
  <li>Re-planificar el pase con las correcciones necesarias.</li>
</ol>

<div data-panel-type=""note""><p><strong>💡 Nota:</strong> No volver a intentar el pase hasta que la causa raíz esté identificada y corregida en ambiente pre-productivo.</p></div>",
                },
            },
        };

        return System.Text.Json.JsonSerializer.Serialize(
            doc,
            new System.Text.Json.JsonSerializerOptions { WriteIndented = false }
        );
    }
}
