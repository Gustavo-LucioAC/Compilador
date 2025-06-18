using System.Globalization;
using Compilador.Lexico;
using Compilador.Sintatico;

public class Parser
{
    public readonly List<Token> _tokens;
    public int _position = 0;

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
    }

    public ProgramNode ParseProgram()
    {
        var statements = new List<AstNode>();

        while (!IsAtEnd())
        {
            var stmt = ParseStatement();
            statements.Add(stmt);
        }

        return new ProgramNode(statements);
    }
    public AstNode ParseExpression() => ParseAssignment();
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
            value = ParseExpression();
        }

        Consume(TokenType.Semicolon, "Esperado ';'");
        return new VarDeclaration(name.Value, type.Value, value);
    }

    public AstNode ParseAssignment()
    {
        var expr = ParseEquality();

        if (Match(TokenType.Assign))
        {
            var equals = Previous();
            var value = ParseAssignment();

            if (expr is IdentifierNode id)
            {
                return new AssignmentNode(id.Name, value);
            }

            throw Error("Invalid assignment target.");
        }

        return expr;
    }

    public AstNode ParseEquality()
    {
        var expr = ParseComparison();

        while (Match(TokenType.Equal) || Match(TokenType.NotEqual))
        {
            string op = Previous().Value;
            var right = ParseComparison();
            expr = new BinaryExpression(expr, op, right);
        }

        return expr;
    }

    public AstNode ParseComparison()
    {
        var expr = ParseTerm();

        while (Match(TokenType.LessThan) || Match(TokenType.LessOrEqual) || Match(TokenType.GreaterThan) || Match(TokenType.GreaterOrEqual))
        {
            string op = Previous().Value;
            var right = ParseTerm();
            expr = new BinaryExpression(expr, op, right);
        }

        return expr;
    }

    public AstNode ParseTerm()
    {
        var expr = ParseFactor();

        while (Match(TokenType.Plus) || Match(TokenType.Minus))
        {
            string op = Previous().Value;
            var right = ParseFactor();
            expr = new BinaryExpression(expr, op, right);
        }

        return expr;
    }


    public AstNode ParsePrimary()
    {
        if (Match(TokenType.Number))
            return new LiteralNode(int.Parse(Previous().Value));

        if (Match(TokenType.Float))
            return new LiteralNode(float.Parse(Previous().Value, CultureInfo.InvariantCulture));

        if (Match(TokenType.Char))
            return new LiteralNode(Previous().Value[0]);

        if (Match(TokenType.String))
            return new LiteralNode(Previous().Value);

        if (Match(TokenType.True))
            return new LiteralNode(true);

        if (Match(TokenType.False))
            return new LiteralNode(false);

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

                Consume(TokenType.RightParen, "Esperado ')' após os argumentos.");
                return new FunctionCallNode(name, args);
            }

            return new IdentifierNode(name);
        }

        if (Match(TokenType.LeftParen))
        {
            var expr = ParseExpression();
            Consume(TokenType.RightParen, "Esperado ')' após expressão.");
            return expr;
        }

        throw Error($"Esperado expressão primária (literal, identificador, chamada de função ou expressão entre parênteses)");
    }

    public AstNode ParseUnary()
    {
        if (Match(TokenType.Minus) || Match(TokenType.Plus))
        {
            string op = Previous().Value;
            var right = ParseUnary();
            return new UnaryExpression(op, right);
        }

        return ParsePrimary();
    }
    public AstNode ParseLiteral()
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

    private AstNode ParseReturnStatement()
    {
        var returnToken = Expect(TokenType.Return);
        var expr = ParseExpression();
        Expect(TokenType.Semicolon);
        return new ReturnNode(expr)
        {
            Line = returnToken.Line,
            Column = returnToken.Column
        };
    }

    public AstNode ParsePrintStatement()
    {
        Expect(TokenType.Print);
        Expect(TokenType.LeftParen);
        var expr = ParseExpression();
        Expect(TokenType.RightParen);
        Expect(TokenType.Semicolon);
        return new PrintNode(expr);
    }

    public AstNode ParseInputStatement()
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
        if (Match(TokenType.Var)) return ParseDeclaration();
        if (Match(TokenType.If)) return ParseIfStatement();
        if (Match(TokenType.While)) return ParseWhileStatement();
        if (Match(TokenType.For)) return ParseForStatement();
        if (Match(TokenType.Return)) return ParseReturnStatement();
        if (Match(TokenType.Print)) return ParsePrintStatement();
        
        return ParseExpressionStatement();
    }

    public AstNode ParseIfStatement()
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

    public AstNode ParseFactor()
    {
        var expr = ParseUnary();

        while (Match(TokenType.Multiply) || Match(TokenType.Divide) || Match(TokenType.Modulo))
        {
            string op = Previous().Value;
            var right = ParseUnary();
            expr = new BinaryExpression(expr, op, right);
        }

        return expr;
    }

    public AstNode ParseWhileStatement()
    {
        Expect(TokenType.While);
        Expect(TokenType.LeftParen);
        var condition = ParseExpression();
        Expect(TokenType.RightParen);
        var body = new BlockNode(ParseBlock());
        return new WhileNode(condition, body);
    }

    public AstNode ParseForStatement()
    {
        Consume(TokenType.For, "Esperado 'for'.");

        Consume(TokenType.LeftParen, "Esperado '(' após 'for'.");

        // 1. Inicialização (pode ser declaração var ou expressão ou vazio)
        AstNode? initialization = null;
        if (!Check(TokenType.Semicolon))
        {
            if (Check(TokenType.Var))
            {
                initialization = ParseVarDeclarationInFor();
            }
            else
            {
                initialization = ParseExpression();
            }
        }
        Consume(TokenType.Semicolon, "Esperado ';' após inicialização do for.");

        // 2. Condição (expressão ou vazio)
        AstNode? condition = null;
        if (!Check(TokenType.Semicolon))
        {
            condition = ParseAssignment(); // usar ParseAssignment aqui também
        }
        Consume(TokenType.Semicolon, "Esperado ';' após condição do for.");

        // 3. Incremento (expressão ou vazio)
        AstNode? increment = null;
        if (!Check(TokenType.RightParen))
        {
            increment = ParseAssignment();  // <-- aqui, troque ParseExpression por ParseAssignment
        }
        Consume(TokenType.RightParen, "Esperado ')' após incremento do for.");

        // 4. Corpo do for (bloco)
        var body = ParseBlock();

        return new ForNode(initialization, condition, increment, new BlockNode(body));
    }

    public AstNode ParseVarDeclarationInFor()
    {
        Consume(TokenType.Var, "Esperado 'var'");

        Token name = Consume(TokenType.Identifier, "Esperado nome de variável");
        Consume(TokenType.Colon, "Esperado ':'");
        Token type = Consume(TokenType.Identifier, "Esperado tipo da variável");

        AstNode? value = null;
        if (Match(TokenType.Assign))
        {
            value = ParseAssignment();
        }
        return new VarDeclaration(name.Value, type.Value, value);
    }

    public AstNode ParseExpressionStatement()
    {
        var expr = ParseExpression();
        Expect(TokenType.Semicolon);
        return new ExpressionStatementNode(expr);
    }

    public AstNode ParseFunctionDeclaration()
    {
        Expect(TokenType.Func);
        
        var returnType = Expect(TokenType.Identifier).Value; // pega "int"
        var name = Expect(TokenType.Identifier).Value;        // pega "soma"
        
        Expect(TokenType.LeftParen);

        var parameters = new List<Parameter>();
        if (!Match(TokenType.RightParen))
        {
            do
            {
                var paramType = Expect(TokenType.Identifier).Value; // pega "int"
                var paramName = Expect(TokenType.Identifier).Value; // pega "a"
                parameters.Add(new Parameter(paramName, paramType));
            } while (Match(TokenType.Comma));
        }

        Expect(TokenType.RightParen);
        var body = new BlockNode(ParseBlock());
        return new FunctionNode(name, parameters, returnType, body);
    }

    public List<AstNode> ParseBlock()
    {
        Expect(TokenType.LeftBrace);
        var statements = new List<AstNode>();
        while (!Match(TokenType.RightBrace))
        {
            statements.Add(ParseStatement());
        }
        return statements;
    }

    public bool Match(TokenType type)
    {
        if (Check(type))
        {
            Advance();
            return true;
        }
        return false;
    }

    public Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();
        throw Error(message);
    }

    public bool Check(TokenType type)
    {
        if (IsAtEnd() && type != TokenType.EOF) return false;
        return Peek().Type == type;
    }

    public Token Advance()
    {
        if (!IsAtEnd()) _position++;
        return Previous();
    }

    public Token Expect(TokenType expectedType)
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