using System;

namespace SimpleCalculator
{
    /// <summary>
    /// Enum for operations
    /// </summary>
    public enum Operation
    {
        Add,
        Subtract,
        Multiply,
        Divide,
    }

    /// <summary>
    /// Interface for calculator operations
    /// </summary>
    public interface ICalculator
    {
        double Calculate(double a, double b, Operation operation);
    }

    /// <summary>
    /// Struct for holding calculation details
    /// </summary>
    public struct Calculation
    {
        public double Operand1 { get; }
        public double Operand2 { get; }
        public Operation Operation { get; }

        public Calculation(double operand1, double operand2, Operation operation)
        {
            Operand1 = operand1;
            Operand2 = operand2;
            Operation = operation;
        }
    }

    /// <summary>
    /// Record for calculation result
    /// </summary>
    public record CalculationResult(double Result, string Description);

    /// <summary>
    /// Class implementing the ICalculator interface
    /// </summary>
    public class Calculator : ICalculator
    {
        /// <summary>
        /// Calculate the operation
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="operation"></param>
        /// <returns></returns>
        /// <exception cref="DivideByZeroException"></exception>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public double Calculate(double a, double b, Operation operation)
        {
            return operation switch
            {
                Operation.Add => a + b,
                Operation.Subtract => a - b,
                Operation.Multiply => a * b,
                Operation.Divide => b != 0 ? a / b : throw new DivideByZeroException("Cannot divide by zero."),
                _ => throw new ArgumentOutOfRangeException(nameof(operation), operation, null),
            };
        }

        public CalculationResult PerformCalculation(Calculation calc)
        {
            double result = Calculate(calc.Operand1, calc.Operand2, calc.Operation);
            string description = $"{calc.Operand1} {calc.Operation} {calc.Operand2} = {result}";
            return new CalculationResult(result, description);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var calculator = new Calculator();

            Console.WriteLine("Simple Calculator");
            Console.WriteLine("Choose an operation: Add, Subtract, Multiply, Divide");
            string userInput = Console.ReadLine();
            Operation operation;

            if (Enum.TryParse(userInput, true, out operation))
            {
                Console.Write("Enter first number: ");
                double operand1 = Convert.ToDouble(Console.ReadLine());

                Console.Write("Enter second number: ");
                double operand2 = Convert.ToDouble(Console.ReadLine());

                Calculation calculation = new Calculation(operand1, operand2, operation);
                CalculationResult result = calculator.PerformCalculation(calculation);

                Console.WriteLine(result.Description);
            }
            else
            {
                Console.WriteLine("Invalid operation.");
            }
        }
    }
}
