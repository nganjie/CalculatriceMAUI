using System.Globalization;
using System.Text;

namespace CalculatriceMAUI
{
    public enum Operation
    {
        None,
        Add,
        Subtract,
        Multiply,
        Divide
    }

    /// <summary>
    /// Logique de la calculatrice, indépendante de l'interface.
    /// La saisie en cours est conservée sous forme de texte (séparateur ".")
    /// et les calculs sont effectués en decimal pour éviter les erreurs d'arrondi
    /// classiques du type 0,1 + 0,2 = 0,30000000000000004.
    /// </summary>
    public class CalculatorEngine
    {
        private const int MaxDigits = 15;
        private const int DisplayDecimals = 10;
        private const char GroupSeparator = ' ';
        private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

        private string entry = "0";
        private decimal? exactValue;
        private decimal accumulator;
        private Operation pending = Operation.None;
        private Operation lastOperation = Operation.None;
        private decimal lastOperand;
        private bool newEntry = true;
        private bool operandEntered;
        private bool evaluated;
        private decimal memory;

        public string Expression { get; private set; } = string.Empty;
        public bool HasError { get; private set; }
        public string ErrorMessage { get; private set; } = string.Empty;
        public bool HasMemory => memory != 0;

        public string Display => HasError ? ErrorMessage : FormatEntry(entry);

        private decimal Current => exactValue ?? decimal.Parse(entry, NumberStyles.Float, Inv);

        public void InputDigit(int digit)
        {
            if (HasError)
                Clear();

            if (evaluated)
                StartNewCalculation();

            if (newEntry)
            {
                if (pending == Operation.None)
                    Expression = string.Empty;

                entry = digit.ToString(Inv);
                exactValue = null;
                newEntry = false;
                operandEntered = true;
                return;
            }

            if (CountDigits(entry) >= MaxDigits)
                return;

            if (entry == "0")
                entry = digit.ToString(Inv);
            else if (entry == "-0")
                entry = "-" + digit.ToString(Inv);
            else
                entry += digit.ToString(Inv);

            exactValue = null;
            operandEntered = true;
        }

        public void InputDecimalPoint()
        {
            if (HasError)
                Clear();

            if (evaluated)
                StartNewCalculation();

            if (newEntry)
            {
                if (pending == Operation.None)
                    Expression = string.Empty;

                entry = "0.";
                exactValue = null;
                newEntry = false;
                operandEntered = true;
                return;
            }

            if (entry.Contains('.') || entry.Contains('E'))
                return;

            entry += ".";
            exactValue = null;
            operandEntered = true;
        }

        public void SetOperation(Operation operation)
        {
            if (HasError || operation == Operation.None)
                return;

            if (pending != Operation.None && operandEntered && !evaluated)
            {
                Expression = $"{Format(accumulator)} {Symbol(pending)} {Format(Current)}";
                if (!TryApply(accumulator, pending, Current, out decimal result))
                    return;

                accumulator = result;
                ShowValue(result);
            }
            else if (pending == Operation.None || evaluated)
            {
                accumulator = Current;
            }

            pending = operation;
            evaluated = false;
            newEntry = true;
            operandEntered = false;
            Expression = $"{Format(accumulator)} {Symbol(operation)}";
        }

        public void Evaluate()
        {
            if (HasError)
                return;

            decimal left;
            decimal right;
            Operation operation;

            if (evaluated && lastOperation != Operation.None)
            {
                // Un nouvel appui sur "=" répète la dernière opération.
                left = Current;
                operation = lastOperation;
                right = lastOperand;
            }
            else if (pending != Operation.None)
            {
                left = accumulator;
                operation = pending;
                right = Current;
            }
            else
            {
                Expression = $"{Format(Current)} =";
                evaluated = true;
                newEntry = true;
                return;
            }

            Expression = $"{Format(left)} {Symbol(operation)} {Format(right)} =";
            if (!TryApply(left, operation, right, out decimal result))
                return;

            lastOperation = operation;
            lastOperand = right;
            pending = Operation.None;
            ShowValue(result);
            evaluated = true;
            newEntry = true;
            operandEntered = false;
        }

        public void Clear()
        {
            entry = "0";
            exactValue = null;
            accumulator = 0;
            pending = Operation.None;
            lastOperation = Operation.None;
            lastOperand = 0;
            newEntry = true;
            operandEntered = false;
            evaluated = false;
            HasError = false;
            ErrorMessage = string.Empty;
            Expression = string.Empty;
        }

        public void Backspace()
        {
            if (HasError)
            {
                Clear();
                return;
            }

            if (evaluated)
            {
                // Après "=", on efface seulement la ligne de l'opération.
                Expression = string.Empty;
                evaluated = false;
                lastOperation = Operation.None;
                return;
            }

            if (newEntry)
                return;

            exactValue = null;
            if (entry.Contains('E'))
                entry = "0";
            else
                entry = entry[..^1];

            if (entry.Length == 0 || entry == "-" || entry == "-0")
                entry = "0";
        }

        public void ToggleSign()
        {
            if (HasError)
                return;

            if (newEntry && !evaluated)
            {
                if (pending == Operation.None)
                    Expression = string.Empty;

                entry = "-0";
                exactValue = null;
                newEntry = false;
                operandEntered = true;
                return;
            }

            entry = entry.StartsWith('-') ? entry[1..] : "-" + entry;
            if (exactValue.HasValue)
                exactValue = -exactValue.Value;
            if (entry == "-0")
                entry = "0";
            operandEntered = true;
        }

