using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Fttd
{
    /// <summary>
    /// Логика взаимодействия для Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
            gr.Children.Add(CreateGrid(60, 35));
        }

        public int Daysbetweendates(DateTime dataA, DateTime dataB)
        {
            return (dataB - dataA).Days;
        }

        public Grid CreateGrid(int col, int row)
        {
            Grid daytask = new Grid() { ShowGridLines = false, };
            for (int i = 0; i < col; ++i)
            {
                daytask.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(40) });
            }
            for (int i = 0; i < row; ++i)
            {
                daytask.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });
            }
            for (int i = 0; i < col; i++)
            {
                TextBlock tbTemp = new TextBlock();
                tbTemp.VerticalAlignment = VerticalAlignment.Bottom;
                tbTemp.HorizontalAlignment = HorizontalAlignment.Center;
                DateTime today = DateTime.Now;
                DateTime tomorrow = today.AddDays(i);
                tbTemp.Text = tomorrow.ToString("dd.MM");
                Border border = new Border();
                BrushConverter bc1 = new BrushConverter();
                border.BorderBrush = (Brush)bc1.ConvertFrom("#FF000000");
                border.BorderThickness = new Thickness(1, 0, 0, 0);
                border.Child = tbTemp;
                daytask.Children.Add(border);
                Grid.SetRow(border, 0);
                Grid.SetColumn(border, i);
                for (int j = 0; j < row; j++)
                {
                    Border border1 = new Border();
                    border1.BorderBrush = (Brush)bc1.ConvertFrom("#FF000000");
                    border1.BorderThickness = new Thickness(1, 0, 0, 0);
                    daytask.Children.Add(border1);
                    Grid.SetRow(border1, j);
                    Grid.SetColumn(border1, i);
                }
            }
            Random a = new Random();
            for (int i = 1; i < 15; i++)
            {
                Rectangle rectangle = new Rectangle();
                int f = a.Next(1, 60);
                rectangle.Width = f * 40 - 6;
                rectangle.Height = 20;
                rectangle.VerticalAlignment = VerticalAlignment.Center;
                rectangle.HorizontalAlignment = HorizontalAlignment.Left;
                rectangle.RadiusX = 10;
                rectangle.RadiusY = 10;
                BrushConverter bc = new BrushConverter();
                rectangle.Fill = (Brush)bc.ConvertFrom("#FF7C56BF");
                rectangle.Stroke = (Brush)bc.ConvertFrom("#FF000000");
                rectangle.Margin = new Thickness(3, 0, 0, 0);
                daytask.Children.Add(rectangle);
                Grid.SetRow(rectangle, i);
                Grid.SetColumn(rectangle, 0);
                Grid.SetColumnSpan(rectangle, f);
            }
            return daytask;

        }
    }
}
