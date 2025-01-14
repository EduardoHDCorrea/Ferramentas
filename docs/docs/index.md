---
  template: home.html
  hide:
    - toc
    - navigation
---

<div id="fusiondoc-home" markdown>
<section id="fusiondoc-home-main">
<section id="fusiondoc-home-main-inner">
<h1>Ferramentas.Cli</h1>
<p>
Descrever alterações de PRs, listar os projetos alterados, e destacar pacotes
que foram alterados e devem ser atualizados é um processo entediante e toma
tempo de desenvolvimento.
</p>
<p>
Não mais! Com apenas um comando, e algumas respostas aqui e ali, você consegue
gerar uma descrição completa das alterações da sua tarefa.
</p>
<p>
Ferramentas.Cli é uma ferramenta de linha de comando projetada para otimizar
fluxos de trabalho e facilitar tarefas do dia a dia no desenvolvimento da empresa.
Com comandos intuitivos e funcionalidades poderosas, ela transforma processos
complexos em ações simples e eficientes.
</p>
<nav>
<a href="./api">API</a>
<a href="https://www.nuget.org/packages/Ferramentas.Cli/">Obter ferramenta</a>
</nav>
</section>
</section>

<aside id="fusiondoc-home-scroll">
Desça um pouco para ver as principais funções implementadas!
</aside>

<section id="fusiondoc-home-belowfold" markdown>

<h2 class="first">Descrever pull requests</h2>

Descrever alterações de PRs, listar os projetos alterados, e destacar pacotes
que foram alterados e devem ser atualizados é um processo entediante e toma
tempo de desenvolvimento.

Não mais! Com apenas um comando, e algumas respostas aqui e ali, você consegue
gerar uma descrição completa das alterações da sua tarefa.

Essa função coleta informações dos commits da branch especificada e gera um arquivo `markdown` com base nos dados
passados iterativamente.

Os projetos alterados na branch são listados e separados por `teste`, `fonte`, e `pacote`, assim facilitando a
criação de descrições de PR que precisam especificar os projetos alterados.

A listagem de pacotes alterados é feita com base na existência do campo `<Version>` nos `.csproj` dos projetos que
tiveram alterações, então ela pode não ser precisa em todos os casos.

-----

Por exemplo, para gerar a descrição de um PR simples, ou seja, que a tarefa e a branch possuam o mesmo identificador,
podemos executar o comando da seguinte forma:

!!! information "Contexto"
	Assume-se, neste caso, que o identificador da tarefa seja o mesmo da branch.
	Por exemplo, a branch `tarefa/1234` refere-se à tarefa `#1234`.

```bash
skyinfo resumir-pr caminho/do/repositório tarefa/1234
```

-----

<h2 class="second">Gerar comandos CRUD</h2>

O comando `criar-comandos-crud-base` tem como propósito principal agilizar a criação de novas entidades no `core` *(ou qualquer projeto que utilize a mesma estrutura)*.

Ao executá-lo dentro de um projeto do `domínio`, especificando o nome da entidade, serão gerados os comandos `Adicionar`, `Atualizar`, e `Remover` da entidade, já incluindo as propriedades da entidade no comando base `AdicionarAtualizar`.

-----

Por exemplo, dada a seguinte entidade:

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

-----

<h2 class="third">Obter caminho relativo entre dois diretórios</h2>

Comando simples que retorna o caminho relativo de um diretório até outro.

-----

Por exemplo, dada a seguinte estrutura do diretório:

```
./
├─ diretorioA/
│  ├─ diretorioB/
│  │  ├─ destino/
├─ diretorioC/
│  ├─ origem/
```

Podemos obter o caminho relativo do diretório `origem` até o `destino` da seguinte forma:

```bash
skyinfo caminho-relativo ./diretorioC/origem ./diretorioA/diretorioB/destino
```

O resultado será: `../../diretorioA/diretorioB/destino`

-----

## Ficou interessado?

Essas são apenas algumas das funcionalidades implementadas, então caso queira ver mais detalhes sobre cada comando e seus diferentes usos, [dê uma olhada na aba de comandos.](./api)

</section>
</div>