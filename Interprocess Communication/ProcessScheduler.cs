using System.Diagnostics;

namespace Interprocess_Communication
{
    internal class ProcessScheduler
    {
        private const int QuantumOfTime_MS = 10;

        private const int CountRepetitionsToCompleteThePass = 20;

        private volatile List<Process> ListOfProcesses = new List<Process>();

        private volatile List<Process> ProcessQueue = new List<Process>();

        private static volatile bool RunUperation = true;

        private volatile bool _ForAbsolutePrioritets;

        public ProcessScheduler(bool ForAbsolutePrioritets)
        {
            _ForAbsolutePrioritets = ForAbsolutePrioritets;

            OperationsWithProcesses();
        }

        private async void OperationsWithProcesses()
        {
            await Task.Run(() =>
            {
                while (RunUperation)
                {
                    for (int i = 0; i < CountRepetitionsToCompleteThePass && RunUperation; i++)
                    {
                        RunOperationByFirst();

                        SortQueque();
                    }

                    var CountProcess = ProcessQueue.Count;

                    for (int i = 0; i < CountProcess && RunUperation; i++)
                    {
                        RunOperationByFirst();
                    }
                }
            });

        }

        private void RunOperationByFirst()
        {
            var process = GetFirstProcessInQuequeAndRemove();

            if (process == null)
                return;

            process.Condition = Process.States.R;

            var ProcessWork_MS = process.RequiredTime_MS;

            if (QuantumOfTime_MS > ProcessWork_MS)
                ListOfProcesses.Remove(process);
            else
            {
                process.RequiredTime_MS = ProcessWork_MS - QuantumOfTime_MS;

                AddProcessInQueque(process);
            }

            Thread.Sleep(Math.Min(QuantumOfTime_MS, ProcessWork_MS) * 10);

            if (process.RequiredTime_MS == 0)
                process.Condition = Process.States.Z;
            else
                process.Condition = Process.States.W;
        }

        private void SortQueque()
        {
            //Thread.Sleep(20000);
            if (ProcessQueue.Count > 1)
            {
                if (_ForAbsolutePrioritets)
                    ProcessQueue = ProcessQueue
                        .OrderBy(proc => proc.Priorety)
                        .ThenBy(proc => proc.RequiredTime_MS)
                        .ToList();   //В первую очередь сортировка по приоритетам
                else
                    ProcessQueue = ProcessQueue
                        .OrderBy(proc => proc.RequiredTime_MS)
                        .ThenBy(proc => proc.Priorety)
                        .ToList();   //В первую очередь сортировка по времени
            }
        }

        private void AddProcessInQueque(in Process process)
        {
            if (process == null)
                return;

            ProcessQueue.Add(process);
        }

        private Process GetFirstProcessInQuequeAndRemove()
        {
            if (ProcessQueue.Count == 0)
                return null;

            var first = ProcessQueue.First();

            ProcessQueue.Remove(first);

            return first;
        }

        public void AddNewProcess(int WorkingTime, sbyte Priorety = 0) //0 стандартный приоритет
        {
            int ProcessID = 0;

            if (ListOfProcesses.Count > 0)
                ProcessID = ListOfProcesses.OrderByDescending(process => process.ID_Process).First().ID_Process + 1;


            ListOfProcesses.Add(new Process(ProcessID, WorkingTime, Priorety));

            var Process = ListOfProcesses.Last();

            Process.Condition = Process.States.W;

            AddProcessInQueque(Process);
        }

        private Process GetProcessByID(int ID_Process)
        {
            if (ListOfProcesses.Count == 0)
                throw new ArgumentOutOfRangeException("В системе на данный момент нет ни одного процесса.");

            var ListProcess = ListOfProcesses.Where(process => process.ID_Process == ID_Process);

            if (ListProcess.Count() == 0)
                throw new ArrayTypeMismatchException("Процесса с заданным ID не существует!");

            return ListProcess.First();
        }

        public void ChangeProcessWorkingTime(int ID_Process, int WorkingTime)
        {
            if (WorkingTime < 0)
                throw new ArgumentException("Время работы процесса не может быть меньше 0");

            var Process = GetProcessByID(ID_Process);

            Process.RequiredTime_MS = WorkingTime;
        }

        public void ChangeProcessPriorety(int ID_Process, sbyte Priorety)
        {
            var Process = GetProcessByID(ID_Process);

            Process.Priorety = Priorety;
        }

        public List<string> GetListInfoProcess() => FillListProcessList(ListOfProcesses);

        public List<string> GetListQuequeInfoProcess() => FillListProcessList(ProcessQueue);

        private List<string> FillListProcessList(in List<Process> ProcessList)
        {
            List<string> list = new List<string>();

            list.Add("ID\tTime\tStatus\tPriorety");

            foreach (var process in ProcessList)
            {
                var ID = process.ID_Process.ToString();
                var Time = process.RequiredTime_MS.ToString();
                var Status = GetCharStatus(process.Condition);
                var Priorety = process.Priorety.ToString();

                list.Add(string.Join('\t', ID, Time, Status, Priorety));
            }

            return list;
        }

        private char GetCharStatus(Process.States status)
        {
            switch (status)
            {
                case Process.States.R:
                    return 'R';
                case Process.States.W:
                    return 'W';
                case Process.States.Z:
                    return 'Z';

                default:
                    throw new ArgumentException($"Статус процесса \"{status}\", не обнаружен!");
            }
        }

        ~ProcessScheduler()
        {
            RunUperation = false;
        }
    }
}
