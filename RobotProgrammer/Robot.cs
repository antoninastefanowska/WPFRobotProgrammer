using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace RobotProgrammer
{
    public class Robot
    {
        private Canvas canvas;
        private Rectangle control;
        private Button startButton;
        private int tileSize;
        private int width = 30;
        private int height = 40;

        public Position CurrentPosition { get; set; }
        public Compass CurrentOrientation { get; set; }
        public int CurrentInstructionIndex { get; set; }
        public Program CurrentProgram { get; set; }

        public bool Started { get; private set; }
        public bool Running { get; private set; }

        public static Robot Instance { get; private set; }

        private List<Storyboard> animations;

        public static void CreateInstance(Canvas canvas, Button startButton, int tileSize)
        {
            Instance = new Robot(canvas, startButton, tileSize);
        }

        private Robot(Canvas canvas, Button startButton, int tileSize)
        {
            this.canvas = canvas;
            this.tileSize = tileSize;
            this.startButton = startButton;
            CreateControl();
            Started = false;
            Running = false;
            animations = new List<Storyboard>();
        }

        private void CreateControl()
        {
            control = new Rectangle();
            control.Width = width;
            control.Height = height;
            control.Fill = new SolidColorBrush(Colors.BlueViolet);
            control.RenderTransformOrigin = new Point(0.5, 0.5);
            control.Visibility = Visibility.Hidden;
            canvas.Children.Add(control);
        }

        public void RunSimulation()
        {
            SetInitialPosition();
            SetInitialDirection();
            CreateAnimations();
            control.Visibility = Visibility.Visible;
            animations[0].Begin();
            CurrentProgram.Instructions[0].IsActive = true;
            startButton.IsEnabled = false;
        }

        public void PauseSimulation()
        {
            animations[CurrentInstructionIndex].Pause();
            Running = false;
        }

        public void ResumeSimulation()
        {
            animations[CurrentInstructionIndex].Resume();
            Running = true;
        }

        public void FinishSimulation()
        {
            control.Visibility = Visibility.Hidden;
            canvas.Children.Remove(control);
            CreateControl();
        }

        public void SetInitialDirection()
        {
            switch (CurrentOrientation.Orientation)
            {
                case Compass.OrientationType.North:
                    FaceNorth();
                    break;
                case Compass.OrientationType.East:
                    FaceEast();
                    break;
                case Compass.OrientationType.West:
                    FaceWest();
                    break;
                case Compass.OrientationType.South:
                    FaceSouth();
                    break;
            }
        }

        public void SetInitialPosition()
        {
            double x = (CurrentPosition.X + 0.5) * tileSize;
            double y = (CurrentPosition.Y + 0.5) * tileSize;
            Canvas.SetLeft(control, x);
            Canvas.SetTop(control, y);
        }

        public void FaceNorth()
        {
            RotateTransform north = new RotateTransform(0);
            control.RenderTransform = north;
        }

        public void FaceEast()
        {
            RotateTransform east = new RotateTransform(90);
            control.RenderTransform = east;
        }

        public void FaceWest()
        {
            RotateTransform west = new RotateTransform(270);
            control.RenderTransform = west;
        }

        public void FaceSouth()
        {
            RotateTransform south = new RotateTransform(180);
            control.RenderTransform = south;
        }

        private void CreateAnimations()
        {
            for (int i = CurrentInstructionIndex; i < CurrentProgram.Instructions.Count; i++)
            {
                Instruction instruction = CurrentProgram.Instructions[i];
                switch (instruction.InstructionType)
                {
                    case Instruction.Type.Forward:
                        animations.Add(GetMoveAnimation(instruction.Repeat));
                        break;
                    case Instruction.Type.TurnLeft:
                        animations.Add(GetTurnAnimation(-90));
                        CurrentOrientation.TurnLeft();
                        break;
                    case Instruction.Type.TurnRight:
                        animations.Add(GetTurnAnimation(90));
                        CurrentOrientation.TurnRight();
                        break;
                }
            }
        }

        private Storyboard GetMoveAnimation(int repeat)
        {
            Storyboard storyboard = new Storyboard();
            DoubleAnimation move = new DoubleAnimation();

            switch (CurrentOrientation.Orientation)
            {
                case Compass.OrientationType.North:
                    move.By = -repeat * tileSize;
                    Storyboard.SetTargetProperty(move, new PropertyPath("(Canvas.Top)"));
                    break;
                case Compass.OrientationType.South:
                    move.By = repeat * tileSize;
                    Storyboard.SetTargetProperty(move, new PropertyPath("(Canvas.Top)"));
                    break;
                case Compass.OrientationType.West:
                    move.By = -repeat * tileSize;
                    Storyboard.SetTargetProperty(move, new PropertyPath("(Canvas.Left)"));
                    break;
                case Compass.OrientationType.East:
                    move.By = repeat * tileSize;
                    Storyboard.SetTargetProperty(move, new PropertyPath("(Canvas.Left)"));
                    break;
            }
            move.Duration = new Duration(TimeSpan.FromSeconds(repeat * 0.5));
            storyboard.Completed += InstructionAnimation_Completed;

            SineEase sineEase = new SineEase();
            sineEase.EasingMode = EasingMode.EaseInOut;
            move.EasingFunction = sineEase;

            Storyboard.SetTarget(move, control);
            storyboard.Children.Add(move);
            return storyboard;
        }

        private Storyboard GetTurnAnimation(double angle)
        {
            Storyboard storyboard = new Storyboard();
            DoubleAnimation turn = new DoubleAnimation();

            turn.By = angle;
            turn.Duration = new Duration(TimeSpan.FromSeconds(0.5));

            storyboard.Completed += InstructionAnimation_Completed;

            SineEase sineEase = new SineEase();
            sineEase.EasingMode = EasingMode.EaseInOut;
            turn.EasingFunction = sineEase;

            Storyboard.SetTarget(turn, control);
            Storyboard.SetTargetProperty(turn, new PropertyPath("(UIElement.RenderTransform).(RotateTransform.Angle)"));
            storyboard.Children.Add(turn);
            return storyboard;
        }

        private void InstructionAnimation_Completed(object sender, EventArgs e)
        {
            CurrentProgram.Instructions[CurrentInstructionIndex].IsActive = false;
            CurrentInstructionIndex++;
            if (CurrentInstructionIndex < CurrentProgram.Instructions.Count)
            {
                CurrentProgram.Instructions[CurrentInstructionIndex].IsActive = true;
                animations[CurrentInstructionIndex].Begin();
            }
            else
            {
                animations.Clear();
                CurrentInstructionIndex = 0;
                startButton.IsEnabled = true;
                Running = false;
                Started = false;
            }
        }
    }
}
