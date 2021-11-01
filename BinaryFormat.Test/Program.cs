using System.Text;

namespace TestNamespace
{
    [GenerateReaderWriter]
    [LittleEndian]
    public partial class BinaryClass
    {
        public MachSymbolName symbolName;
        public uint a;
        public int b;
        public long c;
    }

    /// <summary>
    /// Represents 16-byte null-terminated UTF-8 string
    /// </summary>
    /// <remarks>
    /// Shows how custom logic can augment the generated reader above
    /// </remarks>
    public class MachSymbolName
    {
        public static MachSymbolName Read(ReadOnlySpan<byte> buffer, out int bytesRead)
        {
            bytesRead = 16;
            return new MachSymbolName
            {
	        Name = Encoding.UTF8.GetString(buffer.Slice(0, 16)).TrimEnd('\0'),
            };
        }

	public string Name { get; set; }
    }
}

class Program
{
    public static void Main(string[] args)
    {
        var bc = TestNamespace.BinaryClass.Read(
            new byte[]
            {
                (byte)'N', (byte)'M', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                1, 0, 0, 0, 2, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0
            },
            out int _);
        Console.WriteLine("Name: " + bc.symbolName.Name);
        Console.WriteLine("a: " + bc.a);
        Console.WriteLine("b: " + bc.b);
        Console.WriteLine("c: " + bc.c);
    }
}