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

    public static Process ObterProcessoDeRestoreDotNet(this DirectoryInfo diretórioRaíz)
    {
        var processo = ObterProcessoBase();
        processo.DefinirDiretórioDeExecução(diretórioRaíz.FullName);
        processo.DefinirComandoParaExecutar(
            "dotnet restore --verbosity quiet"
        );
        return processo;
    }

    public static Process ObterProcessoDeBuildDotNet(this DirectoryInfo diretórioRaíz)
    {
        var processo = ObterProcessoBase();
        processo.DefinirDiretórioDeExecução(diretórioRaíz.FullName);
        processo.DefinirComandoParaExecutar(
            "dotnet build --no-restore --verbosity quiet"
        );
        return processo;
    }

    public static Process ObterProcessoDeExecuçãoDeTestes(
        this DirectoryInfo diretórioRaíz, string caminhoDoRunSettings
    )
    {
        var processo = ObterProcessoBase();
        processo.DefinirDiretórioDeExecução(diretórioRaíz.FullName);
        processo.DefinirComandoParaExecutar(
            $"dotnet test --no-build --no-restore --settings {caminhoDoRunSettings}" // -p:TestTfmsInParallel=false
        );
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
            if (eventArgs.Data is not { } projeto)
                return;

            if (!projeto.Contains('/') && !projeto.Contains('\\'))
                return;

            try
            {
                var informaçõesDoArquivo = new FileInfo(Path.GetFullPath(projeto, diretórioDeTrabalho));
                var diretórioDoProjeto = informaçõesDoArquivo.DirectoryName;
                if (diretórioDoProjeto is null)
                    return;

                outputHandlerLista.Add(diretórioDoProjeto);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        };

        return processo;
    }

    private static Process ObterProcessoBase()
    {
        var processo = new Process();
        processo.EnableRaisingEvents = true;
        processo.StartInfo = new ProcessStartInfo("cmd.exe")
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