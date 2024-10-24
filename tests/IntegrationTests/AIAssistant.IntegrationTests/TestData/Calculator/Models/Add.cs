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
    public double ResultField2;

    /// <summary>
    /// Calculates the sum of two numbers and updates the result field.
    /// </summary>
    /// <returns>The result of the addition as a double.</returns>
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

/// <summary>
/// A utility class for performing addition operations on Add class.
/// </summary>
public class AddUtils
{
    /// <summary>
    /// Calculates the sum of a list of integers.
    /// </summary>
    /// <param name="numbers">A list of integers to be summed.</param>
    /// <returns>The total sum of the integers in the list.</returns>
    private double BulkAdd(IList<int> numbers)
    {
        return numbers.Sum();
    }
}
