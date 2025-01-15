# Gerar Comandos CRUD

Este comando `criar-comandos-crud-base` tem como propósito principal agilizar a criação de novas entidades no `core` *(ou qualquer projeto que utilize a mesma estrutura)*.

Ao executá-lo em um projeto do `domínio`, especificando o nome da entidade, serão gerados os comandos `Adicionar`, `Atualizar`, e `Remover` da entidade, já incluindo as propriedades da entidade no comando base `AdicionarAtualizar`.

-----

## Exemplo

Dada a seguinte entidade:

```c#
using Solução.Infra.Enumerados;
using Solução.Dominio.Exemplo.Abstrações;
using Solução.Dominio.Exemplo.Entidades;

namespace Solução.Dominio.Exemplo.Entidades;

public class EntidadeDeExemplo : Entidade
{
    public UmEnumeradoDaInfra EnumeradoAmigável { get; set; }
    public int Número { get; set; }
}
```

O comando base herdará as referências de `using` da entidade, e irá gerar o seguinte comando base:

```c#
using AutoMapper;
using Solução.Infra.Bus.Abstracoes;
using Solução.Infra.Enumerados;
using Solução.Dominio.Exemplo.Abstrações;
using Solução.Dominio.Exemplo.Entidades;

namespace Solução.Dominio.Exemplo.ComandosManipuladores.Comandos;

[AutoMap(typeof(EntidadeDeExemplo), ReverseMap = true)]
public class AdicionarAtualizarEntidadeDeExemploComandoBase<TRetorno> : Comando<TRetorno>
{
	public UmEnumeradoDaInfra EnumeradoAmigável { get; set; }
    public int Número { get; set; }
}
```