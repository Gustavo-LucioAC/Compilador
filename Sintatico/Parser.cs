using System.Globalization;
using Compilador.Lexico;
using Compilador.Sintatico;

public class Parser
{
    private readonly List<Token> _tokens;
    private int _position;

    public Parser(List<Token> tokens)
    {
        _tokens = tokens;
        _position = 0;
    }

    private Token Current => _position < _tokens.Count ? _tokens[_position] : _tokens[^1];
    
    private Token Advance()
    {
        if (_position < _tokens.Count)
            return _tokens[_position++];
        return _tokens[^1]; // Retorna EOF token se passar do fim
    }

    private bool Match(params TokenType[] types)
    {
        if (types.Contains(Current.Type))
        {
            Advance();
            return true;
        }
        return false;
    }

    private Token? MatchToken(params TokenType[] types)
    {
        if (types.Contains(Current.Type))
        {
            return Advance();
        }
        return null;
    }

    private Token Expect(TokenType type, string error)
    {
        if (Current.Type == type)
            return Advance();

        throw new Exception($"Erro na linha {Current.Line}: {error}, encontrado '{Current.Value}'");
    }

    public ProgramNode ParseProgram()
    {
        var program = new ProgramNode();

        while (Current.Type != TokenType.EOF)
        {
            var stmt = ParseStatement();
            if (stmt != null)
                program.Statements.Add(stmt);
        }

        return program;
    }

    private StatementNode ParseStatement()
    {
        return Current.Type switch
        {
            TokenType.Var => ParseVariableDeclaration(),
            TokenType.Print => ParsePrint(),
            TokenType.Input => ParseInput(),
            TokenType.If => ParseIf(),
            TokenType.While => ParseWhile(),
            TokenType.Identifier when Peek().Type == TokenType.Assign => ParseAssignment(),
            _ => throw new Exception($"Comando inesperado na linha {Current.Line}: '{Current.Value}'")
        };
    }

    private VariableDeclarationNode ParseVariableDeclaration()
    {
        Expect(TokenType.Var, "Esperado 'var'");
        var name = Expect(TokenType.Identifier, "Esperado nome da variável").Value;
        Expect(TokenType.Colon, "Esperado ':' após nome da variável");

        var typeToken = MatchToken(TokenType.Int, TokenType.FloatType, TokenType.CharType, TokenType.BoolType, TokenType.StringType);
        if (typeToken == null)
            throw new Exception("Esperado tipo da variável");

        string type = typeToken.Value;

        ExpressionNode? initializer = null;
        if (Match(TokenType.Assign))
        {
            initializer = ParseExpression();
        }

        Expect(TokenType.Semicolon, "Esperado ';' após declaração");

        return new VariableDeclarationNode { Name = name, Type = type, Initializer = initializer };
    }

    private PrintNode ParsePrint()
    {
        Expect(TokenType.Print, "Esperado 'print'");
        Expect(TokenType.LeftParen, "Esperado '(' após print");
        var expr = ParseExpression();
        Expect(TokenType.RightParen, "Esperado ')' após expressão");
        Expect(TokenType.Semicolon, "Esperado ';' após print");
        return new PrintNode { Expression = expr };
    }

    private InputNode ParseInput()
    {
        Expect(TokenType.Input, "Esperado 'input'");
        Expect(TokenType.LeftParen, "Esperado '(' após input");
        var variable = Expect(TokenType.Identifier, "Esperado nome da variável");
        Expect(TokenType.RightParen, "Esperado ')' após variável");
        Expect(TokenType.Semicolon, "Esperado ';' após input");
        return new InputNode { VariableName = variable.Value };
    }

    private IfNode ParseIf()
    {
        Expect(TokenType.If, "Esperado 'if'");
        Expect(TokenType.LeftParen, "Esperado '(' após 'if'");
        var condition = ParseExpression();
        Expect(TokenType.RightParen, "Esperado ')' após condição");

        var thenBlock = ParseBlock();

        List<StatementNode>? elseBlock = null;
        if (Match(TokenType.Else))
        {
            elseBlock = ParseBlock();
        }

        return new IfNode
        {
            Condition = condition,
            ThenBlock = thenBlock,
            ElseBlock = elseBlock
        };
    }

