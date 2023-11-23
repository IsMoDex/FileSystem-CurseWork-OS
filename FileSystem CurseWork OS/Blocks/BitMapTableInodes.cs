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

        public static int CountElements
        {
            get
            {
                long FreeSpace = SuperBlock.SizeFileSystem - SuperBlock.EndByte;
                return (int)(FreeSpace * 0.15) / TableInodes.OverallSize;
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

        public static bool[] GetSectorArray(FileStream fs)
        {
            var data = new byte[CountElements];

            fs.Seek(StartByte - 1, SeekOrigin.Begin);
            fs.Read(data, 0, CountElements * OverallSize);

            return data.Select(x => BitConverter.ToBoolean(new byte[] { x })).ToArray();
        }
    }
}
