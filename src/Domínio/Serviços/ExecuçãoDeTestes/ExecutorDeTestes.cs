using Ferramentas.Infraestrutura.Sistema;
using Spectre.Console;

namespace Ferramentas.Domínio.Serviços.ExecuçãoDeTestes;

public class ExecutorDeTestes : IDisposable
{
    public DirectoryInfo DiretórioRaíz { get; private set; } = new(Diretórios.DiretórioDeExecução);
    private readonly List<ProjetoDeTestes> projetosDeTeste = [];

    public ExecutorDeTestes()
    {
        var diretórioDaSolução = DiretórioRaíz.ObterPrimeiroArquivoComExtensão("sln")?.Directory;
        if (diretórioDaSolução is null)
            throw new Exception("O diretório especificado não possui nenhuma solução (.sln).");

        DiretórioRaíz = diretórioDaSolução;

        projetosDeTeste = DiretórioRaíz
            .ObterProjetosDeTesteDoDiretório()
            .ConvertAll(x =>
                new ProjetoDeTestes(x.FullName, resultado =>
                {
                    AnsiConsole.WriteLine($"Projeto: {x.Name}");
                    AnsiConsole.WriteLine($"Mensagem: {resultado.Mensagem}");
                })
            );
    }

    public ExecutorDeTestes(string diretórioRaíz) : base()
    {
        DiretórioRaíz = new(diretórioRaíz);
    }

    public void ExecutarTestes()
    {
        var processoDeTeste = DiretórioRaíz.ObterProcessoDeExecuçãoDeTestes();
        processoDeTeste.OutputDataReceived += (_, args) =>
        {
            AnsiConsole.
        };
        processoDeTeste.Start();
        processoDeTeste.WaitForExit();
    }

    public void Dispose() =>
        projetosDeTeste.ForEach(x => x.Dispose());
}