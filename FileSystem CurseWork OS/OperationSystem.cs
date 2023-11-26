using FileSystem_CurseWork_OS.Blocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileSystem_CurseWork_OS
{
    class OperationSystem
    {
        FileSystem fs;

        string _path = "FS.data";
        string UserFile = "SAM";
        ushort IDUser = 0;

        public OperationSystem()
        {

            File.Delete(_path);

            if (File.Exists(_path))
            {
                fs = new FileSystem(_path, false);

                string Data = new string(Enumerable.Repeat('B', DataClasters.DataSectorSize / 2 + 1).ToArray());

                //CreateNewFile("TestFile", "txt", "rwxrwx", 0, new string (Enumerable.Repeat('A', DataClasters.DataSectorSize * 3 - DataClasters.DataSectorSize / 2).ToArray()));

                fs.WriteStringInFile("TestFile", Data, false);


                //TestFS();
            }
            else
            {
                using (FileStream stream = new FileStream(_path, FileMode.Create))
                {
                    var Count = SuperBlock.SizeFileSystem;

                    for (long i = 0; i < Count; ++i)
                    {
                        stream.WriteByte(0);
                    }
                }

                fs = new FileSystem(_path, true);

                fs.CreateNewFile(UserFile, string.Empty, "rw----", 0);
                CreateNewUser("root", string.Empty);
            }
        }

        public void CreateFile(string Name, string Extension, string Value = null)
        {
            fs.CreateNewFile(Name, Extension, "rwxrwx", IDUser, Value);
        }

        public void ChangeFileAcess(string NameFile, string NewAcess)
        {
            if (fs.GetNumberFileСreator(NameFile) != IDUser)
                throw new FieldAccessException("У вас нет прав доступа к этому файлу!");

            fs.ChangeFileAcess(NameFile, NewAcess);
        }

        public void CreateNewUser(string Name, string Password)
        {
            //Добавь проверку на пользователя
            var UsersData = Regex.Replace(fs.GetContentFromFile(UserFile), "\0+", "").Split('\n');

            foreach(var userdata in UsersData) 
            {
                //var SplitData = userdata.Split(":");
                //var NameUser = SplitData[0];
                //var PasswordUser = SplitData[1];

                string NameSelectUser = Regex.Replace(userdata, @":.*", "");

                if (NameSelectUser.Equals(Name))
                    throw new ArgumentException("Пользователь уже существует!");
            }

            fs.WriteStringInFile(UserFile, $"{Name}:{Password}\n", true);
        }

        public List<string> GetListFiles()
        {
            return fs.GetListInformationAboutAllFiles();
        }

    }
}
