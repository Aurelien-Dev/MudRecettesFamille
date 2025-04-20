using MudBlazor;

namespace RecettesFamille.Managers
{
    public static class CustomThemeManager
    {
        public static readonly MudTheme Customtheme = new()
        {
            LayoutProperties = new()
            {
                DefaultBorderRadius = "10px",
               
            },
            PaletteLight = new()
            {
                Primary = "#d18363",//Colors.LightBlue.Darken2,
                AppbarBackground = "#84533e", //Colors.LightBlue.Darken2,
            },
            PaletteDark = new()
            {
                TextPrimary = "#ffffffdd",
                AppbarText = "#ffffffdd",
                DrawerText = "#ffffffbc",
            },
        };
    }
}