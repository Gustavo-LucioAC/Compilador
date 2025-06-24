# Compilador

Este projeto é um compilador simples em C# com as etapas de:
- Análise Léxica
- Análise Sintática
- Análise Semântica
- Interpretação de código

## Estrutura do Projeto

```
Compilador/
│
├── Compilador.csproj
├── Program.cs
│
├── Lexico/
│   ├── Lexer.cs
│   ├── Token.cs
│   └── TokenType.cs
│
├── Sintatico/
│   ├── Parser.cs
│   ├── AstNodes.cs
│   └── FunctionSymbol.cs
│
├── SemanticAnalyzer/
│   └── SemanticAnalyzer.cs
│
└── Interpreter/
    └── Interpreter.cs
```

## Componentes

### Léxico (`Lexico/`)
- **Lexer.cs**: converte o código-fonte em uma sequência de tokens.
- **Token.cs**: define a estrutura de um token.
- **TokenType.cs**: enumeração de tipos de tokens (palavras-chave, operadores, literais etc.).

### Sintático (`Sintatico/`)
- **Parser.cs**: gera a AST (Abstract Syntax Tree) a partir dos tokens.
- **AstNodes.cs**: classes que representam nós de declaração, expressão, laços, funções.
- **FunctionSymbol.cs**: modelo de símbolo de função para a análise semântica.

### Semântico (`SemanticAnalyzer/`)
- **SemanticAnalyzer.cs**: verifica tipos, compatibilidade de expressões, escopos e declarações.
- **Symbol & Scope**: gerenciam o contexto de variáveis e funções.

### Interpretador (`Interpreter/`)
- **Interpreter.cs**: executa a AST interpretativamente, mantendo estado de variáveis e funções.

### Ponto de Entrada
- **Program.cs**: orquestra o fluxo completo:
  1. Leitura do código-fonte (arquivo `.txt` ou teste embutido)
  2. Tokenização (Lexer)
  3. Parsing (Parser)
  4. Análise Semântica (SemanticAnalyzer)
  5. Interpretação (Interpreter)

## Funcionalidades Suportadas

- Declaração de variáveis tipadas (`var nome: tipo = valor;`)
- Literais: números inteiros e float, strings, chars, booleanos
- Atribuição e inicialização
- Entrada (`input`) e saída (`print`)
- Controle de fluxo: `if/else`, `while`, `for`
- Operadores aritméticos (`+ - * / %`)
- Operadores relacionais (`< <= > >= == !=`)
- Funções com parâmetros e retorno (`func nome(params): tipo { ... }`)

## Como Utilizar

1. **Pré-requisitos**  
   - .NET 6.0 ou superior instalado.

2. **Compilação**  
   ```bash
   dotnet build
   ```

3. **Execução**  
   - Sem argumentos:  
     ```bash
     dotnet run
     ```
     usa o código de teste interno.  
   - Com arquivo de entrada:  
     ```bash
     dotnet run -- caminho/para/program.txt
     ```

4. **Exemplo de `program.txt`**  
   ```
   print("Olá, mundo!");
   var x: int = 3;
   while (x > 0) {
       print(x);
       x = x - 1;
   }

   func dobro(n: int): int {
       return n * 2;
   }

   print(dobro(x));
   ```

## Exemplo de Saída

```
Olá, mundo!
3
2
1
6
```
Gustavo Lúcio Alves Cruz - 1221116563
Beatriz Silva Murta - RA: 1221127974

MIT © 2025
