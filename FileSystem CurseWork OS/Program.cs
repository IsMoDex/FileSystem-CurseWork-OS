using FileSystem_CurseWork_OS;
using System.Text;
using System.Text.RegularExpressions;

class Program
{
    static OperationSystem os;
    static void Main(string[] args)
    {
        os = new OperationSystem();
        Console.WriteLine("Egor Woyandus [Version 1]\n");

        Autorization();

        while (true)
        {
            Console.Write(os.NameCurrentUser + ">");

            try
            {
                switch (Console.ReadLine())
                {
                    case "ls":
                        var LSlist = os.GetListFiles();
                        Console.WriteLine($"итого {LSlist.Count}");
                        Console.WriteLine(string.Join("", LSlist));
                        break;

                    case string s when Regex.IsMatch(s, @"^cp\s.{1,}\s.{1,}$"):
                        var NamesToCopy = Regex.Replace(s, @"^cp\s", "").Split(' ');
                        os.CopyFile(NamesToCopy[0], NamesToCopy[1]);
                        break;

                    case string s when Regex.IsMatch(s, @"^touch\s(.+|.+\s.+)$"):
                        var DataOfFileToCreate = Regex.Replace(s, @"^touch\s", "");

                        var FullNameFileToCreate = Regex.Replace(DataOfFileToCreate, @"\s.+", "");
                        var ValuesFileToCreate = Regex.Replace(DataOfFileToCreate, @"^" + FullNameFileToCreate + @"\s{0,1}", "");

                        if (ValuesFileToCreate.Length == 0)
                            ValuesFileToCreate = null;

                        os.CreateFile(Path.GetFileNameWithoutExtension(FullNameFileToCreate), Path.GetExtension(FullNameFileToCreate), ValuesFileToCreate);

                        break;

                    case string s when Regex.IsMatch(s, @"^rm\s.+$"):
                        os.RemoveFile(Regex.Replace(s, @"^rm\s", ""));  //Возможно нужно будет править ткк я не очищаю инод
                        break;

                    case string s when Regex.IsMatch(s, @"^echo"):

                        if(Regex.IsMatch(s, @"^echo\s.+\s>>\s.+$"))
                        {
                            //Если добавить в конец
                            var EchoVal = Regex.Replace(s, @"^echo\s", "");
                            var Values = Regex.Replace(EchoVal, @"\s>>.+", "");
                            var NameFile = Regex.Replace(EchoVal, @".+>>\s", "");
                            os.BeforeWritingFile(NameFile, Values);
                        }
                        else if(Regex.IsMatch(s, @"^echo\s.+\s>\s.+$")) 
                        {
                            //Если перезаписать файл
                            var EchoVal = Regex.Replace(s, @"^echo\s", "");
                            var Values = Regex.Replace(EchoVal, @"\s>.+", "");
                            var NameFile = Regex.Replace(EchoVal, @".+>\s", "");
                            os.OverwriteFile(NameFile, Values);
                        }
                        else if(Regex.IsMatch(s, @"^echo\s.+$"))
                        {
                            //Просто сказать в консоль
                            var Value = Regex.Replace(s, @"^echo\s", "");
                            Console.WriteLine(Value);
                        }
                        else
                            Console.WriteLine("echo. Не верный параметр.");

                        break;

                    case string s when Regex.IsMatch(s, @"^cat\s.{1,}$"):
                        var NameFileToRead = Regex.Replace(s, @"^cat\s", "");
                        Console.WriteLine(os.GetFileContent(NameFileToRead));
                        break;

                    case string s when Regex.IsMatch(s, @"^chmod\s.{1,}\s.{1,}$"):
                        var ValuesToChangeAcess = Regex.Replace(s, @"^chmod\s", "").Split(' '); //Проверить работу с другими пользователями
                        os.ChangeFileAcess(ValuesToChangeAcess[1], ValuesToChangeAcess[0]);
                        break;

                    case string s when Regex.IsMatch(s, @"^chown\s.{1,}\s.{1,}$"):
                        var ValuesToChangeOwn = Regex.Replace(s, @"^chown\s", "").Split(' ');
                        os.ChangeFileCreator(ValuesToChangeOwn[1], ValuesToChangeOwn[0]);
                        break;

                    case string s when Regex.IsMatch(s, @"^rename\s.+\s.+$"):
                        var ValuesToChangeName = Regex.Replace(s, @"^rename\s", "").Split(' ');
                        os.RenameVile(ValuesToChangeName[0], ValuesToChangeName[1]);
                        break;

                    case string s when Regex.IsMatch(s, @"^useradd\s.{1,}\s.{1,}\s.{1,}$"):
                        var ValuesToCreateUser = Regex.Replace(s, @"^useradd\s", "").Split(' ');
                        os.CreateNewUser(ValuesToCreateUser[0], ValuesToCreateUser[1], bool.Parse(ValuesToCreateUser[2]));
                        break;

                    case string s when Regex.IsMatch(s, @"^userdel\s.{1,}$"):
                        var NameUserToDelete = Regex.Replace(s, @"^userdel\s", "");
                        os.DeleteUser(NameUserToDelete);
                        break;

                    case string s when Regex.IsMatch(s, @"^login\s.+\s.+$"):
                        var ValuesToLogin = Regex.Replace(s, @"^login\s", "").Split(' ');
                        os.ChangeUser(ValuesToLogin[0], ValuesToLogin[1]);
                        break;

                    case "logout":

                        Console.WriteLine("Вы уверены что хотите выйти из системы? Y/N");
                        if (Regex.IsMatch(Console.ReadLine(), @"^[yY]"))
                            return;

                        break;

                    case "formatting":
                        Console.WriteLine("Вы уверены что хотите форматировать диск? Все данные будут удалены! Y/N");
                        if (Regex.IsMatch(Console.ReadLine(), @"^[yY]"))
                        {
                            Console.WriteLine("Введите размер сектора в байтах:");
                            var SizeSector = int.Parse(Console.ReadLine());
                            Console.WriteLine("Введите количество секторов в файловой системе:");
                            var CountClasters = int.Parse(Console.ReadLine());

                            os = new OperationSystem(SizeSector, CountClasters);
                            Autorization();
                        }
                        break;

                    case "users":
                        Console.WriteLine(string.Join('\n', os.GetAllUsers()));
                        break;

                    case "clear":
                        Console.Clear();
                        break;

                    case "help":

                        Console.WriteLine(
                            "ls\tОтображает содержимое корневой директории.\n" +
                            "cp\t<file>\tКопирует файлы  <file> \n" +
                            "touch\t<file>\tСоздает новый файл <file> или обновляет время его последнего доступа и модификации.\n" +
                            "rm\t<file>\tУдаляет указанный файл.\n" +
                            "echo\t<text> > <file>\tЗаписывает текст <text> в файл <file>. Может быть использована для дописывания в конец файла с >>.\n" +
                            "cat\t<file>\tВыводит текст из файла <file> в консоль.\n" +
                            "chmod\t<permissions> <file>\tИзменяет права доступа к файлу в соответствии с указанными <permissions>.\n" +
                            "chown\t<user> <file>\tИзменяет владельца (<user>) файла <file>.\n" +
                            "rename\t<file> <name>\tИзменяет название <name> файла <file>.\n" +
                            "useradd\t<username> <passowrd> <admin>\tСоздает нового пользователя с указанным именем <username>, паролем <passowrd> и правами администратора true или false в <admin>.\n" +
                            "userdel\t<username>\tУдаляет пользователя с указанным именем <username>.\n" +
                            "login\t<username> <passowrd>\tВход в систему под указанным именем пользователя <username> с использованием пароля <passowrd>.\n" +
                            "logout\t-\tВыход из системы.\n" +
                            "formatting\t-\tФорматирует диск.\n" +
                            "users\t-\tОтображает всех существующих пользователей в системе\n" +
                            "clear\t-\tОчистить консоль\n"
                            );

                        break;

                    default:
                        Console.WriteLine("Команда не обнаружена, вы можете воспользоваться help для получения списка команд.");
                        break;
                }
            } catch(Exception ex) 
            {
                Console.WriteLine(ex.Message);
            }
        }

    }

    static void Autorization()
    {
        while (true)
        {
            if (os.CountUsers < 2)
            {
                Console.WriteLine("Создайте учетную запись, придумайте имя и пароль.");
                Console.Write("Name:");
                var Name = Console.ReadLine();
                Console.Write("Password:");
                var Passowrd = Console.ReadLine();
                try
                {
                    os.CreateNewUser(Name, Passowrd, true);
                    os.ChangeUser(Name, Passowrd);
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            else
            {
                Console.WriteLine("Войдите в существующую учетную запись.");
                Console.Write("Name:");
                var Name = Console.ReadLine();
                Console.Write("Password:");
                var Passowrd = Console.ReadLine();

                try
                {
                    os.ChangeUser(Name, Passowrd);
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }

}
