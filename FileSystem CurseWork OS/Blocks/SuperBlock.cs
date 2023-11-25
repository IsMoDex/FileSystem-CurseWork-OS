using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem_CurseWork_OS.Blocks
{
    internal class SuperBlock : BlockBody
    {
        public const byte NameFileSystemSize = 5;
        public const byte SizeSectorSize = sizeof(Int32);
        public const byte CountSectorsSize = sizeof(Int32);

        public const string NameFileSystem = "TryFS";      //Название файловой системы
        public static Int32 SizeSector = byte.MaxValue;      //Размер сектора
        public static Int32 CountSectors = 1000;            //Количество секторов

        public SuperBlock(FileStream fileStream, int Element) : base(fileStream, Element, CountElements, OverallSize, StartByte, EndByte)
        {
        }
        
        public static Int64 SizeFileSystem { get { return CountSectors * SizeSector; } }

        static public int CountElements
        {
            get
            {
                return 3;
            }
        }
        static public int OverallSize
        {
            get
            {
                int OverallSize = 0;
                OverallSize += NameFileSystemSize;
                OverallSize += SizeSectorSize;
                OverallSize += CountSectorsSize;

                return OverallSize;
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
    }
}
