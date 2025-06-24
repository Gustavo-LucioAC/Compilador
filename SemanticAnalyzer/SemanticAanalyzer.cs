using Compilador.Sintatico;

public class SemanticAnalyzer
{
    private Scope _currentScope = new Scope();

    public void Analyze(ProgramNode program)
    {
        foreach (var stmt in program.Statements)
        {
            AnalyzeStatement(stmt);
        }
    }

    private void AnalyzeStatement(StatementNode stmt)
    {
        switch (stmt)
        {
            case VariableDeclarationNode varDecl:
                AnalyzeVariableDeclaration(varDecl);
                break;
            case AssignmentNode assign:
                AnalyzeAssignment(assign);
                break;
            case FunctionNode funcDecl:
                AnalyzeFunctionDeclaration(funcDecl);
                break;
            case ReturnNode ret:
                var returnType = AnalyzeExpression(ret.Value);
                break;
            case PrintNode print:
                AnalyzeExpression(print.Expression);
                break;
            case InputNode input:
                if (_currentScope.Resolve(input.VariableName) == null)
                    throw new Exception($"Erro semântico: variável '{input.VariableName}' não declarada.");
                break;
            case IfNode ifNode:
                AnalyzeExpression(ifNode.Condition);
                EnterScope();
                foreach (var s in ifNode.ThenBlock)
                    AnalyzeStatement(s);
                ExitScope();

                if (ifNode.ElseBlock != null)
                {
                    EnterScope();
                    foreach (var s in ifNode.ElseBlock)
                        AnalyzeStatement(s);
                    ExitScope();
                }
                break;
            case WhileNode whileNode:
                AnalyzeExpression(whileNode.Condition);
                EnterScope();
                foreach (var s in whileNode.Body)
                    AnalyzeStatement(s);
                ExitScope();
                break;
            case ForNode forNode:
                EnterScope();
                AnalyzeStatement(forNode.Initialization);
                var condType = AnalyzeExpression(forNode.Condition);
                if (condType != "bool")
                    throw new Exception("Erro semântico: condição do for deve ser booleana.");
                AnalyzeStatement(forNode.Increment);
                foreach (var bodyStmt in forNode.Body)
                    AnalyzeStatement(bodyStmt);
                ExitScope();
                break;
            default:
                throw new Exception("Tipo de comando não suportado no analisador semântico.");
        }
    }

    private void AnalyzeFunctionDeclaration(FunctionNode funcDecl)
    {

        if (!_currentScope.Define(new Symbol(funcDecl.Name, funcDecl.ReturnType)))
            throw new Exception($"Erro semântico: função '{funcDecl.Name}' já declarada.");

        EnterScope();

        foreach (var (paramName, paramType) in funcDecl.Parameters)
        {
            if (!_currentScope.Define(new Symbol(paramName, paramType)))
                throw new Exception($"Erro semântico: parâmetro '{paramName}' duplicado na função '{funcDecl.Name}'.");
        }

        foreach (var stmt in funcDecl.Body)
        {
            AnalyzeStatement(stmt);
        }

        ExitScope();
    }

    private void AnalyzeVariableDeclaration(VariableDeclarationNode varDecl)
    {
        if (!_currentScope.Define(new Symbol(varDecl.Name, varDecl.Type)))
            throw new Exception($"Erro semântico: variável '{varDecl.Name}' já declarada neste escopo.");

        if (varDecl.Initializer != null)
        {
            var initType = AnalyzeExpression(varDecl.Initializer);
            if (!IsTypeCompatible(varDecl.Type, initType))
                throw new Exception($"Erro semântico: tipo incompatível na inicialização da variável '{varDecl.Name}'. Esperado '{varDecl.Type}', encontrado '{initType}'.");
        }
    }

    private void AnalyzeAssignment(AssignmentNode assign)
    {
        var symbol = _currentScope.Resolve(assign.Name);
        if (symbol == null)
            throw new Exception($"Erro semântico: variável '{assign.Name}' não declarada.");

        var valueType = AnalyzeExpression(assign.Value);
        if (!IsTypeCompatible(symbol.Type, valueType))
            throw new Exception($"Erro semântico: tipo incompatível na atribuição da variável '{assign.Name}'. Esperado '{symbol.Type}', encontrado '{valueType}'.");
    }

    private string AnalyzeExpression(ExpressionNode expr)
    {
        switch (expr)
        {
            case LiteralNode lit:
                return GetLiteralType(lit.Value);

            case IdentifierNode id:
                var symbol = _currentScope.Resolve(id.Name);
                if (symbol == null)
                    throw new Exception($"Erro semântico: variável '{id.Name}' não declarada.");
                return symbol.Type;

            case BinaryExpressionNode bin:
                var leftType = AnalyzeExpression(bin.Left);
                var rightType = AnalyzeExpression(bin.Right);

                if (!IsTypeCompatible(leftType, rightType))
                    throw new Exception($"Erro semântico: tipos incompatíveis no operador '{bin.Operator}'.");


                if (new[] { "==", "!=", "<", "<=", ">", ">=" }.Contains(bin.Operator))
                    return "bool";

                return leftType;

            case FunctionCallNode call:
                var funcSymbol = _currentScope.Resolve(call.FunctionName);
                if (funcSymbol == null)
                    throw new Exception($"Erro semântico: função '{call.FunctionName}' não declarada.");
                return funcSymbol.Type;

            default:
                throw new Exception("Expressão não suportada no analisador semântico.");
        }
    }

    private bool IsTypeCompatible(string expected, string actual)
    {
        if (expected == actual)
            return true;

        if (expected == "float" && actual == "int")
            return true;

        return false;
    }

    private string GetLiteralType(object value)
    {
        return value switch
        {
            int => "int",
            double => "float",
            char => "char",
            bool => "bool",
            string => "string",
            _ => throw new Exception("Tipo literal desconhecido")
        };
    }

    private void EnterScope()
    {
        _currentScope = new Scope(_currentScope);
    }

    private void ExitScope()
    {
        if (_currentScope.Parent == null)
            throw new Exception("Não existe escopo pai para sair.");

        _currentScope = _currentScope.Parent;
    }
}

public class Symbol
{
    public string Name { get; }
    public string Type { get; }

    public Symbol(string name, string type)
    {
        Name = name;
        Type = type;
    }
}

public class Scope
{
    private readonly Dictionary<string, Symbol> _symbols = new();
    private readonly Scope? _parent;

    public Scope? Parent => _parent;

    public Scope(Scope? parent = null)
    {
        _parent = parent;
    }

    public bool Define(Symbol symbol)
    {
        if (_symbols.ContainsKey(symbol.Name))
            return false;

        _symbols[symbol.Name] = symbol;
        return true;
    }

    public Symbol? Resolve(string name)
    {
        if (_symbols.TryGetValue(name, out var symbol))
            return symbol;

        return _parent?.Resolve(name);
    }
}
