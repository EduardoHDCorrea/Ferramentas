using Ferramentas.Domínio.Comandos.CriarComandosCrudBase;
using Ferramentas.Domínio.Comandos.CriarOrganizaçãoTeste;
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
    }
);

app.Run(args);