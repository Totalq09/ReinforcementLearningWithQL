using Prism.Commands;
using QL.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

namespace QL
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public QLViewModel ViewModel { get; set; }

        public ICommand GenerateCommand { get; set; }

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

            this.LoaderModal.Background = new SolidColorBrush(Color.FromArgb(220, 60, 60, 60));
        }

        public void ReinitializeArea()
        {
            this.LockButtons();
            this.LockSliders();
            this.Loader.Visibility = Visibility.Visible;

            Task.Run(() => 
            {
                ViewModel.GenerateNewArea();
            })
            .ContinueWith((task) =>
            {
                DrawMap();
                this.UnlockAll();
                this.Loader.Visibility = Visibility.Hidden;
            }, TaskScheduler.FromCurrentSynchronizationContext());     
        }

        private void DrawMap()
        {
            var grid = new Grid();
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

                    if (ViewModel.AreaModel.WorldSize < 24)
                    {
                        Viewbox viewBox1 = new Viewbox();
                        viewBox1.Stretch = Stretch.UniformToFill;
                        var t1 = new TextBlock();
                        t1.Text = "0.5";
                        viewBox1.Child = t1;

                        t1.HorizontalAlignment = HorizontalAlignment.Center;
                        t1.VerticalAlignment = VerticalAlignment.Center;

                        Grid.SetRow(viewBox1, 0);
                        Grid.SetColumn(viewBox1, 0);
                        innerGrid.Children.Add(viewBox1);
                    }

                    Rectangle rect = new Rectangle();
                    rect.Stroke = Brushes.Black;
                    this.SetBackgroundBasedOnContent(rect, ViewModel.AreaModel.Cells[i, j].CellContent);
                    rect.StrokeThickness = 1;

                    Grid.SetRow(rect, 0);
                    Grid.SetColumn(rect, 0);

                    innerGrid.Children.Add(rect);

                    Grid.SetRow(innerGrid, i);
                    Grid.SetColumn(innerGrid, j);

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
                    rectangle.Fill = Brushes.IndianRed;
                    break;
            }
        }

        public void LockButtons()
        {
            this.Simulate.IsEnabled = false;
            this.Generate.IsEnabled = false;
            this.Learn.IsEnabled = false;
        }

        public void LockSliders()
        {
            this.IterationsPerSecondInput.IsEnabled = false;
            this.IterationInput.IsEnabled = false;
            this.ExitQuantityInput.IsEnabled = false;
            this.WorldSizeInput.IsEnabled = false;
        }

        public void UnlockAll()
        {
            this.Simulate.IsEnabled = true;
            this.Generate.IsEnabled = true;
            this.Learn.IsEnabled = true;

            this.IterationsPerSecondInput.IsEnabled = true;
            this.IterationInput.IsEnabled = true;
            this.ExitQuantityInput.IsEnabled = true;
            this.WorldSizeInput.IsEnabled = true;
        }
    }
}
