namespace Calculator.Models;

/// <summary>
/// Divide to values
/// </summary>
/// <param name="number1"></param>
/// <param name="number2"></param>
public class Divide(double number1, double number2) : IOperation
{
    public double Calculate()
    {
       return DivideNumbers();
    }
    
    private double DivideNumbers()
    {
        if (number1 == 0)
        {
            throw new DivideByZeroException("Cannot divide by zero.");
        }
        return number1 / number2;
    }
}