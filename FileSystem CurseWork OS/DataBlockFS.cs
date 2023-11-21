using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem_CurseWork_OS
{
    internal interface DataBlockFS
    {
        static public int OverallSize { get; }
        static public long StartByte { get; }

        static public long EndByte { get; }

        public static long GetOffset(long Element)
        {
            if (EndByte < Element)
                throw new IndexOutOfRangeException("Элемента под заданными номером не существует, вы вышли за пределы блока.");

            return StartByte + (Element * OverallSize);
        }
    }
}
