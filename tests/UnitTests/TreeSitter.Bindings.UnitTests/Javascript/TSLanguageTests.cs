using System.Runtime.InteropServices;
using Xunit;

namespace TreeSitter.Bindings.Javascript.UnitTests;

/// <summary>Provides validation of the <see cref="TSLanguage" /> struct.</summary>
public static unsafe partial class TSLanguageTests
{
    /// <summary>Validates that the <see cref="TSLanguage" /> struct is blittable.</summary>
    [Fact]
    public static void IsBlittableTest()
    {
        Assert.Equal(sizeof(TSLanguage), Marshal.SizeOf<TSLanguage>());
    }

    /// <summary>Validates that the <see cref="TSLanguage" /> struct has the right <see cref="LayoutKind" />.</summary>
    [Fact]
    public static void IsLayoutSequentialTest()
    {
        Assert.True(typeof(TSLanguage).IsLayoutSequential);
    }

    /// <summary>Validates that the <see cref="TSLanguage" /> struct has the correct size.</summary>
    [Fact]
    public static void SizeOfTest()
    {
        Assert.Equal(1, sizeof(TSLanguage));
    }
}
