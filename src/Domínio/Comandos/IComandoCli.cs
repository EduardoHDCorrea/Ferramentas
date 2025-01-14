using Spectre.Console.Cli;

namespace Ferramentas.Domínio.Comandos;

public interface IComandoCli
{
    public static abstract ICommandConfigurator InjetarComando(IConfigurator configurator);
}