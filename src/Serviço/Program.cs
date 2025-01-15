using Ferramentas.Domínio.Comandos.CriarComandosCrudBase;
using Ferramentas.Domínio.Comandos.CriarOrganizaçãoTeste;
using Ferramentas.Domínio.Comandos.ExecutarTestesDaSolução;
using Ferramentas.Domínio.Comandos.ObterCaminhoRelativo;
using Ferramentas.Domínio.Comandos.ResumirPr;
using Spectre.Console.Cli;

var app = new CommandApp();
app.Configure(
    configurator =>
    {
        ObterCaminhoRelativoComando.InjetarComando(configurator);
        CriarOrganizaçãoTesteComando.InjetarComando(configurator);
        CriarComandosCrudBaseComando.InjetarComando(configurator);
        ResumirPrComando.InjetarComando(configurator);
        ExecutarTestesDaSoluçãoComando.InjetarComando(configurator);
    }
);

args = [
        "executar-testes",
        @"C:\Sky\TerraMedia\infra"
    ];

app.Run(args);