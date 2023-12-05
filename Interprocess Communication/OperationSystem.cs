using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interprocess_Communication
{
    internal class OperationSystem
    {
        ProcessScheduler processScheduler;

        static bool _ForConstParametries = true;

        public OperationSystem(bool ForConstParametries)
        {
            _ForConstParametries = ForConstParametries;
            processScheduler = new ProcessScheduler(_ForConstParametries);
        }

        public void AddNewProcess(int WorkingTime, sbyte Priorety = 0) => processScheduler.AddNewProcess(WorkingTime, Priorety);

        public void ChangeProcessWorkinTime(int ID_Process, int WorkingTime) => processScheduler.ChangeProcessWorkingTime(ID_Process, WorkingTime);

        public void ChangeProcessPriorety(int ID_Process, sbyte Priorety)
        {
            if (_ForConstParametries)
                throw new ArgumentException("Нельзя менять приоритеты процессам пока система находиться в режиме статичных приоритетов.");

            processScheduler.ChangeProcessPriorety(ID_Process, Priorety);
        }

        public void GenerateProcess(int Count)
        {
            Random random = new Random();

            while(Count-- > 0)
                AddNewProcess(random.Next(0, 1000), (sbyte)random.Next(Process.MinRange, Process.MaxRange));
        }

    }
}
