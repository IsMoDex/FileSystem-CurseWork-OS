using Interprocess_Communication;

class Program
{
    static OperationSystem os;
    static void Main(string[] args)
    {
        bool ConstParametries = false;

        Console.WriteLine("");

        os = new OperationSystem(ConstParametries);
    }
}