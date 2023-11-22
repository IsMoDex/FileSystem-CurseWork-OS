using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem_CurseWork_OS.Blocks
{
    internal class TableInodes : BlockBody
    {
        public TableInodes(FileStream fileStream, int Element) : base(fileStream, Element, CountElements, OverallSize, StartByte, EndByte)
        {
        }

        private const byte NameFileSize = 50;  //Для UTF8
        private const byte FileExtensionSize = 5;
        private const byte FileLenghtSize = sizeof(UInt64);
        private const byte FileAcessSize = 6;
        private const byte IDUserSize = sizeof(UInt16);
        private const byte NumberStartClasterSize = sizeof(int);

        private static long CountElements
        {
            get
            {
                return BitMapTableInodes.CountElements;
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
                WriteBytesOperation(fs, _StartBytePositionSelectedElement + NameFileSize + FileExtensionSize, BitConverter.GetBytes(value), FileLenghtSize);
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

        public static int OverallSize
        {
            get
            {
                byte OverallSize = 0;

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

                long SizeAllEntries = CountElements * OverallSize;

                return SizeAllEntries + LastPosBitMapInodes;
            }
        }
    }
}
