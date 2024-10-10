namespace Calculator;

/// <summary>
/// Subtract two values
/// </summary>
/// <param name="number1"></param>
/// <param name="number2"></param>
public class Subtract(double number1, double number2) : IOperation
{
    public double Result { get; set; }
    public double ResultField;

    public double Calculate()
    {
        Result = number1 - number2;
        ResultField = Result;

        return Result;
    }
}
