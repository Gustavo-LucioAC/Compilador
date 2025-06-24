using System;
using System.Collections.Generic;
using System.Linq;
using Compilador.Sintatico;

public class Interpreter
{
    private Dictionary<string, object> _variables = new();
    private readonly Dictionary<string, FunctionNode> _functions = new();

    public void Interpret(ProgramNode program)
    {
        foreach (var stmt in program.Statements)
        {
            if (stmt is FunctionNode func)
                _functions[func.Name] = func;
            else
                ExecuteStatement(stmt);
        }
    }

    private void ExecuteStatement(StatementNode stmt)
    {
        switch (stmt)
        {
            case VariableDeclarationNode varDecl:
                ExecuteVariableDeclaration(varDecl);
                break;
            case AssignmentNode assign:
                ExecuteAssignment(assign);
                break;
            case PrintNode print:
                var value = EvaluateExpression(print.Expression);
                Console.WriteLine(value);
                break;
            case InputNode input:
                Console.Write($"{input.VariableName}: ");
                var inputValue = Console.ReadLine() ?? "";

                if (!_variables.ContainsKey(input.VariableName))
                    throw new Exception($"Variável '{input.VariableName}' não declarada.");

                var currentValue = _variables[input.VariableName];
                var convertedValue = ConvertInputToType(inputValue, currentValue.GetType());
                _variables[input.VariableName] = convertedValue;
                break;
            case IfNode ifNode:
                var condition = EvaluateExpression(ifNode.Condition);
                if (IsTruthy(condition))
                    ExecuteBlock(ifNode.ThenBlock);
                else if (ifNode.ElseBlock != null)
                    ExecuteBlock(ifNode.ElseBlock);
                break;
            case WhileNode whileNode:
                while (IsTruthy(EvaluateExpression(whileNode.Condition)))
                {
                    ExecuteBlock(whileNode.Body);
                }
                break;
            case ForNode forNode:
                ExecuteStatement(forNode.Initialization);
                while (IsTruthy(EvaluateExpression(forNode.Condition)))
                {
                    ExecuteBlock(forNode.Body);
                    ExecuteStatement(forNode.Increment);
                }
                break;
            default:
                throw new Exception("Comando não suportado no interpretador.");
        }
    }

    private void ExecuteBlock(List<StatementNode> statements)
    {
        foreach (var stmt in statements)
        {
            ExecuteStatement(stmt);
        }
    }

    private void ExecuteVariableDeclaration(VariableDeclarationNode varDecl)
    {
        object? value = varDecl.Initializer != null
            ? EvaluateExpression(varDecl.Initializer)
            : GetDefaultValue(varDecl.Type);

        _variables[varDecl.Name] = value!;
    }

    private void ExecuteAssignment(AssignmentNode assign)
    {
        if (!_variables.ContainsKey(assign.Name))
            throw new Exception($"Variável '{assign.Name}' não declarada.");

        var value = EvaluateExpression(assign.Value);
        _variables[assign.Name] = value;
    }

    private object EvaluateExpression(ExpressionNode expr)
    {
        switch (expr)
        {
            case LiteralNode lit:
                return lit.Value!;
            case IdentifierNode id:
                if (!_variables.TryGetValue(id.Name, out var value))
                    throw new Exception($"Variável '{id.Name}' não declarada.");
                return value!;
            case BinaryExpressionNode bin:
                var left = EvaluateExpression(bin.Left);
                var right = EvaluateExpression(bin.Right);
                return EvaluateBinaryOperation(bin.Operator, left, right); // ✅ Aqui executa a operação
            case FunctionCallNode call:
                if (!_functions.TryGetValue(call.FunctionName, out var func))
                    throw new Exception($"Função '{call.FunctionName}' não declarada.");

                if (call.Arguments.Count != func.Parameters.Count)
                    throw new Exception($"Função '{call.FunctionName}' esperava {func.Parameters.Count} argumentos, mas recebeu {call.Arguments.Count}.");

                var args = call.Arguments.Select(EvaluateExpression).ToList();
                return ExecuteFunction(func, args);
            default:
                throw new Exception("Expressão não suportada no interpretador.");
        }
    }

    private object ExecuteFunction(FunctionNode func, List<object> args)
    {
        var previousVariables = _variables;
        _variables = new Dictionary<string, object>();

        for (int i = 0; i < func.Parameters.Count; i++)
        {
            var paramName = func.Parameters[i].Item1;
            _variables[paramName] = args[i];
        }

        object? returnValue = null;

        foreach (var stmt in func.Body)
        {
            if (stmt is ReturnNode ret)
            {
                returnValue = EvaluateExpression(ret.Value);
                break;
            }

            ExecuteStatement(stmt);
        }

        _variables = previousVariables;

        return returnValue ?? GetDefaultValue(func.ReturnType ?? "int");
    }

    private object EvaluateBinaryOperation(string op, object left, object right)
    {
        bool leftIsNumber = double.TryParse(left.ToString(), out double leftNum);
        bool rightIsNumber = double.TryParse(right.ToString(), out double rightNum);

        return op switch
        {
            "+" when left is int && right is int => (int)left + (int)right,
            "+" when leftIsNumber && rightIsNumber => leftNum + rightNum,
            "+" => left.ToString() + right.ToString(),
            "-" when left is int && right is int => (int)left - (int)right,
            "*" when left is int && right is int => (int)left * (int)right,
            "/" when left is int && right is int => 
                (int)right == 0 ? throw new Exception("Divisão por zero.") : (int)left / (int)right,
            "%" when left is int && right is int => (int)left % (int)right,
            "%" when leftIsNumber && rightIsNumber => leftNum % rightNum,
            "==" => Equals(left, right),
            "!=" => !Equals(left, right),
            "<" when leftIsNumber && rightIsNumber => leftNum < rightNum,
            "<=" when leftIsNumber && rightIsNumber => leftNum <= rightNum,
            ">" when leftIsNumber && rightIsNumber => leftNum > rightNum,
            ">=" when leftIsNumber && rightIsNumber => leftNum >= rightNum,
            _ => throw new Exception($"Operação '{op}' não suportada para os operandos fornecidos.")
        };
    }

    private bool IsTruthy(object value)
    {
        return value switch
        {
            null => false,
            bool b => b,
            int i => i != 0,
            double d => d != 0.0,
            string s => !string.IsNullOrEmpty(s),
            _ => true
        };
    }

    private object GetDefaultValue(string type)
    {
        return type switch
        {
            "int" => 0,
            "float" => 0.0,
            "bool" => false,
            "char" => '\0',
            "string" => "",
            _ => null!
        };
    }

    private object ConvertInputToType(string input, Type targetType)
    {
        try
        {
            if (targetType == typeof(int)) return int.Parse(input);
            if (targetType == typeof(double)) return double.Parse(input);
            if (targetType == typeof(bool)) return bool.Parse(input);
            if (targetType == typeof(char)) return string.IsNullOrEmpty(input) ? '\0' : input[0];
            if (targetType == typeof(string)) return input;

            throw new Exception($"Tipo {targetType.Name} não suportado para input.");
        }
        catch
        {
            throw new Exception($"Entrada inválida para o tipo {targetType.Name}: '{input}'");
        }
    }

    private bool IsTypeCompatible(object left, object right)
    {
        if (left == null || right == null) return false;

        var leftType = left.GetType();
        var rightType = right.GetType();

        // Permitir comparação entre int e double
        if ((leftType == typeof(int) && rightType == typeof(double)) ||
            (leftType == typeof(double) && rightType == typeof(int)))
            return true;

        return leftType == rightType;
    }
}
