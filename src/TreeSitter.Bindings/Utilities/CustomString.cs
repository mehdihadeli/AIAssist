using System.Runtime.InteropServices;
using System.Text;

namespace TreeSitter.Bindings.Utilities;

public unsafe class GeneratedCString
{
    // Pointer to the internal sbyte value
    public sbyte* Value { get; private set; }

    // Length of the string
    public int Length { get; private set; }

    // Constructor
    public GeneratedCString(string value)
    {
        // Convert the string to a byte array (UTF-8 encoding)
        byte[] byteArray = Encoding.UTF8.GetBytes(value);
        Length = byteArray.Length;

        // Allocate memory for sbyte array
        Value = (sbyte*)System.Runtime.InteropServices.Marshal.AllocHGlobal(Length);

        // Copy bytes to sbyte pointer
        for (int i = 0; i < Length; i++)
        {
            Value[i] = (sbyte)byteArray[i];
        }
    }

    public GeneratedCString(sbyte* sbytePointer)
    {
        if (sbytePointer == null)
        {
            return;
        }

        // Get the length of the string (assuming it's null-terminated)
        int length = 0;
        while (System.Runtime.InteropServices.Marshal.ReadByte((IntPtr)sbytePointer, length) != 0)
        {
            length++;
        }

        // Convert byte array to string using the desired encoding
        Value = sbytePointer;
        Length = length;
    }

    // Destructor to free allocated memory
    ~GeneratedCString()
    {
        // Free allocated memory
        System.Runtime.InteropServices.Marshal.FreeHGlobal((IntPtr)Value);
    }

    // Implicit conversion to sbyte*
    public static implicit operator sbyte*(in GeneratedCString value) => value.Value;

    // Implicit conversion from string to GeneratedCString
    public static implicit operator string(GeneratedCString value) => value.ToString();

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

    // Example method to get the length of the GeneratedCString
    public int GetLength()
    {
        return Length;
    }
}
