using System.ComponentModel;
using Spectre.Console;
using Spectre.Console.Cli;
using TextCopy;

namespace Ferramentas.Domínio.Comandos.ObterCaminhoRelativo;

public class ObterCaminhoRelativoComando : Command<ObterCaminhoRelativoComando.Parâmetros>, IComandoCli
{
    public static ICommandConfigurator InjetarComando(IConfigurator configurator) =>
        configurator.AddCommand<ObterCaminhoRelativoComando>("caminho-relativo")
            .WithDescription("Obtém o caminho relativo entre dois diretórios.")
            .WithExample("caminho-relativo", @"D:\Temp\core", @"D:\Temp\Output")
            .WithAlias("cr");

    public override int Execute(CommandContext context, Parâmetros settings)
    {
        ClipboardService.SetText(Path.GetRelativePath(settings.Origem, settings.Destino));
        AnsiConsole.MarkupLine(
            $"[bold green]Caminho relativo copiado para a área de transferência:[/] {
                Path.GetRelativePath(settings.Origem, settings.Destino)}"
        );
        return 0;
    }

    public sealed class Parâmetros : CommandSettings
    {
        [CommandArgument(0, "<ORIGEM>"), Description("Caminho do diretório de origem.")]
        public required string Origem { get; set; }

        [CommandArgument(1, "<DESTINO>"), Description("Caminho do diretório de destino.")]
        public required string Destino { get; set; }
    }
}