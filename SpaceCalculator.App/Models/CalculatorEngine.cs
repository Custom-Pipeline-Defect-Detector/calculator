using System.Globalization;

namespace SpaceCalculator.Models;

public static class CalculatorEngine
{
    private static readonly Dictionary<string, int> OperatorPrecedence = new()
    {
        {"+", 1},
        {"-", 1},
        {"*", 2},
        {"/", 2},
        {"^", 3}
    };

    private static readonly HashSet<string> Functions = new(StringComparer.OrdinalIgnoreCase)
    {
        "sin", "cos", "tan", "log", "ln", "sqrt", "abs", "fact", "neg"
    };

    private static readonly Dictionary<string, double> Constants = new(StringComparer.OrdinalIgnoreCase)
    {
        {"pi", Math.PI},
        {"tau", Math.PI * 2},
        {"e", Math.E},
        {"g0", 9.80665}
    };

    public static double Evaluate(string expression)
    {
        var tokens = Tokenize(expression);
        var rpn = ToRpn(tokens);
        return EvaluateRpn(rpn);
    }

    private static IEnumerable<string> Tokenize(string expression)
    {
        var tokens = new List<string>();
        var current = string.Empty;
        for (var i = 0; i < expression.Length; i++)
        {
            var c = expression[i];

            if (char.IsWhiteSpace(c))
            {
                continue;
            }

            if (char.IsLetter(c))
            {
                current += c;
                while (i + 1 < expression.Length && char.IsLetter(expression[i + 1]))
                {
                    current += expression[++i];
                }
                tokens.Add(current);
                current = string.Empty;
            }
            else if (char.IsDigit(c) || c == '.')
            {
                current += c;
                while (i + 1 < expression.Length && (char.IsDigit(expression[i + 1]) || expression[i + 1] == '.'))
                {
                    current += expression[++i];
                }
                tokens.Add(current);
                current = string.Empty;
            }
            else if (c is '+' or '*' or '/' or '^')
            {
                tokens.Add(c.ToString());
            }
            else if (c == '-')
            {
                var previous = tokens.LastOrDefault();
                if (string.IsNullOrEmpty(previous) || previous is "+" or "-" or "*" or "/" or "^" or "(")
                {
                    tokens.Add("neg");
                }
                else
                {
                    tokens.Add("-");
                }
            }
            else if (c is '(' or ')')
            {
                tokens.Add(c.ToString());
            }
            else if (c == '!')
            {
                tokens.Add("fact");
            }
            else
            {
                throw new InvalidOperationException($"Unrecognized character '{c}'.");
            }
        }

        return tokens;
    }

    private static Queue<string> ToRpn(IEnumerable<string> tokens)
    {
        var output = new Queue<string>();
        var operators = new Stack<string>();

        foreach (var token in tokens)
        {
            if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
            {
                output.Enqueue(token);
            }
            else if (Constants.ContainsKey(token))
            {
                output.Enqueue(token);
            }
            else if (Functions.Contains(token))
            {
                operators.Push(token);
            }
            else if (OperatorPrecedence.ContainsKey(token))
            {
                while (operators.Count > 0 &&
                       ((Functions.Contains(operators.Peek())) ||
                        (OperatorPrecedence.TryGetValue(operators.Peek(), out var prec) && prec >= OperatorPrecedence[token])))
                {
                    output.Enqueue(operators.Pop());
                }

                operators.Push(token);
            }
            else if (token == "(")
            {
                operators.Push(token);
            }
            else if (token == ")")
            {
                while (operators.Count > 0 && operators.Peek() != "(")
                {
                    output.Enqueue(operators.Pop());
                }

                if (operators.Count == 0)
                {
                    throw new InvalidOperationException("Mismatched parentheses.");
                }

                operators.Pop();

                if (operators.Count > 0 && Functions.Contains(operators.Peek()))
                {
                    output.Enqueue(operators.Pop());
                }
            }
            else
            {
                throw new InvalidOperationException($"Unknown token '{token}'.");
            }
        }

        while (operators.Count > 0)
        {
            var op = operators.Pop();
            if (op is "(" or ")")
            {
                throw new InvalidOperationException("Mismatched parentheses.");
            }
            output.Enqueue(op);
        }

        return output;
    }

    private static double EvaluateRpn(Queue<string> rpn)
    {
        var stack = new Stack<double>();

        while (rpn.Count > 0)
        {
            var token = rpn.Dequeue();
            if (double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var number))
            {
                stack.Push(number);
            }
            else if (Constants.TryGetValue(token, out var constant))
            {
                stack.Push(constant);
            }
            else if (OperatorPrecedence.ContainsKey(token))
            {
                if (stack.Count < 2)
                {
                    throw new InvalidOperationException("Not enough operands.");
                }

                var b = stack.Pop();
                var a = stack.Pop();

                stack.Push(token switch
                {
                    "+" => a + b,
                    "-" => a - b,
                    "*" => a * b,
                    "/" => b == 0 ? throw new DivideByZeroException("Division by zero.") : a / b,
                    "^" => Math.Pow(a, b),
                    _ => throw new InvalidOperationException($"Unsupported operator '{token}'.")
                });
            }
            else if (Functions.Contains(token))
            {
                ApplyFunction(stack, token);
            }
            else
            {
                throw new InvalidOperationException($"Unknown token '{token}'.");
            }
        }

        if (stack.Count != 1)
        {
            throw new InvalidOperationException("Invalid expression.");
        }

        return stack.Pop();
    }

    private static void ApplyFunction(Stack<double> stack, string function)
    {
        if (stack.Count == 0)
        {
            throw new InvalidOperationException("Function missing operand.");
        }

        switch (function.ToLowerInvariant())
        {
            case "sin":
                stack.Push(Math.Sin(stack.Pop()));
                break;
            case "cos":
                stack.Push(Math.Cos(stack.Pop()));
                break;
            case "tan":
                stack.Push(Math.Tan(stack.Pop()));
                break;
            case "log":
                var value = stack.Pop();
                if (value <= 0) throw new InvalidOperationException("Log domain error.");
                stack.Push(Math.Log10(value));
                break;
            case "ln":
                var lnValue = stack.Pop();
                if (lnValue <= 0) throw new InvalidOperationException("Ln domain error.");
                stack.Push(Math.Log(lnValue));
                break;
            case "sqrt":
                var sqrtValue = stack.Pop();
                if (sqrtValue < 0) throw new InvalidOperationException("Sqrt domain error.");
                stack.Push(Math.Sqrt(sqrtValue));
                break;
            case "abs":
                stack.Push(Math.Abs(stack.Pop()));
                break;
            case "fact":
                var raw = stack.Pop();
                if (raw < 0 || raw > 170) throw new InvalidOperationException("Factorial domain error.");
                if (Math.Abs(raw % 1) > double.Epsilon) throw new InvalidOperationException("Factorial requires integers.");
                stack.Push(Factorial((int)raw));
                break;
            case "neg":
                stack.Push(-stack.Pop());
                break;
            default:
                throw new InvalidOperationException($"Unknown function '{function}'.");
        }
    }

    private static double Factorial(int n)
    {
        double result = 1;
        for (var i = 2; i <= n; i++)
        {
            result *= i;
        }
        return result;
    }
}
