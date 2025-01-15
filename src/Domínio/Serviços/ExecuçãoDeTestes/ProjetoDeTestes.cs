using System.Globalization;
using System.Xml.Linq;
using Ferramentas.Domínio.Serviços.ExecuçãoDeTestes.Dtos;

namespace Ferramentas.Domínio.Serviços.ExecuçãoDeTestes;

public class ProjetoDeTestes : IDisposable
{
    public const string NomeArquivoTrx = "resultado_testes.trx";
    public DirectoryInfo Diretório { get; private set; }
    public ResultadoDosTestes Resultado { get; set; } = new();
    public DateTime Início { get; set; }
    public DateTime Fim { get; set; }
    public TimeSpan Duração => Fim - Início;
    public delegate void ResultadoAtualizadoCallback(ResultadoDosTestes resultado);

    private readonly FileSystemWatcher? monitoradorDeArquivos;

    public ProjetoDeTestes(string diretório, Action<ResultadoDosTestes> resultadoAtualizadoCallback)
    {
        Diretório = new DirectoryInfo(diretório);
        monitoradorDeArquivos = new FileSystemWatcher(diretório, NomeArquivoTrx);
        monitoradorDeArquivos.EnableRaisingEvents = true;
        monitoradorDeArquivos.Created += (_, args) =>
        {
            if (args.Name != NomeArquivoTrx)
                return;

            AtualizarResultado(args.FullPath);
            resultadoAtualizadoCallback(Resultado);
            monitoradorDeArquivos?.Dispose();
        };
    }

    private void AtualizarResultado(string arquivoTrx)
    {
        const string formatoDeDataTrx = "yyyy-MM-ddTHH:mm:ss.fffffffK";
        try
        {
            var documentoXml = XDocument.Load(arquivoTrx);
            var times = documentoXml.Descendants().FirstOrDefault(x => x.Name.LocalName == "Times");
            var errorInfo = documentoXml.Descendants().FirstOrDefault(x => x.Name.LocalName == "ErrorInfo");
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

            File.Delete(arquivoTrx);
        }
        catch (Exception)
        {
            Resultado = new ResultadoDosTestes
            {
                Sucesso = false,
                Mensagem = "Erro ao processar o arquivo de resultado dos testes."
            };
        }
    }

    public void Dispose() =>
        monitoradorDeArquivos?.Dispose();
}