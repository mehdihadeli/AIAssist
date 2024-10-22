using System.Runtime.InteropServices;
using System.Text;

namespace TreeSitter.Bindings.Utilities;

public unsafe class GeneratedCString
{
    // Pointer to the internal sbyte value
    public sbyte* Value { get; }

    // Length of the string
    public int Length { get; }

    // Constructor - takes ownership of the allocated memory
    public GeneratedCString(string value)
    {
        byte[] byteArray = Encoding.UTF8.GetBytes(value);
        Length = byteArray.Length;

        // Allocate memory for sbyte array
        Value = (sbyte*)Marshal.AllocHGlobal(Length);

        // Copy bytes to sbyte pointer
        for (int i = 0; i < Length; i++)
        {
            Value[i] = (sbyte)byteArray[i];
        }
    }

    // Constructor - assumes ownership of sbyte pointer
    public GeneratedCString(sbyte* sbytePointer)
    {
        if (sbytePointer == null)
        {
            throw new ArgumentNullException(nameof(sbytePointer));
        }

        int length = 0;
        while (Marshal.ReadByte((IntPtr)sbytePointer, length) != 0)
        {
            length++;
        }

        // Take ownership of the pointer
        Value = sbytePointer;
        Length = length;
    }

    // Implicit conversion to sbyte*
    public static implicit operator sbyte*(in GeneratedCString value) => value.Value;

    // Implicit conversion from string to GeneratedCString
    public static implicit operator string(GeneratedCString value) => value.ToString();

    // Implicit conversion from sbyte* to GeneratedCString
    public static implicit operator GeneratedCString(sbyte* value) => new(value);

    // Override ToString to return the internal string representation
    public override string ToString()
    {
        byte[] byteArray = new byte[Length];
        for (int i = 0; i < Length; i++)
        {
            byteArray[i] = (byte)Value[i];
        }
        return Encoding.UTF8.GetString(byteArray);
    }
}
