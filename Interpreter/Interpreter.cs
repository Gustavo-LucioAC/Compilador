using System;
using System.Collections.Generic;
using Compilador.Sintatico;

namespace Compilador.Interpreter
{
    public class Interpreter
    {
        private readonly Dictionary<string, object?> _variables = new();
        private readonly Dictionary<string, FunctionNode> _functions = new();
        private readonly Stack<Dictionary<string, object?>> _locals = new();
        private Dictionary<string, string> _variableTypes = new();

        private object? EvaluateExpression(AstNode node)
        {
            switch (node)
            {
                case LiteralNode literal:
                    return literal.Value;

                case IdentifierNode id:
                    if (_variables.TryGetValue(id.Name, out var value))
                        return value;
                    throw new Exception($"Variável '{id.Name}' não declarada.");

                case UnaryExpression unary:
                    var operand = EvaluateExpression(unary.Operand);
                    return unary.Operator switch
                    {
                        "-" => operand switch
                        {
                            int i => -i,
                            float f => -f,
                            _ => throw new Exception("Operador '-' aplicado a tipo inválido")
                        },
                        "+" => operand switch
                        {
                            int i => i,
                            float f => f,
                            _ => throw new Exception("Operador '+' aplicado a tipo inválido")
                        },
                        _ => throw new Exception($"Operador unário não suportado: {unary.Operator}")
                    };

                case BinaryExpression binary:
                    var left = EvaluateExpression(binary.Left);
                    var right = EvaluateExpression(binary.Right);

                    return (left, right, binary.Operator) switch
                    {
                        // Inteiros
                        (int l, int r, "+") => l + r,
                        (int l, int r, "-") => l - r,
                        (int l, int r, "*") => l * r,
                        (int l, int r, "/") => r != 0 ? l / r : throw new DivideByZeroException(),
                        (int l, int r, "%") => r != 0 ? l % r : throw new DivideByZeroException(),
                        (int l, int r, "==") => l == r,
                        (int l, int r, "!=") => l != r,
                        (int l, int r, "<") => l < r,
                        (int l, int r, "<=") => l <= r,
                        (int l, int r, ">") => l > r,
                        (int l, int r, ">=") => l >= r,

                        // Floats
                        (float l, float r, "+") => l + r,
                        (float l, float r, "-") => l - r,
                        (float l, float r, "*") => l * r,
                        (float l, float r, "/") => r != 0 ? l / r : throw new DivideByZeroException(),
                        (float l, float r, "==") => l == r,
                        (float l, float r, "!=") => l != r,
                        (float l, float r, "<") => l < r,
                        (float l, float r, "<=") => l <= r,
                        (float l, float r, ">") => l > r,
                        (float l, float r, ">=") => l >= r,

                        // Booleanos
                        (bool l, bool r, "==") => l == r,
                        (bool l, bool r, "!=") => l != r,
                        (bool l, bool r, "&&") => l && r,
                        (bool l, bool r, "||") => l || r,

                        // Strings (opcional)
                        (string l, string r, "+") => l + r,

                        _ => throw new Exception($"Operação binária inválida: {left} {binary.Operator} {right}")
                    };

                case FunctionCallNode funcCall:
                    return ExecuteFunctionCall(funcCall);

                default:
                    throw new Exception($"Tipo de expressão não suportado: {node.GetType().Name}");
            }
        }

        private void ExecuteStatement(AstNode node)
        {
            switch (node)
            {
                case VarDeclaration varDecl:
                    var value = varDecl.InitialValue != null ? EvaluateExpression(varDecl.InitialValue) : null;
                    _variables[varDecl.Name] = value;
                    break;

                case AssignmentNode assign:
                    var val = EvaluateExpression(assign.Value);
                    if (_variables.ContainsKey(assign.Name))
                    {
                        _variables[assign.Name] = val;
                    }
                    else
                    {
                        throw new Exception($"Variável '{assign.Name}' não declarada.");
                    }
                    break;

                case PrintNode print:
                    var result = EvaluateExpression(print.Expression);
                    Console.WriteLine(result);
                    break;

                case ExpressionStatementNode exprStmt:
                    EvaluateExpression(exprStmt.Expression);
                    break;

                default:
                    throw new Exception($"Tipo de instrução não suportado: {node.GetType().Name}");
            }
        }
        public void Execute(ProgramNode program)
        {
            foreach (var stmt in program.Statements)
            {
                if (stmt is FunctionNode func)
                {
                    _functions[func.Name] = func;
                }
                else
                {
                    ExecuteStatement(stmt);
                }
            }
        }

