class Program
{
    static void Main(string[] args)
    {
        string sourceCode;

        if (args.Length > 0 && File.Exists(args[0]))
        {
            sourceCode = File.ReadAllText(args[0]);
        }
        else
        {
            sourceCode = @"
                print(""Digite um número inteiro:"");
                var inteiro: int;
                input(inteiro);

                print(""Digite um número real:"");
                var real: float;
                input(real);

                func somar(a: float, b: float): float {
                    var resultado: float = a + b;
                    return resultado;
                }

                var soma: float = somar(inteiro, real);
                print(""Resultado da soma:"");
                print(soma);

                var texto: string = ""Soma = "";
                print(texto);
                print(soma);

                print(""Contagem regressiva com while:"");
                var count: int = 5;
                while (count > 0) {
                    print(count);
                    count = count - 1;
                }

                print(""Multiplicação com for:"");
                for (var i: int = 1; i <= 5; i = i + 1;) {
                    print(i * 2);
                }
                ";
        }

        var lexer = new Lexer(sourceCode);
        var tokens = lexer.Tokenize();
        foreach (var t in tokens)
            Console.WriteLine($"[{t.Line},{t.Column}] {t.Type,-12} '{t.Value}'");

        var parser = new Parser(tokens);
        var program = parser.ParseProgram();

        var semanticAnalyzer = new SemanticAnalyzer();
        semanticAnalyzer.Analyze(program);

        var interpreter = new Interpreter();
        interpreter.Interpret(program);
    }
}