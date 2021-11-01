# Playground for C# source generator to (de)compose binary blobs

Input:
```csharp
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
```
Output:
```csharp
    public partial class CodeDirectoryHeader
    {
        public const int BinarySize = 44;

        public static CodeDirectoryHeader ReadLittleEndian(ReadOnlySpan<byte> buffer, out int bytesRead)
        {
            var result = new CodeDirectoryHeader
            {
                Magic = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(0, 4)),
                Size = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(4, 4)),
                Version = (CodeDirectoryVersion)BinaryPrimitives.ReadInt32LittleEndian(buffer.Slice(8, 4)),
                Flags = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(12, 4)),
                HashesOffset = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(16, 4)),
                IdentifierOffset = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(20, 4)),
                SpecialSlotCount = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(24, 4)),
                CodeSlotCount = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(28, 4)),
                ExecutableLength = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(32, 4)),
                HashSize = buffer[36],
                HashType = buffer[37],
                Platform = buffer[38],
                Log2PageSize = buffer[39],
                Reserved = BinaryPrimitives.ReadUInt32LittleEndian(buffer.Slice(40, 4)),
            };
            bytesRead = 44;
            return result;
        }

        public static CodeDirectoryHeader ReadBigEndian(ReadOnlySpan<byte> buffer, out int bytesRead)
        {
            var result = new CodeDirectoryHeader
            {
                Magic = BinaryPrimitives.ReadUInt32BigEndian(buffer.Slice(0, 4)),
                Size = BinaryPrimitives.ReadUInt32BigEndian(buffer.Slice(4, 4)),
                Version = (CodeDirectoryVersion)BinaryPrimitives.ReadInt32BigEndian(buffer.Slice(8, 4)),
                Flags = BinaryPrimitives.ReadUInt32BigEndian(buffer.Slice(12, 4)),
                HashesOffset = BinaryPrimitives.ReadUInt32BigEndian(buffer.Slice(16, 4)),
                IdentifierOffset = BinaryPrimitives.ReadUInt32BigEndian(buffer.Slice(20, 4)),
                SpecialSlotCount = BinaryPrimitives.ReadUInt32BigEndian(buffer.Slice(24, 4)),
                CodeSlotCount = BinaryPrimitives.ReadUInt32BigEndian(buffer.Slice(28, 4)),
                ExecutableLength = BinaryPrimitives.ReadUInt32BigEndian(buffer.Slice(32, 4)),
                HashSize = buffer[36],
                HashType = buffer[37],
                Platform = buffer[38],
                Log2PageSize = buffer[39],
                Reserved = BinaryPrimitives.ReadUInt32BigEndian(buffer.Slice(40, 4)),
            };
            bytesRead = 44;
            return result;
        }

        public static CodeDirectoryHeader Read(ReadOnlySpan<byte> buffer, bool isLittleEndian, out int bytesRead)
        {
            return isLittleEndian ? ReadLittleEndian(buffer, out bytesRead) : ReadBigEndian(buffer, out bytesRead);
        }

        public void WriteLittleEndian(Span<byte> buffer, out int bytesWritten)
        {
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(0, 4), Magic);
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(4, 4), Size);
            BinaryPrimitives.WriteInt32LittleEndian(buffer.Slice(8, 4), (Int32)Version);
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(12, 4), Flags);
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(16, 4), HashesOffset);
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(20, 4), IdentifierOffset);
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(24, 4), SpecialSlotCount);
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(28, 4), CodeSlotCount);
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(32, 4), ExecutableLength);
            buffer[36] = HashSize;
            buffer[37] = HashType;
            buffer[38] = Platform;
            buffer[39] = Log2PageSize;
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.Slice(40, 4), Reserved);
            bytesWritten = 44;
        }

        public void WriteBigEndian(Span<byte> buffer, out int bytesWritten)
        {
            BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(0, 4), Magic);
            BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(4, 4), Size);
            BinaryPrimitives.WriteInt32BigEndian(buffer.Slice(8, 4), (Int32)Version);
            BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(12, 4), Flags);
            BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(16, 4), HashesOffset);
            BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(20, 4), IdentifierOffset);
            BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(24, 4), SpecialSlotCount);
            BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(28, 4), CodeSlotCount);
            BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(32, 4), ExecutableLength);
            buffer[36] = HashSize;
            buffer[37] = HashType;
            buffer[38] = Platform;
            buffer[39] = Log2PageSize;
            BinaryPrimitives.WriteUInt32BigEndian(buffer.Slice(40, 4), Reserved);
            bytesWritten = 44;
        }

        public void Write(Span<byte> buffer, bool isLittleEndian, out int bytesWritten)
        {
            if (isLittleEndian)
            {
                WriteLittleEndian(buffer, out bytesWritten);
            }
            else
            {
                WriteBigEndian(buffer, out bytesWritten);
            }
        }
    }
```
