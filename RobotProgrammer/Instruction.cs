using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotProgrammer
{
    public class Instruction : INotifyPropertyChanged
    {
        public enum Type
        {
            Forward,
            TurnLeft,
            TurnRight
        }

        public Type InstructionType { get; }
        public int Repeat { get; set; }

        private bool isActive;
        public bool IsActive
        {
            get
            {
                return isActive;
            }
            set
            {
                isActive = value;
                OnPropertyChanged("IsActive");
            }
        }

        public Instruction(Type instructionType)
        {
            InstructionType = instructionType;
            Repeat = 1;
        }

        public string GetSnippet()
        {
            switch (InstructionType)
            {
                case Type.Forward:
                    return "goForward(" + Repeat.ToString() + ");";
                case Type.TurnLeft:
                    return "turnLeft();";
                case Type.TurnRight:
                    return "turnRight();";
            }
            return null;
        }

        public override string ToString()
        {
            switch (InstructionType)
            {
                case Type.Forward:
                    return "Forward(" + Repeat.ToString() + ")";
                case Type.TurnLeft:
                    return "Turn Left";
                case Type.TurnRight:
                    return "Turn Right";
            }
            return null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
