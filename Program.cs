class Program
{
    static void Main()
    {
        string sourceCode = @"
            var x: int = 5;
            var y: float = 3.2;
            var nome: string = ""João"";
            var ativo: bool = false;

            print(""Nome:"");
            print(nome);

            if (ativo) {
                print(""Usuário está ativo"");
            } else {
                print(""Usuário inativo"");
            }

            var contador: int = 0;
            while (contador < 3) {
                print(""Contador:"");
                print(contador);
                contador = contador + 1;
            }

            func soma(a: int, b: int): int {
                return a + b;
            }

            var resultado: int = soma(10, 20);
            print(""Resultado da soma:"");
            print(resultado);
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