        private object? ExecuteNode(AstNode node)
        {
            return node switch
            {
                Literal literal => literal.Value,
                LiteralNode literalNode => literalNode.Value,
                IdentifierNode id => GetVariable(id.Name),
                PrintNode print => ExecutePrint(print),
                InputNode input => ExecuteInput(input),
                IfNode ifNode => ExecuteIf(ifNode),
                ForNode forNode => ExecuteFor(forNode),
                WhileNode whileNode => ExecuteWhile(whileNode),
                VarDeclaration varDecl => ExecuteVarDeclaration(varDecl),
                BlockNode block => ExecuteBlock(block),
                FunctionCallNode funcCall => ExecuteFunctionCall(funcCall),
                ReturnNode returnNode => throw new ReturnException(ExecuteNode(returnNode.Expression)),
                _ => throw new Exception($"Tipo de nó não suportado: {node.GetType().Name}")
            };
        }

        private object? ExecutePrint(PrintNode node)
        {
            var value = ExecuteNode(node.Expression);
            Console.WriteLine(value);
            return null;
        }

        private object? ExecuteInput(InputNode node)
        {
            Console.Write($"{node.VariableName} = ");
            var input = Console.ReadLine();

            if (!_variables.ContainsKey(node.VariableName))
                throw new Exception($"Variável '{node.VariableName}' não foi declarada.");

            var currentValue = _variables[node.VariableName];

            if (currentValue is int)
                _variables[node.VariableName] = int.Parse(input!);
            else if (currentValue is float)
                _variables[node.VariableName] = float.Parse(input!);
            else if (currentValue is bool)
                _variables[node.VariableName] = bool.Parse(input!);
            else if (currentValue is char)
                _variables[node.VariableName] = string.IsNullOrEmpty(input) ? '\0' : input[0];
            else
                _variables[node.VariableName] = input;

            return null;
        }

        private object? ExecuteFunctionCall(FunctionCallNode node)
        {
            if (!_functions.TryGetValue(node.FunctionName, out var func))
                throw new Exception($"Função '{node.FunctionName}' não definida.");

            if (func.Parameters.Count != node.Arguments.Count)
                throw new Exception($"Função '{node.FunctionName}' esperava {func.Parameters.Count} argumentos, mas recebeu {node.Arguments.Count}.");

            var localScope = new Dictionary<string, object?>();

            for (int i = 0; i < func.Parameters.Count; i++)
            {
                var argValue = ExecuteNode(node.Arguments[i]);
                var argType = InferType(node.Arguments[i]);

                if (argType != func.Parameters[i].Type)
                {
                    throw new Exception($"Tipo incorreto para argumento {i + 1} na chamada de função '{node.FunctionName}': esperado '{func.Parameters[i].Type}', mas recebeu '{argType}'");
                }

                localScope[func.Parameters[i].Name] = argValue;
            }

            _locals.Push(localScope);
            try
            {
                var result = ExecuteNode(func.Body);

                // Verificação de retorno ausente (passo 3)
                if (func.ReturnType != "void" && result is not ReturnException)
                    throw new Exception($"Função '{func.Name}' deve retornar um valor do tipo '{func.ReturnType}'.");

                return result;
            }
            catch (ReturnException ex)
            {
                var actualReturnType = InferType(new LiteralNode(ex.Value!));
                if (actualReturnType != func.ReturnType)
                    throw new Exception($"Função '{func.Name}' deveria retornar '{func.ReturnType}', mas retornou '{actualReturnType}'.");

                return ex.Value;
            }
            finally
            {
                _locals.Pop();
            }
        }

        private object? ExecuteWhile(WhileNode node)
        {
            while (ExecuteNode(node.Condition) is bool condition && condition)
            {
                ExecuteNode(node.Body);
            }

            return null;
        }

        private object? ExecuteIf(IfNode node)
        {
            var condition = ExecuteNode(node.Condition);

            if (condition is bool b && b)
            {
                return ExecuteNode(node.ThenBranch);
            }
            else if (node.ElseBranch != null)
            {
                return ExecuteNode(node.ElseBranch);
            }

            return null;
        }

        private object? ExecuteFor(ForNode node)
        {
            ExecuteNode(node.Initialization);

            while (ExecuteNode(node.Condition) is bool condition && condition)
            {
                ExecuteNode(node.Body);
                ExecuteNode(node.Increment);
            }

            return null;
        }


