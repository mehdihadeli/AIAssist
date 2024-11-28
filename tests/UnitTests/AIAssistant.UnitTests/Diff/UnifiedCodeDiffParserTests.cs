using AIAssist.Diff;
using AIAssist.Models;
using FluentAssertions;

namespace AIAssistant.UnitTests.Diff;

public class UnifiedCodeDiffParserTests() : BaseTest("Project")
{
    private readonly UnifiedDiffParser _parser = new();

    [Fact]
    public void GetFileChanges_For_Statistics_ShouldParseMethodRefactoringAndImportAddition_Correctly()
    {
        // Arrange
        string diff =
            @"```diff
--- Project/Statistics.cs
+++ Project/Statistics.cs
@@ -1,5 +1,6 @@
using System;
using System.Collections.Generic;
+using System.Linq;

public class Statistics
{
-    public double CalculateAverage(List<int> numbers)
-    {
-        int sum = Sum(numbers);
-        return sum / (double)numbers.Count;
-    }
+    public double CalculateAverage(List<int> numbers) => numbers.Average();
-    private int Sum(List<int> numbers)
-    {
-        int total = 0;
-        foreach (int number in numbers)
-        {
-            total += number;
-        }
-        return total;
-    }
}
```";

        // Act: Parse the diff
        var fileChanges = _parser.ParseDiffResult(diff, WorkingDir);

        fileChanges.Replacements.Should().HaveCount(1);
        var fileChange = fileChanges.First();

        // Assert: Verify file change details
        fileChange.FilePath.Should().Be("Project/Statistics.cs");
        fileChange.FileCodeChangeType.Should().Be(ActionType.Update);

        // Assert: Verify change lines
        var changeLines = fileChange.ChangeLines;
        changeLines.Should().HaveCount(22); // Expect 10 changes in total (lines added, removed, and modified)

        changeLines[2].Content.Should().Be("using System.Linq;");
        changeLines[2].LineCodeChangeType.Should().Be(LineItemChangeType.Add);
        changeLines[2].LineNumber.Should().Be(3);

        changeLines[3].Content.Should().Be("");
        changeLines[3].LineCodeChangeType.Should().Be(LineItemChangeType.Unchanged);
        changeLines[3].LineNumber.Should().Be(4);

        changeLines[6].Content.Should().Be("    public double CalculateAverage(List<int> numbers)");
        changeLines[6].LineCodeChangeType.Should().Be(LineItemChangeType.Delete);
        changeLines[6].LineNumber.Should().Be(7);

        changeLines[11]
            .Content.Should()
            .Be("    public double CalculateAverage(List<int> numbers) => numbers.Average();");
        changeLines[11].LineCodeChangeType.Should().Be(LineItemChangeType.Add);
        changeLines[11].LineNumber.Should().Be(12);
    }

    [Fact]
    public void GetFileChanges_For_Hello_ShouldParseNewFileAddition_Correctly()
    {
        string diff =
            @"```diff
--- /dev/null
+++ Project/hello.cs
@@ -0,0 +1,10 @@
+using System;
+
+namespace Project
+{
+    class Hello
+    {
+        static void Main()
+        {
+            Console.WriteLine(""Hello, World!"");
+        }
+    }
+}
```";

        // Act: Parse the diff
        var fileChanges = _parser.GetFileChanges(diff);

        // Assert: Verify that one file change was parsed (new file creation)
        fileChanges.Should().HaveCount(1);

        var fileChange = fileChanges[0];

        // Assert: Verify file change details
        fileChange.FilePath.Should().Be("Project/hello.cs");
        fileChange.FileCodeChangeType.Should().Be(ActionType.Add);

        // Assert: Verify the lines that were added to the new file
        var changeLines = fileChange.ChangeLines;
        changeLines.Should().HaveCount(12); // Expecting 10 lines added

        changeLines.All(x => x.LineCodeChangeType == LineItemChangeType.Add).Should().BeTrue();

        // Assert each line of the newly added file
        changeLines[0].Content.Should().Be("using System;");
        changeLines[0].LineCodeChangeType.Should().Be(LineItemChangeType.Add);
        changeLines[0].LineNumber.Should().Be(0);

        changeLines[1].Content.Should().Be("");
        changeLines[1].LineCodeChangeType.Should().Be(LineItemChangeType.Add);
        changeLines[1].LineNumber.Should().Be(1);

        changeLines[2].Content.Should().Be("namespace Project");
        changeLines[2].LineCodeChangeType.Should().Be(LineItemChangeType.Add);
        changeLines[2].LineNumber.Should().Be(2);

        changeLines[3].Content.Should().Be("{");
        changeLines[3].LineCodeChangeType.Should().Be(LineItemChangeType.Add);
        changeLines[3].LineNumber.Should().Be(3);

        changeLines[4].Content.Should().Be("    class Hello");
        changeLines[4].LineCodeChangeType.Should().Be(LineItemChangeType.Add);

        changeLines[5].Content.Should().Be("    {");
        changeLines[5].LineCodeChangeType.Should().Be(LineItemChangeType.Add);

        changeLines[6].Content.Should().Be("        static void Main()");
        changeLines[6].LineCodeChangeType.Should().Be(LineItemChangeType.Add);

        changeLines[7].Content.Should().Be("        {");
        changeLines[7].LineCodeChangeType.Should().Be(LineItemChangeType.Add);

        changeLines[8].Content.Should().Be("            Console.WriteLine(\"Hello, World!\");");
        changeLines[8].LineCodeChangeType.Should().Be(LineItemChangeType.Add);

        changeLines[9].Content.Should().Be("        }");
        changeLines[9].LineCodeChangeType.Should().Be(LineItemChangeType.Add);
        changeLines[9].LineNumber.Should().Be(9);
    }

    [Fact]
    public void GetFileChanges_For_InventoryItem_ShouldParseFullUnifiedDiffWithMultipleHunks()
    {
        // Arrange
        string diff =
            @"```diff
--- InventoryItem.cs
+++ InventoryItem.cs
@@ -5,7 +5,6 @@
     public int ItemId { get; private set; }
     public string Name { get; private set; }
     public string Description { get; set; }
     public int QuantityInStock { get; private set; }
-    public decimal Price { get; set; }
+    public decimal UnitCost { get; set; }
     public string Supplier { get; set; }

@@ -13,8 +12,8 @@
     public InventoryItem(int itemId, string name, decimal price, string description = "", int quantityInStock = 0)
     {
         ItemId = itemId;
         Name = name;
-        Price = price;
-        Description = description;
+        UnitCost = unitCost;
         QuantityInStock = quantityInStock;
         Supplier = supplier;
     }

@@ -46,8 +45,7 @@
     public override string ToString()
     {
         return $""Item ID: {ItemId}\nName: {Name}\n"" +
-           $""Description: {Description}\n"" +
-           $""Price: {Price:C}\nQuantity in Stock: {QuantityInStock}\n"" +
+           $""Supplier: {Supplier}\nUnit Cost: {UnitCost:C}\n"" +
            $""Quantity in Stock: {QuantityInStock}\nTotal Value: {CalculateTotalValue():C}"";
     }
 }
```";

        // Act
        var fileChanges = _parser.GetFileChanges(diff);

        // Assert
        fileChanges.Should().HaveCount(1);

        var fileChange = fileChanges.First();
        fileChange.FilePath.Should().Be("InventoryItem.cs");
        fileChange.FileCodeChangeType.Should().Be(ActionType.Update);
        fileChange.ChangeLines.Should().HaveCount(28);

        // Hunk 1
        fileChange.ChangeLines[0].Content.Should().Be("     public int ItemId { get; private set; }");
        fileChange.ChangeLines[0].LineCodeChangeType.Should().Be(LineItemChangeType.Unchanged);
        fileChange.ChangeLines[0].LineNumber.Should().Be(5);

        fileChange.ChangeLines[4].Content.Should().Be("    public decimal Price { get; set; }");
        fileChange.ChangeLines[4].LineCodeChangeType.Should().Be(LineItemChangeType.Delete);
        fileChange.ChangeLines[4].LineNumber.Should().Be(9);

        fileChange.ChangeLines[5].Content.Should().Be("    public decimal UnitCost { get; set; }");
        fileChange.ChangeLines[5].LineCodeChangeType.Should().Be(LineItemChangeType.Add);
        fileChange.ChangeLines[5].LineNumber.Should().Be(10);

        // Hunk 2
        fileChange.ChangeLines[8].LineCodeChangeType.Should().Be(LineItemChangeType.Unchanged);
        fileChange
            .ChangeLines[8]
            .Content.Should()
            .Be(
                "     public InventoryItem(int itemId, string name, decimal price, string description = \", int quantityInStock = 0)"
            );
        fileChange.ChangeLines[8].LineNumber.Should().Be(13);

        fileChange.ChangeLines[12].LineCodeChangeType.Should().Be(LineItemChangeType.Delete);
        fileChange.ChangeLines[12].Content.Should().Be("        Price = price;");
        fileChange.ChangeLines[12].LineNumber.Should().Be(17);

        // Hunk 3
        fileChange.ChangeLines[19].LineCodeChangeType.Should().Be(LineItemChangeType.Unchanged);
        fileChange.ChangeLines[19].Content.Should().Be("     public override string ToString()");
        fileChange.ChangeLines[19].LineNumber.Should().Be(46);

        fileChange.ChangeLines[24].LineCodeChangeType.Should().Be(LineItemChangeType.Add);
        fileChange
            .ChangeLines[24]
            .Content.Should()
            .Be("           $\"Supplier: {Supplier}\\nUnit Cost: {UnitCost:C}\\n\" +");
        fileChange.ChangeLines[24].LineNumber.Should().Be(51);
    }
}
