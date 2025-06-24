Compilador Simples em C#

Este repositório contém a implementação completa de um compilador simples para uma linguagem inspirada em C#/Java, com suporte a:

- Tipos primitivos: int, float, bool, char, string
- Declaração de variáveis: var nome: tipo = valor;
- Entrada e saída: input(var); e print(expr);
- Controle de fluxo: if/else, while, for
- Funções: func nome(params): tipo { ... } com parâmetros tipados e retorno
- Operadores: aritméticos (+, -, *, /, %), relacionais (<, <=, >, >=, ==, !=)

Estrutura do Projeto

/Compilador
│
├─ Compilador.Lexico/
│   ├─ Lexer.cs           Tokenização do input
│   └─ Token.cs           Definição de Token e TokenType
│
├─ Compilador.Sintatico/
│   ├─ Parser.cs          Constrói a AST a partir dos tokens
│   ├─ AstNodes.cs        Definição de nós da AST
│   └─ FunctionSymbol.cs  Símbolo de função para semântica
│
├─ Compilador.Semantico/
│   └─ SemanticAnalyzer.cs  Verificação de tipos, escopos e chamadas
│
├─ Compilador.Interpretador/
│   └─ Interpreter.cs     Executa a AST interpretativamente
│
└─ Program.cs            Entry point (file read or embedded test)

Como Funciona

1. Leitura do código-fonte
   - Se for passado um arquivo .txt como argumento, o programa lê esse arquivo.
   - Caso contrário, usa um bloco de teste embutido.

2. Análise Léxica (Lexer)
   - Divide o texto em tokens (palavras-chave, identificadores, literais, operadores, símbolos).
   - Ignora comentários e espaços em branco.

3. Análise Sintática (Parser)
   - Constrói a AST com nós de declaração, expressão, laços, funções etc.
   - Métodos principais: ParseProgram, ParseStatement, ParseExpression.

4. Análise Semântica (SemanticAnalyzer)
   - Valida escopos e compatibilidade de tipos.
   - Registra funções e variáveis antes de analisar corpo e chamadas.
   - Garante que chamadas de função e operadores usem tipos corretos.

5. Interpretação (Interpreter)
   - Avalia a AST em tempo de execução.
   - Mantém dicionário de variáveis e tabela de funções.
   - Executa print, input, laços, expressões e chamadas de função.

Como Usar

1. Compile:
   dotnet build

2. Execute:
   - Sem argumentos: dotnet run
   - Com arquivo: dotnet run -- caminho/para/program.txt

Formato do program.txt

print("Olá, mundo!");
var x: int = 10;
while (x > 0) {
    print(x);
    x = x - 1;
}
func dobro(n: int): int {
    return n * 2;
}
print(dobro(5));

Exemplo de saída:

Olá, mundo!
10
9
8
7
6
5
4
3
2
1
10

Licença

MIT © 2025
