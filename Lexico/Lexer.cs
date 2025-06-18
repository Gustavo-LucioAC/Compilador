using Compilador.Lexico;

public class Lexer
{
    private readonly string _input;
    private int _position;
    private int _line;
    private int _column;

    public Lexer(string input)
    {
        _input = input;
        _position = 0;
        _line = 1;
        _column = 1;
    }

    public Token NextToken()
    {
        SkipWhitespaceAndComments();

        if (_position >= _input.Length)
            return new Token(TokenType.EOF, "", _line, _column);

        char current = Peek();

        if (char.IsDigit(current))
            return ReadNumber();

        if (char.IsLetter(current) || current == '_')
            return ReadIdentifier();

        if (current == '\'')
            return ReadChar();

        if (current == '"')
            return ReadString();

        return ReadSymbolOrOperator();
    }

    private void SkipWhitespaceAndComments()
    {
        while (_position < _input.Length)
        {
            if (char.IsWhiteSpace(Peek()))
            {
                if (Peek() == '\n')
                {
                    _line++;
                    _column = 1;
                }
                else
                {
                    _column++;
                }
                _position++;
            }
            else if (Peek() == '/' && PeekOrNull() == '/')
            {
                while (_position < _input.Length && Peek() != '\n')
                {
                    _position++;
                    _column++;
                }
            }
            else if (Peek() == '/' && PeekOrNull() == '*')
            {
                _position += 2;
                _column += 2;
                while (_position < _input.Length &&
                       !(Peek() == '*' && PeekOrNull() == '/'))
                {
                    if (Peek() == '\n')
                    {
                        _line++;
                        _column = 1;
                    }
                    else
                    {
                        _column++;
                    }
                    _position++;
                }
                if (_position < _input.Length)
                {
                    _position += 2;
                    _column += 2;
                }
            }
            else break;
        }
    }

    private Token ReadNumber()
    {
        int startColumn = _column;
        int start = _position;
        bool hasDot = false;

        while (_position < _input.Length &&
              (char.IsDigit(Peek()) || (Peek() == '.' && !hasDot)))
        {
            if (Peek() == '.')
                hasDot = true;

            _position++;
            _column++;
        }

        string value = _input.Substring(start, _position - start);
        TokenType type = hasDot ? TokenType.Float : TokenType.Number;

        return new Token(type, value, _line, startColumn);
    }

    private Token ReadIdentifier()
    {
        int startColumn = _column;
        int start = _position;

        while (_position < _input.Length && (char.IsLetterOrDigit(Peek()) || Peek() == '_'))
        {
            _position++;
            _column++;
        }

        string value = _input.Substring(start, _position - start);
        TokenType type = value switch
        {
            "if" => TokenType.If,
            "else" => TokenType.Else,
            "while" => TokenType.While,
            "for" => TokenType.For,
            "return" => TokenType.Return,
            "var" => TokenType.Var,
            "print" => TokenType.Print,
            "input" => TokenType.Input,
            "func" => TokenType.Func,
            "true" or "false" => TokenType.Boolean,
            _ => TokenType.Identifier
        };

        return new Token(type, value, _line, startColumn);
    }

    private Token ReadString()
    {
        int startColumn = _column;
        _position++; _column++; // skip opening "

        int start = _position;
        var sb = new System.Text.StringBuilder();
        while (_position < _input.Length && Peek() != '"')
        {
            if (Peek() == '\\' && PeekOrNull() == '"')
            {
                sb.Append('"');
                _position += 2;
                _column += 2;
            }
            else
            {
                if (Peek() == '\n')
                {
                    _line++;
                    _column = 1;
                }
                else
                {
                    _column++;
                }

                sb.Append(Peek());
                _position++;
            }
        }

        if (_position >= _input.Length)
            return new Token(TokenType.Invalid, "String não terminada", _line, startColumn);

        _position++; _column++; // skip closing "

        return new Token(TokenType.String, sb.ToString(), _line, startColumn);
    }

    private Token ReadChar()
    {
        int startColumn = _column;
        _position++; _column++; // skip opening '

        if (_position >= _input.Length || Peek() == '\n')
            return new Token(TokenType.Invalid, "Char mal formado", _line, startColumn);

        char valueChar = Peek();
        _position++; _column++;

        if (_position >= _input.Length || Peek() != '\'')
            return new Token(TokenType.Invalid, "Char mal formado", _line, startColumn);

        _position++; _column++; // skip closing '

        return new Token(TokenType.Char, valueChar.ToString(), _line, startColumn);
    }

    private Token ReadSymbolOrOperator()
    {
        int startColumn = _column;
        char current = Advance();
        char? next = PeekOrNull();

        return (current, next) switch
        {
            ('=', '=') => CreateTwoCharToken(TokenType.Equal, "=="),
            ('!', '=') => CreateTwoCharToken(TokenType.NotEqual, "!="),
            ('<', '=') => CreateTwoCharToken(TokenType.LessOrEqual, "<="),
            ('>', '=') => CreateTwoCharToken(TokenType.GreaterOrEqual, ">="),
            ('+', _) => new Token(TokenType.Plus, "+", _line, startColumn),
            ('-', _) => new Token(TokenType.Minus, "-", _line, startColumn),
            ('*', _) => new Token(TokenType.Multiply, "*", _line, startColumn),
            ('/', _) => new Token(TokenType.Divide, "/", _line, startColumn),
            ('%', _) => new Token(TokenType.Modulo, "%", _line, startColumn),
            ('=', _) => new Token(TokenType.Assign, "=", _line, startColumn),
            ('<', _) => new Token(TokenType.LessThan, "<", _line, startColumn),
            ('>', _) => new Token(TokenType.GreaterThan, ">", _line, startColumn),
            ('(', _) => new Token(TokenType.LeftParen, "(", _line, startColumn),
            (')', _) => new Token(TokenType.RightParen, ")", _line, startColumn),
            ('{', _) => new Token(TokenType.LeftBrace, "{", _line, startColumn),
            ('}', _) => new Token(TokenType.RightBrace, "}", _line, startColumn),
            (',', _) => new Token(TokenType.Comma, ",", _line, startColumn),
            (';', _) => new Token(TokenType.Semicolon, ";", _line, startColumn),
            (':', _) => new Token(TokenType.Colon, ":", _line, startColumn),

            _ => new Token(TokenType.Invalid, current.ToString(), _line, startColumn),
        };
    }

    private Token CreateTwoCharToken(TokenType type, string symbol)
    {
        _position++; _column++; // consume second character
        return new Token(type, symbol, _line, _column - 2);
    }

    private char Peek() => _input[_position];

    private char? PeekOrNull() =>
        _position + 1 < _input.Length ? _input[_position + 1] : (char?)null;

    private char Advance()
    {
        char c = _input[_position];
        _position++;
        _column++;
        return c;
    }

    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();

        while (true)
        {
            Token token = NextToken();
            tokens.Add(token);

            if (token.Type == TokenType.EOF || token.Type == TokenType.Invalid)
                break;
        }

        return tokens;
    }
}