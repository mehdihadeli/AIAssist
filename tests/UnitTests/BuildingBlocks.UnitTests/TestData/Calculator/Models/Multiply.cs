namespace BuildingBlocks.UnitTests.TestData.Calculator.Models;

/// <summary>
/// Multiply two values
/// </summary>
/// <param name="number1"></param>
/// <param name="number2"></param>
public class Multiply(double number1, double number2) : IOperation
{
    public double Calculate()
    {
        return number1 * number2;
    }
}
