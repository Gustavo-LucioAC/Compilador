namespace Compilador.Sintatico
{
    public abstract class Node { }

    public class ProgramNode : Node
    {
        public List<StatementNode> Statements { get; set; } = new();
    }

    public abstract class StatementNode : Node { }

    public class VariableDeclarationNode : StatementNode
    {
        public required string Name { get; set; }
        public required string Type { get; set; }
        public ExpressionNode? Initializer { get; set; }
    }

    public class PrintNode : StatementNode
    {
        public required ExpressionNode Expression { get; set; }
    }

    public class InputNode : StatementNode
    {
        public required string VariableName { get; set; }
    }

    public class IfNode : StatementNode
    {
        public required ExpressionNode Condition { get; set; }
        public required List<StatementNode> ThenBlock { get; set; }
        public List<StatementNode>? ElseBlock { get; set; }
    }

    public class WhileNode : StatementNode
    {
        public required ExpressionNode Condition { get; set; }
        public required List<StatementNode> Body { get; set; }
    }

    public class ForNode : StatementNode
    {
        public required StatementNode Initialization { get; set; }
        public required ExpressionNode Condition { get; set; }
        public required StatementNode Increment { get; set; }
        public required List<StatementNode> Body { get; set; }
    }

    public class FunctionNode : StatementNode
    {
        public required string Name { get; set; }
        public required List<(string Name, string Type)> Parameters { get; set; }
        public required string ReturnType { get; set; }
        public required List<StatementNode> Body { get; set; }
    }

    public class ReturnNode : StatementNode
    {
        public required ExpressionNode Value { get; set; }
    }

    // EXPRESSÕES

    public abstract class ExpressionNode : Node { }

    public class BinaryExpressionNode : ExpressionNode
    {
        public required ExpressionNode Left { get; set; }
        public required string Operator { get; set; }
        public required ExpressionNode Right { get; set; }
    }

    public class LiteralNode : ExpressionNode
    {
        public required object Value { get; set; }
    }

    public class IdentifierNode : ExpressionNode
    {
        public required string Name { get; set; }
    }

    public class AssignmentNode : StatementNode
    {
        public required string Name { get; set; }
        public required ExpressionNode Value { get; set; }
    }

    public class FunctionCallNode : ExpressionNode
    {
        public required string FunctionName { get; set; }
        public required List<ExpressionNode> Arguments { get; set; }
    }
    public class FunctionSymbol : Symbol
    {
        public List<string> ParamTypes { get; }
        public FunctionSymbol(string name, string returnType, List<string> paramTypes)
            : base(name, returnType)
        {
            ParamTypes = paramTypes;
        }
    }
}