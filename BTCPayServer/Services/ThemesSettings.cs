using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace BTCPayServer.Services;

public enum ThemeExtension
{
    [Display(Name = "Does not extend a BTCPay Server theme, fully custom")]
    Custom,
    [Display(Name = "Extends the BTCPay Server Light theme")]
    Light,
    [Display(Name = "Extends the BTCPay Server Dark theme")]
    Dark
}

public class ThemeSettings
{
    [Display(Name = "Use custom theme")]
    public bool CustomTheme { get; set; }

    [Display(Name = "Custom Theme Extension Type")]
    public ThemeExtension CustomThemeExtension { get; set; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Populate)]
    [MaxLength(500)]
    [Display(Name = "Custom Theme CSS URL")]
    public string CustomThemeCssUri { get; set; }

    public string CustomThemeFileId { get; set; }

    public string LogoFileId { get; set; }

    public bool FirstRun { get; set; } = true;

    public override string ToString()
    {
        // no logs
        return string.Empty;
    }

    public string CssUri
    {
        get => CustomTheme ? CustomThemeCssUri : "/main/themes/default.css";
    }
}
