using System.Windows;
using System.Windows.Input;


namespace Fttd
{
    /// <summary>
    /// Логика взаимодействия для AddStateTask.xaml
    /// </summary>
    public partial class AddStateTask : Window
    {
        public AddStateTask()
        {
            InitializeComponent();
            employee.Text = State.employee.ShortName;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if (stateTask.Text != "" && stateTask.Text != " ")
            {
                Dbaccess dbaccess = new Dbaccess();
                dbaccess.Dbinsert("task_status", "[task], [detail], [employee], [status], [problem], [solution], [data]",
                    "'" + numTask.Text + "', '" + detail.Text + "', '" + employee.Text + "', '" + stateTask.Text + "', '" + problem.Text + "', " +
                    "'" + solution.Text + "', '" + dateNow.Text + "'");
                State.UpdateTaskStatusColl();
                this.Close();
                foreach (Window window in Application.Current.Windows)
                {
                    if (window.GetType() == typeof(Window1))
                    {
                        (window as Window1).gr.Children.Clear();
                        if (State.taskColl != null) { (window as Window1).gr.Children.Add((window as Window1).CreateGrid(60, (window as Window1).OutputTask())); }
                    }
                }
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Заполните статус задания");
            }           
        }

        private void RowDefinition_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            try { this.DragMove(); }
            catch { }
        }
    }
}
