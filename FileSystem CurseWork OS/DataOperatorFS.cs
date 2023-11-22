using FileSystem_CurseWork_OS.Blocks;
using System.Text;

namespace FileSystem_CurseWork_OS
{
    internal static class DataOperatorFS
    {
        public static void WriteSuperBlock(FileStream fs)
        {
            fs.Seek(0, SeekOrigin.Begin);

            fs.Write(Encoding.UTF8.GetBytes(SuperBlock.NameFileSystem));        //Название файловой системы
            fs.Write(BitConverter.GetBytes(SuperBlock.SizeSector), 0, 1);       //Размер сектора
            fs.Write(BitConverter.GetBytes(SuperBlock.CountSectors));         //Количество секторов
        }

        public static void ReadSuperBlock(FileStream fs)
        {
            fs.Seek(Encoding.UTF8.GetByteCount(Blocks.SuperBlock.NameFileSystem), SeekOrigin.Begin);

            var bytes = new byte[sizeof(Int64)];

            fs.Read(bytes, 0, 1);

            Blocks.SuperBlock.SizeSector = bytes[0];

            fs.Read(bytes, 0, bytes.Length);

            Blocks.SuperBlock.CountSectors = BitConverter.ToInt64(bytes);
        }
    }
}
