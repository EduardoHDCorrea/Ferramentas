using System.ComponentModel;
using Ferramentas.Cli.Infraestrutura;
using Ferramentas.Cli.Infraestrutura.ServiçosEstáticos;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Ferramentas.Cli.Comandos;

internal sealed class ObterProjetosAlteradosNaBranchComando : Command<ObterProjetosAlteradosNaBranchComando.Parâmetros>
{
    public sealed class Parâmetros : CommandSettings
    {
        [CommandArgument(0, "<DIRETÓRIO>"), Description("Caminho do diretório raíz do repositório.")]
        public required string Diretório { get; set; }

        [CommandArgument(1, "<BRANCH>"), Description("Nome da branch.")]
        public required string Branch { get; set; }

        [CommandOption("-t|--tarefa <TAREFA>"), Description("Identificador da tarefa. Por padrão pega o nome da branch.")]
        public string? IdentificadorDaTarefa { get; set; }
    }

    public override int Execute(CommandContext contexto, Parâmetros parâmetros)
    {
        var branch = parâmetros.Branch;
        var identificadorDaTarefa = parâmetros.IdentificadorDaTarefa ?? branch.Split("/").Last();
        var diretóriosAlterados = new List<string>();

        if (parâmetros.IdentificadorDaTarefa is null)
        {
            var usarIdentificadorDaBranch = AnsiConsole.Prompt(
                new TextPrompt<bool>($"Usar o identificador '#{identificadorDaTarefa}' da branch como identificador da tarefa?")
                    .AddChoice(true)
                    .AddChoice(false)
                    .DefaultValue(true)
                    .WithConverter(x => x ? "Sim" : "Não")
            );

            if (!usarIdentificadorDaBranch)
                identificadorDaTarefa = AnsiConsole.Prompt(
                    new TextPrompt<string>($"Informe o identificador da tarefa desejado: (sem '#')")
                        .DefaultValue(identificadorDaTarefa)
                );
        }

        AnsiConsole.Status().Start("Buscando projetos alterados...", statusContext =>
        {
            statusContext.Spinner(Spinner.Known.Default);
            statusContext.SpinnerStyle(Style.Parse("yellow"));

            diretóriosAlterados.AddRange(ListarProjetosAlteradosNosCommitsFiltrados(
                parâmetros.Diretório,
                branch,
                identificadorDaTarefa
            ));
        });

        AnsiConsole.MarkupLine($"[bold]Projetos alterados na branch '{branch}' com a tarefa '#{identificadorDaTarefa}':[/]");

        var listaVisual = diretóriosAlterados
            .ConvertAll(x => new Text(x));

        AnsiConsole.Write(new Rows(listaVisual));



        return 0;
    }

    private static List<string> ListarProjetosAlteradosNosCommitsFiltrados(
        string diretórioDoRepositório,
        string branch,
        string identificadorDaTarefa
    )
    {
        var commits = ObterInformaçõesDosCommitsDaBranch(branch, diretórioDoRepositório);
        var pattern = "#" + identificadorDaTarefa;
        var commitsFiltrados = commits
            .Where(c => c.Mensagem.StartsWith(pattern))
            .ToList();

        var projetosAlterados = new HashSet<string>();
        foreach (var commit in commitsFiltrados)
        {
            var arquivos = ObterArquivosAlteradosDoCommit(commit.Hash, diretórioDoRepositório);
            foreach (var arquivo in arquivos)
                try
                {
                    var projetoDoArquivo = ObterProjetoDoArquivo(arquivo, diretórioDoRepositório);
                    if (projetoDoArquivo is null)
                        continue;

                    projetosAlterados.Add(Path.GetFileNameWithoutExtension(projetoDoArquivo));
                }
                catch (Exception exception)
                {
                    AnsiConsole.Markup($"[red]{exception.Message}[/]");
                }
        }

        return projetosAlterados.ToList();
    }

    private static List<InformaçõesDoCommit> ObterInformaçõesDosCommitsDaBranch(
        string branch,
        string diretórioDoRepositório
    )
    {
        var comandoDeLogDoGit = $"git log {branch} --format=%H::%s";
        var retornoDoComando = comandoDeLogDoGit.ExecutarComandoComRetorno(diretórioDoRepositório);

        var commits = new List<InformaçõesDoCommit>();
        foreach (var linha in retornoDoComando)
        {
            var partes = linha.Split(["::"], 2, StringSplitOptions.None);
            if (partes.Length == 2)
                commits.Add(new InformaçõesDoCommit { Hash = partes[0], Mensagem = partes[1] });
        }

        return commits;
    }

    private static List<string> ObterArquivosAlteradosDoCommit(string hashDoCommit, string diretórioDoRepositório)
    {
        var comandoGitShow = $"git -c core.quotepath=false show --pretty=\"\" --name-only {hashDoCommit}";
        var retorno = comandoGitShow.ExecutarComandoComRetorno(diretórioDoRepositório);
        return retorno;
    }

    private static string? ObterProjetoDoArquivo(string caminhoDoArquivo, string caminhoDoRepositório)
    {
        var diretórioDoArquivo = Path.GetDirectoryName(caminhoDoArquivo);
        if (diretórioDoArquivo is null)
            return null;

        diretórioDoArquivo = diretórioDoArquivo.Trim('"');
        var diretório = new DirectoryInfo(Path.GetFullPath(diretórioDoArquivo, caminhoDoRepositório));
        while (diretório is not null)
        {
            var arquivosCsproj = Directory.GetFiles(
                diretório.FullName,
                "*.csproj",
                SearchOption.TopDirectoryOnly
            );

            if (arquivosCsproj.Length > 0)
                return arquivosCsproj[0];

            diretório = diretório.Parent;
        }

        return null;
    }

    private class InformaçõesDoCommit
    {
        public string Hash { get; set; } = string.Empty;
        public string Mensagem { get; set; } = string.Empty;
    }
}