using Compilador.Lexico;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Digite o código para analisar ou pressione Enter para usar o exemplo padrão:");
        string input = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(input))
        {
            input = @"
                var a: int = 42;
                var pi: float = 3.14;
                var c: char = 'x';
                var flag: bool = true;
                var nome: string = ""João"";
            ";
        }

        Lexer lexer = new Lexer(input);

        Console.WriteLine("\nTokens encontrados:\n");

        Token token;
        do
        {
            token = lexer.NextToken();
            Console.WriteLine(token);
        }
        while (token.Type != TokenType.EOF);
    }
}