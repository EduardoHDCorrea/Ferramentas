using Ferramentas.Domínio.Dtos.Enumerados;
using Ferramentas.Infraestrutura.ManipulaçãoDeTexto;
using Spectre.Console;

namespace Ferramentas.Domínio.Dtos;

public class ConfiguraçãoDeExecuçãoDeTestes
{
    public int NúmeroMáximoDeTestesEmParalelo { get; set; }
    public string DiretórioDeResultados { get; set; } = @".\TestResults";
    public int TempoMáximoDeExecuçãoDosTestesEmMilissegundos { get; set; } = 10000;
    public BooleanXml UtilizarBlame { get; set; }
    public string VerbosidadeDoConsole { get; set; } = "quiet";

    public void AtualizarVariáveisComValoresDaConfiguração()
    {
        foreach (var propriedade in GetType().GetProperties())
        {
            try
            {
                var valorDaPropriedade = propriedade.GetValue(this);
                switch (valorDaPropriedade)
                {
                    case null:
                        continue;
                    case string valorString:
                        Variáveis.DefinirVariável(propriedade.Name, valorString);
                        break;
                    default:
                        Variáveis.DefinirVariável(propriedade.Name, valorDaPropriedade.ToString());
                        break;
                }
            }
            catch (Exception e)
            {
                AnsiConsole.MarkupLineInterpolated($"[red]Erro ao definir variável {propriedade.Name} com valor {propriedade.GetValue(this)}:[/]");
                AnsiConsole.WriteException(e);
                throw;
            }
        }
    }
}