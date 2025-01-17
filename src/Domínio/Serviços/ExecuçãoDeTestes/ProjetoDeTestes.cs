using System.Globalization;
using System.Xml.Linq;
using Ferramentas.Domínio.Serviços.ExecuçãoDeTestes.Dtos;
using Spectre.Console;

namespace Ferramentas.Domínio.Serviços.ExecuçãoDeTestes;

public class ProjetoDeTestes
{
    public DirectoryInfo Diretório { get; private set; }
    public ResultadoDosTestes Resultado { get; set; } = new();
    public bool EmExecução { get; set; }
    public DateTime Início { get; set; }
    public DateTime Fim { get; set; }
    public TimeSpan Duração => Fim - Início;

    public ProjetoDeTestes(string diretório) =>
        Diretório = new DirectoryInfo(diretório);

    public void AtualizarResultado(string arquivoTrx)
    {
        const string formatoDeDataTrx = "yyyy-MM-ddTHH:mm:ss.fffffffK";
        try
        {
            var documentoXml = XDocument.Load(arquivoTrx);
            var times = documentoXml.Descendants().FirstOrDefault(x => x.Name.LocalName == "Times");
            var errorInfo = documentoXml.Descendants().FirstOrDefault(x => x.Name.LocalName == "ErrorInfo");
            var resultSummary = documentoXml.Descendants()
                .FirstOrDefault(x => x.Name.LocalName == "ResultSummary");
            if (times is not null)
            {
                var start = times.Attribute("start")?.Value ?? string.Empty;
                var finish = times.Attribute("finish")?.Value ?? string.Empty;

                if (!string.IsNullOrEmpty(start))
                    Início = DateTime.ParseExact(
                        times.Attribute("start")!.Value,
                        formatoDeDataTrx,
                        CultureInfo.InvariantCulture
                    );

                if (!string.IsNullOrEmpty(finish))
                    Fim = DateTime.ParseExact(
                        times.Attribute("finish")!.Value,
                        formatoDeDataTrx,
                        CultureInfo.InvariantCulture
                    );
            }

            if (errorInfo is not null)
            {
                Resultado.StackTrace = errorInfo.Elements().FirstOrDefault(x => x.Name.LocalName == "StackTrace")?.Value
                 ?? string.Empty;
                Resultado.Mensagem = errorInfo.Elements().FirstOrDefault(x => x.Name.LocalName == "Message")?.Value
                 ?? string.Empty;
            }

            var counters = resultSummary?.Descendants().FirstOrDefault(x => x.Name.LocalName == "Counters");
            if (counters is not null)
                Resultado.Sucesso = counters.Attribute("failed")?.Value == "0";
        }
        catch (Exception e)
        {
            Resultado = new ResultadoDosTestes
            {
                Sucesso = false,
                Mensagem = "Erro ao processar o arquivo de resultado dos testes. " + e.Message
            };
        }
    }
}