using System.Xml.Linq;

namespace Ferramentas.Domínio.Serviços.ExecuçãoDeTestes.Dtos;

public class ContadoresDoResultadoDosTestes
{
    public int Total { get; set; }
    public int Executado { get; set; }
    public int Sucesso { get; set; }
    public int Falha { get; set; }
    public int Inconclusivo { get; set; }
    public int Erro { get; set; }
    public int Timeout { get; set; }
    public int Abortado { get; set; }
    public int SucessoMasExecuçãoFoiAbortada { get; set; }
    public int NãoExecutável { get; set; }
    public int Desconectado { get; set; }
    public int ComAviso { get; set; }
    public int Completado { get; set; }
    public int EmProgresso { get; set; }
    public int Pendente { get; set; }

    public ContadoresDoResultadoDosTestes(XElement? contadores)
    {
        if (contadores is null)
            return;

        Total = ObterAtributoEmInteiro(contadores.Attribute("total"));
        Executado = ObterAtributoEmInteiro(contadores.Attribute("executed"));
        Sucesso = ObterAtributoEmInteiro(contadores.Attribute("passed"));
        Falha = ObterAtributoEmInteiro(contadores.Attribute("failed"));
        Inconclusivo = ObterAtributoEmInteiro(contadores.Attribute("inconclusive"));
        Erro = ObterAtributoEmInteiro(contadores.Attribute("error"));
        Timeout = ObterAtributoEmInteiro(contadores.Attribute("timeout"));
        Abortado = ObterAtributoEmInteiro(contadores.Attribute("aborted"));
        SucessoMasExecuçãoFoiAbortada = ObterAtributoEmInteiro(contadores.Attribute("passedButRunAborted"));
        NãoExecutável = ObterAtributoEmInteiro(contadores.Attribute("notRunnable"));
        Desconectado = ObterAtributoEmInteiro(contadores.Attribute("disconnected"));
        ComAviso = ObterAtributoEmInteiro(contadores.Attribute("warning"));
        Completado = ObterAtributoEmInteiro(contadores.Attribute("completed"));
        EmProgresso = ObterAtributoEmInteiro(contadores.Attribute("inProgress"));
        Pendente = ObterAtributoEmInteiro(contadores.Attribute("pending"));
    }

    private static int ObterAtributoEmInteiro(XAttribute? atributo) =>
        atributo is not null ? int.Parse(atributo.Value) : 0;
}