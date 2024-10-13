using BuildingBlocks.Utils;

namespace TreeSitter.Bindings.UnitTests.TestData;

public static class TestDataConstants
{
    public static class CalculatorApp
    {
        // Private constants for file names
        private const string ProgramFile = "Program.cs";
        private const string CalculatorCsprojFile = "Calculator.csproj";
        private const string OperationFile = "IOperation.cs";
        private const string AddFile = "Add.cs";
        private const string SubtractFile = "Subtract.cs";
        private const string DivideFile = "Divide.cs";
        private const string MultiplyFile = "Multiply.cs";
        private const string TestDataFolder = "TestData/Calculator";

        // Public constants for relative file paths
        public const string AddRelativeFilePath = "Models/Add.cs";
        public const string SubtractRelativeFilePath = "Models/Subtract.cs";
        public const string MultiplyRelativeFilePath = "Models/Multiply.cs";
        public const string DivideRelativeFilePath = "Models/Divide.cs";
        public const string ProgramRelativeFilePath = "Program.cs";
        public const string OperationRelativeFilePath = "IOperation.cs";
        public const string CsprojRelativeFilePath = "Calculator.csproj";

        // Static members for file initialization
        public static string ProgramContentFile => FilesUtilities.RenderTemplate(TestDataFolder, ProgramFile, null);
        public static string CalculatorCsprojContentFile =>
            FilesUtilities.RenderTemplate(TestDataFolder, CalculatorCsprojFile, null);
        public static string OperationContentFile => FilesUtilities.RenderTemplate(TestDataFolder, OperationFile, null);
        public static string AddContentFile => FilesUtilities.RenderTemplate(TestDataFolder + "/Models", AddFile, null);
        public static string DivideContentFile =>
            FilesUtilities.RenderTemplate(TestDataFolder + "/Models", DivideFile, null);
        public static string MultiplyContentFile =>
            FilesUtilities.RenderTemplate(TestDataFolder + "/Models", MultiplyFile, null);
        public static string SubtractContentFile =>
            FilesUtilities.RenderTemplate(TestDataFolder + "/Models", SubtractFile, null);
    }

    public static class SimpleCalculatorApp
    {
        // Private constants for file names
        private const string TestDataFolder = "TestData/SimpleCalculator";
        private const string ProgramFile = "Program.cs";

        // Public constants for relative file paths
        public const string ProgramRelativeFilePath = "Program.cs";

        public static string ProgramContentFile => FilesUtilities.RenderTemplate(TestDataFolder, ProgramFile, null);
    }
}
