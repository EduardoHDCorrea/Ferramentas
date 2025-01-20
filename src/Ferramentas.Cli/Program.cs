using Ferramentas.Cli.Domínio.ObterCaminhoRelativo;
using Ferramentas.Cli.Domínio.ResumirPr;
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
                    .WithExample(
                        "resumir-pr",
                        @"D:\Temp\core",
                        "tarefa/15860",
                        "-t",
                        "15970;16036;16202;16052",
                        "-o",
                        @"C:\Temp\Output"
                    )
                    .WithAlias("rp");
                x.AddCommand<ObterCaminhoRelativoComando>("caminho-relativo")
                    .WithDescription("Obtém o caminho relativo entre dois diretórios.")
                    .WithExample("caminho-relativo", @"D:\Temp\core", @"D:\Temp\Output")
                    .WithAlias("cr");
            }
        );

        app.Run(args);
    }
}