        private object? ExecuteVarDeclaration(VarDeclaration node)
        {
            object? value = node.InitialValue != null ? ExecuteNode(node.InitialValue) : null;

            if (node.InitialValue != null)
            {
                string actualType = InferType(node.InitialValue);
                if (actualType != node.Type)
                    throw new Exception($"Tipo incorreto: variável '{node.Name}' esperava '{node.Type}', mas recebeu '{actualType}'");
            }

            DefineVariable(node.Name, value);

            return null;
        }

        private void DefineVariable(string name, object? value)
        {
            if (_locals.Count > 0)
            {
                var currentScope = _locals.Peek();
                currentScope[name] = value;
            }
            else
            {
                _variables[name] = value;
            }
        }

        private object? EvaluateBinary(BinaryExpression binary)
        {
            var left = EvaluateExpression(binary.Left);
            var right = EvaluateExpression(binary.Right);
            var op = binary.Operator;

            return (left, right, op) switch
            {
                (int l, int r, "+") => l + r,
                (int l, int r, "-") => l - r,
                (int l, int r, "*") => l * r,
                (int l, int r, "/") => r != 0 ? l / r : throw new DivideByZeroException(),

                (float l, float r, "+") => l + r,
                (float l, float r, "-") => l - r,
                (float l, float r, "*") => l * r,
                (float l, float r, "/") => r != 0 ? l / r : throw new DivideByZeroException(),

                (int l, int r, "<") => l < r,
                (int l, int r, ">") => l > r,
                (int l, int r, "==") => l == r,

                (string l, string r, "+") => l + r,

                _ => throw new Exception($"Operação binária não suportada: {left} {op} {right}")
            };
        }

        private object? ExecuteBlock(BlockNode block)
        {
            foreach (var stmt in block.Statements)
            {
                try
                {
                    var result = ExecuteNode(stmt);
                }
                catch (ReturnException ex)
                {
                    throw;
                }
            }

            return null;
        }

        public object? GetVariable(string name)
        {
            if (_variables.TryGetValue(name, out var value))
                return value;

            throw new Exception($"Variável '{name}' não declarada.");
        }

        private class ReturnException(object? value) : Exception
        {
            public object? Value { get; } = value;
        }

        private class Function(List<Parameter> parameters, BlockNode body, string returnType)
        {
            public List<Parameter> Parameters { get; } = parameters;
            public BlockNode Body { get; } = body;
            public string ReturnType { get; } = returnType;
        }

        private string InferType(AstNode node)
        {
            return node switch
            {
                LiteralNode lit => lit.Value switch
                {
                    int => "int",
                    float => "float",
                    bool => "bool",
                    string => "string",
                    char => "char",
                    _ => throw new Exception($"Tipo literal desconhecido: {lit.Value}")
                },

                IdentifierNode id => GetVariableType(id.Name),

                BinaryExpression bin => InferBinaryExpressionType(bin),

                FunctionCallNode call => GetFunctionReturnType(call.FunctionName),

                _ => throw new Exception($"Não foi possível inferir o tipo do nó: {node.GetType().Name}")
            };
        }
        private string GetVariableType(string name)
        {
            foreach (var scope in _locals.Reverse())
                if (scope.TryGetValue(name, out var val))
                    return val is not null ? InferType(new LiteralNode(val)) : throw new Exception($"Não foi possível inferir o tipo da variável '{name}'");

            if (_variables.TryGetValue(name, out var globalVal))
                return globalVal is not null ? InferType(new LiteralNode(globalVal)) : throw new Exception($"Não foi possível inferir o tipo da variável global '{name}'");

            throw new Exception($"Variável '{name}' não declarada");
        }
        private string InferBinaryExpressionType(BinaryExpression bin)
        {
            var leftType = InferType(bin.Left);
            var rightType = InferType(bin.Right);
            var op = bin.Operator;

            if (leftType != rightType)
                throw new Exception($"Operação inválida: tipos diferentes '{leftType}' e '{rightType}' com operador '{op}'");

            return op switch
            {
                "+" or "-" or "*" or "/" when leftType is "int" or "float" => leftType,
                "==" or "!=" or "<" or "<=" or ">" or ">=" => "bool",
                "&&" or "||" when leftType == "bool" => "bool",
                _ => throw new Exception($"Operador '{op}' não suportado para tipo '{leftType}'")
            };
        }
        private string GetFunctionReturnType(string functionName)
        {
            if (_functions.TryGetValue(functionName, out var func))
                return func.ReturnType;

            throw new Exception($"Função '{functionName}' não declarada");
        }
    }
}
