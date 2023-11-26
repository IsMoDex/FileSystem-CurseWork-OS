using FileSystem_CurseWork_OS.Blocks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FileSystem_CurseWork_OS
{
    class FileSystem
    {
        private string _path = "FS.data";

        public FileSystem(string path, bool Create)
        {
            _path = path;
            //File.Delete(_path);

            if (!Create)
            {

                using (FileStream fs = new FileStream(_path, FileMode.Open))
                {
                    //DataOperatorFS.ReadSuperBlock(fs);

                    //Считываем данные суперблока
                    fs.Seek(SuperBlock.NameFileSystemSize, SeekOrigin.Begin);

                    var bytes = new byte[sizeof(Int32)];

                    fs.Read(bytes, 0, bytes.Length);

                    SuperBlock.SizeSector = BitConverter.ToInt32(bytes);

                    fs.Read(bytes, 0, bytes.Length);

                    SuperBlock.CountSectors = BitConverter.ToInt32(bytes);
                }

                //string Data = new string(Enumerable.Repeat('B', DataClasters.DataSectorSize / 2 + 1).ToArray());

                ////CreateNewFile("TestFile", "txt", "rwxrwx", 0, new string (Enumerable.Repeat('A', DataClasters.DataSectorSize * 3 - DataClasters.DataSectorSize / 2).ToArray()));

                //WriteStringInFile("TestFile", Data, false);
                

                //TestFS();
                
                
            }
            else
            {
                using (FileStream fs = new FileStream(_path, FileMode.Open))
                {
                    //var Count = SuperBlock.SizeFileSystem;

                    //for (long i = 0; i < Count; ++i)
                    //{
                    //    fs.WriteByte(0);
                    //}

                    ////DataOperatorFS.WriteSuperBlock(fs);

                    //Записываем SuperBlock
                    fs.Seek(0, SeekOrigin.Begin);

                    int size = 0;
                    size += Encoding.UTF8.GetBytes(SuperBlock.NameFileSystem).Count();
                    size += BitConverter.GetBytes(SuperBlock.SizeSector).Count();
                    size += BitConverter.GetBytes(SuperBlock.CountSectors).Count();

                    fs.Write(Encoding.UTF8.GetBytes(SuperBlock.NameFileSystem), 0, SuperBlock.NameFileSystemSize);        //Название файловой системы
                    fs.Write(BitConverter.GetBytes(SuperBlock.SizeSector));           //Размер сектора
                    fs.Write(BitConverter.GetBytes(SuperBlock.CountSectors));         //Количество секторов
                }
            }
        }

        /// <summary>
        /// Метод создающий файл с заданными параметрами.
        /// </summary>
        /// <param name="Name">Название файла</param>
        /// <param name="Extension">Расширения файла</param>
        /// <param name="Acess">Доступ к файлу</param>
        /// <param name="IDUser">Код пользователя создавшего файл</param>
        /// <param name="Value">Данные для записи в файл</param>
        public void CreateNewFile(string Name, string Extension, string Acess, ushort IDUser, string Value = null)
        {
            using(var fs = new FileStream(_path, FileMode.Open))
            {
                var hash = GetFreeHash(fs, Name);
                var BitMapInode = new BitMapTableInodes(fs, hash);

                BitMapInode.Write = true;

                var IndexClaster = GetFreeClaster(fs);

                var Claster = new BitMapDataClasters(fs, IndexClaster);
                Claster.Write = true;

                var Inode = new TableInodes(fs, hash);
                Inode.NameFile = Name;
                Inode.FileExtension = Extension;
                Inode.FileLenght = 1;
                Inode.FileAcess = Acess;
                Inode.IDUser = IDUser;
                Inode.NumberStartClaster = IndexClaster;

                if (Value != null)
                {
                    WriteStringInFile(fs, in Inode, Value, false);
                }

            }
        }

        /// <summary>
        /// Метод для поулчения содрежимого из файла.
        /// </summary>
        /// <param name="Name"></param>
        /// <returns></returns>
        public string GetContentFromFile(string Name)
        {
            using (var fs = new FileStream(_path, FileMode.Open))
            {
                var Inode = GetInodeByName(fs, Name);
                return Inode.Content;
            }
        }
        public void ChangeFileAcess(string Name, string NewAcces)
        {
            if (!Regex.IsMatch(NewAcces, @"^[r-][w-][x-][r-][w-][x-]$"))
                throw new ArgumentException("Не корректный ввод доступа.");

            using (var fs = new FileStream(_path, FileMode.Open))
            {
                var Inode = GetInodeByName(fs,Name);

                Inode.FileAcess = NewAcces;
            }
        }
        public string GetFileAcess(string Name)
        {
            using (var fs = new FileStream(_path, FileMode.Open))
            {
                var Inode = GetInodeByName(fs, Name);

                return Inode.FileAcess;
            }
        }

        public ushort GetNumberFileСreator(string Name)
        {
            using (var fs = new FileStream(_path, FileMode.Open))
            {
                var Inode = GetInodeByName(fs, Name);
         
                return Inode.IDUser;
            }
        }

        /// <summary>
        /// Метод получения свободного хеша из таблицы инодов
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="Name"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
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
                    else
                    {
                        var NameSelectFile = new TableInodes(fs, NewHash).NameFile;

                        var charactersToAdd = Enumerable.Repeat('\0', NameSelectFile.Length - Name.Length).ToArray();

                        var CurrentFullName = Name + new string(charactersToAdd);

                        if(NameSelectFile.Equals(CurrentFullName))
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
        /// <summary>
        /// Хеш-метод
        /// </summary>
        /// <param name="Value"></param>
        /// <returns></returns>
        private int GetHash(string Value)
        {
            var Hash = Encoding.UTF8.GetBytes(Value).Select((x, index) => x + index).Sum();

            var CountElements = TableInodes.CountElements;

            return Hash % CountElements;
        }
        private int LinearRehash(int originalHash)
        {
            return (originalHash + 1) % TableInodes.CountElements;
        }
        
        /// <summary>
        /// Метод получения свободного кластера из таблицы кластеров.
        /// </summary>
        /// <param name="fs"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private int GetFreeClaster(FileStream fs)
        {
            var _BitMapClasters = BitMapDataClasters.GetSectorArray(fs);
            var EmptyIndex = Array.IndexOf(_BitMapClasters, false);

            if (EmptyIndex != -1)
            {
                return EmptyIndex;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Место на диске закончилось, все кластеры заполнены!");
            }
        }

        /// <summary>
        /// Метод записи данных в выобранный файл по иноду
        /// </summary>
        /// <param name="fs">Файловый поток</param>
        /// <param name="inode">Инод выбранного файла</param>
        /// <param name="Text">Текст для записи</param>
        /// <param name="Append">Добавить или змаменить (True/False)</param>
        private void WriteStringInFile(FileStream fs, in TableInodes inode, string Text, bool Append)
        {
            if(Append)
            {
                var LastIndex = GetIndexLastFileClaster(fs, in inode);

                var LastClaster = new DataClasters(fs, LastIndex);

                var SizeData = DataClasters.DataSectorSize;
                var DataSector = LastClaster.DataSector;

                if (DataSector.IndexOf('\0') != -1)
                    DataSector = DataSector.Substring(0, DataSector.IndexOf('\0'));

                if (Text.Length > SizeData - DataSector.Length)
                {
                    DataSector += Text.Substring(0, SizeData - DataSector.Length);
                    LastClaster.DataSector = DataSector;

                    var list = WriteDataInNewLineClasters(fs, Text);

                    LastClaster.NumberNextBlock = list[0];
                }
                else
                {
                    DataSector += Text;
                    LastClaster.DataSector = DataSector;
                }
            }
            else
            {
                var CurrentClaster = new DataClasters(fs, inode.NumberStartClaster);
                uint FileLenght = inode.FileLenght;

                for (int i = 0; i < FileLenght; ++i)
                {
                    var PrewClaster = CurrentClaster;
                    CurrentClaster = new DataClasters(fs, CurrentClaster.NumberNextBlock);
                    CurrentClaster.DataSector = string.Empty;

                    var CurrentFreeMapClaster = new BitMapDataClasters(fs, PrewClaster.NumberCurrentBlock);
                    CurrentFreeMapClaster.Write = false;

                    PrewClaster.NumberNextBlock = 0;
                }

                var ListClasters = WriteDataInNewLineClasters(fs, Text);
                inode.NumberStartClaster = ListClasters[0];
                inode.FileLenght = (uint)ListClasters.Count;
            }
        }
        /// <summary>
        /// Запись данных в файл по имени.
        /// </summary>
        /// <param name="NameFile">Названия файла для редактирования</param>
        /// <param name="Text">Текст для записи(будет разбит по кластерам)</param>
        /// <param name="Append">Добавить или змаменить (True/False)</param>
        public void WriteStringInFile(string NameFile, string Text, bool Append)
        {
            using(var fs = new FileStream(_path, FileMode.Open))
            {
                var Inode = GetInodeByName(fs, NameFile);
                WriteStringInFile(fs, Inode, Text, Append);
            }
        }

        /// <summary>
        /// Метод получения инода по имени файла.
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="NameFile"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private TableInodes GetInodeByName(FileStream fs, string NameFile)
        {
            var Hash = GetHash(NameFile);
            var Count = TableInodes.CountElements;

            for(int i = 0; i < Count; ++i)
            {
                var NewHash = (Hash + i) % Count;
                var Inode = new TableInodes(fs, NewHash);

                var NameSelectFile = Inode.NameFile;

                var charactersToAdd = Enumerable.Repeat('\0', NameSelectFile.Length - NameFile.Length).ToArray();

                var CurrentFullName = NameFile + new string(charactersToAdd);

                if (NameSelectFile.Equals(CurrentFullName))
                    return Inode;

            }

            throw new ArgumentOutOfRangeException("Файла с таким именем не существует!");
        } 

        //private TableInodes GetInodeByName(string NameFile)
        //{
        //    using (var fs = new FileStream(_path, FileMode.Open))
        //    {
        //        return GetInodeByName(fs, NameFile);
        //    }
        //}

        /// <summary>
        /// Метод разбиения и записи строки по кластерам.
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="Data"></param>
        /// <returns></returns>
        private List<int> WriteDataInNewLineClasters(FileStream fs, string Data)
        {
            List<int> list = new List<int>();

            var DataArray = Encoding.UTF8.GetBytes(Data);

            int SizeData = DataClasters.DataSectorSize;

            int Count = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(DataArray.Length) / Convert.ToDouble(SizeData)));

            DataClasters PrewClaster = null;

            for(int i = 0; i < Count; ++i)
            {
                //Дописать разбиение строки на сектора
                var DataInWrite = DataArray.Skip(i * SizeData).Take(SizeData).ToArray();

                var ID = GetFreeClaster(fs);

                var CurrentFreeMapClaster = new BitMapDataClasters(fs, ID);
                CurrentFreeMapClaster.Write = true;

                var CurrentFreeClaster = new DataClasters(fs, ID);
                CurrentFreeClaster.DataSector = Encoding.UTF8.GetString(DataInWrite);


                if (PrewClaster != null)
                    PrewClaster.NumberNextBlock = ID;

                PrewClaster = CurrentFreeClaster;   

                list.Add(ID);
            }

            return list;
        }

        /// <summary>
        /// Метод получения последнего номера кластера инода.
        /// </summary>
        /// <param name="fs"></param>
        /// <param name="inode"></param>
        /// <returns></returns>
        private int GetIndexLastFileClaster(FileStream fs, in TableInodes inode)
        {
            //var _BitMapClasters = BitMapDataClasters.GetSectorArray(fs);

            var CurrentClaster = new DataClasters(fs, inode.NumberStartClaster);
            uint FileLenght = inode.FileLenght;

            for(int i = 0; i < FileLenght - 1; ++i)
            {
                CurrentClaster = new DataClasters(fs, CurrentClaster.NumberNextBlock);
            }

            return CurrentClaster.NumberCurrentBlock;
        }
        
        public List<string> GetListInformationAboutAllFiles()
        {
            using (var fs = new FileStream(_path, FileMode.Open))
            {
                var list = new List<string>();

                var BitMapArray = BitMapTableInodes.GetSectorArray(fs);

                for(int i = 0; i < BitMapArray.Length; ++i)
                {
                    if (BitMapArray[i])
                    {
                        var Inode = new TableInodes(fs, i);

                        string str = $"" +
                            $"{Inode.FileAcess}\t" +
                            $"{Inode.IDUser}\t" +
                            $"{Inode.FileLenght * SuperBlock.SizeSector}\t" +
                            $"{Inode.NameFile}\t" +
                            $"{Inode.FileExtension}\n";

                        list.Add(str);
                    }
                }

                return list;
            }
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
