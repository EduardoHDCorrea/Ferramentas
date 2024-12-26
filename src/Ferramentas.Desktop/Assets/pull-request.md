## Estados afetados:

Ambos

## Comportamentos introduzidos, alterados, ou removidos:

> Descrição de comportamentos introduzidos, alterados, ou removidos. Por exemplo, a criação de um novo endpoint, a
> alteração de um comportamento de um comando existente, ou a remoção de um serviço.

- Criação de um evento manipulador para fazer emissão da nota para a prefeitura caso a organização possua configuração
  de prestação de contas definida.
- Introdução da entidade de configuração de prestação de contas para prefeitura, junto com seu endpoint CRUD.
- Criação do consumidor de retorno da emissão da nota fiscal prestada para validar o resultado e permitir que o usuário
  obtenha o arquivo gerado, ou para visualizar os erros que foram retornados.
- Criação do endpoint de prestação manual de contas para a prefeitura, para lidar com casos onde a emissão automática da
  nota não é possível.

## Configuração para habilitar a opção:

Deve-se configurar a prestação de contas na organização para utilizar da emissão de notas.

## Endpoints afetados:

> Lista de endpoints afetados pelo PR. Por exemplo, `/NotasDeServico`, `/Usuarios`, ou `/Organizacoes`.

- `/PrestarContasDasNotasDeServicoParaAPrefeitura`
- `/ConfiguracaoDePrestacaoDeContasParaAPrefeitura`

## Procedimento de teste realizado:

- Testes automatizados executados com sucesso.
- Devido a introdução de um novo serviço externo, o qual é responsável pela emissão das notas com a DLL da ACBrLib, foi
  necessária a execução local de todo o sistema para validar o comportamento obtido.

## Depende do Pull Request:

!2842

## Projetos alterados:

> - Alterações: Número de arquivos alterados por commit no projeto.
> - Projeto: Nome do projeto alterado.
> - Tipo: Tipo de projeto alterado (Fonte, Testes, Pacote).

| Alterações | Projeto                                                                           | Tipo   |
|------------|-----------------------------------------------------------------------------------|--------|
| 35         | SkyInfo.Core.Dominio.Cartorio.DocumentosFiscais.PrestaçãoDeContas                 | Fonte  |
| 6          | SkyInfo.Core.Servico.Api.Monolito                                                 | Fonte  |
| 23         | SkyInfo.Core.Infraestrutura                                                       | Fonte  |
| 2          | SkyInfo.Core.Aplicacao.Mensageria                                                 | Fonte  |
| 1          | SkyInfo.Core.Servico.Armazenamento                                                | Fonte  |
| 13         | SkyInfo.Core.Dominio.Cartorio.DocumentosFiscais.PrestaçãoDeContas.Aceitacao.Tests | Teste  |
| 2          | SkyInfo.Core.Servico.Api.Monolito.Aceitacao.Tests                                 | Teste  |
| 1          | SkyInfo.Core.Dominio.Aceitacao.Base.Tests                                         | Teste  |
| 1          | SkyInfo.Core.Servico.Conversor.Consumidor.Aceitacao.Tests                         | Teste  |
| 1          | SkyInfo.Core.Dominio.Aceitacao.Tests                                              | Teste  |
| 1          | SkyInfo.Core.Dominio.GeralOrganizacao.Organizacao                                 | Pacote |
| 2          | SkyInfo.Core.Aplicacao                                                            | Pacote |

## Pacotes alterados:

> Projetos versionados que foram alterados, portanto, podem não ser pacotes que precisam ser gerados e publicados.

- `SkyInfo.Core.Dominio.GeralOrganizacao.Organizacao`
- `SkyInfo.Core.Aplicacao`
