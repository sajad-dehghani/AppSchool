using MudBlazor;

namespace NovinApp.Client.Service
{
    public static class AppTheme
    {
        public static MudTheme CreateTheme()
        {
            var persianFonts = new[] { "IRANSansfanum", "IRANSans", "Vazir", "dana", "Tahoma", "sans-serif" };

            var theme = new MudTheme()
            {
                PaletteLight = new PaletteLight()
                {
                    Primary = "#2563eb",
                    PrimaryDarken = "#1d4ed8",
                    PrimaryLighten = "#60a5fa",
                    PrimaryContrastText = "#ffffff",

                    Secondary = "#7c3aed",
                    SecondaryDarken = "#6d28d9",
                    SecondaryLighten = "#a78bfa",
                    SecondaryContrastText = "#ffffff",

                    Tertiary = "#0d9488",
                    TertiaryContrastText = "#ffffff",

                    Info = "#0284c7",
                    InfoContrastText = "#ffffff",

                    Success = "#10b981",
                    SuccessContrastText = "#ffffff",

                    Warning = "#f59e0b",
                    WarningContrastText = "#ffffff",

                    Error = "#ef4444",
                    ErrorContrastText = "#ffffff",

                    Background = "#f8fafc",
                    BackgroundGray = "#f1f5f9",
                    Surface = "#ffffff",

                    AppbarBackground = "#1e3a8a",
                    AppbarText = "#ffffff",

                    DrawerBackground = "#ffffff",
                    DrawerText = "#1e293b",
                    DrawerIcon = "#475569",

                    TextPrimary = "#0f172a",
                    TextSecondary = "#475569",
                    TextDisabled = "#94a3b8",

                    ActionDefault = "#475569",
                    ActionDisabled = "#cbd5e1",

                    LinesDefault = "#e2e8f0",
                    TableLines = "#f1f5f9",
                    Divider = "#e2e8f0",
                    DividerLight = "#f1f5f9"
                },
                PaletteDark = new PaletteDark()
                {
                    Primary = "#3b82f6",
                    PrimaryDarken = "#2563eb",
                    PrimaryLighten = "#93c5fd",
                    PrimaryContrastText = "#ffffff",

                    Secondary = "#a78bfa",
                    SecondaryDarken = "#8b5cf6",
                    SecondaryLighten = "#c4b5fd",
                    SecondaryContrastText = "#ffffff",

                    Tertiary = "#2dd4bf",
                    TertiaryContrastText = "#ffffff",

                    Info = "#38bdf8",
                    InfoContrastText = "#ffffff",

                    Success = "#34d399",
                    SuccessContrastText = "#ffffff",

                    Warning = "#fbbf24",
                    WarningContrastText = "#ffffff",

                    Error = "#f87171",
                    ErrorContrastText = "#ffffff",

                    Background = "#0f172a",
                    BackgroundGray = "#1e293b",
                    Surface = "#1e293b",

                    AppbarBackground = "#1e293b",
                    AppbarText = "#f8fafc",

                    DrawerBackground = "#0f172a",
                    DrawerText = "#f1f5f9",
                    DrawerIcon = "#94a3b8",

                    TextPrimary = "#f8fafc",
                    TextSecondary = "#94a3b8",
                    TextDisabled = "#64748b",

                    ActionDefault = "#94a3b8",
                    ActionDisabled = "#475569",

                    LinesDefault = "#334155",
                    TableLines = "#1e293b",
                    Divider = "#334155",
                    DividerLight = "#1e293b"
                },
                LayoutProperties = new LayoutProperties()
                {
                    DefaultBorderRadius = "12px",
                    AppbarHeight = "64px"
                }
            };

            // Set typography properties with string values
            theme.Typography.Default.FontFamily = persianFonts;
            theme.Typography.Default.LineHeight = "1.7";

            theme.Typography.H1.FontFamily = persianFonts;
            theme.Typography.H1.LineHeight = "1.3";

            theme.Typography.H2.FontFamily = persianFonts;
            theme.Typography.H2.LineHeight = "1.35";

            theme.Typography.H3.FontFamily = persianFonts;
            theme.Typography.H3.LineHeight = "1.4";

            theme.Typography.H4.FontFamily = persianFonts;
            theme.Typography.H4.LineHeight = "1.4";

            theme.Typography.H5.FontFamily = persianFonts;
            theme.Typography.H5.LineHeight = "1.5";

            theme.Typography.H6.FontFamily = persianFonts;
            theme.Typography.H6.LineHeight = "1.5";

            theme.Typography.Subtitle1.FontFamily = persianFonts;
            theme.Typography.Subtitle1.LineHeight = "1.6";

            theme.Typography.Subtitle2.FontFamily = persianFonts;
            theme.Typography.Subtitle2.LineHeight = "1.6";

            theme.Typography.Body1.FontFamily = persianFonts;
            theme.Typography.Body1.LineHeight = "1.7";

            theme.Typography.Body2.FontFamily = persianFonts;
            theme.Typography.Body2.LineHeight = "1.65";

            theme.Typography.Button.FontFamily = persianFonts;
            theme.Typography.Button.LineHeight = "1.75";

            theme.Typography.Caption.FontFamily = persianFonts;
            theme.Typography.Caption.LineHeight = "1.5";

            theme.Typography.Overline.FontFamily = persianFonts;
            theme.Typography.Overline.LineHeight = "1.5";

            return theme;
        }
    }
}
