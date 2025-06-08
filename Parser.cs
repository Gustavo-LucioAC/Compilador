using System.Collections.Generic;
using Compilador.Lexico;

public class Parser
{
    private readonly List<Token> _tokens;
    private int _position = 0;

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    public AstNode? ParseDeclaration()
    {
        // var
        if (!Match(TokenType.Var))
            throw Error("Esperado 'var'");

        // <identificador>
        Token name = Consume(TokenType.Identifier, "Esperado nome de variável");

        // :
        Consume(TokenType.Colon, "Esperado ':'");

        // <tipo>
        Token type = Consume(TokenType.Identifier, "Esperado tipo da variável");

        // = ou ; (valor opcional)
        AstNode? value = null;
        if (Match(TokenType.Assign))
        {
            value = ParseLiteral();
        }

        // ;
        Consume(TokenType.Semicolon, "Esperado ';'");

        return new VarDeclaration(name.Value, type.Value, value);
    }

    private AstNode ParseLiteral()
    {
        Token token = Advance();

        return token.Type switch
        {
            TokenType.Number or TokenType.Float or TokenType.String
            or TokenType.Char or TokenType.Boolean => new Literal(token.Value),

            _ => throw Error("Esperado literal"),
        };
    }

    // Utilitários
    private bool Match(TokenType type)
    {
        if (Check(type))
        {
            Advance();
            return true;
        }
        return false;
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();
        throw Error(message);
    }

    private bool Check(TokenType type)
    {
        if (IsAtEnd()) return false;
        return Peek().Type == type;
    }

    private Token Advance()
    {
        if (!IsAtEnd()) _position++;
        return Previous();
    }

    private bool IsAtEnd() => Peek().Type == TokenType.EOF;

    private Token Peek() => _tokens[_position];

    private Token Previous() => _tokens[_position - 1];

    private Exception Error(string message) =>
        new Exception($"Erro na linha {Peek().Line}, coluna {Peek().Column}: {message}");
}
