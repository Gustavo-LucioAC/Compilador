using Compilador.Interpreter;
using Compilador.Lexico;
using Compilador.Sintatico;

class Program
{
    static void Main(string[] args)
    {
            string codigoFonte = @"
            var x: int = 42;
            print(x);
        ";

        var lexer = new Lexer(codigoFonte);
        var tokens = new List<Token>();
        Token token;
        do
        {
            token = lexer.NextToken();
            tokens.Add(token);
        } while (token.Type != TokenType.EOF);

        var parser = new Parser(tokens);
        var ast = new List<AstNode>();
        while (!parser.IsAtEnd())
        {
            ast.Add(parser.ParseStatement());
        }

        var interpreter = new Interpreter();
        //interpreter.Execute(ast);
    }
}