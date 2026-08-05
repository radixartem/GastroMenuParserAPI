namespace GastroLeinefeldeMenuParser;

public class AppSettings
{
    public string Url { get; set; } = "https://essen-auf-raedern-eichsfeld.de/tagesangebot";
    public string ExportPath { get; set; } = "./exports";
    public bool EnableExport { get; set; } = true;
    public string? CategoryFilter { get; set; }
}