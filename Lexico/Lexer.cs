using Compilador;
using Compilador.Lexico;
using System.Collections.Generic;

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
        SkipWhitespace();

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

    private void SkipWhitespace()
    {
        while (_position < _input.Length && char.IsWhiteSpace(Peek()))
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
            "true" => TokenType.Boolean,
            "false" => TokenType.Boolean,
            _ => TokenType.Identifier
        };

        return new Token(type, value, _line, startColumn);
    }

    private Token ReadString()
    {
        int startColumn = _column;
        _position++; // skip "
        _column++;

        int start = _position;

        while (_position < _input.Length && Peek() != '"')
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

        if (_position >= _input.Length)
            return new Token(TokenType.Invalid, "String não terminada", _line, _column);

        string value = _input.Substring(start, _position - start);
        _position++; // skip closing "
        _column++;

        return new Token(TokenType.String, value, _line, startColumn);
    }
    
    private Token ReadChar()
    {
        int startColumn = _column;
        _position++; _column++;
        if (_position >= _input.Length || Peek() == '\n')
            return new Token(TokenType.Invalid, "Caractere mal formado", _line, startColumn);

        char valueChar = Peek();
        _position++; _column++;

        if (_position >= _input.Length || Peek() != '\'')
            return new Token(TokenType.Invalid, "Char mal formado", _line, startColumn);

        _position++; _column++;

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
        _position++;
        _column++;
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
}
