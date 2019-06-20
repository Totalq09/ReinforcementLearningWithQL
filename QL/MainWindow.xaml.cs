using Prism.Commands;
using QL.Helpers;
using QL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace QL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public QLViewModel ViewModel { get; set; }

        public QLEngine Engine { get; set; }

        public ICommand GenerateCommand { get; set; }
        public ICommand LearnCommand { get; set; }
        public ICommand TriggerAutoCommand { get; set; }
        public ICommand GoFurtherCommand { get; set; }
        public ICommand ResetCommand { get; set; }
        public ICommand PlayCommand { get; set; }

        private Grid AreaGrid { get; set; }

        private CancellationTokenSource PlayCancellationToken { get; set; }
        private bool OnPlay { get; set; }
        private bool ResetCalled { get; set; }

        public MainWindow()
        {
            InitializeComponent();
            ViewModel = new QLViewModel();
            DataContext = new
            {
                vm = ViewModel,
                windowVm = this
            };
            GenerateCommand = new DelegateCommand(ReinitializeArea);
            LearnCommand = new DelegateCommand(StartLearning);
            TriggerAutoCommand = new DelegateCommand(TriggerAutoIteration);
            GoFurtherCommand = new DelegateCommand(GoFurther);
            ResetCommand = new DelegateCommand(Reset);
            PlayCommand = new DelegateCommand(Play);

            this.LoaderModal.Background = new SolidColorBrush(Color.FromArgb(220, 60, 60, 60));
            this.AreaGrid = new Grid();

            this.PlayCancellationToken = new CancellationTokenSource();
        }

        public void StartLearning()
        {
            this.LockButtons();
            this.LockSliders();
            this.UndoReset();

            Engine = new QLEngine(ViewModel.AreaModel, 
                int.Parse(ViewModel.Iterations), 
                int.Parse(ViewModel.IterationLength), 
                double.Parse(ViewModel.IterationsPerSecond), 
                ViewModel.ManualIteration, 
                ViewModel.UnlimitedIterations);      
            
            Engine.ProgressUpdate += (s, e) =>
            {
                this.Dispatcher.Invoke(
                    DispatcherPriority.Render, (Action)(() =>
                {
                    this.DrawMap();
                }));
            };
            Engine.IterationUpdate += (s, e) =>
            {
                this.Dispatcher.Invoke(
                    DispatcherPriority.Render, (Action)(() =>
                    {
                        ViewModel.CurrentIteration = (e as IterationUpdateEventArgs).Iteration.ToString();
                    }));
            };

            Task.Run(() =>
            {
                ViewModel.AgentReadyToPlay = false;
                Engine.Initialize();
                Engine.Learn();
            })
             .ContinueWith((task) =>
             {
                 DrawMap();
                 this.UnlockAll();
                 ViewModel.EnvironmentReadyForLearning = true;

                 if (ResetCalled)
                     ResetCalled = false;
                 else
                    ViewModel.AgentReadyToPlay = true;

             }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void Play()
        {
            if (Engine == null)
                return;

            if(this.OnPlay == true)
            {
                PlayCancellationToken.Cancel();
                return;
            }

            this.LockSliders();
            this.LockButtons();

            this.OnPlay = true;
            this.PlayButton.Content = "Stop Simulation";
            Task.Run(() =>
            {
                Engine.Play(PlayCancellationToken.Token);
            })
            .ContinueWith((task) =>
            {
                DrawMap();
                PlayCancellationToken = new CancellationTokenSource();
                this.UnlockAll();
                this.ViewModel.AgentReadyToPlay = true;
                this.ViewModel.EnvironmentReadyForLearning = true;
                this.Loader.Visibility = Visibility.Hidden;
                this.OnPlay = false;
                this.PlayButton.Content = "Show Agent Behaviour";
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void ReinitializeArea()
        {
            this.LockButtons();
            this.LockSliders();
            this.Loader.Visibility = Visibility.Visible;

            Task.Run(() => 
            {
                ViewModel.EnvironmentReadyForLearning = false;
                ViewModel.AgentReadyToPlay = false;
                ViewModel.GenerateNewArea();
            })
            .ContinueWith((task) =>
            {
                DrawMap();
                this.UnlockAll();
                this.Loader.Visibility = Visibility.Hidden;
                ViewModel.EnvironmentReadyForLearning = true;
            }, TaskScheduler.FromCurrentSynchronizationContext());     
        }

        public void DrawMap()
        {
            var grid = this.AreaGrid;
            grid.ColumnDefinitions.Clear();
            grid.RowDefinitions.Clear();
            grid.Children.Clear();
            grid.Margin = new Thickness(5, 0, 0, 0);
            grid.MinHeight = 1000;
            grid.MinWidth = 995;

            for (int i = 0; i < ViewModel.AreaModel.WorldSize; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.RowDefinitions.Add(new RowDefinition());
            }

            for (int i = 0; i < ViewModel.AreaModel.WorldSize; i++)
            {
                for (int j = 0; j < ViewModel.AreaModel.WorldSize; j++)
                {
                    Grid innerGrid = new Grid();

                    innerGrid.MinWidth = 1000 / ViewModel.AreaModel.WorldSize;
                    innerGrid.MinHeight = 1000 / ViewModel.AreaModel.WorldSize;

                    innerGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    innerGrid.RowDefinitions.Add(new RowDefinition());
                    innerGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    innerGrid.RowDefinitions.Add(new RowDefinition());
                    innerGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    innerGrid.RowDefinitions.Add(new RowDefinition());

                    Rectangle rect = new Rectangle();
                    rect.Stroke = Brushes.Black;
                    this.SetBackgroundBasedOnContent(rect, ViewModel.AreaModel.Cells[i, j].CellContent);
                    rect.StrokeThickness = 1;

                    Grid.SetRow(rect, 0);
                    Grid.SetColumn(rect, 0);
                    Grid.SetColumnSpan(rect, 3);
                    Grid.SetRowSpan(rect, 3);

                    innerGrid.Children.Add(rect);

                    Grid.SetRow(innerGrid, i);
                    Grid.SetColumn(innerGrid, j);

                    if (ViewModel.AreaModel.WorldSize < 36 && ViewModel.AreaModel.Cells[i, j].CellContent != CellContentEnum.Wall &&
                        ViewModel.AreaModel.Cells[i, j].CellContent != CellContentEnum.Exit)
                    {
                        if(!ViewModel.AreaModel.Exits.Any(item => item.Item1 == i && item.Item2 == j))
                        {
                            Viewbox viewBox1 = new Viewbox();
                            var t1 = new TextBlock();
                            t1.Text = ViewModel.AreaModel.Cells[i, j].CurrentReward0.ToString("0.0#");
                            viewBox1.Child = t1;

                            Viewbox viewBox2 = new Viewbox();
                            var t2 = new TextBlock();
                            t2.Text = ViewModel.AreaModel.Cells[i, j].CurrentReward3.ToString("0.0#");
                            viewBox2.Child = t2;

                            Viewbox viewBox3 = new Viewbox();
                            var t3 = new TextBlock();
                            t3.Text = ViewModel.AreaModel.Cells[i, j].CurrentReward1.ToString("0.0#");
                            viewBox3.Child = t3;

                            Viewbox viewBox4 = new Viewbox();
                            var t4 = new TextBlock();
                            t4.Text = ViewModel.AreaModel.Cells[i, j].CurrentReward2.ToString("0.0#");
                            viewBox4.Child = t4;

                            t1.HorizontalAlignment = HorizontalAlignment.Center;
                            t1.VerticalAlignment = VerticalAlignment.Center;
                            t2.HorizontalAlignment = HorizontalAlignment.Center;
                            t2.VerticalAlignment = VerticalAlignment.Center;
                            t3.HorizontalAlignment = HorizontalAlignment.Center;
                            t3.VerticalAlignment = VerticalAlignment.Center;
                            t4.HorizontalAlignment = HorizontalAlignment.Center;
                            t4.VerticalAlignment = VerticalAlignment.Center;

                            Grid.SetRow(viewBox1, 0);
                            Grid.SetColumn(viewBox1, 1);
                            innerGrid.Children.Add(viewBox1);

                            Grid.SetRow(viewBox2, 1);
                            Grid.SetColumn(viewBox2, 0);
                            innerGrid.Children.Add(viewBox2);

                            Grid.SetRow(viewBox3, 1);
                            Grid.SetColumn(viewBox3, 2);
                            innerGrid.Children.Add(viewBox3);

                            Grid.SetRow(viewBox4, 2);
                            Grid.SetColumn(viewBox4, 1);
                            innerGrid.Children.Add(viewBox4);
                        }                        
                    }

                    grid.Children.Add(innerGrid);
                }
            }

            MainGrid.Children.RemoveAt(0);
            MainGrid.Children.Insert(0, grid);
        }

        private void SetBackgroundBasedOnContent(Rectangle rectangle, CellContentEnum cellContentEnum)
        {
            switch(cellContentEnum)
            {
                case CellContentEnum.Exit:
                    rectangle.Fill = Brushes.Aqua;
                    break;
                case CellContentEnum.Free:
                    rectangle.Fill = Brushes.White;
                    break;
                case CellContentEnum.Wall:
                    rectangle.Fill = new SolidColorBrush(Color.FromRgb(40, 40, 40));
                    break;
                case CellContentEnum.Agent:
                    rectangle.Fill = Brushes.IndianRed; //new SolidColorBrush(Color.FromArgb(128, 0xcd, 0x5c, 0x5c));
                    break;
            }
        }

        public void LockButtons()
        {
            //this.ViewModel.AgentReadyToPlay = false;
            this.GenerateButton.IsEnabled = false;
            this.ViewModel.EnvironmentReadyForLearning = false;
            //this.ResetButton.IsEnabled = false;
        }

        public void LockSliders()
        {
            this.IterationsPerSecondInput.IsEnabled = false;
            this.IterationLengthInput.IsEnabled = false;
            this.IterationInput.IsEnabled = false;
            this.ExitQuantityInput.IsEnabled = false;
            this.WorldSizeInput.IsEnabled = false;
        }

        public void UnlockAll()
        {
            //this.ViewModel.AgentReadyToPlay = true;
            this.GenerateButton.IsEnabled = true;
            //this.ViewModel.EnvironmentReadyForLearning = true;

            this.IterationsPerSecondInput.IsEnabled = true;
            this.IterationLengthInput.IsEnabled = true;
            this.IterationInput.IsEnabled = true;
            this.ExitQuantityInput.IsEnabled = true;
            this.WorldSizeInput.IsEnabled = true;
           // this.ResetButton.IsEnabled = true;
        }

        public void TriggerAutoIteration()
        {
            ViewModel.ManualIteration = !ViewModel.ManualIteration;
            if(Engine != null)
                Engine.ManualIteration = ViewModel.ManualIteration;
        }

        public void GoFurther()
        {
            if(Engine != null)
            {
                Engine.GoFurtherWithIteration = true;
            }
        }

        public void Reset()
        {
            if (Engine != null)
            { 
                Engine.Reset = true;
                this.ResetCalled = true;
            }
        }

        public void UndoReset()
        {
            if (Engine != null)
            {
                Engine.Reset = false;
                this.ResetCalled = false;
            }
        }
    }
}
