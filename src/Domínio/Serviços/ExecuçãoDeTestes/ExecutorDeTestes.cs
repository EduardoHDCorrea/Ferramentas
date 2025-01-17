using System.Text.Json;
using System.Xml.Linq;
using Ferramentas.Domínio.Dtos;
using Ferramentas.Domínio.Serviços.ExecuçãoDeTestes.Extensões;
using Ferramentas.Infraestrutura.ManipulaçãoDeTexto;
using Ferramentas.Infraestrutura.Sistema;
using Spectre.Console;

namespace Ferramentas.Domínio.Serviços.ExecuçãoDeTestes;

public class ExecutorDeTestes : IDisposable
{
    private const string NomeDoArquivoDeRunSettings = "testes.runsettings";
    private DirectoryInfo DiretórioRaíz { get; set; } = new(Diretórios.DiretórioDeExecução);
    private readonly List<ProjetoDeTestes> projetosDeTeste;
    private readonly ConfiguraçãoDeExecuçãoDeTestes configuraçãoDeExecuçãoDeTestes;
    private FileSystemWatcher? fileSystemWatcher;

    public ExecutorDeTestes(string? diretório = null)
    {
        if (diretório is not null)
            DiretórioRaíz = new DirectoryInfo(diretório);

        var diretórioDaSolução = DiretórioRaíz.ObterPrimeiroArquivoComExtensão("sln")?.Directory;
        DiretórioRaíz = diretórioDaSolução
         ?? throw new Exception("O diretório especificado não possui nenhuma solução (.sln).");

        Variáveis.DefinirVariável("Temp", Path.TrimEndingDirectorySeparator(Path.GetTempPath()));
        Variáveis.DefinirVariável("Solução", diretórioDaSolução.Name);

        configuraçãoDeExecuçãoDeTestes = JsonSerializer.Deserialize<ConfiguraçãoDeExecuçãoDeTestes>(
            File.ReadAllText(
                Path.Combine(
                    Diretórios.DiretórioDeConfiguração.FullName,
                    $"{nameof(ConfiguraçãoDeExecuçãoDeTestes)}.json"
                )
            )
        )!;

        configuraçãoDeExecuçãoDeTestes.DiretórioDeResultados = configuraçãoDeExecuçãoDeTestes.DiretórioDeResultados
            .SubstituirVariáveisNoTexto();
        configuraçãoDeExecuçãoDeTestes.AtualizarVariáveisComValoresDaConfiguração();
        CriarRunSettingsDaSolução();

        projetosDeTeste = DiretórioRaíz
            .ObterProjetosDeTesteDoDiretório()
            .ConvertAll(x => new ProjetoDeTestes(x.FullName));
    }

    public void ExecutarTestes()
    {
        fileSystemWatcher = new FileSystemWatcher();
        fileSystemWatcher.Path = configuraçãoDeExecuçãoDeTestes.DiretórioDeResultados;
        fileSystemWatcher.Filter = "*.trx";
        fileSystemWatcher.EnableRaisingEvents = true;

        AnsiConsole.Status().Start("Executando restore da solução...", RealizarRestoreDaSolução);

        AnsiConsole.Status().Start("Executando build da solução...", RealizarBuildDaSolução);

        AnsiConsole.Status()
            .AutoRefresh(true)
            .Start("Executando testes...", ctx =>
                {
                    var processoDeTeste = DiretórioRaíz.ObterProcessoDeExecuçãoDeTestes(
                        Path.Combine(DiretórioRaíz.FullName, NomeDoArquivoDeRunSettings)
                    );

                    fileSystemWatcher.Changed += (_, args) =>
                    {
                        var trx = XDocument.Load(args.FullPath);
                        AtualizarResultadoDoProjeto(trx, ctx);
                    };

                    processoDeTeste.OutputDataReceived += (_, args) =>
                    {
                        if (args.Data is null)
                            return;

                        const string prefixo = ExtensõesDeResultadoDosTestesEmTrx
                            .PrefixoDeInícioDaExecuçãoDosTestesEmPortuguês;
                        if (!args.Data.Contains(prefixo))
                            return;

                        var nomeDoProjeto = args.Data.ExtrairNomeDoProjetoNaStringDeExecuçãoDosTestes();
                        IniciandoTestesDoProjeto(nomeDoProjeto, ctx);
                    };

                    processoDeTeste.Start();
                    processoDeTeste.BeginOutputReadLine();
                    processoDeTeste.BeginErrorReadLine();
                    processoDeTeste.WaitForExit();
                }
            );
    }

