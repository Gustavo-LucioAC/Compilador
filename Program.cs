using Compilador.Interpreter;
using Compilador.Lexico;
using Compilador.Sintatico;
using System;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Digite o caminho do arquivo de código-fonte (.txt):");

        string? input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input))
        {
            Console.WriteLine("Caminho inválido.");
            return;
        }
        string path = input;

        string codigo = File.ReadAllText(path);

        try
        {
            var lexer = new Lexer(codigo);
            var tokens = new List<Token>();
            Token token;
            do
            {
                token = lexer.NextToken();
                tokens.Add(token);
            } while (token.Type != TokenType.EOF);

            foreach (var t in tokens)
            Console.WriteLine($"{t.Type} - '{t.Value}' (linha {t.Line}, coluna {t.Column})");

            var parser = new Parser(tokens);

            var programNode = parser.ParseProgram();
            var interpreter = new Interpreter();
            interpreter.Execute(programNode);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Erro ao interpretar o código: {ex.Message}");
        }
    }
}