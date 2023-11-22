using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem_CurseWork_OS.Blocks
{
    internal class BitMapTableInodes : BlockBody
    {
        public BitMapTableInodes(FileStream fileStream, int Element) : base(fileStream, Element, CountElements, OverallSize, StartByte, EndByte)
        {
        }

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

        public static long CountElements
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
                return SuperBlock.EndByte + CountElements;
            }
        }

    }
}
