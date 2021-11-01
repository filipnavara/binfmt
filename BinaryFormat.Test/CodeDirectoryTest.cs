using System.Text;

namespace BinaryFormat.Test
{
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
