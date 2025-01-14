namespace Ferramentas.Domínio;

public static class Diretórios
{
    public static readonly string DiretórioBase = AppDomain.CurrentDomain.BaseDirectory;

    public static readonly string DiretórioDeTemplates = Path.Combine(
        DiretórioBase,
        "Templates"
    );

    public static DirectoryInfo DiretórioDeConfiguração => Directory.CreateDirectory(
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            ".ferramentasCli"
        )
    );

    public static string DiretórioDeExecução => Directory.GetCurrentDirectory();
}