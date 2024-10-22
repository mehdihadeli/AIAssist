namespace Calculator;

/// <summary>
/// Divide to values
/// </summary>
/// <param name="number1"></param>
/// <param name="number2"></param>
public class Divide(double number1, double number2) : IOperation
{
    public double Result { get; set; }
    public double ResultField;

    public double Calculate()
    {
        Result = DivideNumbers();
        ResultField = Result;

        return Result;
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
