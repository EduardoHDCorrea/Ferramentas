using Spectre.Console.Cli;

namespace Ferramentas.Domínio.Comandos.CriarOrganizaçãoTeste;

public class CriarOrganizaçãoTesteComando : Command<CriarOrganizaçãoTesteComando.Parâmetros>, IComandoCli
{
    public static ICommandConfigurator InjetarComando(IConfigurator configurator) =>
        configurator.AddCommand<CriarOrganizaçãoTesteComando>("criar-organização-teste");

    public override int Execute(CommandContext context, Parâmetros settings) => 0;

    public class Parâmetros : CommandSettings
    {
        public string UrlDoServidor { get; set; } = "http://localhost:5000";
        public string NomeDaOrganização { get; set; } = Ulid.NewUlid().ToString();
        public string EmailDoUsuário { get; set; } = "usuario@teste.com";
        public string NomeDoUsuário { get; set; } = "Usuário Teste";
        public string SenhaDoUsuário { get; set; } = "123";
    }
}