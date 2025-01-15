# Obter Caminho Relativo

Comando simples que retorna o caminho relativo de um diretório até outro.

-----

## Exemplo

Dada a seguinte estrutura do diretório:

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