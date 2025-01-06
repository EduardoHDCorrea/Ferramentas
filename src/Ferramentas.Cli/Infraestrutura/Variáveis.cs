namespace Ferramentas.Cli.Infraestrutura;

public static class Variáveis
{
    public static readonly Dictionary<string, string> VariáveisDefinidas = new();

    public static void DefinirVariável(string nomeDaVariável, string valorDaVariável)
    {
        if (!VariáveisDefinidas.TryAdd(nomeDaVariável, valorDaVariável))
            VariáveisDefinidas[nomeDaVariável] = valorDaVariável;
    }

    public static string ObterVariável(string nomeDaVariável) =>
        VariáveisDefinidas[nomeDaVariável];
}