    private void CriarRunSettingsDaSolução()
    {
        Directory.CreateDirectory(configuraçãoDeExecuçãoDeTestes.DiretórioDeResultados);
        var runSettingsOriginal = File.ReadAllText(
            Path.Combine(Diretórios.DiretórioDeTemplates, "testes.runsettings")
        );

        File.WriteAllText(
            Path.Combine(DiretórioRaíz.FullName, "testes.runsettings"),
            runSettingsOriginal.SubstituirVariáveisNoTexto()
        );
    }

    private void RealizarRestoreDaSolução(StatusContext ctx)
    {
        using var processo = DiretórioRaíz.ObterProcessoDeRestoreDotNet();
        var erro = string.Empty;

        processo.ErrorDataReceived += (_, args) => erro = args.Data;
        processo.Start();
        processo.BeginErrorReadLine();
        processo.WaitForExit();

        if (!string.IsNullOrEmpty(erro))
            throw new Exception($"Falha ao executar o restore da solução. \nErro: {erro}");

        var tempoDeExecução = processo.StartTime.ObterTextoDeTempoDecorrido(processo.ExitTime);
        AnsiConsole.MarkupLineInterpolated($"[green]Restore concluído com sucesso.[/] [dim]({tempoDeExecução}')[/]");
    }

    private void RealizarBuildDaSolução(StatusContext ctx)
    {
        using var processo = DiretórioRaíz.ObterProcessoDeBuildDotNet();
        var erro = string.Empty;

        processo.ErrorDataReceived += (_, args) =>
            erro = args.Data;

        processo.Start();
        processo.BeginErrorReadLine();
        processo.WaitForExit();

        if (!string.IsNullOrEmpty(erro))
            throw new Exception($"Falha ao executar o build da solução. \nErro: {erro}");

        var tempoDeExecução = processo.StartTime.ObterTextoDeTempoDecorrido(processo.ExitTime);
        AnsiConsole.MarkupLineInterpolated($"[green]Build concluído com sucesso.[/] [dim]({tempoDeExecução}')[/]");
    }

    private void IniciandoTestesDoProjeto(string nomeDoProjeto, StatusContext ctx)
    {
        var projeto = projetosDeTeste.FirstOrDefault(x => x.Diretório.Name == nomeDoProjeto);
        if (projeto is null)
            return;

        projeto.EmExecução = true;
        var testesEmExecução = projetosDeTeste
            .Where(x => x.EmExecução)
            .Select(x => x.Diretório.Name)
            .ToArray();

        var testesEmExecuçãoAgregados = testesEmExecução.Aggregate((t, x) => t + " | " + x);
        var testesConcluídos = projetosDeTeste.Count(x => x.Duração != TimeSpan.Zero);
        var testesTotais = projetosDeTeste.Count;

        ctx.Status($"({testesConcluídos}/{testesTotais}) [plum1]Testes em execução:[/] [dim]{testesEmExecuçãoAgregados}[/]");
    }

    private void AtualizarResultadoDoProjeto(XDocument trx, StatusContext ctx)
    {
        var nomeDoProjeto = trx.ObterNomeDoProjeto();
        var resultado = trx.ObterResumoDoResultado();
        var contadores = resultado?.ObterContadoresDoResultado();
        var sucesso = trx.TodosTestesPassaram();
        var tempoDeExecução = trx.ObterTemposDeExecuçãoDoTeste();
        if (resultado is null || tempoDeExecução is null)
            return;

        var projeto = projetosDeTeste.FirstOrDefault(x => x.Diretório.Name == nomeDoProjeto);
        if (projeto is null)
            return;

        projeto.Início = tempoDeExecução.InícioDoTeste;
        projeto.Fim = tempoDeExecução.FimDoTeste;
        projeto.EmExecução = false;
        projeto.Resultado.Contadores = contadores;
        projeto.Resultado.Sucesso = sucesso;

        if (sucesso)
            AnsiConsole.MarkupLineInterpolated($"[lime]Resultado dos testes do projeto '{nomeDoProjeto}': Sucesso[/]");
        else
            AnsiConsole.MarkupLineInterpolated($"[red]Resultado dos testes do projeto '{nomeDoProjeto}': Falha[/]");
    }

    public void Dispose()
    {
        fileSystemWatcher?.Dispose();
    }
}