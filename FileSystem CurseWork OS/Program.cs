using FileSystem_CurseWork_OS;
using System.Text;

class Program
{
    static void Main(string[] args)
    {
        var os = new OperationSystem();
        Console.WriteLine("Egor Woyandus [Version 1]\n");

        while(true)
        {
            Console.Write(">");

            switch (Console.ReadLine())
            {
                case "ls":
                    var LSlist = os.GetListFiles();
                    Console.WriteLine($"итого {LSlist.Count}");
                    Console.WriteLine(string.Join("", LSlist));
                    break;
            }

        }

    }

}
