# Clawssets
Clawssets é um compilador de recursos (assets) para a [Claw Engine](../../../engine), baseado no sistema de arquivos.
Este é um projeto pessoal e contribuições de código não são aceitas.

# Como Utilizar
O formato dos assets pode ser encontrado em [Spec](./spec). Os tipos de assets suportados podem ser encontrados em [Docs](./docs).
Para utilizar, basta compilar o executável do projeto em modo Release e executá-lo na pasta em que seus assets estão. Todos os assets serão compilados para uma pasta `.build`, na mesma pasta.

## CLI
```
> /caminho/da/clawssets [/caminho/dos/assets] [-o/--output /copiar/para/1 /copiar/para/2]
```
Para evitar a necessidade de `--output [caminhos]`, você pode criar um arquivo `outputs`, em que cada linha é um caminho (relativo à pasta).
