using Prism.Commands;
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
        }

        public void ReinitializeArea()
        {
            ViewModel.GenerateNewArea();

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
                    innerGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    innerGrid.RowDefinitions.Add(new RowDefinition());
                    innerGrid.ColumnDefinitions.Add(new ColumnDefinition());
                    innerGrid.RowDefinitions.Add(new RowDefinition());

                    if(ViewModel.AreaModel.WorldSize < 24)
                    {
                        Viewbox viewBox1 = new Viewbox();
                        viewBox1.Stretch = Stretch.UniformToFill;
                        var t1 = new TextBlock();
                        t1.Text = "0.5";
                        viewBox1.Child = t1;

                        Viewbox viewBox2 = new Viewbox();
                        viewBox2.Stretch = Stretch.UniformToFill;
                        var t2 = new TextBlock();
                        t2.Text = "0.5";
                        viewBox2.Child = t2;

                        Viewbox viewBox3 = new Viewbox();
                        viewBox3.Stretch = Stretch.UniformToFill;
                        var t3 = new TextBlock();
                        t3.Text = "0.5";
                        viewBox3.Child = t3;

                        Viewbox viewBox4 = new Viewbox();
                        viewBox4.Stretch = Stretch.UniformToFill;
                        var t4 = new TextBlock();
                        t4.Text = "0.5";
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

                    Rectangle rect = new Rectangle();
                    rect.Stroke = Brushes.Black;
                    rect.StrokeThickness = 1;

                    Grid.SetRow(rect, 0);
                    Grid.SetColumn(rect, 0);

                    Grid.SetRowSpan(rect, 3);
                    Grid.SetColumnSpan(rect, 3);

                    innerGrid.Children.Add(rect);

                    Grid.SetRow(innerGrid, i);
                    Grid.SetColumn(innerGrid, j);

                    grid.Children.Add(innerGrid);
                }
            }

            MainGrid.Children.RemoveAt(0);
            MainGrid.Children.Insert(0, grid);
        }
    
    }
}
