using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.SymbolStore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    for(int i = 0; i < CountRepetitionsToCompleteThePass && RunUperation;  i++)
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

            var ProcessWork_MS = process.RequiredTime_MS;

            if (QuantumOfTime_MS > ProcessWork_MS)
                ListOfProcesses.Remove(process);
            else
            {
                process.RequiredTime_MS = ProcessWork_MS - QuantumOfTime_MS;
                AddProcessInQueque(process);
            }

            //Task.Delay(Math.Min(QuantumOfTime_MS, ProcessWork_MS));
            Thread.Sleep(Math.Min(QuantumOfTime_MS, ProcessWork_MS));
        }

        private void SortQueque()
        {
            //Thread.Sleep(20000);
            if(ProcessQueue.Count > 1)
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
                ProcessID = ListOfProcesses.OrderBy(process => process.ID_Process).First().ID_Process + 1;

            ListOfProcesses.Add(new Process(ProcessID, WorkingTime, Priorety));
            AddProcessInQueque(ListOfProcesses.Last());
        }

        private Process GetProcessByID(int ID_Process)
        {
            if (ListOfProcesses.Count == 0)
                throw new ArgumentOutOfRangeException("В системе на данный момент нет ни одного процесса.");

            var ListProcess = ListOfProcesses.Where(process => process.ID_Process == ID_Process);

            if(ListProcess.Count() == 0)
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

        ~ProcessScheduler()
        {
            RunUperation = false;
        }
    }
}
