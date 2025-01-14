namespace Ferramentas.Domínio;

public static class Diretórios
{
    public static readonly string DiretórioBase = AppDomain.CurrentDomain.BaseDirectory;

    public static readonly string DiretórioDeTemplates = Path.Combine(
        DiretórioBase,
        "Templates"
    );

    public static string DiretórioDeExecução => Directory.GetCurrentDirectory();
}