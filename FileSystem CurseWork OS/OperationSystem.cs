using FileSystem_CurseWork_OS.Blocks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace FileSystem_CurseWork_OS
{
    class OperationSystem
    {
        FileSystem fs;

        string _path = "FS.data";

        string UserFile = "SAM";
        public string[] UsersData
        {
            get
            {
                var UsersData = Regex.Replace(fs.GetContentFromFile(UserFile), "\0+", "").Split('\n');
                return UsersData;
            }
        }
        public int CountUsers
        {
            get
            {
                return UsersData.Length - 1;
            }
        }

        ushort IDUser = 0;
        bool Admin = true;

        public string NameCurrentUser
        {
            get
            {
                return GetNameUserByNumber(IDUser);
            }
        }

        public OperationSystem(int SizeSector, int CountSectors)
        {
            SuperBlock.SizeSector = SizeSector;
            SuperBlock.CountSectors = CountSectors;

            File.Delete(_path);

            CreateOrReadFileSystem();
        }

        public OperationSystem()
        {
            CreateOrReadFileSystem();
        }

        private void CreateOrReadFileSystem()
        {
            //File.Delete(_path);

            if (File.Exists(_path))
            {
                fs = new FileSystem(_path, false);

                string Data = new string(Enumerable.Repeat('B', DataClasters.DataSectorSize / 2 + 1).ToArray());

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
                CreateNewUser("root", string.Empty, true);
            }
        }

        public void CreateFile(string Name, string Extension, string Value = null) => fs.CreateNewFile(Name, Extension, "rwxrwx", IDUser, Value);

        public void CreateNewUser(string Name, string Password, bool ForAdmin)
        {
            if (ForAdmin && !Admin)
                throw new FieldAccessException("У вас недостаточно прав, для добавления пользователя с правами администратора.");

            if (Regex.IsMatch(Name, ":") || Regex.IsMatch(Password, ":"))
                throw new ArgumentException("В имени и пароле пользователей не должны присутствовать знаки: {:}");

            //Добавь проверку на пользователя
            var UsersData = this.UsersData;

            if (UsersData.Length >= ushort.MaxValue) 
                throw new Exception("В системе присутствует максимальное количество пользователей!");

            if(UsersData.Where(x => Regex.IsMatch(x, @$"^{Name}")).Count() > 0)
                throw new ArgumentException("Пользователь уже существует!");

            //var NewUsersData = new string[UsersData.Length + 1];

            //UsersData.CopyTo(NewUsersData, UsersData.Length - 1);

            //if(ForAdmin)
            //    NewUsersData[NewUsersData.Length - 1] = $"{Name}:{Password}:1";
            //else
            //    NewUsersData[NewUsersData.Length - 1] = $"{Name}:{Password}:0";

            //fs.WriteStringInFile(UserFile, string.Join('\n', NewUsersData), false);

            var NewUser = string.Empty;

            if (ForAdmin)
                NewUser = $"{Name}:{Password}:1";
            else
                NewUser = $"{Name}:{Password}:0";

            fs.WriteStringInFile(UserFile, NewUser + "\n", true);
        }

        public List<string> GetListFiles()
        {
            var list = fs.GetListInformationAboutAllFiles();

            string pattern = @"^(.+?)\t(\d+)\t(.*)$";

            for(int i = 0; i < list.Count; i++)
            {
                list[i] = Regex.Replace(list[i], pattern, m =>
                {
                    ushort secondColumnValue = ushort.Parse(m.Groups[2].Value);
                    string newSecondColumnValue = GetNameUserByNumber(secondColumnValue);
                    return $"{m.Groups[1].Value}\t{newSecondColumnValue}\t{m.Groups[3].Value}";
                });
            }

            return list;
        }

        public void CopyFile(string Name, string NewName)
        {
            Checking_Access_Read_CurrentUserToFile(Name);

            fs.CopyFile(Name, NewName);
        }
        public void RemoveFile(string Name)
        {
            Checking_Access_ChangeParametrs_CurrentUserToFile(Name);

            fs.RemoveFile(Name);
        }

        public void RenameVile(string Name, string NewName)
        {
            Checking_Access_Write_CurrentUserToFile(Name);

            fs.RenameFile(Name, NewName);
        }

        public void OverwriteFile(string Name, string Value)
        {
            Checking_Access_Write_CurrentUserToFile(Name);

            fs.WriteStringInFile(Name, Value, false);
        }

        public void BeforeWritingFile(string Name, string Value)
        {
            Checking_Access_Write_CurrentUserToFile(Name);

            fs.WriteStringInFile(Name, Value, true);
        }

        public string GetFileContent(string Name)
        {
            Checking_Access_Read_CurrentUserToFile(Name);

            return fs.GetContentFromFile(Name);
        }

        public void ChangeFileAcess(string Name, string NewAcess)
        {
            Checking_Access_ChangeParametrs_CurrentUserToFile(Name);

            fs.ChangeFileAcess(Name, NewAcess);
        }

        public void ChangeFileCreator(string Name, string NewCreator)
        {
            Checking_Access_OnlyAdmin_CurrentUserToFile(Name);

            var Number = GetNumberUserByName(NewCreator);

            fs.ChangeNumberFileCreator(Name, Number);
        }

        private ushort GetNumberUserByName(string NameUser)
        {
            var UsersData = this.UsersData;
            UsersData = UsersData.Select(x => Regex.Replace(x, @":.*$", "")).ToArray();

            var ID = Array.IndexOf(UsersData, NameUser);

            if (ID == -1)
                throw new Exception("Пользователя с такми именем не существует!");

            return Convert.ToUInt16(ID);
        }

        private string GetNameUserByNumber(ushort Number)
        {
            var UsersData = this.UsersData;
            UsersData = UsersData.Select(x => Regex.Replace(x, @":.*$", "")).ToArray();

            try
            {
                if (Number > UsersData.Length - 1) 
                    throw new Exception();

                return UsersData[Number];
            } catch
            {
                throw new Exception("Пользователя с таким номером не обнаружено!");
            }

        }

        public void ChangeUser(string Name, string Password)
        {
            ushort NumberUser = GetNumberUserByName(Name);

            var UsersData = this.UsersData;

            var Name_Password_Access = UsersData[NumberUser].Split(':');

            if (!Name_Password_Access[1].Equals(Password))
                throw new ArgumentException("Не верные данные пользователя!");

            IDUser = NumberUser;

            switch(Name_Password_Access[2])
            {
                case "0":
                    Admin = false;
                    break;

                case "1":
                    Admin = true;
                    break;

                default:
                    throw new Exception("Not Bolean");
            }
        }

        public void DeleteUser(string NameUser)
        {
            if (!Admin)
                throw new FieldAccessException("У вас нет доступа для удаления пользователей!");

            var NumberUserToDelete = GetNumberUserByName(NameUser);

            if (IDUser == NumberUserToDelete) 
                throw new FieldAccessException("Вы не можете удалить текущую рабочую учетную запись!");

            if(GetNumberUserByName("root") == NumberUserToDelete)
                throw new FieldAccessException("Вы не можете удалить системного пользователя root!");

            var UsersData = this.UsersData;
            UsersData[NumberUserToDelete] = string.Empty;
            fs.DeleteFilesBelongingUser(NumberUserToDelete);

            //for (ushort i = 0; i < UsersData.Length; i++)
            //{
            //    if (Regex.IsMatch(UsersData[i], @$"^{NameUser}"))
            //    {
            //        fs.DeleteFilesBelongingUser(i);
            //        UsersData[i] = string.Empty;
            //        break;
            //    }
            //}

            fs.WriteStringInFile(UserFile, string.Join('\n', UsersData.Where(x => x.Length > 0)), false);
        }

        private void Checking_Access_OnlyAdmin_CurrentUserToFile(string Name)
        {
            if (!Admin)
                throw new FieldAccessException("У вас нет прав изменять параметры этого файла!");

            if (Name.Equals(UserFile))
                throw new FieldAccessException("Вы не можете изменить системный файл с пользователяеми!");
        }
        private void Checking_Access_ChangeParametrs_CurrentUserToFile(string Name)
        {
            var NumberCreator = fs.GetNumberFileСreator(Name);

            if (NumberCreator != IDUser || !Admin)
                throw new FieldAccessException("У вас нет прав изменять параметры этого файла!");

            if (Name.Equals(UserFile))
                throw new FieldAccessException("Вы не можете изменить системный файл с пользователяеми!");
        }
        private void Checking_Access_Write_CurrentUserToFile(string Name)
        {
            var Acess = fs.GetFileAcess(Name);
            var NumberCreator = fs.GetNumberFileСreator(Name);

            if ((NumberCreator == IDUser && !Regex.IsMatch(Acess, $"^.w.+")) || !Admin || (NumberCreator != IDUser && !Regex.IsMatch(Acess, $"^....w.")))
                throw new FieldAccessException("У вас нет прав записи или изменения этого файла!");

            if (Name.Equals(UserFile))
                throw new FieldAccessException("Вы не можете изменить системный файл с пользователяеми!");

        }

        private void Checking_Access_Read_CurrentUserToFile(string Name)
        {
            var Acess = fs.GetFileAcess(Name);
            var NumberCreator = fs.GetNumberFileСreator(Name);

            if ((NumberCreator == IDUser && !Regex.IsMatch(Acess, $"^r..+")) || !Admin || (NumberCreator != IDUser && !Regex.IsMatch(Acess, $"^...r..")))
                throw new FieldAccessException("У вас нет прав чтения этого файла!");

            if (Name.Equals(UserFile) && !Admin)
                throw new FieldAccessException("Вы не можете считать системный файл с пользователяеми!");
        }

    }
}
