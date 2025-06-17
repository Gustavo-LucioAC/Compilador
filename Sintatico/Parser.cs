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

    // NOVO MÉTODO: ParseProgram
    public ProgramNode ParseProgram()
    {
        var program = new ProgramNode();

        while (!Check(TokenType.EOF))
        {
            var stmt = ParseStatement();
            program.Statements.Add(stmt);
        }

        return program;
    }

    public AstNode ParseDeclaration()
    {
        if (!Match(TokenType.Var))
            throw Error("Esperado 'var'");

        Token name = Consume(TokenType.Identifier, "Esperado nome de variável");
        Consume(TokenType.Colon, "Esperado ':'");
        Token type = Consume(TokenType.Identifier, "Esperado tipo da variável");

        AstNode? value = null;
        if (Match(TokenType.Assign))
        {
            value = ParseLiteral();
        }

        Consume(TokenType.Semicolon, "Esperado ';'");
        return new VarDeclaration(name.Value, type.Value, value);
    }

    private AstNode ParseExpression() => ParsePrimary();

    private AstNode ParsePrimary()
    {
        if (Match(TokenType.Identifier))
        {
            string name = Previous().Value;

            if (Match(TokenType.LeftParen))
            {
                var args = new List<AstNode>();
                if (!Check(TokenType.RightParen))
                {
                    do
                    {
                        args.Add(ParseExpression());
                    } while (Match(TokenType.Comma));
                }

                Consume(TokenType.RightParen, "Esperado ')' após os argumentos da função.");
                return new FunctionCallNode(name, args);
            }

            return new IdentifierNode(name);
        }

        throw Error("Esperado expressão primária (literal, identificador ou chamada de função)");
    }

    private AstNode ParseLiteral()
    {
        Token token = Advance();

        return token.Type switch
        {
            TokenType.Number => new LiteralNode(int.Parse(token.Value)),
            TokenType.Float => new LiteralNode(float.Parse(token.Value)),
            TokenType.Boolean => new LiteralNode(bool.Parse(token.Value)),
            TokenType.String => new LiteralNode(token.Value),
            TokenType.Char => new LiteralNode(token.Value[0]),
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

    public AstNode ParseStatement()
    {
        if (Check(TokenType.Var)) return ParseDeclaration();
        if (Check(TokenType.Print)) return ParsePrintStatement();
        if (Check(TokenType.Input)) return ParseInputStatement();
        if (Check(TokenType.If)) return ParseIfStatement();
        if (Check(TokenType.While)) return ParseWhileStatement();
        if (Check(TokenType.For)) return ParseForStatement();
        if (Check(TokenType.Func)) return ParseFunctionDeclaration();

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

    public bool IsAtEnd() => Peek().Type == TokenType.EOF;

    public Token Peek() => _tokens[_position];

    public Token Previous() => _tokens[_position - 1];

    public Exception Error(string message) =>
        new Exception($"Erro na linha {Peek().Line}, coluna {Peek().Column}: {message}");
}