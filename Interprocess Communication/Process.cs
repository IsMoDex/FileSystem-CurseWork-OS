using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interprocess_Communication
{
    internal class Process
    {
        public Process(int ID_Process, int Time, byte Priorety)
        {
            this.ID_Process = ID_Process;
            RequiredTime_MS = Time;
            this.Priorety = Priorety;
        }

        private int ID_Process { get; }

        public int RequiredTime_MS;
        public byte Priorety
        {
            get => Priorety;
            set
            {
                if (value > 19 || value < -20)
                    throw new ArgumentOutOfRangeException("Приоритет должен быть в диапазоне от -20 до 19 включительно");

                Priorety = value;
            }
        }
        
    }
}
