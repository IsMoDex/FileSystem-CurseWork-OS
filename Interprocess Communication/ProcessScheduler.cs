using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        public ProcessScheduler() { OperationsWithProcesses(); }

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
                ProcessQueue = ProcessQueue.OrderBy(proc => proc.RequiredTime_MS).ThenBy(proc => proc.Priorety).ToList();
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

        ~ProcessScheduler()
        {
            RunUperation = false;
        }
    }
}
