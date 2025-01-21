using MudBlazor;

namespace RecettesFamille.Managers
{
    public static class CustomThemeManager
    {
        public static readonly MudTheme Customtheme = new()
        {
            PaletteLight = new PaletteLight()
            {

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