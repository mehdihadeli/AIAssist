namespace Calculator;

/// <summary>
/// Add two value
/// </summary>
/// <param name="number1"></param>
/// <param name="number2"></param>
public class Add(double number1, double number2) : IOperation
{
    public double Result { get; set; }
    public double ResultField;

    public double Calculate()
    {
        Result = AddNumbers(number1, number2);
        ResultField = Result;

        return Result;
    }

    private double AddNumbers(double first, double second)
    {
        return first + second;
    }
}
