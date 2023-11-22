using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem_CurseWork_OS.Blocks
{
    internal abstract class BlockBody
    {
        protected long _StartBytePositionSelectedElement;
        protected FileStream fs;

        public BlockBody(FileStream fileStream, int Element, long _CountElements, int _OverallSize, long _StartByte, long _EndByte)
        {
            CountElements = _CountElements;
            OverallSize = _OverallSize;
            StartByte = _StartByte;
            EndByte = _EndByte;

            _StartBytePositionSelectedElement = GetOffset(Element) - 1; // - 1 Особенность в связи с Seek
            fs = fileStream;
        }

        static public long CountElements;

        static public int OverallSize;

        public static long StartByte;

        public static long EndByte;

        public static long GetOffset(long Element)
        {
            if (CountElements < Element || Element < 0)
                throw new IndexOutOfRangeException("Элемента под заданными номером не существует, вы вышли за пределы блока.");

            return StartByte + (Element * OverallSize);
        }

        protected void WriteBytesOperation(FileStream fs, long OffSet, byte[] value, byte MaxSize)
        {
            fs.Seek(OffSet, SeekOrigin.Begin);

            var insertData = new byte[MaxSize];

            value.CopyTo(insertData, 0);

            fs.Write(insertData, 0, insertData.Length);
        }

        protected void WriteBytesOperation(FileStream fs, long OffSet, byte value, byte MaxSize)
        {
            WriteBytesOperation(fs, OffSet, new byte[] { value }, MaxSize);
        }

        protected byte[] ReadBytesOperation(FileStream fs, long OffSet, byte Count)
        {
            fs.Seek(OffSet, SeekOrigin.Begin);

            byte[] buffer = new byte[Count];
            fs.Read(buffer, 0, Count);
            return buffer;
        }

        protected byte ReadBytesOperation(FileStream fs, long OffSet)
        {
            return ReadBytesOperation(fs, OffSet, 1)[0];
        }
    }
}
