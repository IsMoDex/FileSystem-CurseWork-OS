using System.Text;

namespace FileSystem_CurseWork_OS
{
    internal static class DataOperatorFS
    {
        public sealed class SuperBlock : DataBlockFS //BIOS PARAMETER BLOCK
        {
            public static string NameFileSystem = "TryFS";      //Название файловой системы
            public static byte SizeSector = byte.MaxValue;      //Размер сектора
            public static Int64 CountSectors = 1000;   //Количество секторов
            public static Int64 SizeFileSystem { get { return CountSectors * SizeSector; } }
            //public static Int64 SizeRootDirectory = 0;         //Размер корневого каталога
            //public static byte OffsetToFATTable = 0;            //Смещение к таблице FAT
            //public static byte OffsetToRootDirectory = 0;       //Смещение к корневому каталогу
            //public static long OffsetToDataArea = 0;           //Смещение к области данных

            static public int OverallSize
            {
                get
                {
                    return Encoding.UTF8.GetByteCount(NameFileSystem) + sizeof(byte) + sizeof(Int64);
                }
            }
            public static int StartByte
            {
                get
                {
                    return 0;
                }
            }
            public static int EndByte
            {
                get
                {
                    int nameSize = Encoding.UTF8.GetByteCount(SuperBlock.NameFileSystem);
                    int sizeOfByte = sizeof(byte);
                    int sizeOfInt64 = sizeof(Int64);

                    int offset = nameSize + sizeOfByte + sizeOfInt64;

                    return offset;
                }
            }
            public static int GetOffset(int Element)
            {
                switch (Element)
                {
                    case 1:
                        return StartByte;
                        break;

                    case 2:
                        return StartByte + Encoding.UTF8.GetByteCount(NameFileSystem);

                    case 3:
                        return EndByte - sizeof(Int64) - 1;

                    default:
                        throw new IndexOutOfRangeException("Элемента под заданными номером не существует, вы вышли за пределы блока.");
                }
            }

            //public static int StartByte
            //{
            //    int nameSize = Encoding.UTF8.GetByteCount(SuperBlock.NameFileSystem);
            //    int sizeOfByte = sizeof(byte);
            //    int sizeOfInt64 = sizeof(Int64);

            //    //// Выравнивание данных на границу Int64 (8 байт)
            //    //int alignment = 8;

            //    int offset = nameSize + sizeOfByte + sizeOfInt64;

            //    //// Выравниваем offset
            //    //int remainder = offset % alignment;
            //    //if (remainder != 0)
            //    //{
            //    //    offset += alignment - remainder;
            //    //}

            //    return offset;
            //}
        }

        public static void WriteSuperBlock(FileStream fs)
        {
            fs.Seek(0, SeekOrigin.Begin);

            fs.Write(Encoding.UTF8.GetBytes(SuperBlock.NameFileSystem));        //Название файловой системы
            fs.Write(BitConverter.GetBytes(SuperBlock.SizeSector), 0, 1);       //Размер сектора
            fs.Write(BitConverter.GetBytes(SuperBlock.CountSectors));         //Количество секторов
            //fs.Write(BitConverter.GetBytes(SuperBlock.SizeRootDirectory));      //Размер корневого каталога
        }

        public static void ReadSuperBlock(FileStream fs)
        {
            fs.Seek(Encoding.UTF8.GetByteCount(SuperBlock.NameFileSystem), SeekOrigin.Begin);
            SuperBlock.SizeSector = ReadByte(fs);
            SuperBlock.CountSectors = BitConverter.ToInt64(ReadBytes(fs, sizeof(Int64)));
            //SuperBlock.SizeRootDirectory = BitConverter.ToInt64(ReadBytes(fs, sizeof(Int64)));
        }

        public sealed class BitMapTableInodes
        {
            private long _StartBytePositionSelectedElement;
            private FileStream fs;
            public bool Write
            {
                set
                {
                    WriteBytesOperation(fs, _StartBytePositionSelectedElement, BitConverter.GetBytes(value), sizeof(bool));
                }
                get
                {
                    var bytes = ReadBytesOperation(fs, _StartBytePositionSelectedElement, sizeof(bool));
                    return BitConverter.ToBoolean(bytes);
                }
            }

            public BitMapTableInodes(FileStream filestream, int Element)
            {
                _StartBytePositionSelectedElement = GetOffset(Element) - 1; // - 1 Особенность в связи с Seek
                fs = filestream;
            }
            public static long CountMaxEntriesInTableInode
            {
                get
                {
                    long FreeSpace = SuperBlock.SizeFileSystem - SuperBlock.EndByte;
                    return (long)(FreeSpace * 0.15) / TableInodes.OverallSize;
                }
            }

