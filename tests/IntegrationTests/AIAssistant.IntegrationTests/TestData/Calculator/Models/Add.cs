namespace Calculator.Models;
/// <summary>
/// Add two value
/// </summary>
/// <param name="number1"></param>
/// <param name="number2"></param>
public class Add(double number1, double number2) : IOperation
{
    public double Calculate()
    {
        return AddNumbers();
    }
    
    private double AddNumbers()
    {
        return number1 / number2;
    }
}