    private WhileNode ParseWhile()
    {
        Expect(TokenType.While, "Esperado 'while'");
        Expect(TokenType.LeftParen, "Esperado '(' após 'while'");
        var condition = ParseExpression();
        Expect(TokenType.RightParen, "Esperado ')' após condição");
        var body = ParseBlock();
        return new WhileNode { Condition = condition, Body = body };
    }

    private AssignmentNode ParseAssignment()
    {
        var name = Expect(TokenType.Identifier, "Esperado nome de variável").Value;
        Expect(TokenType.Assign, "Esperado '=' na atribuição");
        var value = ParseExpression();
        Expect(TokenType.Semicolon, "Esperado ';' após atribuição");
        return new AssignmentNode { Name = name, Value = value };
    }

    private List<StatementNode> ParseBlock()
    {
        Expect(TokenType.LeftBrace, "Esperado '{'");
        var statements = new List<StatementNode>();

        while (Current.Type != TokenType.RightBrace && Current.Type != TokenType.EOF)
        {
            statements.Add(ParseStatement());
        }

        Expect(TokenType.RightBrace, "Esperado '}'");
        return statements;
    }

    private ExpressionNode ParseExpression()
    {
        return ParseEquality();
    }

    private ExpressionNode ParseEquality()
    {
        var expr = ParseComparison();

        while (Match(TokenType.Equal, TokenType.NotEqual))
        {
            string op = _tokens[_position - 1].Value;
            var right = ParseComparison();
            expr = new BinaryExpressionNode { Left = expr, Operator = op, Right = right };
        }

        return expr;
    }

    private ExpressionNode ParseComparison()
    {
        var expr = ParseTerm();

        while (Match(TokenType.LessThan, TokenType.LessOrEqual, TokenType.GreaterThan, TokenType.GreaterOrEqual))
        {
            string op = _tokens[_position - 1].Value;
            var right = ParseTerm();
            expr = new BinaryExpressionNode { Left = expr, Operator = op, Right = right };
        }

        return expr;
    }

    private ExpressionNode ParseTerm()
    {
        var expr = ParseFactor();

        while (Match(TokenType.Plus, TokenType.Minus))
        {
            string op = _tokens[_position - 1].Value;
            var right = ParseFactor();
            expr = new BinaryExpressionNode { Left = expr, Operator = op, Right = right };
        }

        return expr;
    }

    private ExpressionNode ParseFactor()
    {
        var expr = ParseUnary();

        while (Match(TokenType.Multiply, TokenType.Divide, TokenType.Modulo))
        {
            string op = _tokens[_position - 1].Value;
            var right = ParseUnary();
            expr = new BinaryExpressionNode { Left = expr, Operator = op, Right = right };
        }

        return expr;
    }

    private ExpressionNode ParseUnary()
    {
        if (Match(TokenType.Minus))
        {
            string op = "-";
            var right = ParsePrimary();
            return new BinaryExpressionNode { Left = new LiteralNode { Value = 0 }, Operator = op, Right = right };
        }

        return ParsePrimary();
    }

    private ExpressionNode ParsePrimary()
    {
        var token = Advance();

        return token.Type switch
        {
            TokenType.Number => new LiteralNode { Value = int.Parse(token.Value, CultureInfo.InvariantCulture) },
            TokenType.Float => new LiteralNode { Value = double.Parse(token.Value, CultureInfo.InvariantCulture) },
            TokenType.Char => new LiteralNode { Value = token.Value[0] },
            TokenType.String => new LiteralNode { Value = token.Value },
            TokenType.Boolean => new LiteralNode { Value = token.Value == "true" },
            TokenType.Identifier => new IdentifierNode { Name = token.Value },
            TokenType.LeftParen => ParseGrouped(),
            _ => throw new Exception($"Expressão inválida na linha {token.Line}")
        };
    }

    private ExpressionNode ParseGrouped()
    {
        var expr = ParseExpression();
        Expect(TokenType.RightParen, "Esperado ')' após expressão");
        return expr;
    }

    private Token Peek() =>
        _position + 1 < _tokens.Count ? _tokens[_position + 1] : _tokens[^1];
}
