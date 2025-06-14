using System;
using System.Collections.Generic;
using Compilador.Lexico;
using Compilador.Sintatico;
using Compilador.AST;

Console.WriteLine("Digite o código para analisar ou pressione Enter para usar o exemplo padrão:");
string input = Console.ReadLine();

if (string.IsNullOrWhiteSpace(input))
{
    input = @"
                func soma(a: int, b: int) : int {
                    return a + b;
                }

                var resultado: int = 0;

                for (var i: int = 0; i < 5; i = i + 1) {
                    resultado = soma(resultado, i);
                    print(resultado);
                }
            ";
}

// Inicializa o Lexer
Lexer lexer = new Lexer(input);

Console.WriteLine("\nTokens encontrados:\n");
List<Token> tokens = new List<Token>();
Token token;
do
{
    token = lexer.NextToken();
    tokens.Add(token);
    Console.WriteLine(token);
} while (token.Type != TokenType.EOF);

Console.WriteLine("\nAnalisando sintaxe e gerando AST...\n");

try
{
    // Inicializa o Parser com os tokens coletados
    Parser parser = new Parser(tokens);

    List<AstNode> ast = new List<AstNode>();
    while (parser.Peek().Type != TokenType.EOF)
    {
        AstNode node = parser.ParseStatement();
        ast.Add(node);
    }

    Console.WriteLine("AST gerada:");
    foreach (var node in ast)
    {
        PrintNode(node, 0);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Erro durante parsing: {ex.Message}");
}

void PrintNode(AstNode node, int indent)
{
    string indentStr = new string(' ', indent * 2);

    switch (node)
    {
        case VarDeclaration varDecl:
            Console.WriteLine($"{indentStr}VarDeclaration: {varDecl.Name} : {varDecl.Type}");
            if (varDecl.InitialValue != null)
                PrintNode(varDecl.InitialValue, indent + 1);
            break;

        case FuncDeclaration funcDecl:
            Console.WriteLine($"{indentStr}FuncDeclaration: {funcDecl.Name} returns {funcDecl.ReturnType}");
            Console.WriteLine($"{indentStr}Parameters:");
            foreach (var param in funcDecl.Parameters)
            {
                Console.WriteLine($"{indentStr}  - {param.Name} : {param.Type}");
            }
            Console.WriteLine($"{indentStr}Body:");
            foreach (var stmt in funcDecl.Body)
            {
                PrintNode(stmt, indent + 1);
            }
            break;

        case ForNode forNode:
            Console.WriteLine($"{indentStr}For:");
            Console.WriteLine($"{indentStr} Initialization:");
            PrintNode(forNode.Initialization, indent + 1);
            Console.WriteLine($"{indentStr} Condition:");
            PrintNode(forNode.Condition, indent + 1);
            Console.WriteLine($"{indentStr} Increment:");
            PrintNode(forNode.Increment, indent + 1);
            Console.WriteLine($"{indentStr} Body:");
            foreach (var stmt in forNode.Body)
            {
                PrintNode(stmt, indent + 1);
            }
            break;

        case BinaryOperation binOp:
            Console.WriteLine($"{indentStr}BinaryOperation: {binOp.Operator}");
            PrintNode(binOp.Left, indent + 1);
            PrintNode(binOp.Right, indent + 1);
            break;

        case CallNode call:
            Console.WriteLine($"{indentStr}Call: {call.FunctionName}");
            Console.WriteLine($"{indentStr}Arguments:");
            foreach (var arg in call.Arguments)
            {
                PrintNode(arg, indent + 1);
            }
            break;

        case AssignmentNode assign:
            Console.WriteLine($"{indentStr}Assignment: {assign.VariableName}");
            PrintNode(assign.Expression, indent + 1);
            break;

        case Literal literal:
            Console.WriteLine($"{indentStr}Literal: {literal.Value}");
            break;

        case IdentifierNode id:
            Console.WriteLine($"{indentStr}Identifier: {id.Name}");
            break;

        case PrintNode printNode:
            Console.WriteLine($"{indentStr}Print:");
            PrintNode(printNode.Expression, indent + 1);
            break;

        case ReturnNode retNode:
            Console.WriteLine($"{indentStr}Return:");
            PrintNode(retNode.Expression, indent + 1);
            break;

        default:
            Console.WriteLine($"{indentStr}Nodo desconhecido: {node.GetType().Name}");
            break;
    }
}
