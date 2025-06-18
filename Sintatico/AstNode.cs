namespace Compilador.Sintatico
{
    public abstract class AstNode
    {
        public int Line { get; set; }
        public int Column { get; set; }
    }

    public class Literal(string value) : AstNode
    {
        public string Value { get; } = value;
    }

    public class VarDeclaration(string name, string type, AstNode? initialValue) : AstNode
    {
        public string Name { get; } = name;
        public string Type { get; } = type;
        public AstNode? InitialValue { get; } = initialValue;
    }

    public class BlockNode(List<AstNode> statements) : AstNode
    {
        public List<AstNode> Statements { get; } = statements;
    }

    public class VariableDeclaration(string name, string type, AstNode? value) : AstNode
    {
        public string Name { get; } = name;
        public string Type { get; } = type;
        public AstNode? Value { get; } = value;
    }

    public class PrintNode(AstNode expression) : AstNode
    {
        public AstNode Expression { get; } = expression;
    }

    public class InputNode(string variableName) : AstNode
    {
        public string VariableName { get; } = variableName;
    }

    public class IfNode(AstNode condition, BlockNode thenBranch, BlockNode? elseBranch) : AstNode
    {
        public AstNode Condition { get; } = condition;
        public BlockNode ThenBranch { get; } = thenBranch;
        public BlockNode? ElseBranch { get; } = elseBranch;
    }

    public class FunctionNode(string name, List<Parameter> parameters, string returnType, BlockNode body) : AstNode
    {
        public string Name { get; } = name;
        public List<Parameter> Parameters { get; } = parameters;
        public string ReturnType { get; } = returnType;
        public BlockNode Body { get; } = body;
    }

    public class Parameter(string name, string type)
    {
        public string Name { get; } = name;
        public string Type { get; } = type;
    }

    public class WhileNode(AstNode condition, BlockNode body) : AstNode
    {
        public AstNode Condition { get; } = condition;
        public BlockNode Body { get; } = body;
    }

    public class ForNode(AstNode? initialization, AstNode? condition, AstNode? increment, BlockNode body) : AstNode
    {
        public AstNode? Initialization { get; } = initialization;
        public AstNode? Condition { get; } = condition;
        public AstNode? Increment { get; } = increment;
        public BlockNode Body { get; } = body;
    }

    public class FunctionCallNode(string functionName, List<AstNode> arguments) : AstNode
    {
        public string FunctionName { get; } = functionName;
        public List<AstNode> Arguments { get; } = arguments;
    }

    public class ProgramNode : AstNode
    {
        public List<AstNode> Statements { get; }

        public ProgramNode(List<AstNode> statements)
        {
            Statements = statements;
        }
    }

    public class UnaryExpression(string op, AstNode operand) : AstNode
    {
        public string Operator { get; } = op;
        public AstNode Operand { get; } = operand;
    }

    public class ExpressionStatementNode(AstNode expression) : AstNode
    {
        public AstNode Expression { get; } = expression;
    }
    public class AssignmentNode(string name, AstNode value) : AstNode
    {
        public string Name { get; } = name;
        public AstNode Value { get; } = value;
    }

    public static class SupportedTypes
    {
        public static readonly HashSet<string> All = new()
        {
            "int", "float", "char", "bool", "string"
        };
    }

    public class LiteralNode(object value) : AstNode
    {
        public object Value { get; } = value;
    }

    public class IdentifierNode(string name) : AstNode
    {
        public string Name { get; } = name;
    }

    public class BinaryExpression(AstNode left, string op, AstNode right) : AstNode
    {
        public AstNode Left { get; } = left;
        public string Operator { get; } = op;
        public AstNode Right { get; } = right;
    }

    public class FunctionDeclaration(string name, List<Parameter> parameters, string returnType, List<AstNode> body) : AstNode
    {
        public string Name { get; } = name;
        public List<Parameter> Parameters { get; } = parameters;
        public string ReturnType { get; } = returnType;
        public List<AstNode> Body { get; } = body;
    }

    public class ReturnNode(AstNode expression) : AstNode
    {
        public AstNode Expression { get; } = expression;
    }
}