using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using Radzen;

namespace DocAutomation.Web.Services.Ux;

/// <summary>
/// Intercepta navegaciones internas y el cierre del browser cuando hay cambios sin guardar.
/// Las páginas llaman <see cref="MarkDirtyAsync"/> al editar y <see cref="MarkCleanAsync"/> al guardar.
/// </summary>
public sealed class UnsavedChangesGuard : IDisposable
{
    private readonly NavigationManager _nav;
    private readonly IJSRuntime _js;
    private readonly DialogService _dialog;
    private readonly IDisposable? _handler;

    public bool IsDirty { get; private set; }
    public string Message { get; set; } = "Tenés cambios sin guardar. ¿Seguro que querés salir?";

    public UnsavedChangesGuard(NavigationManager nav, IJSRuntime js, DialogService dialog)
    {
        _nav = nav;
        _js = js;
        _dialog = dialog;
        _handler = _nav.RegisterLocationChangingHandler(OnLocationChangingAsync);
    }

    public async Task MarkDirtyAsync()
    {
        if (IsDirty)
            return;
        IsDirty = true;
        await SyncJsAsync();
    }

    public async Task MarkCleanAsync()
    {
        if (!IsDirty)
            return;
        IsDirty = false;
        await SyncJsAsync();
    }

    private async ValueTask SyncJsAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("uxPolish.setUnsavedChanges", IsDirty);
        }
        catch (JSDisconnectedException)
        {
            // Circuit cerrado durante pre-render, ignorar
        }
    }

    private async ValueTask OnLocationChangingAsync(LocationChangingContext context)
    {
        if (!IsDirty)
            return;

        var confirmed = await _dialog.Confirm(
            Message,
            "Cambios sin guardar",
            new ConfirmOptions { OkButtonText = "Salir sin guardar", CancelButtonText = "Quedarse" }
        );

        if (confirmed != true)
        {
            context.PreventNavigation();
        }
        else
        {
            // El usuario eligió salir → limpiamos el flag para que el próximo intento no moleste
            IsDirty = false;
            await SyncJsAsync();
        }
    }

    public void Dispose() => _handler?.Dispose();
}
