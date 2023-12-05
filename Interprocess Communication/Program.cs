using Interprocess_Communication;

class Program
{
    static void Main(string[] args)
    {
        ProcessScheduler processScheduler = new ProcessScheduler();

        Console.WriteLine("Hello, World!");

        processScheduler.AddNewProcess(300, 19);
        processScheduler.AddNewProcess(10, -20);
        processScheduler.AddNewProcess(15);
        processScheduler.AddNewProcess(22);

        Console.ReadLine();
    }
}