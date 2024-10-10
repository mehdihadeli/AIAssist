namespace BuildingBlocks.UnitTests.TestData.Calculator.Models;

/// <summary>
/// Subtract two values
/// </summary>
/// <param name="number1"></param>
/// <param name="number2"></param>
public class Subtract(double number1, double number2) : IOperation
{
    public double Calculate()
    {
        return number1 - number2;
    }
}
