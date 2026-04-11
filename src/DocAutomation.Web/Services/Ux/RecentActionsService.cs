using Microsoft.JSInterop;

namespace DocAutomation.Web.Services.Ux;

public enum RecentActionSeverity
{
    Info,
    Success,
    Warning,
    Error,
}

public record RecentAction(
    Guid Id,
    string Title,
    string? Detail,
    RecentActionSeverity Severity,
    DateTime Timestamp,
    string? Href
);

/// <summary>
/// Log persistente de acciones recientes visible desde el bell icon del header.
/// Persiste en localStorage vía JS interop — sobrevive refreshes pero no se comparte entre usuarios.
/// </summary>
public class RecentActionsService(IJSRuntime js)
{
    private const int MaxItems = 50;
    private List<RecentAction> _actions = new();
    private bool _loaded;

    public IReadOnlyList<RecentAction> Actions => _actions;

    public event Action? ActionsChanged;

    public async Task InitializeAsync()
    {
        if (_loaded)
            return;
        try
        {
            var raw = await js.InvokeAsync<RecentAction[]>("uxPolish.loadRecentActions");
            _actions = raw?.OrderByDescending(a => a.Timestamp).Take(MaxItems).ToList() ?? new();
            _loaded = true;
            ActionsChanged?.Invoke();
        }
        catch (JSDisconnectedException)
        {
            // Circuit cerrado durante pre-render
        }
        catch
        {
            _actions = new();
            _loaded = true;
        }
    }

    public async Task LogAsync(
        string title,
        string? detail = null,
        RecentActionSeverity severity = RecentActionSeverity.Info,
        string? href = null
    )
    {
        var action = new RecentAction(
            Guid.NewGuid(),
            title,
            detail,
            severity,
            DateTime.UtcNow,
            href
        );

        _actions.Insert(0, action);
        if (_actions.Count > MaxItems)
            _actions = _actions.Take(MaxItems).ToList();

        await PersistAsync();
        ActionsChanged?.Invoke();
    }

    public async Task ClearAsync()
    {
        _actions.Clear();
        try
        {
            await js.InvokeVoidAsync("uxPolish.clearRecentActions");
        }
        catch (JSDisconnectedException) { }
        ActionsChanged?.Invoke();
    }

    private async Task PersistAsync()
    {
        try
        {
            await js.InvokeVoidAsync("uxPolish.saveRecentActions", _actions);
        }
        catch (JSDisconnectedException) { }
    }
}
