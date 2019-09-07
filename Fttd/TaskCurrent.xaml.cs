using Fttd.Entities;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
            State.UpdateTaskStatusColl();
            if (TaskEmployeeUpdate()) { gr.Children.Add(CreateGrid(60, OutputTask())); }
        }

        internal bool TaskEmployeeUpdate()
        {
            if (State.employeeColl != null)
            {
                TaskEmployee.Items.Clear();
                foreach (Employees item in State.employeeColl.OrderBy(item => item.LastName).Where(item => item.ShortName != "Нет" && item.ShortName != "Системное сообщение"))
                {
                    TaskEmployee.Items.Add(item.ShortName);
                }
                TaskEmployee.Text = State.employee.ShortName;
                return true;
            }
            else return false;
        }

        internal Collection<TaskDet> OutputTask()
        {
            Collection<TaskDet> ts = new Collection<TaskDet>();
            switch (TaskEmployee.SelectedItem)
            {
                case "Все": foreach (var item in State.taskColl.Where(item => item.TaskIsCurrent == true).OrderBy(item => item.TaskDateIn)) { ts.Add(item); }; break;
                default: foreach (var item in State.taskColl.Where(item => item.TaskIsCurrent == true && item.Leading == Convert.ToString(TaskEmployee.SelectedItem)).OrderBy(item => item.TaskDateIn)) { ts.Add(item); }; break;
            }
            return ts;
        }

        internal Grid CreateGrid(int col, Collection<TaskDet> tasks)
        {
            Grid daytask = new Grid() { ShowGridLines = false };
            for (int i = 0; i < col; ++i)
            {
                daytask.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(40) });
            }
            for (int i = 0; i < tasks.Count + 1; ++i)
            {
                daytask.RowDefinitions.Add(new RowDefinition() { MinHeight = 40, Height = new GridLength(0, GridUnitType.Auto) });
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
                for (int j = 0; j < tasks.Count + 1; j++)
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
                if (tasks[i].TaskDateOut < DateTime.Now || tasks[i].Days <= 0)
                {
                    BrushConverter bc = new BrushConverter();
                    Rectangle rectangle = new Rectangle()
                    {
                        Width = 80 - 6,
                        Height = 6,
                        Stroke = (Brush)bc.ConvertFrom("#FFCED6D8"),
                        Fill = (Brush)bc.ConvertFrom("#FF135291"),
                        RadiusY = 3,
                        RadiusX = 3,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(3, 3, 10, 10),
                        VerticalAlignment = VerticalAlignment.Top
                    };
                    Rectangle rectangle1 = new Rectangle()
                    {
                        Width = 600 - 6,
                        Stroke = (Brush)bc.ConvertFrom("#FFCED6D8"),
                        Fill = (Brush)bc.ConvertFrom("#FFD17875"),
                        RadiusY = 10,
                        RadiusX = 10,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(3, 10, 10, 10),
                        VerticalAlignment = VerticalAlignment.Stretch
                    };

                    TextBlock tbRectangle = new TextBlock()
                    {
                        Width = 50,
                        Margin = new Thickness(3, 13, 0, 0),
                        Text = "   " + tasks[i].TaskName,
                        Foreground = (Brush)bc.ConvertFrom("#000000"),
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    StackPanel stack1 = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Top };

                    Button btAddState = new Button()
                    {
                        Foreground = (Brush)bc.ConvertFrom("#FFFFFFFF"),
                        BorderBrush = (Brush)bc.ConvertFrom("#FFFFFFFF"),
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        BorderThickness = new Thickness(1, 1, 1, 1),
                        Padding = new Thickness(0, -3, 0, 0),
                        Margin = new Thickness(5, 15, 0, 0),
                        Width = 15,
                        Height = 15,
                        Background = null,
                        FontWeight = FontWeights.Bold,
                        Content = "+",                        
                    };
                    btAddState.Click += (object sender, RoutedEventArgs e) => 
                    {
                        AddStateTask addStateTask = new AddStateTask();
                        addStateTask.Show();
                        addStateTask.numTask.Text = tbRectangle.Text.Trim();
                        addStateTask.dateNow.Text = Convert.ToString(DateTime.Now);

                    };  
                    Expander ex = new Expander()
                    {
                        Style = null,                        
                        Header = "Текущее состояние: " + State.taskStatusColl.OrderByDescending(item => item.Data).FirstOrDefault(item => item.Task.Contains(tasks[i].TaskName))?.Status ?? " ",
                        Padding = new Thickness(10, 0, 0, 0),
                        Margin = new Thickness(0, 10, 10, 13),
                        FontSize = 13,
                        Background = null,
                        Foreground = (Brush)bc.ConvertFrom("#000000"),
                        HorizontalContentAlignment = HorizontalAlignment.Left,
                        VerticalContentAlignment = VerticalAlignment.Top,
                        MaxWidth = 500
                    };
                    stack1.Children.Add(tbRectangle);
                    stack1.Children.Add(btAddState);
                    stack1.Children.Add(ex);

                    StackPanel stack2 = new StackPanel() { Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Width = 580 };

                    tbRectangle.AddHandler(TextBlock.MouseLeftButtonUpEvent, new RoutedEventHandler(Element_Click));
                    tbRectangle.AddHandler(TextBlock.MouseRightButtonUpEvent, new RoutedEventHandler(Element_MouseRightClick));

                    ToolTip tool = new ToolTip();
                    ToolTipService.SetShowDuration(rectangle, 60000);
                    ToolTipService.SetPlacement(rectangle, System.Windows.Controls.Primitives.PlacementMode.Mouse);
                    tool.Content = "Задание: " + tasks[i].TaskName + "\nПроект: " + tasks[i].ProjectTaskName + "\nОтветственный: " + tasks[i].Leading + "\nДата выдачи: " + tasks[i].TaskDateIn.ToString("dd.MM.yy") + "\nВыполнить до: " + tasks[i].TaskDateOut.ToString("dd.MM.yy") + "\nПрошло " + Math.Abs(tasks[i].Days).ToString() + " " + DeyOverDeys(tasks[i].Days) + " после\nокончания срока ";
                    rectangle1.ToolTip = tool;
                    stack1.ToolTip = tool;
                    stack2.ToolTip = tool;
                    ex.ToolTip = tool;

                    foreach (TaskStatus item in State.taskStatusColl.Where(item => item.Task == tasks[i].TaskName.Trim()).OrderByDescending(item => item.Data))
                    {
                        stack2.Children.Add( new Expander()
                        {
                            Header = item.Data.ToString() + " " + item.Status,
                            Style = null,
                            Background = null,
                            Padding = new Thickness(20, 0, 0, 0),
                            Foreground = (Brush)bc.ConvertFrom("#000000"),
                            Content = new TextBlock()
                            {
                                Text = "Состояние: " + item.Status + "\nДеталь: " + item.Detail + "\nПроблема: " + item.Problem + "\nРешение: " + item.Solution + "\nСотрудник: " + item.Employee,
                                Margin = new Thickness(20, 0, 0, 0),
                                TextWrapping = TextWrapping.Wrap,
                                Width = 580,
                                Foreground = (Brush)bc.ConvertFrom("#000000")
                            }
                        });
                    }
                    ex.Content = stack2;

                    daytask.Children.Add(rectangle);
                    daytask.Children.Add(rectangle1);
                    daytask.Children.Add(stack1);
                    daytask.VerticalAlignment = VerticalAlignment.Top;

                    Grid.SetRow(stack1, i + 1);
                    Grid.SetColumn(stack1, 0);
                    Grid.SetColumnSpan(stack1, 16);

                    Grid.SetRow(rectangle1, i + 1);
                    Grid.SetColumn(rectangle1, 0);
                    Grid.SetColumnSpan(rectangle1, 16);

                    Grid.SetRow(rectangle, i + 1);
                    Grid.SetColumn(rectangle, 0);
                    Grid.SetColumnSpan(rectangle, 3);
                    //Grid.SetRow(tbRectangle, i + 1);
                    //Grid.SetColumn(tbRectangle, 0);
                    //Grid.SetColumnSpan(tbRectangle, 2);
                }
                else
                {
                    BrushConverter bc = new BrushConverter();
                    Rectangle rectangle = new Rectangle()
                    {                        
                        Height = 6,
                        Stroke = (Brush)bc.ConvertFrom("#FFCED6D8"),
                        Fill = (Brush)bc.ConvertFrom("#FF135291"),
                        RadiusY = 3,
                        RadiusX = 3,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(3, 3, 10, 10),
                        VerticalAlignment = VerticalAlignment.Top
                    };
                    rectangle.Width = (tasks[i].Days + 2) * 40 - 6;
                    Rectangle rectangle1 = new Rectangle()
                    {
                        Width = 600 - 6,
                        Stroke = (Brush)bc.ConvertFrom("#FFCED6D8"),
                        RadiusY = 10,
                        RadiusX = 10,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(3, 10, 10, 10),
                        VerticalAlignment = VerticalAlignment.Stretch
                    };
                    if (tasks[i].Days <= 7) rectangle1.Fill = (Brush)bc.ConvertFrom("#FFBFB939");
                    else rectangle1.Fill = (Brush)bc.ConvertFrom("#FF21A4CB");
                    TextBlock tbRectangle = new TextBlock()
                    {
                        Width = 50,
                        Margin = new Thickness(3, 13, 0, 0),
                        Text = "   " + tasks[i].TaskName,
                        Foreground = (Brush)bc.ConvertFrom("#000000"),
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    StackPanel stack1 = new StackPanel() { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Top };

                    Button btAddState = new Button()
                    {
                        Foreground = (Brush)bc.ConvertFrom("#FFFFFFFF"),
                        BorderBrush = (Brush)bc.ConvertFrom("#FFFFFFFF"),
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        BorderThickness = new Thickness(1, 1, 1, 1),
                        Padding = new Thickness(0, -3, 0, 0),
                        Margin = new Thickness(5, 15, 0, 0),
                        Width = 15,
                        Height = 15,
                        Background = null,
                        FontWeight = FontWeights.Bold,
                        Content = "+",
                    };
                    btAddState.Click += (object sender, RoutedEventArgs e) =>
                    {
                        AddStateTask addStateTask = new AddStateTask();
                        addStateTask.Show();
                        addStateTask.numTask.Text = tbRectangle.Text.Trim();
                        addStateTask.dateNow.Text = Convert.ToString(DateTime.Now);

                    };
                    Expander ex = new Expander()
                    {
                        Style = null,
                        Header = "Текущее состояние: " + State.taskStatusColl.OrderByDescending(item => item.Data).FirstOrDefault(item => item.Task.Contains(tasks[i].TaskName))?.Status ?? " ",
                        Padding = new Thickness(10, 0, 0, 0),
                        Margin = new Thickness(0, 10, 10, 13),
                        FontSize = 13,
                        Background = null,
                        Foreground = (Brush)bc.ConvertFrom("#000000"),
                        HorizontalContentAlignment = HorizontalAlignment.Left,
                        VerticalContentAlignment = VerticalAlignment.Top,
                        MaxWidth = 500
                    };
                    stack1.Children.Add(tbRectangle);
                    stack1.Children.Add(btAddState);
                    stack1.Children.Add(ex);

                    StackPanel stack2 = new StackPanel() { Orientation = Orientation.Vertical, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Width = 580 };

                    tbRectangle.AddHandler(TextBlock.MouseLeftButtonUpEvent, new RoutedEventHandler(Element_Click));
                    tbRectangle.AddHandler(TextBlock.MouseRightButtonUpEvent, new RoutedEventHandler(Element_MouseRightClick));

                    ToolTip tool = new ToolTip();
                    ToolTipService.SetShowDuration(rectangle, 60000);
                    ToolTipService.SetPlacement(rectangle, System.Windows.Controls.Primitives.PlacementMode.Mouse);
                    tool.Content = "Задание: " + tasks[i].TaskName + "\nПроект: " + tasks[i].ProjectTaskName + "\nОтветственный: " + tasks[i].Leading + "\nДата выдачи: " + tasks[i].TaskDateIn.ToString("dd.MM.yy") + "\nВыполнить до: " + tasks[i].TaskDateOut.ToString("dd.MM.yy") + "\nОсталось " + (tasks[i].Days + 2).ToString() + " " + DeyOverDeys(tasks[i].Days + 2);
                    rectangle1.ToolTip = tool;
                    stack1.ToolTip = tool;
                    stack2.ToolTip = tool;
                    ex.ToolTip = tool;
                    foreach (TaskStatus item in State.taskStatusColl.Where(item => item.Task == tasks[i].TaskName.Trim()).OrderByDescending(item => item.Data))
                    {
                        stack2.Children.Add(new Expander()
                        {
                            Header = item.Data.ToString() + " " + item.Status,
                            Style = null,
                            Background = null,
                            Padding = new Thickness(20, 0, 0, 0),
                            Foreground = (Brush)bc.ConvertFrom("#000000"),
                            Content = new TextBlock()
                            {
                                Text = "Состояние: " + item.Status + "\nДеталь: " + item.Detail + "\nПроблема: " + item.Problem + "\nРешение: " + item.Solution + "\nСотрудник: " + item.Employee,
                                Margin = new Thickness(20, 0, 0, 0),
                                TextWrapping = TextWrapping.Wrap,
                                Width = 580,
                                Foreground = (Brush)bc.ConvertFrom("#000000")
                            }
                        });
                    }
                    ex.Content = stack2;

                    daytask.Children.Add(rectangle);
                    daytask.Children.Add(rectangle1);
                    daytask.Children.Add(stack1);
                    daytask.VerticalAlignment = VerticalAlignment.Top;

                    Grid.SetRow(stack1, i + 1);
                    Grid.SetColumn(stack1, 0);
                    Grid.SetColumnSpan(stack1, 16);

                    Grid.SetRow(rectangle1, i + 1);
                    Grid.SetColumn(rectangle1, 0);
                    Grid.SetColumnSpan(rectangle1, 16);

                    Grid.SetRow(rectangle, i + 1);
                    Grid.SetColumn(rectangle, 0);
                    Grid.SetColumnSpan(rectangle, tasks[i].Days + 3);

                }
            }
            return daytask;
        }

        private void Element_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (State.employeeColl != null) Process.Start(@"" + State.DirFiles + "\\" + State.taskColl.First(item => item.TaskIsCurrent == true &&
                item.TaskName.Contains((sender as TextBlock).Text.Trim())).TaskDir + "");
            }
            catch { MessageBox.Show("Файл не привязан к заданию", "Ошибка"); }
        }

        private void Element_MouseRightClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Dbaccess dbaccess = new Dbaccess();
                BrushConverter bc = new BrushConverter();
                Brush brRed = (Brush)bc.ConvertFrom("#FFAE1010");
                Brush brWhite = (Brush)bc.ConvertFrom("#000000");
                if ((sender as TextBlock).Foreground.ToString() != "#FFAE1010")
                {
                    (sender as TextBlock).Foreground = brRed;
                    dbaccess.DbRead("UPDATE [task] SET [iscurrent] = False WHERE [task] = '" + (sender as TextBlock).Text.Trim() + "'");
                }
                else
                {
                    (sender as TextBlock).Foreground = brWhite;
                    dbaccess.DbRead("UPDATE [task] SET [iscurrent] = True WHERE [task] = '" + (sender as TextBlock).Text.Trim() + "'");
                }
            }
            catch { }
        }

        private void gridtop_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 3)
            {
                if (this.WindowState != WindowState.Maximized) this.WindowState = WindowState.Maximized;
                else this.WindowState = WindowState.Normal;
            }
            try { this.DragMove(); }
            catch { }

        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public string DeyOverDeys(int day)
        {
            string x = "дней";
            string a = day.ToString();
            char[] b = a.ToCharArray();
            switch (b[b.Length - 1])
            {
                case '1': x = "день"; break;
                case '2': x = "дня"; break;
                case '3': x = "дня"; break;
                case '4': x = "дня"; break;
                default: break;
            }
            return x;
        }

        private void TaskEmployee_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            gr.Children.Clear();
            if (State.taskColl != null) { gr.Children.Add(CreateGrid(60, OutputTask())); }
        }

    }
}
