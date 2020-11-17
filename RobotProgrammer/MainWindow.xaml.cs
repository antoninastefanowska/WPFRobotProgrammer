using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RobotProgrammer
{
    public partial class MainWindow : Window
    {

        private int width = 15;
        private int height = 15;
        private int tileSize = 25;

        private string port = "OUT_AC";
        private string step = "500";
        private string wheel = "5.5";
        private string axle = "7.5";

        private SolidColorBrush checkedColor = new SolidColorBrush(Colors.LightGreen);
        private SolidColorBrush uncheckedColor = new SolidColorBrush(Colors.AliceBlue);

        private Canvas mapCanvas;
        private Robot robot;
        private Map map;
        private Program program;

        public MainWindow()
        {
            InitializeComponent();
            PopulateMapControl();
            map = new Map(height, width);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SizeToContent = SizeToContent.Manual;
        }

        private Rectangle CreatePositionControl()
        {
            Rectangle rectangle = new Rectangle();
            rectangle.Fill = uncheckedColor;
            rectangle.HorizontalAlignment = HorizontalAlignment.Stretch;
            rectangle.VerticalAlignment = VerticalAlignment.Stretch;
            rectangle.Margin = new Thickness(1);
            rectangle.Width = tileSize;
            rectangle.Height = tileSize;
            rectangle.MouseLeftButtonDown += PositionControl_MouseLeftButtonDown;
            rectangle.MouseRightButtonDown += PositionControl_MouseRightButtonDown;
            return rectangle;
        }

        private void PopulateMapControl()
        {
            ColumnDefinition columnDefinition;
            RowDefinition rowDefinition;

            for (int i = 0; i < width; i++)
            {
                columnDefinition = new ColumnDefinition();
                columnDefinition.Width = GridLength.Auto;
                MapGrid.ColumnDefinitions.Add(columnDefinition);
            }

            for (int i = 0; i < height; i++)
            {
                rowDefinition = new RowDefinition();
                rowDefinition.Height = GridLength.Auto;
                MapGrid.RowDefinitions.Add(rowDefinition);
            }

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    Rectangle rectangle = CreatePositionControl();
                    MapGrid.Children.Add(rectangle);
                    Grid.SetRow(rectangle, j);
                    Grid.SetColumn(rectangle, i);
                }
            }

            mapCanvas = new Canvas();
            MapGrid.Children.Add(mapCanvas);
            Grid.SetColumnSpan(mapCanvas, width);
            Grid.SetRowSpan(mapCanvas, height);

            Robot.CreateInstance(mapCanvas, StartButton, tileSize);
            robot = Robot.Instance;
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            program = map.GenerateProgram();
            program.Port = port;
            program.Step = step;
            program.Wheel = wheel;
            program.Axle = axle;
            InstructionListView.ItemsSource = program.Instructions;
            CodeTextBox.Text = program.GenerateCode();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (!robot.Started)
            {
                robot.CurrentProgram = program;
                robot.CurrentPosition = new Position(map.PositionList[0].X, map.PositionList[0].Y);
                robot.CurrentOrientation = new Compass(map.StartCompass.Orientation);
                robot.RunSimulation();
            }
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            if (robot.Running)
            {
                StopButton.Content = "Wznów";
                robot.PauseSimulation();
            }
            else
            {
                StopButton.Content = "Stop";
                robot.ResumeSimulation();
            }
        }

        private void EndButton_Click(object sender, RoutedEventArgs e)
        {
            robot.FinishSimulation();
        }

        private void PositionControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle rectangle = sender as Rectangle;
            int x = Grid.GetColumn(rectangle);
            int y = Grid.GetRow(rectangle);
            if (map.AddPosition(x, y))
                rectangle.Fill = checkedColor;
        }

        private void PositionControl_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle rectangle = sender as Rectangle;
            int x = Grid.GetColumn(rectangle);
            int y = Grid.GetRow(rectangle);
            if (map.RemovePosition(x, y) && !map.Contains(x, y))
                rectangle.Fill = uncheckedColor;
        }

        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog fileDialog = new SaveFileDialog();
            fileDialog.Filter = "NXC file (*.nxc)|*.nxc";
            if (fileDialog.ShowDialog() == true)
                File.WriteAllText(fileDialog.FileName, CodeTextBox.Text);
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (UIElement rectangle in MapGrid.Children)
                if (rectangle is Rectangle)
                    (rectangle as Rectangle).Fill = uncheckedColor;
            map.Clear();
            program.Clear();
            InstructionListView.ClearValue(ItemsControl.ItemsSourceProperty);
            CodeTextBox.Text = "";
            robot.FinishSimulation();
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            OptionWindow optionWindow = new OptionWindow();
            if (optionWindow.ShowDialog() == true)
            {
                port = optionWindow.Port;
                step = optionWindow.Step;
                wheel = optionWindow.Wheel;
                axle = optionWindow.Axle;
            }
        }
    }
}
