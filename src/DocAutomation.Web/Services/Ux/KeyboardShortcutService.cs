namespace DocAutomation.Web.Services.Ux;

/// <summary>
/// Event bus para atajos de teclado globales. El MainLayout captura el evento desde JS
/// y dispara <see cref="TriggerAsync"/>. Las páginas se suscriben a <see cref="ShortcutTriggered"/>
/// en OnInitialized y se desuscriben en Dispose.
/// </summary>
public class KeyboardShortcutService
{
    public event Func<string, Task>? ShortcutTriggered;

    public async Task TriggerAsync(string key)
    {
        if (ShortcutTriggered is null)
            return;

        var invocations = ShortcutTriggered
            .GetInvocationList()
            .Cast<Func<string, Task>>()
            .Select(handler => handler(key));

        await Task.WhenAll(invocations);
    }
}
