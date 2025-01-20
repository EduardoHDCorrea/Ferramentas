using System.ComponentModel;
using System.Text;
using Ferramentas.Cli.Domínio.ResumirPr.Dtos;
using Ferramentas.Cli.Infraestrutura;
using Ferramentas.Cli.Infraestrutura.ServiçosEstáticos;
using Spectre.Console;
using Spectre.Console.Cli;
using TextCopy;

namespace Ferramentas.Cli.Domínio.ResumirPr;

internal sealed class ResumirPrComando : Command<ResumirPrComando.Parâmetros>
{
    public override int Execute(CommandContext contexto, Parâmetros parâmetros)
    {
        var branch = parâmetros.Branch;
        var identificadores = parâmetros.IdentificadorDaTarefa?.Split(";") ?? [branch.Split("/").Last()];
        var projetosAlterados = new List<ProjetoAlterado>();

        if (parâmetros.IdentificadorDaTarefa is null)
        {
            var usarIdentificadorDaBranch = AnsiConsole.Prompt(
                new TextPrompt<bool>(
                        $"Usar o identificador '#{identificadores.Single()}' da branch como identificador da tarefa?"
                    )
                    .AddChoice(true)
                    .AddChoice(false)
                    .DefaultValue(true)
                    .WithConverter(x => x ? "Sim" : "Não")
            );

            if (!usarIdentificadorDaBranch)
                identificadores =
                [
                    AnsiConsole.Prompt(
                        new TextPrompt<string>("Informe o identificador da tarefa desejado: (sem '#')")
                            .DefaultValue(branch.Split("/").Last())
                    )
                ];
        }

        AnsiConsole.Status()
            .AutoRefresh(true)
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("yellow bold"))
            .Start(
                "Obtendo lista de projetos alterados...",
                context =>
                {
                    projetosAlterados.AddRange(
                        ListarProjetosAlteradosNosCommitsFiltrados(
                            context,
                            Path.GetFullPath(parâmetros.Diretório),
                            branch,
                            identificadores
                        )
                    );
                }
            );

        var tabela = new Table();
        tabela.AddColumn("Projetos Alterados");
        tabela.AddColumn("Tipo do Projeto");
        tabela.AddColumn("Quantidade de Alterações");

        foreach (var projeto in projetosAlterados)
        {
            var tipoDoProjetoEmString = projeto.TipoDoProjeto switch
            {
                TipoDoProjeto.Testes => "[green bold]Teste[/]",
                TipoDoProjeto.Pacote => "[yellow bold]Pacote[/]",
                _                    => "[blue bold]Fonte[/]"
            };

            tabela.AddRow(projeto.NomeDoProjeto, tipoDoProjetoEmString, projeto.QuantidadeDeAlterações.ToString());
        }

        AnsiConsole.Write(tabela);

