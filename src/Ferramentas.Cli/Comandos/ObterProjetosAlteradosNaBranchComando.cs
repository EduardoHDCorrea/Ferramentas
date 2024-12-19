using System.ComponentModel;
using System.Text;
using Ferramentas.Cli.Infraestrutura;
using Ferramentas.Cli.Infraestrutura.ServiçosEstáticos;
using Spectre.Console;
using Spectre.Console.Cli;
using TextCopy;

namespace Ferramentas.Cli.Comandos;

internal sealed class ObterProjetosAlteradosNaBranchComando : Command<ObterProjetosAlteradosNaBranchComando.Parâmetros>
{
    public sealed class Parâmetros : CommandSettings
    {
        [CommandArgument(0, "<DIRETÓRIO>"), Description("Caminho do diretório raíz do repositório.")]
        public required string Diretório { get; set; }

        [CommandArgument(1, "<BRANCH>"), Description("Nome da branch.")]
        public required string Branch { get; set; }

        [CommandOption("-t|--tarefa <TAREFA>"),
         Description("Identificador da tarefa. Por padrão pega o nome da branch.")]
        public string? IdentificadorDaTarefa { get; set; }
        
        [CommandOption("-o|--output <CAMINHO>"),
         Description("Diretório onde o arquivo gerado será armazenado.")]
        public string? DiretórioDeDestinoDoArquivoGerado { get; set; }
    }

    public override int Execute(CommandContext contexto, Parâmetros parâmetros)
    {
        var branch = parâmetros.Branch;
        var identificadorDaTarefa = parâmetros.IdentificadorDaTarefa ?? branch.Split("/").Last();
        var projetosAlterados = new List<string>();

        if (parâmetros.IdentificadorDaTarefa is null)
        {
            var usarIdentificadorDaBranch = AnsiConsole.Prompt(
                new TextPrompt<bool>(
                        $"Usar o identificador '#{identificadorDaTarefa}' da branch como identificador da tarefa?")
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

        AnsiConsole.Status()
            .AutoRefresh(true)
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("yellow bold"))
            .Start("Obtendo lista de projetos alterados...", context =>
            {
                projetosAlterados.AddRange(ListarProjetosAlteradosNosCommitsFiltrados(
                    context,
                    parâmetros.Diretório,
                    branch,
                    identificadorDaTarefa
                ));
            });

        var tabela = new Table();
        tabela.AddColumn("Projetos Alterados");
        tabela.AddColumn("Tipo do Projeto");

        foreach (var projeto in projetosAlterados.OrderBy(x => x.Contains("Tests")))
        {
            var projetoDeTestes = projeto.Contains("Tests", StringComparison.InvariantCultureIgnoreCase);
            tabela.AddRow(projeto, projetoDeTestes ? "[green bold]Teste[/]" : "[blue bold]Fonte[/]");
        }

        AnsiConsole.Write(tabela);

        var estadoAfetado = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .AddChoices("RS", "PR", "Ambos")
                .Title("[blue bold]Qual estado é afetado com este PR?[/]")
        );

        var novosComportamentos = AnsiConsole.Prompt(
            new TextPrompt<string>("[blue bold]Quais comportamentos foram criados, alterados ou corrigidos?[/]")
                .Validate(x => x.Length > 0
                    ? ValidationResult.Success()
                    : ValidationResult.Error("[red bold]Informe ao menos um comportamento.[/]"))
        );

        string? configuraçãoParaHabilitar = null;
        var existeConfiguraçãoParaHabilitar = AnsiConsole.Prompt(
            new TextPrompt<bool>("Existe alguma configuração para habilitar a opção? ")
                .AddChoice(false)
                .AddChoice(true)
                .DefaultValue(false)
                .WithConverter(x => x ? "Sim" : "Não")
        );

        if (existeConfiguraçãoParaHabilitar)
            configuraçãoParaHabilitar = AnsiConsole.Prompt(
                new TextPrompt<string>("Informe a configuração que deve ser habilitada:")
            );

        var endpointAfetado = AnsiConsole.Prompt(
            new TextPrompt<string>("[blue bold]Qual endpoint é afetado com este PR?[/]")
                .DefaultValue("Nenhum.")
        );

        var testesAutomatizadosComSucesso = AnsiConsole.Prompt(
            new TextPrompt<bool>("[blue bold]Os testes automatizados foram executados com sucesso?[/]")
                .AddChoices([true, false])
                .DefaultValue(true)
                .WithConverter(x => x ? "Sim" : "Não")
        );

        string? outroProcedimentoDeTestes = null;
        var houveOutroProcedimentoDeTestes = AnsiConsole.Prompt(
            new TextPrompt<bool>("[blue bold]Houve outro procedimento de testes?[/]")
                .AddChoices([true, false])
                .DefaultValue(false)
                .WithConverter(x => x ? "Sim" : "Não")
        );

        if (houveOutroProcedimentoDeTestes)
            outroProcedimentoDeTestes = AnsiConsole.Prompt(
                new TextPrompt<string>("[blue bold]Informe o procedimento de testes realizado:[/]")
            );

        string? dependênciaDeOutroPr = null;
        var existeDependênciaDeOutroPr = AnsiConsole.Prompt(
            new TextPrompt<bool>("[blue bold]Este PR depende de outro?[/]")
                .AddChoices([true, false])
                .DefaultValue(false)
                .WithConverter(x => x ? "Sim" : "Não")
        );

        if (existeDependênciaDeOutroPr)
            dependênciaDeOutroPr = AnsiConsole.Prompt(
                new TextPrompt<string>("[blue bold]Informe o identificador do PR que este PR depende:[/]")
            );

        string? pacoteParaGerar = null;
        var precisaGerarPacote = AnsiConsole.Prompt(
            new SelectionPrompt<bool>()
                .AddChoices(true, false)
                .Title("[blue bold]Precisa gerar pacote?[/]")
        );

        if (precisaGerarPacote)
            pacoteParaGerar = AnsiConsole.Prompt(
                new TextPrompt<string>("[blue bold]Informe o nome do pacote que deve ser gerado:[/]")
            );

        var caminhoDoArquivoTemplate = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "pull-request-template.md"
        );

