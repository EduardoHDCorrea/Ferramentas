using System.Globalization;
using System.Xml.Linq;
using Ferramentas.Domínio.Serviços.ExecuçãoDeTestes.Dtos;

namespace Ferramentas.Domínio.Serviços.ExecuçãoDeTestes.Extensões;

public static class ExtensõesDeResultadoDosTestesEmTrx
{
    public const string PrefixoDeInícioDaExecuçãoDosTestes = "Running all tests in ";
    public const string PrefixoDeInícioDaExecuçãoDosTestesEmPortuguês = "Execução de teste para ";
    private const string FormatoDeDataTrx = "yyyy-MM-ddTHH:mm:ss.fffffffK";

    public static string ObterNomeDoProjeto(this XDocument trx)
    {
        var resultadoDosTestes = trx.ObterResumoDoResultado();
        if (resultadoDosTestes is null)
            return string.Empty;

        var stdOut = resultadoDosTestes.Descendants()
            .FirstOrDefault(x => x.Name.LocalName == "StdOut");

        var nomeDoProjeto = stdOut.ExtrairNomeDoProjeto();
        return nomeDoProjeto;
    }

    public static bool TodosTestesPassaram(this XDocument trx)
    {
        var resultadoDosTestes = trx.ObterResumoDoResultado();
        var contadores = resultadoDosTestes?.ObterContadoresDoResultado();
        if (contadores is null)
            return false;

        return contadores.Total == contadores.Sucesso;
    }

    public static TemposDoTesteNoTrx? ObterTemposDeExecuçãoDoTeste(this XDocument trx)
    {
        var elemento = trx.Descendants().FirstOrDefault(x => x.Name.LocalName == "Times");
        if (elemento is null)
            return null;

        var resultado = new TemposDoTesteNoTrx();
        var start = elemento.Attribute("start")?.Value ?? string.Empty;
        var finish = elemento.Attribute("finish")?.Value ?? string.Empty;

        if (!string.IsNullOrEmpty(start))
            resultado.InícioDoTeste = DateTime.ParseExact(
                elemento.Attribute("start")!.Value,
                FormatoDeDataTrx,
                CultureInfo.InvariantCulture
            );

        if (!string.IsNullOrEmpty(finish))
            resultado.FimDoTeste = DateTime.ParseExact(
                elemento.Attribute("finish")!.Value,
                FormatoDeDataTrx,
                CultureInfo.InvariantCulture
            );

        return resultado;
    }

    public static string ExtrairNomeDoProjetoNaStringDeExecuçãoDosTestes(this string texto) =>
        texto.ExtrairNomeDoProjetoNaString(PrefixoDeInícioDaExecuçãoDosTestesEmPortuguês);

    public static string ExtrairNomeDoProjetoNaString(this string textoComCaminhoDoProjetoDentro, string prefixo)
    {
        var linhaComCaminho = textoComCaminhoDoProjetoDentro
            .Split('\n')
            .FirstOrDefault(line => line.Contains(prefixo));

        if (linhaComCaminho is null)
            return string.Empty;

        var caminhoDoArquivo = linhaComCaminho
            .Replace(prefixo, string.Empty)
            .Trim()
            .Split(' ')
            .First();

        var nomeDoArquivoSemExtensão = Path.GetFileNameWithoutExtension(caminhoDoArquivo);
        return nomeDoArquivoSemExtensão.Split(Path.PathSeparator).Last();
    }

    public static ContadoresDoResultadoDosTestes ObterContadoresDoResultado(this XElement resumoDoResultado)
    {
        var contadoresXml = resumoDoResultado.Descendants()
            .FirstOrDefault(x => x.Name.LocalName == "Counters");

        return new ContadoresDoResultadoDosTestes(contadoresXml);
    }

    public static XElement? ObterResumoDoResultado(this XDocument resultado) =>
        resultado.Descendants().FirstOrDefault(x => x.Name.LocalName == "ResultSummary");

    private static string ExtrairNomeDoProjeto(this XElement? stdOut)
    {
        if (stdOut is null)
            return string.Empty;

        var mensagem = stdOut.Value;
        return mensagem.ExtrairNomeDoProjetoNaString(PrefixoDeInícioDaExecuçãoDosTestes);
    }
}