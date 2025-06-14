namespace Compilador.Lexico
{
    public enum TokenType
    {
        // Palavras-chave
        If,             // if
        Else,           // else
        While,          // while
        For,            // for
        Return,         // return

        // Identificadores e literais
        Identifier,     // [a-zA-Z_][a-zA-Z0-9_]*
        Number,         // [0-9]+
        Float,       // [0-9]+\.[0-9]+
        Char,       // 'c'
        Boolean,       // true | false
        String,         // ".*?"
        Var,           // var

        // Operadores aritméticos e relacionais
        Plus,           // +
        Minus,          // -
        Multiply,       // *
        Divide,         // /
        Assign,         // =
        Equal,          // ==
        NotEqual,       // !=
        LessThan,       // <
        GreaterThan,    // >
        LessOrEqual,    // <=
        GreaterOrEqual, // >=

        // Delimitadores
        LeftParen,      // (
        RightParen,     // )
        LeftBrace,      // {
        RightBrace,     // }
        Comma,          // ,
        Semicolon,      // ;
        Print,       // print
        Input,      // input
        Func,       // func
        True,       // true
        False,      // false
        Colon, // :

        // Fim de arquivo
        EOF,            // End of file

        // Token inválido
        Invalid,        // Unrecognized token
    }
}
