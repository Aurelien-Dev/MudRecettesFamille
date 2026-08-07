using MudBlazor;

namespace RecettesFamille.Managers
{
    public static class CustomThemeManager
    {
        // Thème pour le layout principal (Recettes)
        public static readonly MudTheme MainTheme = new()
        {
            LayoutProperties = new()
            {
                DefaultBorderRadius = "10px",
            },
            PaletteLight = new PaletteLight()
            {
                Primary = "#d18363",
                AppbarBackground = "#84533e",
            }
        };

        // Thème pour l'administration
        public static readonly MudTheme AdminTheme = new()
        {
            LayoutProperties = new()
            {
                DefaultBorderRadius = "10px",
            },
            PaletteLight = new PaletteLight()
            {
                Primary = "#90caf9",
                TextPrimary = "#1e1e1e",
                AppbarBackground = "#1e1e1e",
                AppbarText = "#ffffffdd",
                DrawerText = "#ffffffbc",
                DrawerBackground = "#2d2d2d",
            },
        };

        // Thème pour Travel Planner (YouSummarize)
        public static readonly MudTheme TravelPlannerTheme = new()
        {
            LayoutProperties = new()
            {
                DefaultBorderRadius = "8px",
            }
        };

        // Thème pour Travel Planner Admin
        public static readonly MudTheme TravelPlannerAdminTheme = new()
        {
            LayoutProperties = new()
            {
                DefaultBorderRadius = "8px",
            }
        };
    }
}