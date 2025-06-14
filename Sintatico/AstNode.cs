namespace Compilador.Sintatico
{
    public abstract class AstNode
    {
        public int Line { get; set; }
        public int Column { get; set; }
    }

    public class Literal (string value) : AstNode
    {
        public string Value { get; } = value;
    }

    public class ForNode(AstNode init, AstNode condition, AstNode increment, BlockNode body) : AstNode
    {
        public AstNode Init { get; } = init;
        public AstNode Condition { get; } = condition;
        public AstNode Increment { get; } = increment;
        public BlockNode Body { get; } = body;
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

    public class PrintNode(AstNode expression) : AstNode
    {
        public AstNode Expression { get; } = expression;
    }

    public class InputNode (string variableName) : AstNode
    {
        public string VariableName { get; } = variableName;
    }

    public class IfNode(AstNode condition, BlockNode thenBlock, BlockNode? elseBlock) : AstNode
    {
        public AstNode Condition { get; } = condition;
        public BlockNode ThenBlock { get; } = thenBlock;
        public BlockNode? ElseBlock { get; } = elseBlock;
    }

    public class WhileNode(AstNode condition, BlockNode body) : AstNode
    {
        public AstNode Condition { get; } = condition;
        public BlockNode Body { get; } = body;
    }

    public class FunctionNode(string name, List<Parameter> parameters, string returnType, BlockNode body) : AstNode
    {
        public string Name { get; } = name;
        public List<Parameter> Parameters { get; } = parameters;
        public string ReturnType { get; } = returnType;
        public BlockNode Body { get; } = body;
    }

    public class Parameter (string name, string type)
    {
        public string Name { get; } = name;
        public string Type { get; } = type;
    }

    public class BinaryExpression (AstNode left, string op, AstNode right) : AstNode
    {
        public AstNode Left { get; } = left;
        public string Operator { get; } = op;
        public AstNode Right { get; } = right;
    }
}