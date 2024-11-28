using AIAssist.Contracts.Diff;
using AIAssist.Diff;
using BuildingBlocks.SpectreConsole;
using FluentAssertions;
using Spectre.Console;

namespace AIAssistant.UnitTests.Diff;

public class CodeDiffUpdaterTests() : BaseTest("Project")
{
    private readonly ICodeDiffUpdater _updater = new CodeDiffUpdater(
        new SpectreUtilities(ThemeLoader.LoadTheme()!, AnsiConsole.Console)
    );
    private readonly ICodeDiffParser _parser = new UnifiedDiffParser();

    protected override IList<TestFile> TestFiles =>
        new List<TestFile>
        {
            new()
            {
                Path = Path.Combine(AppDir, "Person.cs"),
                Content =
                    @"namespace Project
{
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }

        public Person(string name, int age)
        {
            Name = name;
            Age = age;
        }

        public string GetDetails()
        {
            return $""Name: {Name}, Age: {Age}"";
        }

        public void UpdateAge(int newAge)
        {
            if (newAge > 0)
            {
                Age = newAge;
            }
            else
            {
                throw new ArgumentException(""Age must be positive."");
            }
        }
    }
}",
            },
            new()
            {
                Path = Path.Combine(AppDir, "Statistics.cs"),
                Content =
                    @"using System;
using System.Collections.Generic;

public class Statistics
{
    public double CalculateAverage(List<int> numbers)
    {
        int sum = Sum(numbers);
        return sum / (double)numbers.Count;
    }

    private int Sum(List<int> numbers)
    {
        int total = 0;
        foreach (int number in numbers)
        {
            total += number;
        }
        return total;
    }
}",
            },
        };

    [Fact]
    public void ApplyChanges_Person_ShouldInvokeUpdaterWithParsedChanges()
    {
        // Arrange
        string diff =
            @"```diff
--- Project/Person.cs
+++ Project/Person.cs
@@ -5,6 +5,7 @@
         public int Age { get; set; }
+        public string Address { get; set; }  // New property added

         public Person(string name, int age)
         {
             Name = name;
             Age = age;
+            Address = """";  // Default empty address
         }

@@ -13,6 +14,7 @@
         public string GetDetails()
         {
-            return $""Name: {Name}, Age: {Age}"";
+            return $""Name: {Name}, Age: {Age}, Address: {Address}"";  // Modified to include Address
         }

         public void UpdateAge(int newAge)
         {
             if (newAge > 0)
             {
                 Age = newAge;
             }
             else
             {
                 throw new ArgumentException(""Age must be positive."");
             }
         }
+        public void UpdateAddress(string newAddress)  // New method added
+        {
+            Address = newAddress;
+        }
     }
 }
```";

        // Define expected parsed changes
        var expectedChanges = _parser.ParseDiffResults(diff, WorkingDir);

        // Act
        _updater.ApplyChanges(expectedChanges, WorkingDir);

        var filePath = Path.Combine(AppDir, "Person.cs");
        File.Exists(filePath).Should().BeTrue();

        // Verify the content of the file
        var content = File.ReadAllText(filePath);

        var expectedContent =
            @"namespace Project
{
    public class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Address { get; set; }  // New property added

        public Person(string name, int age)
        {
            Name = name;
            Age = age;
            Address = """";  // Default empty address
        }

        public string GetDetails()
        {
            return $""Name: {Name}, Age: {Age}, Address: {Address}"";  // Modified to include Address
        }

        public void UpdateAge(int newAge)
        {
            if (newAge > 0)
            {
                Age = newAge;
            }
            else
            {
                throw new ArgumentException(""Age must be positive."");
            }
        }

        public void UpdateAddress(string newAddress)  // New method added
        {
            Address = newAddress;
        }
    }
}";

        content.Should().BeEquivalentTo(expectedContent);
    }

    [Fact]
    public void ApplyChanges_Hello_ShouldCreateNewFile_WhenFileIsAdded()
    {
        // Arrange: Diff representing a new file creation with content
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

        var expectedChanges = _parser.ParseDiffResults(diff, WorkingDir);

        // Act: Apply the changes using the updater
        _updater.ApplyChanges(expectedChanges, WorkingDir);

        var filePath = Path.Combine(AppDir, "hello.cs");
        File.Exists(filePath).Should().BeTrue();

        // Verify the content of the file
        var content = File.ReadAllText(filePath);

        var expectedContent =
            @"using System;

namespace Project
{
    class Hello
    {
        static void Main()
        {
            Console.WriteLine(""Hello, World!"");
        }
    }
}
";

        content.Should().BeEquivalentTo(expectedContent);
    }

    [Fact]
    public void ApplyChanges_Statistics_ShouldCreateNewFile_WhenFileIsAdded()
    {
        string diff =
            @"```diff
--- Project/Statistics.cs
+++ Project/Statistics.cs
@@ -4,18 +4,7 @@
public class Statistics
{
     public double CalculateAverage(List<int> numbers)
     {
-        int sum = Sum(numbers);
-        return sum / (double)numbers.Count;
+        return numbers.Average();
     }

-    private int Sum(List<int> numbers)
-    {
-        int total = 0;
-        foreach (int number in numbers)
-        {
-            total += number;
-        }
-        return total;
-    }
+    
}
```";

        // Define expected parsed changes
        var expectedChanges = _parser.ParseDiffResults(diff, WorkingDir);

        // Act
        _updater.ApplyChanges(expectedChanges, WorkingDir);

        // Assert: Verify that the file is created
        var filePath = Path.Combine(AppDir, "Statistics.cs");
        File.Exists(filePath).Should().BeTrue();

        // Verify the content of the file
        var content = File.ReadAllText(filePath);

        var expectedContent =
            @"using System;
using System.Collections.Generic;
using System.Linq;

public class Statistics
{
    public double CalculateAverage(List<int> numbers)
    {
        return numbers.Average();
    }
}
";

        content.Should().BeEquivalentTo(expectedContent);
    }
}
