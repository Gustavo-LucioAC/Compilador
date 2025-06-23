using System;
using System.Collections.Generic;
using Compilador.Sintatico;

public class Interpreter
{
    private readonly Dictionary<string, object> _variables = new();

    public void Interpret(ProgramNode program)
    {
        foreach (var stmt in program.Statements)
        {
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
                {
                    ExecuteBlock(ifNode.ThenBlock);
                }
                else if (ifNode.ElseBlock != null)
                {
                    ExecuteBlock(ifNode.ElseBlock);
                }
                break;
            case WhileNode whileNode:
                while (IsTruthy(EvaluateExpression(whileNode.Condition)))
                {
                    ExecuteBlock(whileNode.Body);
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
        object? value = null;
        if (varDecl.Initializer != null)
        {
            value = EvaluateExpression(varDecl.Initializer);
        }
        else
        {
            // Valor padrão para tipos comuns
            value = GetDefaultValue(varDecl.Type);
        }

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
                return EvaluateBinaryOperation(bin.Operator, left, right);
            default:
                throw new Exception("Expressão não suportada no interpretador.");
        }
    }

    private object EvaluateBinaryOperation(string op, object left, object right)
    {
        // Tente converter para double para operações numéricas, se possível
        bool leftIsNumber = double.TryParse(left.ToString(), out double leftNum);
        bool rightIsNumber = double.TryParse(right.ToString(), out double rightNum);

        switch (op)
        {
            case "+":
                if (leftIsNumber && rightIsNumber)
                    return leftNum + rightNum;
                return left.ToString() + right.ToString(); // concatenação string
            case "-":
                if (leftIsNumber && rightIsNumber)
                    return leftNum - rightNum;
                break;
            case "*":
                if (leftIsNumber && rightIsNumber)
                    return leftNum * rightNum;
                break;
            case "/":
                if (leftIsNumber && rightIsNumber)
                {
                    if (rightNum == 0)
                        throw new Exception("Divisão por zero.");
                    return leftNum / rightNum;
                }
                break;
            case "%":
                if (leftIsNumber && rightIsNumber)
                    return leftNum % rightNum;
                break;
            case "==":
                return Equals(left, right);
            case "!=":
                return !Equals(left, right);
            case "<":
                if (leftIsNumber && rightIsNumber)
                    return leftNum < rightNum;
                break;
            case "<=":
                if (leftIsNumber && rightIsNumber)
                    return leftNum <= rightNum;
                break;
            case ">":
                if (leftIsNumber && rightIsNumber)
                    return leftNum > rightNum;
                break;
            case ">=":
                if (leftIsNumber && rightIsNumber)
                    return leftNum >= rightNum;
                break;
        }

        throw new Exception($"Operação '{op}' não suportada para os operandos fornecidos.");
    }

    private bool IsTruthy(object value)
    {
        if (value == null) return false;

        return value switch
        {
            bool b => b,
            int i => i != 0,
            double d => d != 0.0,
            string s => !string.IsNullOrEmpty(s),
            _ => true,
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
            _ => null!,
        };
    }

    private object ConvertInputToType(string input, Type targetType)
    {
        try
        {
            if (targetType == typeof(int))
                return int.Parse(input);
            else if (targetType == typeof(double))
                return double.Parse(input);
            else if (targetType == typeof(bool))
                return bool.Parse(input);
            else if (targetType == typeof(char))
                return string.IsNullOrEmpty(input) ? '\0' : input[0];
            else if (targetType == typeof(string))
                return input;

            throw new Exception($"Tipo {targetType.Name} não suportado para input.");
        }
        catch
        {
            throw new Exception($"Entrada inválida para o tipo {targetType.Name}: '{input}'");
        }
    }
}
