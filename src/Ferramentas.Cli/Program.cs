using Ferramentas.Cli.Comandos;
using Spectre.Console.Cli;

namespace Ferramentas.Cli;

internal static class Program
{
    public static void Main(string[] args)
    {
        args =
        [
            "obter-projetos-alterados-na-branch",
            "C:/Sky/TerraMedia/core", "tarefa/15716"
        ];

        var app = new CommandApp();
        app.Configure(
            x =>
            {
                x.AddCommand<ObterProjetosAlteradosNaBranchComando>("obter-projetos-alterados-na-branch")
                    .WithDescription("Lista os diretórios de projetos alterados em uma branch.")
                    .WithAlias("opab");
            });
        app.Run(args);
    }
}