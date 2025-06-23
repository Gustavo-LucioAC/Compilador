using Compilador.Lexico; 
using Compilador.Sintatico;

class Program
{
    static void Main()
    {
        string sourceCode = @"
            var x: int = 10;
            var y: int = 20;
            var z: int;
            z = x + y * 2;

            print(z);   // Deve imprimir 50

            if (z > 30) {
                print(""z é maior que 30"");
            } else {
                print(""z é menor ou igual a 30"");
            }

            var count: int = 0;
            while (count < 3) {
                print(count);
                count = count + 1;
            }
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
