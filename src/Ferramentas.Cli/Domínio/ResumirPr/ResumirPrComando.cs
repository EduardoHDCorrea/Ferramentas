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
    private static bool _gitCredentialsSetup = false;

    public override int Execute(CommandContext contexto, Parâmetros parâmetros)
    {
        var diretório = Path.GetFullPath(parâmetros.Diretório);
        
        // Configura credenciais Git para evitar múltiplas solicitações de autenticação
        ConfigurarCredenciaisGit(diretório);
        
        // Obtém a branch ativa se não foi especificada como parâmetro
        var branch = parâmetros.Branch;
        if (string.IsNullOrWhiteSpace(branch))
        {
            branch = ObterBranchAtiva(diretório);
            AnsiConsole.MarkupLine($"[yellow bold]Branch ativa:[/] {branch}");
        }
        
        var identificadores = parâmetros.IdentificadorDaTarefa?.Split(";") ?? [branch.Split("/").Last()];
        var projetosAlterados = new List<ProjetoAlterado>();

        if (parâmetros.IdentificadorDaTarefa is null)
        {
            var usarIdentificadorDaBranch = AnsiConsole.Prompt(
                new SelectionPrompt<bool>()
                    .Title($"Usar o identificador da branch como identificador da tarefa? (#{identificadores.Single()})")
                    .AddChoices(true, false)
                    .UseConverter(x => x ? "Sim" : "Não")
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
                            diretório,
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

    private static void ConfigurarCredenciaisGit(string diretórioDoRepositório)
    {
        if (_gitCredentialsSetup)
            return;

        try
        {
            AnsiConsole.Status()
                .AutoRefresh(true)
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("yellow bold"))
                .Start("Configurando autenticação Git...", contexto =>
                {
                    // Verifica se o gerenciador de credenciais está configurado
                    var comandoVerificarHelper = "git config --get credential.helper";
                    var resultado = comandoVerificarHelper.ExecutarComandoComRetorno(diretórioDoRepositório);
                    
                    if (resultado.Count == 0 || string.IsNullOrEmpty(resultado[0]))
                    {
                        // Se não estiver configurado, configura para usar cache em memória por 1 hora
                        var comandoConfigurarCache = "git config --local credential.helper 'cache --timeout=3600'";
                        comandoConfigurarCache.ExecutarComandoComRetorno(diretórioDoRepositório);
                        AnsiConsole.MarkupLine("[dim]Configurado cache de credenciais Git por 1 hora.[/]");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"[dim]Utilizando gerenciador de credenciais Git: {resultado[0]}[/]");
                    }
                    
                    // Realiza uma operação de fetch para autenticar uma única vez no início
                    var comandoFetch = "git fetch origin --quiet";
                    comandoFetch.ExecutarComandoComRetorno(diretórioDoRepositório);
                });

            _gitCredentialsSetup = true;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[yellow]Aviso: Não foi possível configurar credenciais Git: {ex.Message}[/]");
            // Continuamos mesmo com falha, pois pode estar usando SSH ou outro método que não precisa disso
        }
    }

    private static List<ProjetoAlterado> ListarProjetosAlteradosNosCommitsFiltrados(
        StatusContext context,
        string diretórioDoRepositório,
        string branch,
        string[] identificadores
    )
    {
        context.Status("[yellow bold]Identificando branch principal do repositório...[/]");
        var branchPrincipal = ObterBranchPrincipal(diretórioDoRepositório);
        AnsiConsole.MarkupLine($"✓ [green bold]Branch principal identificada: [/][blue bold]{branchPrincipal}[/]");

        context.Status("[yellow bold]Obtendo informações dos commits da branch...[/]");
        var commits = ObterInformaçõesDosCommitsDaBranch(branch, diretórioDoRepositório);
        AnsiConsole.MarkupLine("✓ [green bold]Obtendo informações dos commits da branch...[/]");

        context.Status("[yellow bold]Filtrando commits por identificador da tarefa...[/]");
        var commitsFiltrados = commits
            .Where(c => identificadores.Any(i => c.Mensagem.StartsWith("#" + i)))
            .ToList();
        AnsiConsole.MarkupLine("✓ [green bold]Filtrando commits por identificador da tarefa...[/]");

        context.Status("[yellow bold]Comparando alterações com a branch principal...[/]");
        var arquivosAlterados = ObterArquivosAlteradosComparadosComOrigemPrincipal(
            branch, 
            branchPrincipal, 
            diretórioDoRepositório
        );
        AnsiConsole.MarkupLine("✓ [green bold]Comparando alterações com a branch principal...[/]");

        context.Status("[yellow bold]Organizando projetos alterados...[/]");
        var projetosAlterados = new HashSet<ProjetoAlterado>();
        
        foreach (var arquivo in arquivosAlterados)
        {
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

        AnsiConsole.MarkupLine("✓ [green bold]Organizando projetos alterados...[/]");
        return projetosAlterados.OrderBy(x => x.TipoDoProjeto).ToList();
    }

    private static string ObterBranchPrincipal(string diretórioDoRepositório)
    {
        try
        {
            // Tenta primeiro verificar se existe a branch main remota
            var comando = "git ls-remote --heads origin main";
            var resultadoMain = comando.ExecutarComandoComRetorno(diretórioDoRepositório);
            if (resultadoMain.Count > 0 && !string.IsNullOrWhiteSpace(resultadoMain[0]))
                return "origin/main";

            // Se não encontrou main, tenta master
            comando = "git ls-remote --heads origin master";
            var resultadoMaster = comando.ExecutarComandoComRetorno(diretórioDoRepositório);
            if (resultadoMaster.Count > 0 && !string.IsNullOrWhiteSpace(resultadoMaster[0]))
                return "origin/master";

            // Se ainda não encontrou, tenta determinar a branch padrão do repositório
            comando = "git remote show origin";
            var resultadoRemote = comando.ExecutarComandoComRetorno(diretórioDoRepositório);
            var linhaBranchPadrao = resultadoRemote.FirstOrDefault(linha => 
                linha.Contains("HEAD branch:") || linha.Contains("HEAD branch"));
                
            if (linhaBranchPadrao != null)
            {
                var branchPadrao = linhaBranchPadrao.Split(':').Last().Trim();
                return $"origin/{branchPadrao}";
            }

            // Fallback para origin/master se não encontrar
            return "origin/master";
        }
        catch
        {
            // Em caso de erro, assume origin/master como padrão
            return "origin/master";
        }
    }

    private static List<string> ObterArquivosAlteradosComparadosComOrigemPrincipal(
        string branch,
        string branchPrincipal,
        string diretórioDoRepositório
    )
    {
        var comandoDiff = $"git -c core.quotepath=false diff --name-only {branchPrincipal}...{branch}";
        return comandoDiff.ExecutarComandoComRetorno(diretórioDoRepositório);
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
            return Path.GetFullPath(caminhoDoArquivo, caminhoDoRepositório);

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

    private static string ObterBranchAtiva(string diretórioDoRepositório)
    {
        try
        {
            var comando = "git symbolic-ref --short HEAD";
            var resultado = comando.ExecutarComandoComRetorno(diretórioDoRepositório);
            
            if (resultado.Count > 0 && !string.IsNullOrWhiteSpace(resultado[0]))
                return resultado[0];
            
            // Se estiver em estado de desanexo (detached HEAD), retornar hash curto
            comando = "git rev-parse --short HEAD";
            resultado = comando.ExecutarComandoComRetorno(diretórioDoRepositório);
            if (resultado.Count > 0 && !string.IsNullOrWhiteSpace(resultado[0]))
                return resultado[0];
                
            throw new Exception("Não foi possível determinar a branch ativa");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red bold]Erro ao obter branch atual: {ex.Message}[/]");
            throw new Exception("Falha ao determinar a branch ativa. Verifique se você está em um repositório Git válido.", ex);
        }
    }

    public sealed class Parâmetros : CommandSettings
    {
        [CommandArgument(0, "<DIRETÓRIO>"), Description("Caminho do diretório raíz do repositório.")]
        public required string Diretório { get; set; }

        [CommandArgument(1, "[BRANCH]"), Description("Nome da branch. Se não for informada, usa a branch ativa.")]
        public string? Branch { get; set; }

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