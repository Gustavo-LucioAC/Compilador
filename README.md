# Compilador

Este projeto é um compilador simples com análise léxica, sintática, semântica e interpretação de código, desenvolvido em C#.

## Estrutura do Projeto

Compilador/ ├── Compilador.csproj ├── Program.cs ├── Interpreter/ │ └── Interpreter.cs ├── Lexico/ │ ├── Lexer.cs │ ├── Token.cs │ └── TokenType.cs ├── SemanticAnalyzer/ │ └── SemanticAanalyzer.cs ├── Sintatico/ │ ├── Node.cs │ └── Parser.cs └── ...

## Componentes

### Léxico (`Lexico/`)
- **[`Lexer`](Lexico/Lexer.cs)**: Responsável por transformar o código fonte em uma lista de tokens.
- **[`Token`](Lexico/Token.cs)**: Representa um token identificado pelo lexer.
- **[`TokenType`](Lexico/TokenType.cs)**: Enumeração dos tipos de tokens reconhecidos.

### Sintático (`Sintatico/`)
- **[`Parser`](Sintatico/Parser.cs)**: Constrói a árvore sintática abstrata (AST) a partir dos tokens.
- **[`Node`](Sintatico/Node.cs)**: Define os nós da AST, incluindo comandos, expressões, declarações, etc.

### Semântico (`SemanticAnalyzer/`)
- **[`SemanticAnalyzer`](SemanticAnalyzer/SemanticAanalyzer.cs)**: Realiza a análise semântica, verificando tipos, declarações e escopos.
- **`Symbol` e `Scope`**: Gerenciam símbolos e escopos para a análise semântica.

### Interpretador (`Interpreter/`)
- **[`Interpreter`](Interpreter/Interpreter.cs)**: Executa o programa representado pela AST, simulando a execução do código fonte.

### Principal
- **[`Program`](Program.cs)**: Ponto de entrada do projeto. Faz a integração entre as etapas: léxica, sintática, semântica e interpretação.

## Exemplo de Código Suportado

```cs
var idade: int;
print("Digite sua idade:");
input(idade);
print("Você digitou:");
print(idade);
```


Como Executar
Requisitos: .NET 9.0 ou superior.
1. Compilar:
```cs
dotnet build
```
2. Executar:
```cs
dotnet run
```

## Funcionalidades Suportadas
* Declaração de variáveis com tipos (int, float, char, bool, string)
* Atribuição e inicialização de variáveis
* Comandos de entrada (input) e saída (print)
* Estruturas de controle: if, else, while
* Expressões aritméticas e booleanas
* Análise semântica de tipos e escopos
* Extensões Futuras
* Suporte a funções e procedimentos
* Estruturas de repetição adicionais (for)
* Análise e tratamento de erros mais detalhados
* Geração de código intermediário