            public static int OverallSize
            {
                get
                {
                    return sizeof(bool);
                }
            }
            public static int StartByte
            {
                get
                {
                    return SuperBlock.EndByte + 1;
                }
            }
            public static long EndByte
            {
                get
                {
                    //long FreeSpace = SuperBlock.SizeFileSystem - SuperBlock.EndByte;

                    //long CountMaxEntriesInTableInode = (long)(FreeSpace * 0.15) / TableInodes.OverallSize;

                    return SuperBlock.EndByte + CountMaxEntriesInTableInode;
                }
            }

            public static long GetOffset(long Element)
            {
                if (CountMaxEntriesInTableInode < Element || Element < 0)
                    throw new IndexOutOfRangeException("Элемента под заданными номером не существует, вы вышли за пределы блока.");

                return StartByte + (Element * OverallSize);
            }

            //public static long StartByte
            //{
            //    long FreeSpace = SuperBlock.SizeFileSystem - SuperBlock.StartByte;

            //    long CountMaxEntriesInTableInode = (long)(FreeSpace * 0.15) / TableInodes.OverallSize;

            //    return SuperBlock.StartByte + CountMaxEntriesInTableInode;
            //}
        }

        public sealed class TableInodes : DataBlockFS
        {
            private long _StartBytePositionSelectedElement;
            private FileStream fs;

            private const byte NameFileSize = 50;  //Для UTF8
            private const byte FileExtensionSize = 5;
            private const byte FileLenghtSize = sizeof(UInt64);
            private const byte FileAcessSize = 6;
            private const byte IDUserSize = sizeof(UInt16);
            private const byte NumberStartClasterSize = sizeof(int);
            private static long CountMaxEntriesInTableInode 
            { 
                get
                {
                    return BitMapTableInodes.CountMaxEntriesInTableInode;
                }
            }

            public string NameFile
            {
                set
                {
                    WriteBytesOperation(fs, _StartBytePositionSelectedElement, Encoding.UTF8.GetBytes(value), NameFileSize);
                }
                get
                {
                    var bytes = ReadBytesOperation(fs, _StartBytePositionSelectedElement, NameFileSize);
                    return Encoding.UTF8.GetString(bytes);
                }
            }           //50
            public string FileExtension
            {
                set
                {
                    WriteBytesOperation(fs, _StartBytePositionSelectedElement + NameFileSize, Encoding.UTF8.GetBytes(value), FileExtensionSize);
                }
                get
                {
                    var bytes = ReadBytesOperation(fs, _StartBytePositionSelectedElement + NameFileSize, FileExtensionSize);
                    return Encoding.UTF8.GetString(bytes);
                }
            }      //5
            public UInt64 FileLenght
            {
                set
                {
                    WriteBytesOperation(fs,_StartBytePositionSelectedElement + NameFileSize + FileExtensionSize, BitConverter.GetBytes(value), FileLenghtSize);
                }
                get
                {
                    var bytes = ReadBytesOperation(fs, _StartBytePositionSelectedElement + NameFileSize + FileExtensionSize, FileLenghtSize);
                    return BitConverter.ToUInt64(bytes);
                }
            }         //8
            public string FileAcess
            {
                set
                {
                    WriteBytesOperation(fs, _StartBytePositionSelectedElement + NameFileSize + FileExtensionSize + FileLenghtSize, Encoding.UTF8.GetBytes(value), FileAcessSize);
                }
                get
                {
                    var bytes = ReadBytesOperation(fs, _StartBytePositionSelectedElement + NameFileSize + FileExtensionSize + FileLenghtSize, FileAcessSize);
                    return Encoding.UTF8.GetString(bytes);
                }
            }          //6
            public UInt16 IDUser
            {
                set
                {
                    WriteBytesOperation(fs, _StartBytePositionSelectedElement + NameFileSize + FileExtensionSize + FileLenghtSize + FileAcessSize, BitConverter.GetBytes(value), IDUserSize);
                }
                get
                {
                    var bytes = ReadBytesOperation(fs, _StartBytePositionSelectedElement + NameFileSize + FileExtensionSize + FileLenghtSize + FileAcessSize, IDUserSize);
                    return BitConverter.ToUInt16(bytes);
                }
            }             //2
            public int NumberStartClaster
            {
                set
                {
                    WriteBytesOperation(fs, _StartBytePositionSelectedElement + NameFileSize + FileExtensionSize + FileLenghtSize + FileAcessSize + IDUserSize, BitConverter.GetBytes(value), NumberStartClasterSize);
                }
                get
                {
                    var bytes = ReadBytesOperation(fs, _StartBytePositionSelectedElement + NameFileSize + FileExtensionSize + FileLenghtSize + FileAcessSize + IDUserSize, NumberStartClasterSize);
                    return BitConverter.ToInt32(bytes);
                }
            }    //4
                                                //75

