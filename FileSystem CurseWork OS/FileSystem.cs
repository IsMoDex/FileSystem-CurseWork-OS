using FileSystem_CurseWork_OS.Blocks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FileSystem_CurseWork_OS
{
    class FileSystem
    {
        public string _path = "FS.data";

        public FileSystem()
        {
            //File.Delete(_path);

            if (File.Exists(_path))
            {

                using (FileStream fs = new FileStream(_path, FileMode.Open))
                {
                    //DataOperatorFS.ReadSuperBlock(fs);

                    //Считываем данные суперблока
                    fs.Seek(Encoding.UTF8.GetByteCount(SuperBlock.NameFileSystem), SeekOrigin.Begin);

                    var bytes = new byte[sizeof(Int64)];

                    fs.Read(bytes, 0, 1);

                    SuperBlock.SizeSector = bytes[0];

                    fs.Read(bytes, 0, bytes.Length);

                    SuperBlock.CountSectors = BitConverter.ToInt64(bytes);
                }

                //TestFS();
                
                
            }
            else
            {
                using (FileStream fs = new FileStream(_path, FileMode.Create))
                {
                    var Count = SuperBlock.SizeFileSystem;

                    for (long i = 0; i < Count; ++i)
                    {
                        fs.WriteByte(0);
                    }

                    //DataOperatorFS.WriteSuperBlock(fs);

                    //Записываем SuperBlock
                    fs.Seek(0, SeekOrigin.Begin);

                    fs.Write(Encoding.UTF8.GetBytes(SuperBlock.NameFileSystem));        //Название файловой системы
                    fs.Write(BitConverter.GetBytes(SuperBlock.SizeSector), 0, 1);       //Размер сектора
                    fs.Write(BitConverter.GetBytes(SuperBlock.CountSectors));         //Количество секторов
                }
            }
        }

        public void CreateNewFile(string Name, string Extension, string Acess, ushort IDUser, string Value)
        {
            using(var fs = new FileStream(_path, FileMode.Open))
            {
                var hash = GetFreeHash(fs, Name);
                var BitMapInode = new BitMapTableInodes(fs, hash);

                BitMapInode.Write = true;

                var Inode = new TableInodes(fs, hash)
                {
                    NameFile = Name,
                    FileExtension = Extension,
                    FileAcess = Acess,
                    IDUser = IDUser,
                };
                


            }
        }

        private int GetFreeHash(FileStream fs, string Name)
        {
            var BitMapInodes = BitMapTableInodes.GetSectorArray(fs);

            if (Array.IndexOf(BitMapInodes, false) != -1)
            {
                var hash = GetHash(Name);
                var Count = BitMapInodes.Length;

                //Цикл для резрешения коллизии
                for (int i = 0; i < Count; ++i)
                {
                    var NewHash = (hash + i) % Count;

                    if (!BitMapInodes[NewHash])
                    {
                        return NewHash;
                    }
                    else if (new TableInodes(fs, NewHash).NameFile.Equals(Name))
                    {
                        throw new ArgumentOutOfRangeException("Файл с таким именем уже существует!");
                    }
                }

            }
            else
            {
                throw new ArgumentOutOfRangeException("Место в таблице инодов закончилось.");
            }

            throw new ArgumentOutOfRangeException("Не удалось найти хеш!");
        }

        private int GetHash(string Value)
        {
            var Hash = Encoding.UTF8.GetBytes(Value).Select((x, index) => x + index).Sum();

            return Hash % TableInodes.CountElements;
        }
        private int GetFreeClaster(FileStream fs)
        {
            var _BitMapClasters = BitMapDataClasters.GetSectorArray(fs);

            if (Array.IndexOf(_BitMapClasters, false) != -1)
            {

            }
            else
            {
                throw new ArgumentOutOfRangeException("Место на диске закончилось, все кластеры заполнены!");
            }

            return -1;
        }


        private void TestFS()
        {
            using (FileStream fs = new FileStream(_path, FileMode.Open))
            {
                //int pos = SuperBlock.EndByte;

                //fs.Seek(pos - 1, SeekOrigin.Begin);

                byte[] bytes = Encoding.UTF8.GetBytes("A");

                //fs.Write(bytes, 0, bytes.Length);

                //Console.WriteLine(Encoding.UTF8.GetString(bytes, 0, bytes.Length));



                fs.Seek(BitMapTableInodes.StartByte - 1, SeekOrigin.Begin);

                bytes = Encoding.UTF8.GetBytes("B");

                fs.Write(bytes, 0, bytes.Length);

                Console.WriteLine(Encoding.UTF8.GetString(bytes, 0, bytes.Length));



                fs.Seek(BitMapTableInodes.EndByte - 1, SeekOrigin.Begin);

                bytes = Encoding.UTF8.GetBytes("C");

                fs.Write(bytes, 0, bytes.Length);

                Console.WriteLine(Encoding.UTF8.GetString(bytes, 0, bytes.Length));



                fs.Seek(TableInodes.StartByte - 1, SeekOrigin.Begin);

                bytes = Encoding.UTF8.GetBytes("D");

                fs.Write(bytes, 0, bytes.Length);

                Console.WriteLine(Encoding.UTF8.GetString(bytes, 0, bytes.Length));



                fs.Seek(TableInodes.EndByte - 1, SeekOrigin.Begin);

                bytes = Encoding.UTF8.GetBytes("E");

                fs.Write(bytes, 0, bytes.Length);

                Console.WriteLine(Encoding.UTF8.GetString(bytes, 0, bytes.Length));



                fs.Seek(BitMapDataClasters.StartByte - 1, SeekOrigin.Begin);

                bytes = Encoding.UTF8.GetBytes("F");

                fs.Write(bytes, 0, bytes.Length);

                Console.WriteLine(Encoding.UTF8.GetString(bytes, 0, bytes.Length));



                fs.Seek(BitMapDataClasters.EndByte - 1, SeekOrigin.Begin);

                bytes = Encoding.UTF8.GetBytes("G");

                fs.Write(bytes, 0, bytes.Length);

                Console.WriteLine(Encoding.UTF8.GetString(bytes, 0, bytes.Length));



                fs.Seek(DataClasters.StartByte - 1, SeekOrigin.Begin);

                bytes = Encoding.UTF8.GetBytes("Q");

                fs.Write(bytes, 0, bytes.Length);

                Console.WriteLine(Encoding.UTF8.GetString(bytes, 0, bytes.Length));



                fs.Seek(DataClasters.EndByte - 1, SeekOrigin.Begin);

                bytes = Encoding.UTF8.GetBytes("R");

                fs.Write(bytes, 0, bytes.Length);

                Console.WriteLine(Encoding.UTF8.GetString(bytes, 0, bytes.Length));



                var table = new TableInodes(fs, 0);
                var map = new BitMapTableInodes(fs, 1);

                map.Write = true;

                Console.WriteLine(map.Write);

                table.NameFile = "Hello Wrold";
                table.FileExtension = "rtx";
                table.FileLenght = 123;
                table.FileAcess = "rwxrwx";
                table.IDUser = 1;
                table.NumberStartClaster = 1;

                Console.WriteLine(table.NameFile);
                Console.WriteLine(table.FileExtension);
                Console.WriteLine(table.FileLenght);
                Console.WriteLine(table.FileAcess);
                Console.WriteLine(table.IDUser);
                Console.WriteLine(table.NumberStartClaster);

                table = new TableInodes(fs, 1);

                table.NameFile = "ASD";
                table.FileExtension = "rtx";
                table.FileLenght = 555;
                table.FileAcess = "rwxrwx";
                table.IDUser = 2;
                table.NumberStartClaster = 1;

                Console.WriteLine(table.NameFile);
                Console.WriteLine(table.FileExtension);
                Console.WriteLine(table.FileLenght);
                Console.WriteLine(table.FileAcess);
                Console.WriteLine(table.IDUser);
                Console.WriteLine(table.NumberStartClaster);

                var bitclaster = new BitMapDataClasters(fs, table.NumberStartClaster);
                bitclaster.Write = true;

                var claster = new DataClasters(fs, table.NumberStartClaster);

                claster.NumberNextBlock = 2;
                claster.DataSector = "Data, New Data, Neaxt Data and td\nHello World///";

                Console.WriteLine(claster.NumberNextBlock);
                Console.WriteLine(claster.DataSector);

                bitclaster = new BitMapDataClasters(fs, claster.NumberNextBlock);
                bitclaster.Write = true;

                claster = new DataClasters(fs, claster.NumberNextBlock);

                claster.NumberNextBlock = 0;
                claster.DataSector = "LastData, EndData\nPrint;";

                Console.WriteLine(claster.NumberNextBlock);
                Console.WriteLine(claster.DataSector);
            }
        }
    }
}