        public void Percent()
        {
            if (HasError)
                return;

            decimal value = Current;
            decimal result;

            if (pending is Operation.Add or Operation.Subtract && !evaluated)
            {
                // 200 + 10 % => 200 + 20
                if (!TryCompute(() => accumulator * value / 100m, out result))
                    return;
                Expression = $"{Format(accumulator)} {Symbol(pending)} {Format(result)}";
            }
            else
            {
                result = value / 100m;
                Expression = pending != Operation.None && !evaluated
                    ? $"{Format(accumulator)} {Symbol(pending)} {Format(result)}"
                    : $"{Format(value)} %";
            }

            if (evaluated)
            {
                evaluated = false;
                lastOperation = Operation.None;
            }

            ShowValue(result);
            newEntry = true;
            operandEntered = true;
        }

        public void SquareRoot()
        {
            ApplyUnary("√", value =>
            {
                if (value < 0)
                {
                    SetError("Entrée invalide");
                    return null;
                }
                return (decimal)Math.Sqrt((double)value);
            });
        }

        public void Square()
        {
            ApplyUnary("sqr", value =>
            {
                decimal? result = null;
                if (TryCompute(() => value * value, out decimal square))
                    result = square;
                return result;
            });
        }

        public void Inverse()
        {
            ApplyUnary("1/", value =>
            {
                if (value == 0)
                {
                    SetError("Division par zéro impossible");
                    return null;
                }

                decimal? result = null;
                if (TryCompute(() => 1m / value, out decimal inverse))
                    result = inverse;
                return result;
            });
        }

        public void MemoryClear() => memory = 0;

        public void MemoryRecall()
        {
            if (HasError)
                Clear();

            if (evaluated)
                StartNewCalculation();

            ShowValue(memory);
            newEntry = true;
            operandEntered = true;
        }

        public void MemoryAdd() => UpdateMemory(1);

        public void MemorySubtract() => UpdateMemory(-1);

        private void UpdateMemory(int sign)
        {
            if (HasError)
                return;

            decimal value = Current;
            if (TryCompute(() => memory + sign * value, out decimal result))
            {
                memory = result;
                newEntry = true;
            }
        }

        private void ApplyUnary(string label, Func<decimal, decimal?> function)
        {
            if (HasError)
                return;

            decimal value = Current;
            string text = label == "1/" ? $"1/({Format(value)})" : $"{label}({Format(value)})";
            Expression = pending != Operation.None && !evaluated
                ? $"{Format(accumulator)} {Symbol(pending)} {text}"
                : text;

            decimal? result = function(value);
            if (result is null)
                return;

            if (evaluated)
            {
                evaluated = false;
                lastOperation = Operation.None;
            }

            ShowValue(result.Value);
            newEntry = true;
            operandEntered = true;
        }

        private void StartNewCalculation()
        {
            evaluated = false;
            pending = Operation.None;
            lastOperation = Operation.None;
            Expression = string.Empty;
        }

        private bool TryApply(decimal left, Operation operation, decimal right, out decimal result)
        {
            if (operation == Operation.Divide && right == 0)
            {
                result = 0;
                SetError("Division par zéro impossible");
                return false;
            }

            return TryCompute(() => operation switch
            {
                Operation.Add => left + right,
                Operation.Subtract => left - right,
                Operation.Multiply => left * right,
                Operation.Divide => left / right,
                _ => right
            }, out result);
        }

        private bool TryCompute(Func<decimal> computation, out decimal result)
        {
            try
            {
                result = computation();
                return true;
            }
            catch (OverflowException)
            {
                result = 0;
                SetError("Dépassement de capacité");
                return false;
            }
        }

        private void SetError(string message)
        {
            HasError = true;
            ErrorMessage = message;
            pending = Operation.None;
            lastOperation = Operation.None;
            evaluated = false;
            newEntry = true;
            operandEntered = false;
            entry = "0";
            exactValue = null;
        }

        private void ShowValue(decimal value)
        {
            exactValue = value;
            entry = ToEntryString(value);
        }

        private static string ToEntryString(decimal value)
        {
            decimal abs = Math.Abs(value);
            if (abs >= 1e15m || (abs != 0 && abs < 1e-10m))
                return ((double)value).ToString("0.#########E+0", Inv);

            string text = Math.Round(value, DisplayDecimals).ToString("0.##########", Inv);
            return text == "-0" ? "0" : text;
        }

        private static string Format(decimal value) => FormatEntry(ToEntryString(value));

        private static string FormatEntry(string text)
        {
            if (text.Contains('E'))
                return text.Replace('.', ',');

            bool negative = text.StartsWith('-');
            if (negative)
                text = text[1..];

            int dot = text.IndexOf('.');
            string integerPart = dot >= 0 ? text[..dot] : text;
            string decimalPart = dot >= 0 ? text[dot..].Replace('.', ',') : string.Empty;

            var builder = new StringBuilder();
            for (int i = 0; i < integerPart.Length; i++)
            {
                if (i > 0 && (integerPart.Length - i) % 3 == 0)
                    builder.Append(GroupSeparator);
                builder.Append(integerPart[i]);
            }

            return (negative ? "-" : string.Empty) + builder + decimalPart;
        }

        private static int CountDigits(string text) => text.Count(char.IsDigit);

        private static string Symbol(Operation operation) => operation switch
        {
            Operation.Add => "+",
            Operation.Subtract => "−",
            Operation.Multiply => "×",
            Operation.Divide => "÷",
            _ => string.Empty
        };
    }
}
