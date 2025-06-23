using Compilador.Lexico; 
using Compilador.Sintatico;

class Program
{
    static void Main()
    {
        string sourceCode = @"
            var idade: int;
            print(""Digite sua idade:"");
            input(idade);
            print(""Você digitou:"");
            print(idade);
        ";

        var lexer = new Lexer(sourceCode);
        var tokens = lexer.Tokenize();

        var parser = new Parser(tokens);
        var program = parser.ParseProgram();

        // Análise semântica
        var semanticAnalyzer = new SemanticAnalyzer();
        semanticAnalyzer.Analyze(program);

        // Interpretar
        var interpreter = new Interpreter();
        interpreter.Interpret(program);
    }
}
