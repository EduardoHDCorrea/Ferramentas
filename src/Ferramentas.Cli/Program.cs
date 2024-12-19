using Ferramentas.Cli.Comandos;
using Spectre.Console.Cli;

namespace Ferramentas.Cli;

internal static class Program
{
    public static void Main(string[] args)
    {
        var app = new CommandApp();
        app.Configure(
            x =>
            {
                x.AddCommand<ResumirPrComando>("resumir-pr")
                    .WithDescription("Gera um markdown da descrição do PR.")
                    .WithAlias("rp");
            });
        app.Run(args);
    }
}