Console.WriteLine("Simple Calculator\n");

// Input first number
Console.Write("Enter the first number: ");
double num1 = Convert.ToDouble(Console.ReadLine());

// Input operator
Console.Write("Enter an operation (+, -, *, /): ");
char operation = Convert.ToChar(Console.ReadLine());

// Input second number
Console.Write("Enter the second number: ");
double num2 = Convert.ToDouble(Console.ReadLine());

IOperation calculation = operation switch
{
    '+' => new Add(num1, num2),
    '-' => new Subtract(num1, num2),
    '*' => new Multiply(num1, num2),
    '/' => new Divide(num1, num2),
    _ => throw new InvalidOperationException("Invalid operation"),
};

double result = calculation.Calculate();
Console.WriteLine($"Result: {result}");
