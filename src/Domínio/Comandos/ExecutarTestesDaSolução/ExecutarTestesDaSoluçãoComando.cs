using Ferramentas.Domínio.Serviços.ExecuçãoDeTestes;
using Spectre.Console.Cli;

namespace Ferramentas.Domínio.Comandos.ExecutarTestesDaSolução;

public class ExecutarTestesDaSoluçãoComando : Command<ExecutarTestesDaSoluçãoComando.Parâmetros>, IComandoCli
{
    public static ICommandConfigurator InjetarComando(IConfigurator configurator) =>
        configurator.AddCommand<ExecutarTestesDaSoluçãoComando>("executar-testes");

    public sealed class Parâmetros : CommandSettings
    {
        [CommandArgument(0, "<DIRETÓRIO>")]
        public string DiretórioDeExecução { get; set; } = Diretórios.DiretórioDeExecução;
    }

    public override int Execute(CommandContext context, Parâmetros parâmetros)
    {
        using var executorDeTestes = new ExecutorDeTestes(parâmetros.DiretórioDeExecução);
        executorDeTestes.ExecutarTestes();
        return 0;
    }
}