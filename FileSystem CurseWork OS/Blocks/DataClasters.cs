using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem_CurseWork_OS.Blocks
{
    internal class DataClasters : BlockBody
    {
        private int CurrentElement;
        public DataClasters(FileStream fileStream, int Element) : base(fileStream, Element, CountElements, OverallSize, StartByte, EndByte)
        {
            CurrentElement = Element;
        }

        public static byte DataSectorSize = (byte)(SizeSector - NumberNextBlockSize);
        public const byte NumberNextBlockSize = sizeof(int);

        public int NumberNextBlock
        {
            set
            {
                WriteBytesOperation(fs, _StartBytePositionSelectedElement + DataSectorSize, BitConverter.GetBytes(value), NumberNextBlockSize);
            }
            get
            {
                var bytes = ReadBytesOperation(fs, _StartBytePositionSelectedElement + DataSectorSize, NumberNextBlockSize);
                return BitConverter.ToInt32(bytes);
            }
        }
        public int NumberCurrentBlock
        {
            get { return CurrentElement; }
        }
        public static int CountElements
        {
            get
            {
                return BitMapDataClasters.CountElements;
            }
        }
        private static int SizeSector
        {
            get
            {
                return SuperBlock.SizeSector;
            }
        }
        public string DataSector
        {
            set
            {
                WriteBytesOperation(fs, _StartBytePositionSelectedElement, Encoding.UTF8.GetBytes(value), DataSectorSize);
            }
            get
            {
                var bytes = ReadBytesOperation(fs, _StartBytePositionSelectedElement, DataSectorSize);
                return Encoding.UTF8.GetString(bytes);
            }
        }

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
                return LastPosBitMapDataClasters + CountElements * SuperBlock.SizeSector;
            }
        }

    }
}
