using Microsoft.JSInterop;

namespace DocAutomation.Web.Services.Ux;

public class ThemeService(IJSRuntime js)
{
    public const string LightTheme = "standard-base";
    public const string DarkTheme = "standard-dark-base";

    public string CurrentTheme { get; private set; } = LightTheme;
    public bool IsDark => CurrentTheme.Contains("dark", StringComparison.OrdinalIgnoreCase);

    public event Action? ThemeChanged;

    public async Task InitializeAsync()
    {
        try
        {
            CurrentTheme = await js.InvokeAsync<string>("uxPolish.initTheme");
            ThemeChanged?.Invoke();
        }
        catch (JSDisconnectedException)
        {
            // El circuito se cerró durante pre-render, ignorar
        }
    }

    public async Task ToggleAsync()
    {
        CurrentTheme = IsDark ? LightTheme : DarkTheme;
        await js.InvokeVoidAsync("uxPolish.setTheme", CurrentTheme);
        ThemeChanged?.Invoke();
    }
}
