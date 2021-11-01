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

    public enum CodeDirectoryVersion : int
    {
        Baseline = 0x20001,
        SupportsScatter = 0x20100,
        SupportsTeamId = 0x20200,
        SupportsCodeLimit64 = 0x20300,
        SupportsExecSegment = 0x20400,
        SupportsPreEncrypt = 0x20500,
        HighestVersion = SupportsExecSegment, // TODO: We don't support pre-encryption yet
    }

    [GenerateReaderWriter]
    public partial class CodeDirectoryHeader
    {
        public uint Magic;
        public uint Size;
        public CodeDirectoryVersion Version;
        public uint Flags;
        public uint HashesOffset;
        public uint IdentifierOffset;
        public uint SpecialSlotCount;
        public uint CodeSlotCount;
        public uint ExecutableLength;
        public byte HashSize;
        public byte HashType;
        public byte Platform;
        public byte Log2PageSize;
        public uint Reserved;
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