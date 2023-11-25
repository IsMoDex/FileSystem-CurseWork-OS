using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem_CurseWork_OS.Blocks
{
    internal class BitMapDataClasters : BlockBody
    {
        public BitMapDataClasters(FileStream fileStream, int Element) : base(fileStream, Element, CountElements, OverallSize, StartByte, EndByte)
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

        static public int CountElements
        {
            get
            {
                long LastPosTableInodes = TableInodes.EndByte;

                long FreeSpace = SuperBlock.SizeFileSystem - LastPosTableInodes;

                long CountCurrentElements = FreeSpace / (SuperBlock.SizeSector + 1); //Element = one byte

                return (int)CountCurrentElements;
            }
        }
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
                return LastPosTableInodes + CountElements;
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
