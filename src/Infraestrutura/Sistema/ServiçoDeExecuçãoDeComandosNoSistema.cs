using System.Diagnostics;
using System.Text;

namespace Ferramentas.Infraestrutura.Sistema;

public static class ServiçoDeExecuçãoDeComandosNoSistema
{
    public static List<string> ExecutarComandoComRetorno(this string comando, string diretórioDeExecução)
    {
        var informaçãoDoProcesso = new ProcessStartInfo
        {
            FileName = "cmd",
            Arguments = $"/c {comando}",
            WorkingDirectory = diretórioDeExecução,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,

            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var processo = new Process();
        processo.StartInfo = informaçãoDoProcesso;
        processo.Start();

        var linhas = new List<string>();
        while (!processo.HasExited)
        {
            var linha = processo.StandardOutput.ReadLine();
            if (linha is not null)
                linhas.Add(linha);
        }

        string? restanteDasLinhas;
        while ((restanteDasLinhas = processo.StandardOutput.ReadLine()) != null)
            linhas.Add(restanteDasLinhas);

        processo.WaitForExit();
        return linhas;
    }
}