using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interprocess_Communication
{
    internal class Process
    {
        private sbyte _priorety;

        public Process(int ID_Process, int Time, sbyte Priorety)
        {
            this.ID_Process = ID_Process;
            RequiredTime_MS = Time;
            this.Priorety = Priorety;
        }

        public int ID_Process { get; }

        public int RequiredTime_MS;

        public const sbyte MinRange = -20;
        public const sbyte MaxRange = 19;

        public enum States
        {
            R,
            W,
            Z
        }

        public States Condition = States.W;

        public sbyte Priorety
        {
            get => _priorety;
            set
            {
                if (value > MaxRange || value < MinRange)
                    throw new ArgumentOutOfRangeException($"Приоритет должен быть в диапазоне от {MinRange} до {MaxRange} включительно");

                _priorety = value;
            }
        }

    }
}