        var estadoAfetado = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .AddChoices("RS", "PR", "Ambos")
                .Title("[blue bold]Qual estado é afetado com este PR?[/]")
        );

        var comportamentosAlterados = new HashSet<string>();
        var novosComportamentos = new StringBuilder();
        var comportamentosAlteradosPrompt = AnsiConsole.Prompt(
            new TextPrompt<string>(
                    "[blue bold]Quais comportamentos foram criados, alterados ou corrigidos?[/] [dim]Separar por ponto e vírgula (;) se forem múltiplos.[/]"
                )
                .Validate(
                    x => x.Length > 0
                        ? ValidationResult.Success()
                        : ValidationResult.Error("[red bold]Informe ao menos um comportamento.[/]")
                )
        );

        if (!string.IsNullOrWhiteSpace(comportamentosAlteradosPrompt))
        {
            comportamentosAlterados.UnionWith(comportamentosAlteradosPrompt.Split(";"));
            foreach (var comportamento in comportamentosAlterados)
            {
                var finalDoComportamento = comportamento.EndsWith('.') ? string.Empty : ".";
                novosComportamentos.AppendLine($"- {comportamento}{finalDoComportamento}");
            }
        }

        string? configuraçãoParaHabilitar = null;
        var existeConfiguraçãoParaHabilitar = AnsiConsole.Prompt(
            new SelectionPrompt<bool>()
                .Title("[blue bold]Existe alguma configuração para habilitar a opção?[/]")
                .AddChoices(false, true)
                .UseConverter(x => x ? "Sim" : "Não")
        );

        if (existeConfiguraçãoParaHabilitar)
            configuraçãoParaHabilitar = AnsiConsole.Prompt(
                new TextPrompt<string>("[blue bold]Informe a configuração que deve ser habilitada:[/]")
            );

        var endpointsAfetadosLista = new HashSet<string>();
        var endpointsAfetadosPrompt = AnsiConsole.Prompt(
            new TextPrompt<string>(
                    "[blue bold]Quais endpoints são afetados com este PR?[/] [dim]Separar por ponto e vírgula (;) se forem múltiplos.[/]"
                )
                .AllowEmpty()
        );

        if (!string.IsNullOrWhiteSpace(endpointsAfetadosPrompt))
            endpointsAfetadosLista.UnionWith(endpointsAfetadosPrompt.Split(";"));

        var endpointsAfetados = new StringBuilder();
        foreach (var endpoint in endpointsAfetadosLista)
            endpointsAfetados.AppendLine($"- `/{endpoint}`");

        if (endpointsAfetadosLista.Count == 0)
            endpointsAfetados.AppendLine("Nenhum.");

        var testesAutomatizadosComSucesso = AnsiConsole.Prompt(
            new SelectionPrompt<bool>()
                .Title("[blue bold]Os testes automatizados foram executados com sucesso?[/]")
                .AddChoices(true, false)
                .UseConverter(x => x ? "Sim" : "Não")
        );

        string? outroProcedimentoDeTestes = null;
        var houveOutroProcedimentoDeTestes = AnsiConsole.Prompt(
            new SelectionPrompt<bool>()
                .Title("[blue bold]Houve outro procedimento de testes?[/]")
                .AddChoices(false, true)
                .UseConverter(x => x ? "Sim" : "Não")
        );

        if (houveOutroProcedimentoDeTestes)
            outroProcedimentoDeTestes = AnsiConsole.Prompt(
                new TextPrompt<string>("[blue bold]Informe o procedimento de testes realizado:[/]")
            );

        string? dependênciaDeOutroPr = null;
        var existeDependênciaDeOutroPr = AnsiConsole.Prompt(
            new SelectionPrompt<bool>()
                .Title("[blue bold]Este PR depende de outro?[/]")
                .AddChoices(false, true)
                .UseConverter(x => x ? "Sim" : "Não")
        );

        if (existeDependênciaDeOutroPr)
            dependênciaDeOutroPr = AnsiConsole.Prompt(
                new TextPrompt<string>("[blue bold]Informe o identificador do PR que este PR depende:[/]")
            );

        StringBuilder? pacotesAlterados = null;
        if (projetosAlterados.Exists(x => x.TipoDoProjeto is TipoDoProjeto.Pacote))
        {
            pacotesAlterados = new StringBuilder();
            foreach (var projetosAlterado in projetosAlterados.Where(x => x.TipoDoProjeto is TipoDoProjeto.Pacote))
                pacotesAlterados.AppendLine($"- `{projetosAlterado.NomeDoProjeto}`");
        }

        var caminhoDoArquivoTemplate = Path.Combine(
            AppDomain.CurrentDomain.BaseDirectory,
            "pull-request-template.md"
        );

        var listaDeProjetos = projetosAlterados.ConvertAll(
            x => new
            {
                Projeto = x.NomeDoProjeto,
                Tipo = x.TipoDoProjeto switch
                {
                    TipoDoProjeto.Testes => "Teste",
                    TipoDoProjeto.Pacote => "Pacote",
                    _                    => "Fonte"
                },
                Alterações = x.QuantidadeDeAlterações
            }
        );

        var projetosAlteradosEmListaMarkdown = listaDeProjetos.ToMarkdownTable();

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
        Variáveis.DefinirVariável("Comportamentos", novosComportamentos.ToString());
        Variáveis.DefinirVariável("ConfiguracaoParaHabilitarOpcao", configuraçãoParaHabilitar ?? "Nenhuma.");
        Variáveis.DefinirVariável("Endpoint", endpointsAfetados.ToString() ?? "Nenhum.");
        Variáveis.DefinirVariável("TestesAutomatizadosComSucesso", testesAutomatizadosComSucesso ? "Sim" : "Não");
        Variáveis.DefinirVariável("ProcedimentoDeTestes", procedimentosDeTesteEmListaMarkdown.ToString());
        Variáveis.DefinirVariável("DependeDeOutroPR", dependênciaDeOutroPr ?? "Não.");
        Variáveis.DefinirVariável("GerarNovaVersaoPacote", pacotesAlterados?.ToString() ?? "Nenhum.");
        Variáveis.DefinirVariável("Projetos", projetosAlteradosEmListaMarkdown);

        var conteúdoDoArquivoTemplate = File
            .ReadAllText(caminhoDoArquivoTemplate)
            .SubstituirVariáveisNoTexto();

        var caminhoDoArquivoGerado = Path.Combine(
            parâmetros.DiretórioDeDestinoDoArquivoGerado is not null
                ? Path.GetFullPath(parâmetros.DiretórioDeDestinoDoArquivoGerado)
                : parâmetros.Diretório,
            $"pull-request-{branch.Split('/').Last()}.md"
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
                .UseConverter(x => x ? "Sim" : "Não")
        );

        if (copiarConteúdoGerado)
        {
            ClipboardService.SetText(conteúdoDoArquivoTemplate);
            AnsiConsole.MarkupLine("🎉 [bold green]Conteúdo copiado para a área de transferência.[/]");
        }

        AnsiConsole.MarkupLine("[dim]Esperando 3 segundos para fechar...[/]");
        Thread.Sleep(3000);
        return 0;
    }

    private static List<ProjetoAlterado> ListarProjetosAlteradosNosCommitsFiltrados(
        StatusContext context,
        string diretórioDoRepositório,
        string branch,
        string[] identificadores
    )
    {
        context.Status("[yellow bold]Obtendo informações dos commits da branch...[/]");
        var commits = ObterInformaçõesDosCommitsDaBranch(branch, diretórioDoRepositório);

        AnsiConsole.MarkupLine("✓ [green bold]Obtendo informações dos commits da branch...[/]");

        context.Status("[yellow bold]Filtrando commits por identificador da tarefa...[/]");
        var commitsFiltrados = commits
            .Where(c => identificadores.Any(i => c.Mensagem.StartsWith("#" + i)))
            .ToList();

        AnsiConsole.MarkupLine("✓ [green bold]Filtrando commits por identificador da tarefa...[/]");

        context.Status("[yellow bold]Organizando projetos alterados nos commits filtrados...[/]");
        var projetosAlterados = new HashSet<ProjetoAlterado>();
        foreach (var commit in commitsFiltrados)
        {
            var arquivos = ObterArquivosAlteradosDoCommit(commit.Hash, diretórioDoRepositório);
            foreach (var arquivo in arquivos)
                try
                {
                    var projetoDoArquivo = ObterProjetoDoArquivo(arquivo, diretórioDoRepositório);
                    if (projetoDoArquivo is null)
                        continue;

                    var nomeDoProjeto = Path.GetFileNameWithoutExtension(projetoDoArquivo);

                    var detalhesExistentes = projetosAlterados
                        .FirstOrDefault(x => x.NomeDoProjeto == nomeDoProjeto);
                    if (detalhesExistentes is not null)
                    {
                        detalhesExistentes.QuantidadeDeAlterações++;
                        continue;
                    }

                    var detalhesDoProjeto = new ProjetoAlterado
                    {
                        NomeDoProjeto = nomeDoProjeto,
                        QuantidadeDeAlterações = 1
                    };

                    var projetoEhTeste = detalhesDoProjeto.NomeDoProjeto.Contains(
                        "Tests",
                        StringComparison.InvariantCultureIgnoreCase
                    );

                    if (projetoEhTeste)
                        detalhesDoProjeto.TipoDoProjeto = TipoDoProjeto.Testes;

                    try
                    {
                        var conteúdoDoArquivo = File.ReadAllLines(projetoDoArquivo);
                        if (conteúdoDoArquivo.Any(x => x.Contains("<Version>")))
                            detalhesDoProjeto.TipoDoProjeto = TipoDoProjeto.Pacote;
                    }
                    catch
                    {
                        // Ignorada
                    }

                    projetosAlterados.Add(detalhesDoProjeto);
                }
                catch
                {
                    // Ignorada
                }
        }

        AnsiConsole.MarkupLine("✓ [green bold]Organizando projetos alterados nos commits filtrados...[/]");
        return projetosAlterados.OrderBy(x => x.TipoDoProjeto).ToList();
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
        if (caminhoDoArquivo.EndsWith(".csproj"))
            return caminhoDoArquivo;
        
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

    public sealed class Parâmetros : CommandSettings
    {
        [CommandArgument(0, "<DIRETÓRIO>"), Description("Caminho do diretório raíz do repositório.")]
        public required string Diretório { get; set; }

        [CommandArgument(1, "<BRANCH>"), Description("Nome da branch.")]
        public required string Branch { get; set; }

        [CommandOption("-t|--tarefa <TAREFA>"),
         Description(
             "Identificador da tarefa. Por padrão pega o nome da branch. Separar por ponto e vírgula (;) se forem múltiplos."
         )]
        public string? IdentificadorDaTarefa { get; set; }

        [CommandOption("-o|--output <CAMINHO>"),
         Description("Diretório onde o arquivo gerado será armazenado.")]
        public string? DiretórioDeDestinoDoArquivoGerado { get; set; }
    }

    private class InformaçõesDoCommit
    {
        public string Hash { get; set; } = string.Empty;
        public string Mensagem { get; set; } = string.Empty;
    }
}