        var projetosAlteradosEmListaMarkdown = new StringBuilder();
        foreach (var projeto in projetosAlterados)
            projetosAlteradosEmListaMarkdown.AppendLine($"- `{projeto}`");

        var resultadoTestesAutomatizadosTexto = testesAutomatizadosComSucesso
            ? "sucesso"
            : "falha";
        var procedimentosDeTesteEmListaMarkdown = new StringBuilder();
        procedimentosDeTesteEmListaMarkdown.AppendLine(
            $"- Testes automatizados executados com {resultadoTestesAutomatizadosTexto}."
        );

        if (houveOutroProcedimentoDeTestes)
            procedimentosDeTesteEmListaMarkdown.AppendLine($"- {outroProcedimentoDeTestes}");

        Variáveis.DefinirVariável("EstadoAfetado", estadoAfetado);
        Variáveis.DefinirVariável("Comportamentos", novosComportamentos);
        Variáveis.DefinirVariável("ConfiguracaoParaHabilitarOpcao", configuraçãoParaHabilitar ?? "Nenhuma.");
        Variáveis.DefinirVariável("Endpoint", endpointAfetado);
        Variáveis.DefinirVariável("TestesAutomatizadosComSucesso", testesAutomatizadosComSucesso ? "Sim" : "Não");
        Variáveis.DefinirVariável("ProcedimentoDeTestes", procedimentosDeTesteEmListaMarkdown.ToString());
        Variáveis.DefinirVariável("DependeDeOutroPR", dependênciaDeOutroPr ?? "Não.");
        Variáveis.DefinirVariável("GerarNovaVersaoPacote", pacoteParaGerar ?? "Não.");
        Variáveis.DefinirVariável("Projetos", projetosAlteradosEmListaMarkdown.ToString());

        var conteúdoDoArquivoTemplate = File
            .ReadAllText(caminhoDoArquivoTemplate)
            .SubstituirVariáveisNoTexto();

        var caminhoDoArquivoGerado = Path.Combine(
            parâmetros.DiretórioDeDestinoDoArquivoGerado ?? ".",
            $"pull-request-{identificadorDaTarefa}.md"
        );

        File.WriteAllText(
            caminhoDoArquivoGerado,
            conteúdoDoArquivoTemplate
        );

        var painel = new Panel(new TextPath(caminhoDoArquivoGerado))
            .Header("[bold]Caminho do Arquivo Gerado[/]")
            .Border(BoxBorder.Rounded)
            .HeaderAlignment(Justify.Center);

        AnsiConsole.Write(painel);

        var copiarConteúdoGerado = AnsiConsole.Prompt(
            new SelectionPrompt<bool>()
                .Title("Copiar o conteúdo gerado?")
                .AddChoices(true, false)
        );

        if (copiarConteúdoGerado)
            ClipboardService.SetText(conteúdoDoArquivoTemplate);

        AnsiConsole.Markup("🎉 [bold green]Conteúdo copiado para a área de transferência.[/]");
        AnsiConsole.Markup("[dim]Esperando 3 segundos para fechar...[/]");
        Thread.Sleep(3000);
        return 0;
    }

    private static List<string> ListarProjetosAlteradosNosCommitsFiltrados(
        StatusContext context,
        string diretórioDoRepositório,
        string branch,
        string identificadorDaTarefa
    )
    {
        context.Status("[yellow bold]Obtendo informações dos commits da branch...[/]");
        var commits = ObterInformaçõesDosCommitsDaBranch(branch, diretórioDoRepositório);
        var pattern = "#" + identificadorDaTarefa;

        AnsiConsole.MarkupLine("✓ [green bold]Obtendo informações dos commits da branch...[/]");

        context.Status("[yellow bold]Filtrando commits por identificador da tarefa...[/]");
        var commitsFiltrados = commits
            .Where(c => c.Mensagem.StartsWith(pattern))
            .ToList();

        AnsiConsole.MarkupLine("✓ [green bold]Filtrando commits por identificador da tarefa...[/]");

        context.Status("[yellow bold]Organizando projetos alterados nos commits filtrados...[/]");
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
                catch
                {
                    // ignorada
                }
        }

        AnsiConsole.MarkupLine("✓ [green bold]Organizando projetos alterados nos commits filtrados...[/]");
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