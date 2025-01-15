using System.Diagnostics;
using System.Text;

namespace Ferramentas.Infraestrutura.Sistema;

public static class ExtensõesDeProcessos
{
    public static List<string> ExecutarComandoEObterResultado(this string comando, string diretórioDeExecução)
    {
        using var processo = ObterProcessoBase();
        processo.DefinirComandoParaExecutar(comando);
        processo.DefinirDiretórioDeExecução(diretórioDeExecução);
        processo.Start();

        var linhas = new List<string>();
        while (!processo.HasExited)
        {
            var linha = processo.StandardOutput.ReadLine();
            if (linha is not null)
                linhas.Add(linha);
        }

        while (processo.StandardOutput.ReadLine() is { } restanteDasLinhas)
            linhas.Add(restanteDasLinhas);

        processo.WaitForExit();
        return linhas;
    }

    public static async Task AguardarEncerramentoDoProcessoAsync(
        this Process processo,
        CancellationToken cancellationToken
    ) => await Task.Run(() =>
        {
            while (!processo.HasExited)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    processo.CancelErrorRead();
                    processo.CancelOutputRead();
                    processo.Kill();
                    break;
                }

                Thread.Sleep(100);
            }
        }, cancellationToken);

    public static Process DefinirComandoParaExecutar(this Process process, string comando)
    {
        process.StartInfo.Arguments = $"/c {comando}";
        return process;
    }

    public static Process DefinirDiretórioDeExecução(this Process process, string diretório)
    {
        process.StartInfo.WorkingDirectory = diretório;
        return process;
    }

    public static Process ObterProcessoDeExecuçãoDeTestes(
        this DirectoryInfo diretórioRaíz
    )
    {
        var processo = ObterProcessoBase();
        processo.DefinirDiretórioDeExecução(diretórioRaíz.FullName);
        processo.DefinirComandoParaExecutar("dotnet test --logger trx");
        return processo;
    }

    public static Process ObterProcessoParaListaDeProjetosDeTestesDaSolução(
        this List<string> outputHandlerLista,
        string diretórioDeTrabalho
    )
    {
        var processo = ObterProcessoBase();
        processo.DefinirDiretórioDeExecução(diretórioDeTrabalho);
        processo.DefinirComandoParaExecutar("dotnet sln list");
        processo.OutputDataReceived += (_, eventArgs) =>
        {
            if (eventArgs.Data is { } projeto)
                outputHandlerLista.Add(projeto.Trim());
        };
        return processo;
    }

    private static Process ObterProcessoBase()
    {
        var processo = new Process();
        processo.StartInfo = new ProcessStartInfo("cmd")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WindowStyle = ProcessWindowStyle.Hidden,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        return processo;
    }
}