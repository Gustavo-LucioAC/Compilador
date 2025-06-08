public abstract class AstNode { }

public class VarDeclaration : AstNode
{
    public string Name { get; }
    public string Type { get; }
    public AstNode? Value { get; }

    public VarDeclaration(string name, string type, AstNode? value)
    {
        Name = name;
        Type = type;
        Value = value;
    }
}

public class Literal : AstNode
{
    public string Value { get; }

    public Literal(string value)
    {
        Value = value;
    }
}
