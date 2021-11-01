using System.Text;
using Xunit;

namespace BinaryFormat.Test
{
    public class BasicTest
    {
        [Fact]
        void BasicReader()
        {
            var sbc = SimpleBinaryClass.Read(new byte[] { 1, 0, 0, 0, 2, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0 }, out int bytesRead);
            Assert.Equal(16, bytesRead);
            Assert.Equal(1u, sbc.A);
            Assert.Equal(2, sbc.B);
            Assert.Equal(4, sbc.C);
        }

        [Fact]
        void NestedReader()
        {
            var sbc = NestedBinaryClass.Read(new byte[] {
                    (byte)'N', (byte)'M', 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
                    1, 0, 0, 0, 2, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0
                }, out int bytesRead);
            Assert.Equal(32, bytesRead);
            Assert.Equal("NM", sbc.SymbolName.Name);
            Assert.Equal(1u, sbc.A);
            Assert.Equal(2, sbc.B);
            Assert.Equal(4, sbc.C);
        }
    }

    [GenerateReaderWriter]
    [LittleEndian]
    partial class SimpleBinaryClass
    {
        public uint A;
        public int B;
        public long C { get; set; }
    }

    [GenerateReaderWriter]
    [LittleEndian]
    partial class NestedBinaryClass
    {
        public MachSymbolName SymbolName { get; set; }
        public uint A;
        public int B;
        public long C { get; set; }
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
