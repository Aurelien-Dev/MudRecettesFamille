using MudBlazor;

namespace RecettesFamille.Managers
{
    public static class CustomThemeManager
    {
        public static readonly MudTheme Customtheme = new()
        {
            PaletteLight = new PaletteLight()
            {
                Primary = Colors.LightBlue.Darken2,
                AppbarBackground = "#0F3F59",
            },
            PaletteDark = new PaletteDark()
            {
                TextPrimary = "#ffffffdd",
                AppbarText = "#ffffffdd",
                DrawerText = "#ffffffbc",
            },
        };
    }
}