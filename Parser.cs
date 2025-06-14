using Compilador.Lexico;
using Compilador.Sintatico;

public class Parser
{
    private readonly List<Token> _tokens;
    private int _position = 0;

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    public AstNode ParseDeclaration()
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

    private AstNode ParseExpression()
    {
        return ParseLiteral();
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

    private AstNode ParsePrintStatement()
    {
        Expect(TokenType.Print);
        Expect(TokenType.LeftParen);
        var expr = ParseExpression();
        Expect(TokenType.RightParen);
        Expect(TokenType.Semicolon);
        return new PrintNode(expr);
    }

    private AstNode ParseInputStatement()
    {
        Expect(TokenType.Input);
        Expect(TokenType.LeftParen);
        var variable = Expect(TokenType.Identifier);
        Expect(TokenType.RightParen);
        Expect(TokenType.Semicolon);
        return new InputNode(variable.Value);
    }

    private AstNode ParseStatement()
    {
        if (Check(TokenType.Var))
            return ParseDeclaration();

        if (Check(TokenType.Print))
            return ParsePrintStatement();

        if (Check(TokenType.Input))
            return ParseInputStatement();

        if (Check(TokenType.If))
            return ParseIfStatement();

        if (Check(TokenType.While))
            return ParseWhileStatement();

        if (Check(TokenType.For))
            return ParseForStatement();

        if (Check(TokenType.Func))
            return ParseFunctionDeclaration();

        throw Error("Esperado uma declaração ou instrução");
    }

    private AstNode ParseIfStatement()
    {
        Expect(TokenType.If);
        Expect(TokenType.LeftParen);
        var condition = ParseExpression();
        Expect(TokenType.RightParen);
        
        var thenBlock = new BlockNode(ParseBlock());

        BlockNode? elseBlock = null;
        if (Match(TokenType.Else))
        {
            elseBlock = new BlockNode(ParseBlock());
        }

        return new IfNode(condition, thenBlock, elseBlock);
    }


    private AstNode ParseWhileStatement()
    {
        Expect(TokenType.While);
        Expect(TokenType.LeftParen);
        var condition = ParseExpression();
        Expect(TokenType.RightParen);
        var body = new BlockNode(ParseBlock());
        return new WhileNode(condition, body);
    }

    private AstNode ParseForStatement()
    {
        Expect(TokenType.For);
        Expect(TokenType.LeftParen);
        var init = ParseStatement();
        var condition = ParseExpression();
        Expect(TokenType.Semicolon);
        var increment = ParseStatement();
        Expect(TokenType.RightParen);
        var body = new BlockNode(ParseBlock());
        return new ForNode(init, condition, increment, body);
    }

    private AstNode ParseFunctionDeclaration()
    {
        Expect(TokenType.Func);
        var name = Expect(TokenType.Identifier).Value;
        Expect(TokenType.LeftParen);

        var parameters = new List<Compilador.Sintatico.Parameter>();
        if (!Match(TokenType.RightParen))
        {
            do
            {
                var paramName = Expect(TokenType.Identifier).Value;
                Expect(TokenType.Colon);
                var paramType = Expect(TokenType.Identifier).Value;
                parameters.Add(new Compilador.Sintatico.Parameter(paramName, paramType));
            } while (Match(TokenType.Comma));

            Expect(TokenType.RightParen);
        }

        Expect(TokenType.Colon);
        var returnType = Expect(TokenType.Identifier).Value;
        var body = new BlockNode(ParseBlock());

        return new FunctionNode(name, parameters, returnType, body);
    }

    private List<AstNode> ParseBlock()
    {
        Expect(TokenType.LeftBrace);
        var statements = new List<AstNode>();
        while (!Match(TokenType.RightBrace))
        {
            statements.Add(ParseStatement());
        }
        return statements;
    }

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

    private Token Expect(TokenType expectedType)
    {
        Token token = Advance();
        if (token.Type != expectedType)
        {
            throw new Exception($"Esperado: {expectedType}, mas encontrado: {token.Type} (Valor: '{token.Value}', Linha: {token.Line}, Coluna: {token.Column})");
        }
        return token;
    }

    private bool IsAtEnd() => Peek().Type == TokenType.EOF;

    private Token Peek() => _tokens[_position];

    private Token Previous() => _tokens[_position - 1];

    private Exception Error(string message) =>
        new Exception($"Erro na linha {Peek().Line}, coluna {Peek().Column}: {message}");
}