            public TableInodes(FileStream filestream, int Element)
            {
                _StartBytePositionSelectedElement = GetOffset(Element) - 1; // - 1 Особенность в связи с Seek
                fs = filestream;
            }

            public static int OverallSize
            {
                get
                {
                    byte OverallSize = 0;

                    //OverallSize += 50;  //Для UTF8
                    //OverallSize += 5;
                    //OverallSize += sizeof(UInt64);
                    //OverallSize += 6;
                    //OverallSize += sizeof(UInt16);
                    //OverallSize += sizeof(int);

                    OverallSize += NameFileSize;
                    OverallSize += FileExtensionSize;
                    OverallSize += FileLenghtSize;
                    OverallSize += FileAcessSize;
                    OverallSize += IDUserSize;
                    OverallSize += NumberStartClasterSize;

                    return OverallSize;
                }
            }
            public static long StartByte
            {
                get
                {
                    return BitMapTableInodes.EndByte + 1;
                }
            }
            public static long EndByte
            {
                get
                {
                    long LastPosBitMapInodes = BitMapTableInodes.EndByte;

                    //long CountMaxEntries = LastPosBitMapInodes - SuperBlock.EndByte;

                    long SizeAllEntries = CountMaxEntriesInTableInode * OverallSize;

                    return SizeAllEntries + LastPosBitMapInodes;
                }   
            }

            public static long GetOffset(long Element)
            {
                if (CountMaxEntriesInTableInode < Element || Element < 0)
                    throw new IndexOutOfRangeException("Элемента под заданными номером не существует, вы вышли за пределы блока.");

                return StartByte + (Element * OverallSize);
            }
        }

        public sealed class BitMapDataClasters : DataBlockFS
        {
            static public int OverallSize
            {
                get
                {
                    return 1;
                }
            }
            public static long StartByte
            {
                get
                {
                    return TableInodes.EndByte + 1;
                }
            }
            public static long EndByte
            {
                get
                {
                    long LastPosTableInodes = TableInodes.EndByte;
                    long FreeSpace = SuperBlock.SizeFileSystem - LastPosTableInodes;

                    long CountCurrentElements = FreeSpace / (SuperBlock.SizeSector + 1); //Element = one byte

                    return LastPosTableInodes + CountCurrentElements;
                }
            }
            public static long GetOffset(long Element)
            {
                if (EndByte < Element || Element < 0)
                    throw new IndexOutOfRangeException("Элемента под заданными номером не существует, вы вышли за пределы блока.");

                return StartByte + (Element * OverallSize);
            }
        }

        public sealed class DataClasters : DataBlockFS
        {
            static public int OverallSize
            {
                get
                {
                    return SuperBlock.SizeSector;
                }
            }
            public static long StartByte
            {
                get
                {
                    return BitMapDataClasters.EndByte + 1;
                }
            }
            public static long EndByte
            {
                get
                {
                    long LastPosBitMapDataClasters = BitMapDataClasters.EndByte;

                    long CountSectors = LastPosBitMapDataClasters - TableInodes.EndByte;

                    return LastPosBitMapDataClasters + CountSectors * SuperBlock.SizeSector;
                }
            }
            public static long GetOffset(long Element)
            {
                if (EndByte < Element || Element < 0)
                    throw new IndexOutOfRangeException("Элемента под заданными номером не существует, вы вышли за пределы блока.");

                return StartByte + (Element * OverallSize);
            }
        }

        private static byte ReadByte(FileStream fs)
        {
            byte[] buffer = new byte[sizeof(byte)];
            fs.Read(buffer, 0, buffer.Length);
            return buffer[0];
        }

        private static byte[] ReadBytes(FileStream fs, int Count)
        {
            byte[] buffer = new byte[Count];
            fs.Read(buffer, 0, Count);
            return buffer;
        }

        private static void WriteBytesOperation(FileStream fs, long OffSet, byte[] value, byte MaxSize)
        {
            fs.Seek(OffSet, SeekOrigin.Begin);

            var insertData = new byte[MaxSize];

            value.CopyTo(insertData, 0);

            fs.Write(insertData, 0, insertData.Length);
        }

        private static byte[] ReadBytesOperation(FileStream fs, long OffSet, byte Count)
        {
            fs.Seek(OffSet, SeekOrigin.Begin);

            return ReadBytes(fs, Count);
        }
    }
}
