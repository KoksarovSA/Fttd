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
            Dbaccess dbaccess = new Dbaccess();
            dbaccess.Dbselect("SELECT [task], [project], [dir], [note], [iscurrent], [datein], [dateout] FROM [task] WHERE [iscurrent] = True ORDER BY [datein]");
            List<Tasken> zad = new List<Tasken>();
            for (int j = 0; j < dbaccess.Querydata.Count; ++j)
            {
                string[] vs = dbaccess.Querydata[j];
                bool a = Convert.ToBoolean(vs[4]);
                DateTime b = DateTime.Parse(vs[5]);
                DateTime c = DateTime.Parse(vs[6]);
                Tasken tasken = new Tasken(vs[0], vs[1], vs[2], vs[3], a, b, c);
                zad.Add(tasken);
            }
            gr.Children.Add(CreateGrid(60, zad));
        }

       

        public Grid CreateGrid(int col, List<Tasken> tasks)
        {
            Grid daytask = new Grid() { ShowGridLines = false};
            for (int i = 0; i < col; ++i)
            {
                daytask.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(40) });
            }
            for (int i = 0; i < tasks.Count+1; ++i)
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
                border.BorderThickness = new Thickness(0, 0, 1, 1);
                border.Child = tbTemp;
                daytask.Children.Add(border);
                Grid.SetRow(border, 0);
                Grid.SetColumn(border, i);
                for (int j = 0; j < tasks.Count+1; j++)
                {
                    Border border1 = new Border();
                    border1.BorderBrush = (Brush)bc1.ConvertFrom("#FF000000");
                    border1.BorderThickness = new Thickness(0, 0, 1, 1);
                    daytask.Children.Add(border1);
                    Grid.SetRow(border1, j);
                    Grid.SetColumn(border1, i);
                }
            }

            for (int i = 0; i < tasks.Count; ++i)
            {
                if (tasks[i].Dateout < DateTime.Now || tasks[i].Days <= 0)
                {
                    Rectangle rectangle = new Rectangle();
                    rectangle.Width = 520 - 6;
                    rectangle.Height = 20;
                    rectangle.VerticalAlignment = VerticalAlignment.Center;
                    rectangle.HorizontalAlignment = HorizontalAlignment.Left;
                    rectangle.RadiusX = 10;
                    rectangle.RadiusY = 10;
                    BrushConverter bc = new BrushConverter();                   
                    rectangle.Fill = (Brush)bc.ConvertFrom("#FFD64E48");
                    rectangle.Stroke = (Brush)bc.ConvertFrom("#FF9C9C9C");
                    rectangle.Margin = new Thickness(3, 0, 0, 0);
                    TextBlock tbRectangle = new TextBlock();
                    tbRectangle.VerticalAlignment = VerticalAlignment.Center;
                    tbRectangle.HorizontalAlignment = HorizontalAlignment.Left;
                    tbRectangle.Foreground = (Brush)bc.ConvertFrom("#FFFFFFFF");
                    tbRectangle.Text = "     " + tasks[i].Tasks + "  Проект: " + tasks[i].Project + "  Дата выдачи: " + tasks[i].Datein.ToString("dd.MM.yy") + "  Выполнить до: " + tasks[i].Dateout.ToString("dd.MM.yy");
                    daytask.Children.Add(rectangle);
                    daytask.Children.Add(tbRectangle);
                    Grid.SetRow(rectangle, i+1);
                    Grid.SetColumn(rectangle, 0);
                    Grid.SetColumnSpan(rectangle, 13);
                    Grid.SetRow(tbRectangle, i+1);
                    Grid.SetColumn(tbRectangle, 0);
                    Grid.SetColumnSpan(tbRectangle, 13);
                }
                else
                {
                    Rectangle rectangle = new Rectangle();
                    rectangle.Width = (tasks[i].Days + 2) * 40 - 6;
                    rectangle.Height = 20;
                    rectangle.VerticalAlignment = VerticalAlignment.Center;
                    rectangle.HorizontalAlignment = HorizontalAlignment.Left;
                    rectangle.RadiusX = 10;
                    rectangle.RadiusY = 10;
                    BrushConverter bc = new BrushConverter();
                    if (tasks[i].Days <= 7) rectangle.Fill = (Brush)bc.ConvertFrom("#FFBFB939");
                    else rectangle.Fill = (Brush)bc.ConvertFrom("#FF21A4CB");
                    rectangle.Stroke = (Brush)bc.ConvertFrom("#FF9C9C9C");
                    rectangle.Margin = new Thickness(3, 0, 0, 0);
                    TextBlock tbRectangle = new TextBlock();
                    tbRectangle.VerticalAlignment = VerticalAlignment.Center;
                    tbRectangle.HorizontalAlignment = HorizontalAlignment.Left;
                    tbRectangle.Foreground = (Brush)bc.ConvertFrom("#FFFFFFFF");
                    tbRectangle.Text = "     " + tasks[i].Tasks + "  Проект: " + tasks[i].Project + "  Дата выдачи: " + tasks[i].Datein.ToString("dd.MM.yy") + "  Выполнить до: " + tasks[i].Dateout.ToString("dd.MM.yy");
                    daytask.Children.Add(rectangle);
                    daytask.Children.Add(tbRectangle);
                    Grid.SetRow(rectangle, i+1);
                    Grid.SetColumn(rectangle, 0);
                    Grid.SetColumnSpan(rectangle, tasks[i].Days + 2);
                    Grid.SetRow(tbRectangle, i+1);
                    Grid.SetColumn(tbRectangle, 0);
                    Grid.SetColumnSpan(tbRectangle, 13);
                }
            }
            return daytask;

        }

        private void gridtop_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { this.DragMove(); }
            catch { }
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
    
    
}
