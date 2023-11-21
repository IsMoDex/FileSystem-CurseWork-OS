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
            public static int StartByte
            {
                get
                {
                    return SuperBlock.StartByte + 1;
                }
            }
            public static long EndByte
            {
                get
                {
                    long FreeSpace = SuperBlock.SizeFileSystem - SuperBlock.EndByte;

                    long CountMaxEntriesInTableInode = (long)(FreeSpace * 0.15) / TableInodes.OverallSize;

                    return SuperBlock.EndByte + CountMaxEntriesInTableInode;
                }
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
            public const byte NameFileSize = 50;
            public const byte FileExtensionSize = 5;
            public const byte FileSize = 5;
            public const byte FileAtributesSize = 3;
            public const byte FileAcessSize = 6;
            public const byte IDUserSize = 2;
            public const byte NumberStartClasterSize = 4;

            public static int OverallSize
            {
                get
                {
                    byte OverallSize = 0;
                    OverallSize += TableInodes.NameFileSize;
                    OverallSize += TableInodes.FileExtensionSize;
                    OverallSize += TableInodes.FileSize;
                    OverallSize += TableInodes.FileAtributesSize;
                    OverallSize += TableInodes.FileAcessSize;
                    OverallSize += TableInodes.IDUserSize;
                    OverallSize += TableInodes.NumberStartClasterSize;

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

                    long CountMaxEntries = LastPosBitMapInodes - SuperBlock.EndByte;

                    long SizeAllEntries = CountMaxEntries * OverallSize;

                    return SizeAllEntries + LastPosBitMapInodes;
                }
            }

        }

        public sealed class BitMapDataClasters : DataBlockFS
        {
            static public int OverallSize
            {
                get
                {
                    return sizeof(bool);
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
    }
}
