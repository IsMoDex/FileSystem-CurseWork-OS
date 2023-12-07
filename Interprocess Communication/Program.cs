using Interprocess_Communication;
using System.Text.RegularExpressions;
using System.Threading.Tasks.Dataflow;

class Program
{
    static OperationSystem os;
    static void Main(string[] args)
    {
        os = new OperationSystem(GetParametries());

        Console.Clear();

        MainMenu();
    }

    static bool GetParametries()
    {
        Console.WriteLine("Выбирите вариант запуска системы.");
        Console.WriteLine("Нажмите 1:\nАбсолютные приоритеты\nСтатические приоритеты\n");
        Console.WriteLine("Нажмите 2:\nОтносительные приоритеты\nДинамические приоритеты\n");

        while (true)
        {
            switch (Console.ReadKey(true).Key)
            {
                case ConsoleKey.D1:
                    return true;

                case ConsoleKey.D2:
                    return false;
            }
        }
    }

    static void MainMenu()
    {
        while(true)
        {
            try
            {
                Console.Write(">");

                switch (Console.ReadLine())
                {
                    case string s when Regex.IsMatch(s, @"^newp\s\d+(\s\d+)?$"):

                        var NewProcessSting = Regex.Replace(s, @"^newp\s", "");

                        if (Regex.IsMatch(s, @"^newp\s\d+\s\d+$"))
                        {
                            var Time_and_Priorety = NewProcessSting.Split(' ');
                            var Time = int.Parse(Time_and_Priorety[0]);
                            var Priorety = sbyte.Parse(Time_and_Priorety[1]);
                            os.AddNewProcess(Time, Priorety);
                        }
                        else
                        {
                            var Time = int.Parse(NewProcessSting);
                            os.AddNewProcess(Time);
                        }

                        break;

                    case string s when Regex.IsMatch(s, @"^chpwt(\s\d+){2}$"):

                        var ChangeWorkTimeString = Regex.Replace(s, @"^chpwt\s", "");

                        {
                            var ID_and_WorkTime = ChangeWorkTimeString.Split(' ');

                            var ID = int.Parse(ID_and_WorkTime[0]);
                            var WorkTime = int.Parse(ID_and_WorkTime[1]);

                            os.ChangeProcessWorkinTime(ID, WorkTime);
                        }

                        break;

                    case string s when Regex.IsMatch(s, @"^chpp(\s\d+)\s(\d+|-\d+)$"):

                        var ChangePrioretyString = Regex.Replace(s, @"^chpp\s", "");

                        {
                            var ID_and_Priorety = ChangePrioretyString.Split(' ');

                            var ID = int.Parse(ID_and_Priorety[0]);
                            var Priorety = sbyte.Parse(ID_and_Priorety[1]);

                            os.ChangeProcessPriorety(ID, Priorety);
                        }

                        break;

                    case string s when Regex.IsMatch(s, @"^gen\s\d+$"):

                        var GenerateString = Regex.Replace(s, @"^gen\s", "");

                        {
                            var Count = int.Parse(GenerateString);
                            os.GenerateProcess(Count);
                        }

                        break;

                    case "ps":
                        Console.WriteLine(string.Join('\n', os.GetListInfoProcess()));
                        break;

                    case "top":

                        WriteTopProcessOperation();

                        while (true)
                        {
                            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

                            // Проверяем, была ли нажата комбинация Ctrl + Z
                            if ((keyInfo.Modifiers & ConsoleModifiers.Control) != 0 && keyInfo.Key == ConsoleKey.Z)
                            {
                                WriteTop = false;
                                Console.Clear();
                                break;
                            }
                        }

                        break;

                    case "clear":
                        Console.Clear();
                        break;

                    case "help":

                        Console.WriteLine("newp\t<time> <priority>\tСоздать новый процесс с временем работы <time> и приоритетом <priority>. Приоритет можно не указывать.");
                        Console.WriteLine("chpwt\t<id> <time>\tИзменить время работы процесса с идентификатором <id> на время <time>.");
                        Console.WriteLine("chpp\t<id> <priorety>\tИзменить приоритет процесса с идентификатором <id> на приоритет <priorety>.");
                        Console.WriteLine("gen\t<count>\tСгенерировать количество <count> новый процессов.");
                        Console.WriteLine("ps\tОтобразить все существующие процессы.");
                        Console.WriteLine("top\tОтображение всех существующих процессов в реальном времени в порядке очереди.");
                        Console.WriteLine("clear\tОчищает консолб.");

                        break;

                    default:
                        Console.WriteLine("Команда не обнаружена, вы можете воспользоваться help для получения списка команд.");
                        break;
                }
            } 
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }

    private static volatile bool WriteTop = true; 

    static async void WriteTopProcessOperation()
    {
        WriteTop = true;

        await Task.Run(() =>
        {
            while (WriteTop)
            {
                Console.Clear();
                Console.WriteLine(string.Join('\n', os.GetListQuequeInfoProcess()));
                Thread.Sleep(500);
            }
        });
    }

}