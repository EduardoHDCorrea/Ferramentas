# Ferramentas.Cli

> Este é um pequeno projeto que cria uma `dotnet tool` para facilitar a criação de descrições de PR no setor de
> desenvolvimento da empresa.

## Instalação

```bash
dotnet tool install -g Ferramentas.Cli
```

## Comandos

### `caminho-relativo` ou `cr`:

Obtém o caminho relativo entre dois diretórios.

| Ordem do Parâmetro | Descrição             |
|--------------------|-----------------------|
| 1                  | Diretório de origem.  |
| 2                  | Diretório de destino. |

#### Exemplo

Contexto:

```
./
├─ diretorioA/
│  ├─ diretorioB/
│  │  ├─ destino/
├─ diretorioC/
│  ├─ origem/
```

```bash
skyinfo cr ./diretorioC/origem ./diretorioA/diretorioB/destino
```

- `cr` refere-se ao comando `caminho-relativo`.

Resultado: `../../diretorioA/diretorioB/destino`

### `resumir-pr` - `rp`:

Obtém informações dos commits da branch especificada e gera um arquivo `markdown` com base nos dados
passados iterativamente.

Os projetos alterados na branch são listados e separados por `teste`, `fonte`, e `pacote`, assim facilitando a
criação de descrições de PR que precisam especificar os projetos alterados.

A listagem de pacotes alterados é feita com base na existência do campo `<Version>` nos `.csproj` dos projetos que
tiveram alterações, então ela pode não ser precisa em todos os casos.

### Exemplos

#### Criar descrição de PR simples

> Assume-se, neste caso, que o identificador da tarefa seja o mesmo da branch. Por exemplo, a branch `tarefa/1234`
> refere-se à tarefa `#1234`.

```bash
skyinfo resumir-pr caminho/do/repositório tarefa/1234
```

- `caminho/do/repositório` é o caminho para o repositório local, caso esteja no diretório do repositório, pode-se usar
  `.`.

- `tarefa/1234` é a branch.

#### Criar descrição de PR com identificador de tarefa diferente da branch

```bash
skyinfo resumir-pr caminho/do/repositório tarefa/1234 -t 5678
```

- `caminho/do/repositório` é o caminho para o repositório local, caso esteja no diretório do repositório, pode-se usar
  `.`.
- `tarefa/1234` é a branch.
- `-t 5678` é o identificador da tarefa (não se inclui a `#`).

#### Criar descrição de PR com múltiplas tarefas vinculadas a mesma branch

```bash
skyinfo resumir-pr caminho/do/repositório tarefa/1234 -t 5678;8765;4321;1234
```

- `caminho/do/repositório` é o caminho para o repositório local, caso esteja no diretório do repositório, pode-se usar
  `.`.
- `tarefa/1234` é a branch.
- `-t 5678;8765;4321;1234` são os identificadores das tarefas, separadas por `;`.

#### Criar descrição de PR simples especificando o diretório de saída do arquivo MD gerado

```bash
skyinfo resumir-pr caminho/do/repositório tarefa/1234 -o caminho/do/diretório/de/saída
```

- `caminho/do/repositório` é o caminho para o repositório local, caso esteja no diretório do repositório, pode-se usar
  `.`.
- `tarefa/1234` é a branch.
- `-o caminho/do/diretório/de/saída` define o caminho para o diretório de saída do arquivo MD gerado.

### Exemplo de Resumo de PR

```markdown
## Estados afetados:

RS, ou PR, ou Ambos.

## Comportamentos introduzidos, alterados, ou removidos: 

> Descrição de comportamentos introduzidos, alterados, ou removidos. Por exemplo, a criação de um novo endpoint, a alteração de um comportamento de um comando existente, ou a remoção de um serviço.

- Alteração de comportamento do comando X no endpoint de criação de notas fiscais.

## Configuração para habilitar a opção:

Nenhuma.

## Endpoints afetados:

> Lista de endpoints afetados pelo PR. Por exemplo, `/NotasFiscais`, `/Usuarios`, ou `/Itens`.

- `/NotasFiscais`

## Procedimento de teste realizado:

- Testes automatizados executados com sucesso.

## Depende do Pull Request:

Não depende de outro Pull Request.

## Projetos alterados:

> - Alterações: Número de arquivos alterados por commit no projeto.
> - Projeto: Nome do projeto alterado.
> - Tipo: Tipo de projeto alterado (Fonte, Testes, Pacote).

| Alterações | Projeto                                   | Tipo   |
|------------|-------------------------------------------|--------|
| 12         | Um.Projeto.Alterado                       | Fonte  |
| 15         | Um.Projeto.De.Teste.Alterado              | Testes |
| 5          | Um.Projeto.Que.É.Um.Pacote.E.Foi.Alterado | Pacote |

## Pacotes alterados:

> Projetos versionados que foram alterados, portanto, podem não ser pacotes que precisam ser gerados e publicados.

- `Um.Projeto.Que.É.Um.Pacote.E.Foi.Alterado`

```