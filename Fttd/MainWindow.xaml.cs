using Fttd.Entities;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WinForms = System.Windows.Forms;

namespace Fttd
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            try
            {
                State.BackupFTTDDB();
                TextBoxBD.Text = State.DirDb;
                TextBoxDir.Text = State.DirFiles;
                State.stateTreeView = true;
                State.UpdateDataTreeView();
                TreeviewSet();
                State.UpdateEmployee();
                WhereEmployeeUpdate();
                CheckTask();
                UpdateChatBox(60000);
            }
            catch (Exception e) { MessageBox.Show(e + " Укажите в настройках файл базы данных и базовую директорию хранения файлов.", "Ошибка"); }
        }

        /// <summary>
        /// Метоз запускает проверку новых сообщений по таймеру
        /// </summary>
        internal void UpdateChatBox(int timems)
        {
            System.Timers.Timer timer = new System.Timers.Timer(timems);
            timer.Enabled = true;
            try
            {
                timer.Elapsed += new System.Timers.ElapsedEventHandler((object source, System.Timers.ElapsedEventArgs e) =>
                {
                    timer.Stop();
                    if (State.UpdateMessageColl())
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            Chat_expand.Foreground = new SolidColorBrush(Colors.YellowGreen); 
                            FillChatBox();
                        }); // Меняет цвет иконки сообщения и обновляет чат в основном потоке
                        timer.Start();
                    }
                    else timer.Start();
                });
            }
            catch (Exception) { timer.Start(); }
            timer.AutoReset = true;
        }

        /// <summary>
        /// Метод отправляет системное сообщение если у сотрудника есть отслеживаемые актуальные задания
        /// </summary>
        private void CheckTask()
        {
            Dbaccess dbaccess1 = new Dbaccess();
            dbaccess1.DbRead("DELETE * FROM [chat] WHERE [FromEmployee] = 'Системное сообщение' AND [WhereEmployee] = '" + State.employee.ShortName + "' AND [Message] LIKE 'Задание%'");
            if (State.taskColl != null)
            {
                foreach (TaskDet item in State.taskColl.Where(item => item.TaskIsCurrent == true && item.Leading == State.employee.ShortName))
                {
                    string message = "Задание №" + item.TaskName + " выполнить до " + item.TaskDateOut.ToString();
                    dbaccess1.Dbinsert("chat", "[FromEmployee], [WhereEmployee], [DateTime], [Message]", "'Системное сообщение', '" + State.employee.ShortName + "', '" + Convert.ToString(DateTime.Now) + "', '" + message + "'");
                }
            }
        }

        /// <summary>
        /// Метод отправляет сообщения в БД и обновляет чат
        /// </summary>
        private void SendChatBox()
        {
            Dbaccess dbaccess = new Dbaccess();
            dbaccess.Dbinsert("chat", "[FromEmployee], [WhereEmployee], [DateTime], [Message]", "'" + State.employee.ShortName + "', '" + WhereEmployee.Text + "', '" + Convert.ToString(DateTime.Now) + "', '" + ChatString.Text + "'");
            State.UpdateMessageColl();
            FillChatBox();
        }

        /// <summary>
        /// Метод обновляет чат
        /// </summary>
        internal void FillChatBox()
        {
            ChatBox.Items.Clear();
            foreach (Message item in State.messageColl.OrderBy(item => item.TimeMess))
            {
                StackPanel panel = new StackPanel();
                panel.Orientation = Orientation.Horizontal;
                TextBlock date = new TextBlock() { Text = item.TimeMess.ToString(), Margin = new Thickness(0, 0, 10, 0), VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Colors.Gold) };
                TextBlock from = new TextBlock() { Text = item.FromEmployee.ToString(), Margin = new Thickness(0, 0, 10, 0), VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Colors.YellowGreen) };
                TextBlock to = new TextBlock() { Text = " =>  ", VerticalAlignment = VerticalAlignment.Center };
                TextBlock where = new TextBlock() { Text = item.WhereEmployee.ToString(), Margin = new Thickness(0, 0, 10, 0), VerticalAlignment = VerticalAlignment.Center, Foreground = new SolidColorBrush(Colors.YellowGreen) };
                TextBlock x = new TextBlock() { Text = ": ", Foreground = new SolidColorBrush(Colors.YellowGreen), VerticalAlignment = VerticalAlignment.Center };
                TextBox message = new TextBox() { Text = item.Mess.ToString(), IsReadOnly = true, BorderThickness = new Thickness(0, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
                panel.Children.Add(date);
                panel.Children.Add(from);
                panel.Children.Add(to);
                panel.Children.Add(where);
                panel.Children.Add(x);
                panel.Children.Add(message);
                ChatBox.Items.Add(panel);
                ChatBox.ScrollIntoView(panel);
            };
        }

        /// <summary>
        /// Метод заполняющий комбобокс кому отправить сообщение
        /// </summary>
        private void WhereEmployeeUpdate()
        {
            WhereEmployee.Items.Clear();
            foreach (Employees item in State.employeeColl.OrderBy(item => item.LastName).Where(item => item.ShortName != "Нет" && item.ShortName != "Системное сообщение"))
            {
                WhereEmployee.Items.Add(item.ShortName);
            }
            WhereEmployee.Text = "Все";
        }

        /// <summary>
        /// Метод копирования файлов и добавления данных в БД
        /// </summary>
        /// <param name="dirout">Директория предоставляемая OpenFileDialog</param>
        /// <param name="index">Индекс детали</param>
        /// <param name="name">Название детали</param>
        /// <param name="file_type">Тип файла</param>
        /// <param name="note">Примечание</param>
        public void CopyFile(string dirout, string index, string name, string newfilename, string file_type = "None", string note = "None")
        {
            AllFiles file = new AllFiles(name, index, TextBlock_type.Text, dirout, note);
            switch (TextBlock_type.Text)
            {
                case "Задания":
                    {
                        Dbaccess dbaccess = new Dbaccess();
                        dbaccess.Dbinsert("stack_files", "[detail_index], [detail_name], [file_name], [file_type], [file_dir], [file_note]", "'" + file.Detail_index + "', '" + file.Detail_name + "', '" + file.File_name + "', '" + file_type + "', '" + file.File_dir_in_short + "', '" + file.File_note + "'");
                        break;
                    }
                case "Приспособления":
                    {
                        if (file.Copy_file())
                        {
                            Dbaccess dbaccess = new Dbaccess();
                            dbaccess.Dbinsert("device_files", "[indexdev], [file_name], [file_type], [file_dir], [file_note]", "'" + file.Detail_index + "', '" + file.File_name + "', '" + file_type + "', '" + file.File_dir_in_short + "', '" + file.File_note + "'");
                        }
                        break;
                    }
                case "Детали":
                    {
                        if (file.Copy_file())
                        {
                            Dbaccess dbaccess = new Dbaccess();
                            dbaccess.Dbinsert("stack_files", "[detail_index], [detail_name], [file_name], [file_type], [file_dir], [file_note]", "'" + file.Detail_index + "', '" + file.Detail_name + "', '" + file.File_name + "', '" + file_type + "', '" + file.File_dir_in_short + "', '" + file.File_note + "'");
                        }
                        break;
                    }
                default: break;
            }
            SelectedTreeViewItem();
        }

        /// <summary>
        /// Метод заполняет Treeview наименованиями деталей
        /// </summary>
        public void TreeviewSet()
        {
            TreeViewDet.Items.Clear();
            if (State.stateTreeView) { State.UpdateDataTreeView(); }
            switch (TextBlock_type.Text)
            {
                case "Детали":
                    ComboBoxProekt.Items.Clear();
                    ComboBoxIndex.Items.Clear();
                    ComboBoxName.Items.Clear();
                    foreach (Project itemP in State.projectColl)
                    {
                        ComboBoxProekt.Items.Add(itemP.ProjectName);
                        TreeViewItem ITProekt = new TreeViewItem();
                        ITProekt.Header = itemP.ProjectName;
                        TreeViewDet.Items.Add(ITProekt);
                        foreach (Detail itemD in State.detailColl.Where(itemD => itemD.Project == itemP.ProjectName))
                        {
                            TextBlock ITDet = new TextBlock();
                            ITDet.Text = itemD.DetailName + '|' + itemD.Index;
                            ITProekt.Items.Add(ITDet);
                            ComboBoxIndex.Items.Add(itemD.Index);
                            ComboBoxName.Items.Add(itemD.DetailName);
                        }
                    }
                    ComboBoxZad.Items.Clear();
                    ComboBoxTask.Items.Clear();
                    foreach (TaskDet item in State.taskColl)
                    {
                        ComboBoxZad.Items.Add(item.TaskName);
                        ComboBoxTask.Items.Add(item.TaskName);
                    }
                    ComboBoxRazrab.Items.Clear();
                    foreach (Developer item in State.developerColl.OrderBy(item => item.DeveloperName).Distinct())
                    {
                        ComboBoxRazrab.Items.Add(item.DeveloperName);
                    }
                    ComboBoxNPU.Items.Clear();
                    foreach (Detail item in State.detailColl.OrderByDescending(item => item.Inventory))
                    {
                        ComboBoxNPU.Items.Add(item.Inventory);
                    }
                    break;
                case "Приспособления":
                    ComboBoxIndex.Items.Clear();
                    ComboBoxName.Items.Clear();
                    foreach (Device item in State.deviceColl)
                    {
                        TextBlock IT2 = new TextBlock();
                        IT2.Text = item.DeviceIndex;
                        TreeViewDet.Items.Add(IT2);
                        ComboBoxIndex.Items.Add(item.DeviceIndex);
                    }
                    ComboBoxRazrab.Items.Clear();
                    foreach (Developer item in State.developerColl.OrderBy(item => item.DeveloperName).Distinct())
                    {
                        ComboBoxRazrab.Items.Add(item.DeveloperName);
                    }
                    break;
                case "Задания":
                    ComboBoxProekt.Items.Clear();
                    data1.Text = "";
                    data2.Text = "";
                    chTaskIsCurrent.IsChecked = false;
                    foreach (Project itemP in State.projectColl)
                    {
                        ComboBoxProekt.Items.Add(itemP.ProjectName);
                        TreeViewItem ITProekt = new TreeViewItem();
                        ITProekt.Header = itemP.ProjectName;
                        TreeViewDet.Items.Add(ITProekt);
                        foreach (TaskDet itemD in State.taskColl.Where(itemD => itemD.ProjectTaskName == itemP.ProjectName))
                        {
                            TextBlock ITDet = new TextBlock();
                            ITDet.Text = itemD.TaskName;
                            ITProekt.Items.Add(ITDet);
                            ComboBoxZad.Items.Add(itemD.TaskName);
                            ComboBoxTask.Items.Add(itemD.TaskName);
                        }
                    }
                    break;
                case "Проекты":
                    ComboBoxProekt.Items.Clear();
                    foreach (Project itemP in State.projectColl)
                    {
                        ComboBoxProekt.Items.Add(itemP.ProjectName);
                        TreeViewItem ITProekt = new TreeViewItem();
                        ITProekt.Header = itemP.ProjectName;
                        TreeViewDet.Items.Add(ITProekt);
                    }
                    break;
                case "Графики":
                    ComboBoxProekt.Items.Clear();
                    ComboBoxName.Items.Clear();
                    foreach (Project itemP in State.projectColl)
                    {
                        ComboBoxProekt.Items.Add(itemP.ProjectName);
                        TreeViewItem ITProekt = new TreeViewItem();
                        ITProekt.Header = itemP.ProjectName;
                        TreeViewDet.Items.Add(ITProekt);
                        foreach (Graphics itemD in State.graphicsColl.Where(itemD => itemD.ProjectGrap == itemP.ProjectName))
                        {
                            TextBlock ITDet = new TextBlock();
                            ITDet.Text = itemD.NameGrap;
                            ITProekt.Items.Add(ITDet);
                            ComboBoxName.Items.Add(itemD.NameGrap);
                        }
                    }
                    break;
                case "Документы":

                    ComboBoxIndex.Items.Clear();
                    ComboBoxName.Items.Clear();
                    foreach (Services item in State.servicesColl)
                    {
                        TextBlock IT2 = new TextBlock();
                        IT2.Text = item.NameServ;
                        TreeViewDet.Items.Add(IT2);
                        ComboBoxIndex.Items.Add(item.NameServ);
                    }
                    break;
                default: break;
            }
        }

        /// <summary>
        /// Метод для действия при выделении TreeViewDetItem 
        /// </summary>
        public void SelectedTreeViewItem()
        {
            try
            {
                data1.Text = "";
                data2.Text = "";
                chTaskIsCurrent.IsChecked = false;
                string textItem = (TreeViewDet.SelectedItem as TextBlock).Text;
                switch (TextBlock_type.Text)
                {
                    case "Детали":
                        string[] name = textItem.Split('|');
                        TextBlockPD.Text = GetNoteDetail(name[1]);
                        if (name[1] != "") SetDataGrid(name[1]);
                        break;
                    default:
                        TextBlockPD.Text = GetNoteDetail(textItem);
                        if (textItem != "") SetDataGrid(textItem);
                        break;
                }
            }
            catch { TextBlockPD.Text = ""; TextBlockPF.Text = ""; }
            TextBlockPF.Text = "";
            TextBoxNameFiles.Clear();
            ComboBoxTypeFiles.Text = "";
            TextBoxFiles.Clear();
            TextBoxNote.Clear();
            TextBoxDirFile.Clear();
        }

        /// <summary>
        /// Метод заполняющий DataGridFiles
        /// </summary>
        /// <param name="index">Индекс выбраной в TreeViewDet детали</param>
        public void SetDataGrid(string index)
        {
            ObservableCollection<Table> coll = new ObservableCollection<Table>();
            Dbaccess dbaccess = new Dbaccess();
            switch (TextBlock_type.Text)
            {
                case "Детали":
                    try
                    {
                        dbaccess.Dbselect("SELECT [detail_index], [file_name], [file_type] FROM [stack_files] WHERE [detail_index] = '" + index + "'");
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            coll.Add(new Table() { Name = vs[1], Type = vs[2] });
                        }
                    }
                    catch { }
                    break;
                case "Приспособления":
                    try
                    {
                        dbaccess.Dbselect("SELECT [indexdev], [file_name], [file_type] FROM [device_files] WHERE [indexdev] = '" + index + "'");
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            coll.Add(new Table() { Name = vs[1], Type = vs[2] });
                        }
                    }
                    catch { }
                    break;
                case "Задания":
                    try
                    {
                        dbaccess.Dbselect("SELECT [task], [dir] FROM [task] WHERE [task] = '" + index + "'");
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            string fn = "";
                            string tp = "";
                            if (vs[1] != "") { fn = new DirectoryInfo(vs[1]).Name; tp = "Задание"; }
                            coll.Add(new Table() { Name = fn, Type = tp });
                        }
                    }
                    catch { }
                    break;
                case "Проекты":
                    break;
                case "Графики":
                    try
                    {
                        dbaccess.Dbselect("SELECT [namegrap], [dir] FROM [graphics] WHERE [namegrap] = '" + index + "'");
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            string fn = "";
                            string tp = "";
                            if (vs[1] != "") { fn = new DirectoryInfo(vs[1]).Name; tp = "График"; }
                            coll.Add(new Table() { Name = fn, Type = tp });
                        }
                    }
                    catch { }
                    break;
                case "Документы":
                    try
                    {
                        dbaccess.Dbselect("SELECT [nameserv], [dir] FROM [service] WHERE [nameserv] = '" + index + "'");
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            string fn = "";
                            string tp = "";
                            if (vs[1] != "") { fn = new DirectoryInfo(vs[1]).Name; tp = "Документы"; }
                            coll.Add(new Table() { Name = fn, Type = tp });
                        }
                    }
                    catch { }
                    break;
                default: break;
            }
            DataGridFiles.ItemsSource = coll;
            DataGridFiles.Items.Refresh();
        }

        /// <summary>
        /// Метод получающий данные детали строкой
        /// </summary>
        /// <param name="index">Индекс детали</param>
        /// <returns>Данные детали строкой</returns>
        public string GetNoteDetail(string index)
        {
            string x = "";
            switch (TextBlock_type.Text)
            {
                case "Детали":
                    try
                    {
                        foreach (Detail item in State.detailColl.Where(item => item.Index == index))
                        {
                            x = item.ToString();
                            ComboBoxIndex.Text = item.Index;
                            ComboBoxName.Text = item.DetailName;
                            ComboBoxNPU.Text = Convert.ToString(item.Inventory);
                            ComboBoxZad.Text = item.Task;
                            ComboBoxProekt.Text = item.Project;
                            ComboBoxRazrab.Text = item.Developer;
                        }
                        return x;
                    }
                    catch
                    {
                        return x;
                    }
                case "Приспособления":
                    try
                    {
                        foreach (Device item in State.deviceColl.Where(item => item.DeviceIndex == index))
                        {
                            x = item.ToString();
                            ComboBoxIndex.Text = item.DeviceIndex;
                            ComboBoxName.Text = item.DeviceName;
                            ComboBoxRazrab.Text = item.DeviceDeveloper;
                        }
                        return x;
                    }
                    catch
                    {
                        return x;
                    }
                case "Задания":
                    try
                    {
                        foreach (TaskDet item in State.taskColl.Where(item => item.TaskName == index))
                        {
                            x = item.ToString();
                            ComboBoxZad.Text = item.TaskName;
                            ComboBoxProekt.Text = item.ProjectTaskName;
                            ComboBoxRazrab.Text = item.Leading;
                            data1.SelectedDate = item.TaskDateIn;
                            data2.SelectedDate = item.TaskDateOut;
                            if (item.TaskIsCurrent) chTaskIsCurrent.IsChecked = true;
                        }
                        return x;
                    }
                    catch
                    {
                        return x;
                    }
                case "Проекты":
                    string y = "";
                    return y;
                case "Графики":
                    try
                    {
                        foreach (Graphics item in State.graphicsColl.Where(item => item.NameGrap == index))
                        {
                            x = item.ToString();
                            ComboBoxName.Text = item.NameGrap;
                            ComboBoxProekt.Text = item.ProjectGrap;
                        }
                        return x;
                    }
                    catch
                    {
                        return x;
                    }
                case "Документы":
                    try
                    {
                        foreach (Services item in State.servicesColl.Where(item => item.NameServ == index))
                        {
                            x = item.ToString();
                            ComboBoxIndex.Text = item.NameServ;
                            ComboBoxName.Text = item.Note;
                        }
                        return x;
                    }
                    catch
                    {
                        return x;
                    }
                default: string z = ""; return z;
            }
        }

        /// <summary>
        /// Метод получающий данные файла строкой
        /// </summary>
        /// <param name="index">Индекс детали</param>
        /// <param name="filename">Имя файла</param>
        /// <returns>Данные детали строкой</returns>
        public string GetNoteFiles(string index, string filename)
        {
            Dbaccess dbaccess = new Dbaccess();
            string x = "";
            switch (TextBlock_type.Text)
            {
                case "Детали":
                    try
                    {
                        dbaccess.Dbselect("SELECT [detail_index], [detail_name], [file_name], [file_type], [file_dir], [file_note], [data_add] FROM [stack_files] WHERE [detail_index] = '" + index + "' AND [file_name] = '" + filename + "'");
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            x = "Описание файла\nНазвание файла: " + vs[2] + "\n" + "Тип файла: " + vs[3] + "\n" + "Примечание: " + vs[5] + "\n" + "Дата добавления: " + vs[6];
                            TextBoxNameFiles.Text = vs[2];
                            ComboBoxTypeFiles.Text = vs[3];
                            TextBoxNote.Text = vs[5];
                            TextBoxFiles.Text = vs[4];
                        }
                        return x;
                    }
                    catch { return x; }
                case "Приспособления":
                    try
                    {
                        dbaccess.Dbselect("SELECT [indexdev], [file_name], [file_type], [file_dir], [file_note], [data_add] FROM [device_files] WHERE [indexdev] = '" + index + "' AND [file_name] = '" + filename + "'");
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            x = "Описание файла\nНазвание файла: " + vs[1] + "\n" + "Тип файла: " + vs[2] + "\n" + "Примечание: " + vs[4] + "\n" + "Дата добавления: " + vs[5];
                            TextBoxNameFiles.Text = vs[1];
                            ComboBoxTypeFiles.Text = vs[2];
                            TextBoxNote.Text = vs[4];
                            TextBoxFiles.Text = vs[3];
                        }
                        return x;
                    }
                    catch { return x; }
                case "Задания":
                    try
                    {
                        dbaccess.Dbselect("SELECT [task], [dir], [note] FROM [task] WHERE [task] = '" + index + "'");
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            string fn = "";
                            string tp = "";
                            if (vs[1] != "") { fn = new DirectoryInfo(vs[1]).Name; tp = "Задание"; }
                            x = "Описание файла\nНазвание файла: " + fn + "\n" + "Тип файла: " + tp + "\n" + "Примечание: " + vs[2];
                        }
                        return x;
                    }
                    catch { return x; }
                case "Проекты":
                    string y = "";
                    return y;
                case "Графики":
                    try
                    {
                        dbaccess.Dbselect("SELECT [namegrap], [dir], [data_add] FROM [graphics] WHERE [namegrap] = '" + index + "'");
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            string fn = "";
                            string tp = "";
                            if (vs[1] != "") { fn = new DirectoryInfo(vs[1]).Name; tp = "График"; }
                            x = "Описание файла\nНазвание файла: " + fn + "\n" + "Тип файла: " + tp + "\n" + "Дата добавления: " + vs[2];
                        }
                        return x;
                    }
                    catch { return x; }
                case "Документы":
                    try
                    {
                        dbaccess.Dbselect("SELECT [nameserv], [dir], [note], [data_add] FROM [service] WHERE [nameserv] = '" + index + "'");
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            string fn = "";
                            string tp = "";
                            if (vs[1] != "") { fn = new DirectoryInfo(vs[1]).Name; tp = "Документы"; }
                            x = "Описание файла\nНазвание файла: " + fn + "\n" + "Тип файла: " + tp + "\n" + "Примечание: " + vs[2] + "\n" + "Дата добавления: " + vs[3];
                        }
                        return x;
                    }
                    catch { return x; }
                default:
                    string z = "";
                    return z;
            }
        }

        // Кнопка выход
        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Перетаскивание окна
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 3)
            {
                if (this.WindowState != WindowState.Maximized) this.WindowState = WindowState.Maximized;
                else this.WindowState = WindowState.Normal;
            }
            try { this.DragMove(); }
            catch { }
        }

        // Кнопка настройки
        private void Button_settings_Click(object sender, RoutedEventArgs e)
        {
            switch (SettingsBar.Width.Value)
            {
                case 0: SettingsBar.Width = new GridLength(value: 250, type: GridUnitType.Pixel); break;
                case 250: SettingsBar.Width = new GridLength(value: 0, type: GridUnitType.Pixel); break;
                default: break;
            }
        }

        // Кнопка в настройках добавления файла базы данных
        private void ButtonAddDB_Click(object sender, RoutedEventArgs e)
        {
            WinForms.OpenFileDialog openFile = new WinForms.OpenFileDialog();
            openFile.ShowDialog();
            State.DirDb = openFile.FileName;
            TextBoxBD.Text = State.DirDb;
            MessageBox.Show("После изменения файла базы данных приложение будет перезапущено.", "Изменение файла базы данных");
            this.Close();
            Process.Start(@"Fttd.exe");
        }

        // Кнопка в настройках добавления базовой директории
        private void ButtonAddDir_Click(object sender, RoutedEventArgs e)
        {
            WinForms.FolderBrowserDialog folder = new WinForms.FolderBrowserDialog();
            folder.ShowDialog();
            State.DirFiles = folder.SelectedPath;
            TextBoxDir.Text = State.DirFiles;
            MessageBox.Show("После изменения базовой директории приложение будет перезапущено.", "Изменение Базовой директории");
            this.Close();
            System.Diagnostics.Process.Start(@"Fttd.exe");
        }

        //Кнопка на весь экран
        private void Button_maxsize_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState != WindowState.Maximized) this.WindowState = WindowState.Maximized;
            else this.WindowState = WindowState.Normal;
        }

        //Кнопка свернуть окно
        private void Button_minimized_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        // Кнопка открытия меню добавления детали
        private void ButtonAddDetail_Click(object sender, RoutedEventArgs e)
        {
            int x = 0;
            switch (TextBlock_type.Text)
            {
                case "Детали": x = 460; break;
                case "Приспособления": x = 280; break;
                case "Задания": x = 450; break;
                case "Проекты": x = 160; break;
                case "Графики": x = 280; break;
                case "Документы": x = 280; break;
                default: break;
            }
            if (RowDetail.Height.Value == x)
            {
                RowDetail.Height = new GridLength(value: 40, type: GridUnitType.Pixel);
                ButtonAddDetailInDB.Visibility = Visibility.Hidden;
                ButtonReadDetail.Visibility = Visibility.Visible;
                ButtonRemoveDetail.Visibility = Visibility.Visible;
            }
            else
            {
                RowDetail.Height = new GridLength(value: x, type: GridUnitType.Pixel);
                ButtonAddDetailInDB.Visibility = Visibility.Visible;
                ButtonReadDetailInDB.Visibility = Visibility.Hidden;
                ButtonRemoveDetailInDB.Visibility = Visibility.Hidden;
                ButtonReadDetail.Visibility = Visibility.Hidden;
                ButtonRemoveDetail.Visibility = Visibility.Hidden;
                TextBoxDirFile.IsEnabled = true;
                ButtonAddFile.IsEnabled = true;
            }
        }

        // Кнопка открытия меню изменения детали
        private void ButtonReadDetail_Click(object sender, RoutedEventArgs e)
        {
            int x = 0;
            switch (TextBlock_type.Text)
            {
                case "Детали": x = 460; break;
                case "Приспособления": x = 280; break;
                case "Задания": x = 450; break;
                case "Проекты": x = 160; break;
                case "Графики": x = 280; break;
                case "Документы": x = 280; break;
                default: break;
            }
            if (RowDetail.Height.Value == x)
            {
                RowDetail.Height = new GridLength(value: 40, type: GridUnitType.Pixel);
                ButtonReadDetailInDB.Visibility = Visibility.Hidden;
                ButtonAddDetail.Visibility = Visibility.Visible;
                ButtonRemoveDetail.Visibility = Visibility.Visible;
                ComboBoxIndex.IsEnabled = true;
                ComboBoxNPU.IsEnabled = true;
            }
            else
            {
                RowDetail.Height = new GridLength(value: x, type: GridUnitType.Pixel);
                ButtonReadDetailInDB.Visibility = Visibility.Visible; ;
                ButtonAddDetailInDB.Visibility = Visibility.Hidden;
                ButtonRemoveDetailInDB.Visibility = Visibility.Hidden;
                ButtonAddDetail.Visibility = Visibility.Hidden;
                ButtonRemoveDetail.Visibility = Visibility.Hidden;
                ComboBoxIndex.IsEnabled = false;
                ComboBoxNPU.IsEnabled = false;
                TextBoxDirFile.IsEnabled = true;
                ButtonAddFile.IsEnabled = true;
            }
        }

        // Кнопка открытия меню удаления детали
        private void ButtonRemoveDetail_Click(object sender, RoutedEventArgs e)
        {
            int x = 0;
            switch (TextBlock_type.Text)
            {
                case "Детали": x = 460; break;
                case "Приспособления": x = 280; break;
                case "Задания": x = 450; break;
                case "Проекты": x = 160; break;
                case "Графики": x = 280; break;
                case "Документы": x = 280; break;
                default: break;
            }
            if (RowDetail.Height.Value == x)
            {
                RowDetail.Height = new GridLength(value: 40, type: GridUnitType.Pixel);
                ButtonRemoveDetailInDB.Visibility = Visibility.Hidden;
                ButtonAddDetail.Visibility = Visibility.Visible;
                ButtonReadDetail.Visibility = Visibility.Visible;
                ComboBoxName.IsEnabled = true;
                ComboBoxIndex.IsEnabled = true;
                ComboBoxProekt.IsEnabled = true;
                ComboBoxZad.IsEnabled = true;
                ComboBoxNPU.IsEnabled = true;
                ComboBoxRazrab.IsEnabled = true;
                TextBoxDirFile.IsEnabled = true;
                ButtonAddFile.IsEnabled = true;
            }
            else
            {
                RowDetail.Height = new GridLength(value: x, type: GridUnitType.Pixel);
                ButtonRemoveDetailInDB.Visibility = Visibility.Visible; ;
                ButtonAddDetailInDB.Visibility = Visibility.Hidden;
                ButtonReadDetailInDB.Visibility = Visibility.Hidden;
                ButtonAddDetail.Visibility = Visibility.Hidden;
                ButtonReadDetail.Visibility = Visibility.Hidden;
                ComboBoxName.IsEnabled = false;
                ComboBoxIndex.IsEnabled = false;
                ComboBoxProekt.IsEnabled = false;
                ComboBoxZad.IsEnabled = false;
                ComboBoxNPU.IsEnabled = false;
                ComboBoxRazrab.IsEnabled = false;
                TextBoxDirFile.IsEnabled = false;
                ButtonAddFile.IsEnabled = false;
            }
        }

        // Кнопка открытия меню добавления файла
        private void ButtonAddFiles_Click(object sender, RoutedEventArgs e)
        {
            switch (RowFiles.Height.Value)
            {
                case 40:
                    RowFiles.Height = new GridLength(value: 260, type: GridUnitType.Pixel);
                    ButtonAddFilesInDB.Visibility = Visibility.Visible;
                    ButtonReadFilesInDB.Visibility = Visibility.Hidden;
                    ButtonRemoveFilesInDB.Visibility = Visibility.Hidden;
                    ButtonReadFiles.Visibility = Visibility.Hidden;
                    ButtonRemoveFiles.Visibility = Visibility.Hidden;
                    break;
                case 260:
                    RowFiles.Height = new GridLength(value: 40, type: GridUnitType.Pixel);
                    ButtonAddFilesInDB.Visibility = Visibility.Hidden;
                    ButtonReadFiles.Visibility = Visibility.Visible;
                    ButtonRemoveFiles.Visibility = Visibility.Visible;
                    break;
                default: break;
            }
        }

        //Кнопка открытия меню изменения файла
        private void ButtonReadFiles_Click(object sender, RoutedEventArgs e)
        {
            switch (RowFiles.Height.Value)
            {
                case 40:
                    RowFiles.Height = new GridLength(value: 260, type: GridUnitType.Pixel);
                    ButtonAddFilesInDB.Visibility = Visibility.Hidden;
                    ButtonReadFilesInDB.Visibility = Visibility.Visible;
                    ButtonRemoveFilesInDB.Visibility = Visibility.Hidden;
                    ButtonAddFiles.Visibility = Visibility.Hidden;
                    ButtonRemoveFiles.Visibility = Visibility.Hidden;
                    ButtonDirFiles.Visibility = Visibility.Hidden;
                    TextBoxFiles.IsEnabled = false;
                    TextBoxNameFiles.IsEnabled = false;
                    break;
                case 260:
                    RowFiles.Height = new GridLength(value: 40, type: GridUnitType.Pixel);
                    ButtonReadFilesInDB.Visibility = Visibility.Hidden;
                    ButtonAddFiles.Visibility = Visibility.Visible;
                    ButtonRemoveFiles.Visibility = Visibility.Visible;
                    ButtonDirFiles.Visibility = Visibility.Visible;
                    TextBoxFiles.IsEnabled = true;
                    TextBoxNameFiles.IsEnabled = true;
                    break;
                default: break;
            }
        }

        // Кнопка открытия меню удаления файла
        private void ButtonRemoveFiles_Click(object sender, RoutedEventArgs e)
        {
            switch (RowFiles.Height.Value)
            {
                case 40:
                    RowFiles.Height = new GridLength(value: 260, type: GridUnitType.Pixel);
                    ButtonAddFilesInDB.Visibility = Visibility.Hidden;
                    ButtonReadFilesInDB.Visibility = Visibility.Hidden;
                    ButtonRemoveFilesInDB.Visibility = Visibility.Visible;
                    ButtonAddFiles.Visibility = Visibility.Hidden;
                    ButtonReadFiles.Visibility = Visibility.Hidden;
                    ButtonDirFiles.Visibility = Visibility.Hidden;
                    TextBoxFiles.IsEnabled = false;
                    ComboBoxTypeFiles.IsEnabled = false;
                    TextBoxNameFiles.IsEnabled = false;
                    TextBoxNote.IsEnabled = false;
                    ComboBoxTask.IsEnabled = false;
                    break;
                case 260:
                    RowFiles.Height = new GridLength(value: 40, type: GridUnitType.Pixel);
                    ButtonRemoveFilesInDB.Visibility = Visibility.Hidden;
                    ButtonAddFiles.Visibility = Visibility.Visible;
                    ButtonReadFiles.Visibility = Visibility.Visible;
                    ButtonDirFiles.Visibility = Visibility.Visible;
                    TextBoxFiles.IsEnabled = true;
                    ComboBoxTypeFiles.IsEnabled = true;
                    TextBoxNameFiles.IsEnabled = true;
                    TextBoxNote.IsEnabled = true;
                    ComboBoxTask.IsEnabled = true;
                    break;
                default: break;
            }
        }

        // Действие которое происходит при выделении TreeViewDetItem
        private void TreeViewDet_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            SelectedTreeViewItem();
        }

        // Кнопка добавления детали
        private void ButtonAddDetailInDB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Dbaccess dbaccess = new Dbaccess();
                switch (TextBlock_type.Text)
                {
                    case "Детали":
                        if (ComboBoxIndex.Text != "")
                        {
                            dbaccess.Dbinsert("detail_db", "[detail_index], [detail_name], [inventory], [number_task], [project], [razrabotal]", "'" + ComboBoxIndex.Text + "', '" + ComboBoxName.Text + "', '" + ComboBoxNPU.Text + "', '" + ComboBoxZad.Text + "', '" + ComboBoxProekt.Text + "', '" + ComboBoxRazrab.Text + "'");
                        }
                        else { MessageBox.Show("Ведите название детали", "Ошибка"); }
                        break;
                    case "Приспособления":
                        if (ComboBoxIndex.Text != "")
                        {
                            dbaccess.Dbinsert("device", "[indexdev], [namedev], [razrab]", "'" + ComboBoxIndex.Text + "', '" + ComboBoxName.Text + "', '" + ComboBoxRazrab.Text + "'");
                        }
                        else { MessageBox.Show("Ведите название приспособления", "Ошибка"); }
                        break;
                    case "Задания":
                        if (ComboBoxZad.Text != "" && ComboBoxProekt.Text != "")
                        {
                            if (ComboBoxZad.Items.Contains(ComboBoxZad.Text))
                            {
                                MessageBox.Show("Задание уже есть в базе, будет изменена только актуальность, ответственный и добавлен файл", "Ошибка");
                                if (TextBoxDirFile.Text != "")
                                {
                                    try
                                    {
                                        AllFiles file = new AllFiles("Задания", TextBoxDirFile.Text);
                                        if (File.Exists(file.File_dir_in))
                                        {
                                            MessageBox.Show(file.File_dir_in + "Файл уже есть в базе, он будет привязан к заданию", "Ошибка");
                                        }
                                        else
                                        {
                                            file.Copy_file();
                                        }
                                        dbaccess.DbRead("UPDATE [task] SET [dir] = '" + file.File_dir_in_short + "' WHERE [task] = '" + ComboBoxZad.Text + "'");
                                    }
                                    catch { }
                                }
                                string leading;
                                try { leading = State.employeeColl.First(item => item.ShortName.Contains(ComboBoxRazrab.Text)).ShortName; }
                                catch (Exception) { leading = "Нет"; }
                                dbaccess.DbRead("UPDATE [task] SET [iscurrent] = " + chTaskIsCurrent.IsChecked.Value + ", [leading] = '" + leading + "' WHERE [task] = '" + ComboBoxZad.Text + "'");
                            }
                            else
                            {
                                try
                                {
                                    AllFiles file = new AllFiles("Задания", TextBoxDirFile.Text);
                                    if (File.Exists(file.File_dir_in))
                                    {
                                        MessageBox.Show(file.File_dir_in + "Файл уже есть в базе, он будет привязан к заданию", "Ошибка");
                                    }
                                    else
                                    {
                                        file.Copy_file();
                                    }
                                    string leading;
                                    try { leading = State.employeeColl.First(item => item.ShortName.Contains(ComboBoxRazrab.Text)).ShortName; }
                                    catch (Exception) { leading = "Нет"; }
                                    dbaccess.DbRead("INSERT INTO [task] ([task], [project], [dir], [datein], [dateout], [iscurrent], [leading]) VALUES " +
                                        "('" + ComboBoxZad.Text + "', '" + ComboBoxProekt.Text + "', '" + file.File_dir_in_short + "', '" + data1.Text + "', '" + data2.Text + "', " +
                                        chTaskIsCurrent.IsChecked.Value + ", '" + leading + "')");
                                }
                                catch (Exception ex) { MessageBox.Show(ex + "", "Ошибка"); }
                            }
                        }
                        else { MessageBox.Show("Ведите номер задания, выберите проект и укажите файл", "Ошибка"); }
                        break;
                    case "Проекты":
                        if (ComboBoxProekt.Text != "")
                        {
                            dbaccess.Dbinsert("project", "[project]", "'" + ComboBoxProekt.Text + "'");
                        }
                        else { MessageBox.Show("Ведите проект", "Ошибка"); }
                        break;
                    case "Графики":
                        if (ComboBoxName.Text != "" && ComboBoxProekt.Text != "" && TextBoxDirFile.Text != "")
                        {
                            if (ComboBoxName.Items.Contains(ComboBoxName.Text))
                            {
                                MessageBox.Show("Укажите новый график", "Ошибка");
                            }
                            else
                            {
                                try
                                {
                                    AllFiles file = new AllFiles("Графики", TextBoxDirFile.Text);
                                    if (File.Exists(file.File_dir_in))
                                    {
                                        MessageBox.Show(file.File_dir_in + "Файл уже есть в базе, он будет привязан к заданию.", "Ошибка");
                                    }
                                    else
                                    {
                                        file.Copy_file();
                                    }
                                    dbaccess.Dbinsert("graphics", "[namegrap], [project], [dir]", "'" + ComboBoxName.Text + "', '" + ComboBoxProekt.Text + "', '" + file.File_dir_in_short + "'");
                                }
                                catch (Exception ex) { MessageBox.Show(ex + "", "Ошибка"); }
                            }
                        }
                        else { MessageBox.Show("Ведите название графика, выберите проект и укажите файл", "Ошибка"); }
                        break;
                    case "Документы":
                        if (ComboBoxIndex.Text != "" && ComboBoxName.Text != "" && TextBoxDirFile.Text != "")
                        {
                            if (ComboBoxIndex.Items.Contains(ComboBoxIndex.Text))
                            {
                                MessageBox.Show("Укажите новый документ", "Ошибка");
                            }
                            else
                            {
                                try
                                {
                                    AllFiles file = new AllFiles("Документы", TextBoxDirFile.Text);
                                    if (File.Exists(file.File_dir_in))
                                    {
                                        MessageBox.Show(file.File_dir_in + "Файл уже есть в базе, он будет привязан к заданию.", "Ошибка");
                                    }
                                    else
                                    {
                                        file.Copy_file();
                                    }
                                    dbaccess.Dbinsert("service", "[nameserv], [note], [dir]", "'" + ComboBoxIndex.Text + "', '" + ComboBoxName.Text + "', '" + file.File_dir_in_short + "'");
                                }
                                catch (Exception ex) { MessageBox.Show(ex + "", "Ошибка"); }
                            }
                        }
                        else { MessageBox.Show("Ведите номер документа, краткое описание и укажите файл", "Ошибка"); }
                        break;
                    default: break;
                }
                State.stateTreeView = true;
                TreeviewSet();
            }
            catch (Exception ex) { MessageBox.Show(ex + "", "Ошибка"); }
        }

        // Кнопка изменения детали
        private void ButtonReadDetailInDB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Dbaccess dbaccess = new Dbaccess();
                switch (TextBlock_type.Text)
                {
                    case "Детали":
                        if (ComboBoxIndex.Text != "")
                        {
                            dbaccess.DbRead("UPDATE [detail_db] SET [detail_name] = '" + ComboBoxName.Text + "', [number_task] = '" + ComboBoxZad.Text + "', [project] = '" + ComboBoxProekt.Text + "', [razrabotal] = '" + ComboBoxRazrab.Text + "' WHERE [detail_index] = '" + ComboBoxIndex.Text + "' AND [inventory] = " + Convert.ToDouble(ComboBoxNPU.Text) + "");
                        }
                        else { MessageBox.Show("Выберите деталь", "Ошибка"); }
                        break;
                    case "Приспособления":
                        if (ComboBoxIndex.Text != "")
                        {
                            dbaccess.DbRead("UPDATE [device] SET [namedev] = '" + ComboBoxName.Text + "', [razrab] = '" + ComboBoxRazrab.Text + "' WHERE [indexdev] = '" + ComboBoxIndex.Text + "'");
                        }
                        else { MessageBox.Show("Ведите название приспособления", "Ошибка"); }
                        break;
                    case "Графики":
                        if (ComboBoxName.Text != "")
                        {
                            dbaccess.DbRead("UPDATE [graphics] SET [project] = '" + ComboBoxProekt.Text + "' WHERE [namegrap] = '" + ComboBoxName.Text + "'");
                        }
                        else { MessageBox.Show("Выберите график", "Ошибка"); }
                        break;
                    case "Документы":
                        if (ComboBoxIndex.Text != "")
                        {
                            dbaccess.DbRead("UPDATE [service] SET [note] = '" + ComboBoxName.Text + "' WHERE [nameserv] = '" + ComboBoxIndex.Text + "'");
                        }
                        else { MessageBox.Show("Выберите документ", "Ошибка"); }
                        break;
                    default: break;
                }
                State.stateTreeView = true;
                TreeviewSet();
            }
            catch { }
        }

        // Кнопка удаления детали
        private void ButtonRemoveDetailInDB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Dbaccess dbaccess = new Dbaccess();
                switch (TextBlock_type.Text)
                {
                    case "Детали":
                        if (ComboBoxIndex.Text != "")
                        {
                            dbaccess.DbRead("DELETE FROM [detail_db] WHERE [detail_index] = '" + ComboBoxIndex.Text + "' AND [inventory] = " + ComboBoxNPU.Text + "");
                        }
                        else { MessageBox.Show("Выберите деталь", "Ошибка"); }
                        break;
                    case "Приспособления":
                        if (ComboBoxIndex.Text != "")
                        {
                            dbaccess.DbRead("DELETE FROM [device] WHERE [indexdev] = '" + ComboBoxIndex.Text + "'");
                        }
                        else { MessageBox.Show("Выберите приспособление", "Ошибка"); }
                        break;
                    case "Графики":
                        if (ComboBoxName.Text != "")
                        {
                            try
                            {
                                File.Delete(@"" + State.DirFiles + "\\" + State.graphicsColl.First(item => item.NameGrap.Contains(ComboBoxName.Text)).DirGrap + "");
                                MessageBox.Show("Файл успешно удалён.", "Удаление");
                                dbaccess.DbRead("DELETE FROM [graphics] WHERE [namegrap] = '" + ComboBoxName.Text + "'");
                            }
                            catch { }
                        }
                        else { MessageBox.Show("Выберите график", "Ошибка"); }
                        break;
                    case "Документы":
                        if (ComboBoxIndex.Text != "")
                        {
                            File.Delete(@"" + State.DirFiles + "\\" + State.servicesColl.First(item => item.NameServ.Contains(ComboBoxIndex.Text)).DirServ + "");
                            MessageBox.Show("Файл успешно удалён.", "Удаление");
                            dbaccess.DbRead("DELETE FROM [service] WHERE [nameserv] = '" + ComboBoxIndex.Text + "'");
                        }
                        else { MessageBox.Show("Выберите документ", "Ошибка"); }
                        break;
                    default: break;
                }
                State.stateTreeView = true;
                TreeviewSet();
            }
            catch { }
        }

        // Кнопка добавления директории файла
        private void ButtonDirFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.ShowDialog();
            string dir = openFile.FileName;
            if (dir != "")
            {
                TextBoxFiles.Text = dir;
                TextBoxNameFiles.Text = new DirectoryInfo(dir).Name;
            }
        }

        // Действие которое происходит при выделении DataGridFiles
        private void DataGridFiles_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                int selectedColumn = DataGridFiles.CurrentCell.Column.DisplayIndex;
                var selectedCell = DataGridFiles.SelectedCells[selectedColumn];
                var cellContent = selectedCell.Column.GetCellContent(selectedCell.Item);
                string textDataGrid = (cellContent as TextBlock).Text;
                string textItem = (TreeViewDet.SelectedItem as TextBlock).Text;
                switch (TextBlock_type.Text)
                {
                    case "Детали":
                        string[] name = textItem.Split('|');
                        TextBlockPF.Text = GetNoteFiles(name[1], textDataGrid);
                        break;
                    default:
                        TextBlockPF.Text = GetNoteFiles(textItem, textDataGrid);
                        break;
                }
            }
            catch { }
        }

        // Кнопка добавления файла
        private void ButtonAddFilesInDB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string textItem = (TreeViewDet.SelectedItem as TextBlock).Text;
                switch (TextBlock_type.Text)
                {
                    case "Детали":
                        try
                        {
                            string[] name = textItem.Split('|');
                            CopyFile(TextBoxFiles.Text, name[1], name[0], TextBoxNameFiles.Text, ComboBoxTypeFiles.Text, TextBoxNote.Text);
                        }
                        catch { }
                        break;
                    case "Приспособления":
                        try
                        {
                            CopyFile(TextBoxFiles.Text, textItem, "Приспособление", TextBoxNameFiles.Text, ComboBoxTypeFiles.Text, TextBoxNote.Text);
                        }
                        catch { }
                        break;
                    default: break;
                }
            }
            catch { }
            SelectedTreeViewItem();
        }

        // Кнопка изменения файла
        private void ButtonReadFilesInDB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Dbaccess dbaccess = new Dbaccess();
                string textItem = (TreeViewDet.SelectedItem as TextBlock).Text;
                switch (TextBlock_type.Text)
                {
                    case "Детали":
                        try
                        {
                            string[] name = textItem.Split('|');
                            dbaccess.DbRead("UPDATE [stack_files] SET [file_type] = '" + ComboBoxTypeFiles.Text + "', [file_note] = '" + TextBoxNote.Text + "' WHERE [detail_index] = '" + name[1] + "' AND [file_dir] = '" + TextBoxFiles.Text + "'");
                        }
                        catch { }
                        break;
                    case "Приспособления":
                        try
                        {
                            dbaccess.DbRead("UPDATE [device_files] SET [file_type] = '" + ComboBoxTypeFiles.Text + "', [file_note] = '" + TextBoxNote.Text + "' WHERE [indexdev] = '" + textItem + "' AND [file_dir] = '" + TextBoxFiles.Text + "'");
                        }
                        catch { }
                        break;
                    default: break;
                }
            }
            catch { }
            SelectedTreeViewItem();
        }

        // Кнопка удаления файла
        private void ButtonRemoveFilesInDB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Dbaccess dbaccess = new Dbaccess();
                string textItem = (TreeViewDet.SelectedItem as TextBlock).Text;
                switch (TextBlock_type.Text)
                {
                    case "Детали":
                        try
                        {
                            string[] name = textItem.Split('|');
                            dbaccess.DbRead("DELETE FROM [stack_files] WHERE [detail_index] = '" + name[1] + "' AND [file_dir] = '" + TextBoxFiles.Text + "'");
                            if (ComboBoxTypeFiles.Text == "Задание")
                            { MessageBox.Show("Файл задания может быть привязан к другим деталям. Привязка к данной детали будет удалена.", "Удаление"); }
                            else { File.Delete(@"" + State.DirFiles + "\\" + TextBoxFiles.Text + ""); MessageBox.Show("Файл успешно удалён.", "Удаление"); }
                        }
                        catch { }
                        break;
                    case "Приспособления":
                        try
                        {
                            dbaccess.DbRead("DELETE FROM [device_files] WHERE [indexdev] = '" + textItem + "' AND [file_dir] = '" + TextBoxFiles.Text + "'");
                            if (ComboBoxTypeFiles.Text == "Задание")
                            { MessageBox.Show("Файл задания может быть привязан к другим деталям. Привязка к данной детали будет удалена.", "Удаление"); }
                            else { File.Delete(@"" + State.DirFiles + "\\" + TextBoxFiles.Text + ""); MessageBox.Show("Файл успешно удалён.", "Удаление"); }
                        }
                        catch { }
                        break;
                    default: break;
                }
            }
            catch { }
            SelectedTreeViewItem();
        }

        // Действие которое происходит при двойном нажатии левой клавишой мыши на элемент DataGridFiles
        private void DataGridFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                int selectedColumn = DataGridFiles.CurrentCell.Column.DisplayIndex;
                var selectedCell = DataGridFiles.SelectedCells[selectedColumn];
                var cellContent = selectedCell.Column.GetCellContent(selectedCell.Item);
                string textDataGrid = (cellContent as TextBlock).Text;
                string textItem = (TreeViewDet.SelectedItem as TextBlock).Text;
                switch (TextBlock_type.Text)
                {
                    case "Детали":
                        try
                        {
                            string[] name = textItem.Split('|');
                            TextBlockPF.Text = GetNoteFiles(name[1], textDataGrid);
                            Process.Start(@"" + State.DirFiles + "\\" + TextBoxFiles.Text + "");
                        }
                        catch { }
                        break;
                    case "Приспособления":
                        try
                        {
                            TextBlockPF.Text = GetNoteFiles(textItem, textDataGrid);
                            Process.Start(@"" + State.DirFiles + TextBoxFiles.Text + "");
                        }
                        catch { }
                        break;
                    case "Задания":
                        try
                        {
                            string dir = State.taskColl.First(item => item.TaskDir.Contains(textDataGrid)).TaskDir;
                            TextBlockPF.Text = GetNoteFiles(textItem, textDataGrid);
                            Process.Start(@"" + State.DirFiles + dir + "");
                        }
                        catch { }
                        break;
                    case "Графики":
                        try
                        {
                            string dir = State.graphicsColl.First(item => item.DirGrap.Contains(textDataGrid)).DirGrap;
                            TextBlockPF.Text = GetNoteFiles(textItem, textDataGrid);
                            Process.Start(@"" + State.DirFiles + dir + "");
                        }
                        catch { }
                        break;
                    case "Документы":
                        try
                        {
                            string dir = State.servicesColl.First(item => item.DirServ.Contains(textDataGrid)).DirServ;
                            TextBlockPF.Text = GetNoteFiles(textItem, textDataGrid);
                            Process.Start(@"" + State.DirFiles + dir + "");
                        }
                        catch { }
                        break;
                    default: break;
                }
            }
            catch { }
        }

        // Действие при выборе пункта "Задание" в меню добавление файла
        private void ComboBoxTypeFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                string type = ComboBoxTypeFiles.SelectedItem.ToString();
                if (type.Contains("Задание"))
                {
                    ButtonDirFiles.Visibility = Visibility.Hidden;
                    TextBoxFiles.Visibility = Visibility.Hidden;
                    ComboBoxTask.Visibility = Visibility.Visible;
                    TextBlockFile.Text = "Задание";
                }
                else
                {
                    ButtonDirFiles.Visibility = Visibility.Visible;
                    TextBoxFiles.Visibility = Visibility.Visible;
                    ComboBoxTask.Visibility = Visibility.Hidden;
                    TextBlockFile.Text = "Файл";
                }
            }
            catch { }
        }

        // Действие при выборе номера задания в меню добавление файла (пункт "задание")
        private void ComboBoxTask_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TextBoxFiles.Text = State.taskColl.First(item => item.TaskName.Contains(ComboBoxTask.SelectedItem.ToString())).TaskDir;
            if (TextBoxFiles.Text != "") TextBoxNameFiles.Text =
                new DirectoryInfo(State.taskColl.First(item => item.TaskName.Contains(ComboBoxTask.SelectedItem.ToString())).TaskDir).Name;
        }

        //Кнопка отображения деталей
        private void Button_detail_Click(object sender, RoutedEventArgs e)
        {
            TextBlock_type.Text = "Детали";
            TreeviewSet();
            int[] a = { 60, 60, 60, 60, 60, 60, 0, 0, 0, 60 };
            string[] b = { "Добавить деталь", "Изменить деталь", "Удалить деталь", "Индекс детали", "Название детали", "Задание", "Проект", "№ Плана управления", "Разработал" };
            ReadAddPanel(a, b);
            ComboBoxProekt.IsEditable = false;
            ComboBoxZad.IsEditable = false;
            DataGridFiles.ItemsSource = null;
            DataGridFiles.Items.Refresh();
            ButtonAddFiles.IsEnabled = true;
            ButtonReadFiles.IsEnabled = true;
            ButtonRemoveFiles.IsEnabled = true;
            RowDetail.Height = new GridLength(value: 45, type: GridUnitType.Pixel);
            RowFiles.Height = new GridLength(value: 40, type: GridUnitType.Pixel);
            ButtonAddDetail.Visibility = Visibility.Visible;
            ButtonReadDetail.Visibility = Visibility.Visible;
            ButtonRemoveDetail.Visibility = Visibility.Visible;
            ButtonReadDetail.IsEnabled = true;
            ButtonRemoveDetail.IsEnabled = true;
        }

        //Кнопка отображения приспособлений
        private void Button_device_Click(object sender, RoutedEventArgs e)
        {
            TextBlock_type.Text = "Приспособления";
            TreeviewSet();
            int[] a = { 60, 60, 0, 0, 0, 60, 0, 0, 0, 60 };
            string[] b = { "Добавить приспособление", "Изменить приспособление", "Удалить приспособление", "Индекс приспособления", "Название приспособления", "Задание", "Проект", "№ Плана управления", "Разработал" };
            ReadAddPanel(a, b);
            DataGridFiles.ItemsSource = null;
            DataGridFiles.Items.Refresh();
            ButtonAddFiles.IsEnabled = true;
            ButtonReadFiles.IsEnabled = true;
            ButtonRemoveFiles.IsEnabled = true;
            RowDetail.Height = new GridLength(value: 45, type: GridUnitType.Pixel);
            RowFiles.Height = new GridLength(value: 40, type: GridUnitType.Pixel);
            ButtonAddDetail.Visibility = Visibility.Visible;
            ButtonReadDetail.Visibility = Visibility.Visible;
            ButtonRemoveDetail.Visibility = Visibility.Visible;
            ButtonReadDetail.IsEnabled = true;
            ButtonRemoveDetail.IsEnabled = true;
        }

        //Кнопка отображения заданий
        private void Button_task_Click(object sender, RoutedEventArgs e)
        {
            TextBlock_type.Text = "Задания";
            TreeviewSet();
            int[] a = { 0, 0, 60, 60, 0, 60, 60, 80, 30, 60 };
            string[] b = { "Добавить задание", "Изменить задание", "Удалить задание", "Индекс детали", "Название детали", "Задание", "Проект", "№ Плана управления", "Ответственный" };
            ReadAddPanel(a, b);
            ComboBoxProekt.IsEditable = false;
            ComboBoxZad.IsEditable = true;
            DataGridFiles.ItemsSource = null;
            DataGridFiles.Items.Refresh();
            ButtonAddFiles.IsEnabled = false;
            ButtonReadFiles.IsEnabled = false;
            ButtonRemoveFiles.IsEnabled = false;
            RowDetail.Height = new GridLength(value: 45, type: GridUnitType.Pixel);
            RowFiles.Height = new GridLength(value: 40, type: GridUnitType.Pixel);
            ButtonAddDetail.Visibility = Visibility.Visible;
            ButtonReadDetail.Visibility = Visibility.Visible;
            ButtonRemoveDetail.Visibility = Visibility.Visible;
            ButtonReadDetail.IsEnabled = false;
            ButtonRemoveDetail.IsEnabled = false;
        }

        //Кнопка отображения проектов
        private void Button_project_Click(object sender, RoutedEventArgs e)
        {
            TextBlock_type.Text = "Проекты";
            TreeviewSet();
            int[] a = { 0, 0, 0, 60, 0, 0, 0, 0, 0, 60 };
            string[] b = { "Добавить проект", "Изменить проект", "Удалить проект", "Индекс детали", "Название детали", "Задание", "Проект", "№ Плана управления", "Разработал" };
            ReadAddPanel(a, b);
            ComboBoxProekt.IsEditable = true;
            DataGridFiles.ItemsSource = null;
            DataGridFiles.Items.Refresh();
            ButtonAddFiles.IsEnabled = false;
            ButtonReadFiles.IsEnabled = false;
            ButtonRemoveFiles.IsEnabled = false;
            RowDetail.Height = new GridLength(value: 45, type: GridUnitType.Pixel);
            RowFiles.Height = new GridLength(value: 40, type: GridUnitType.Pixel);
            ButtonAddDetail.Visibility = Visibility.Visible;
            ButtonReadDetail.Visibility = Visibility.Visible;
            ButtonRemoveDetail.Visibility = Visibility.Visible;
            ButtonReadDetail.IsEnabled = false;
            ButtonRemoveDetail.IsEnabled = false;
        }

        //Кнопка отображения графиков
        private void Button_graphics_Click(object sender, RoutedEventArgs e)
        {
            TextBlock_type.Text = "Графики";
            TreeviewSet();
            int[] a = { 0, 60, 0, 60, 0, 0, 60, 0, 0, 60 };
            string[] b = { "Добавить график", "Изменить график", "Удалить график", "Индекс приспособления", "Название графика", "Задание", "Проект", "№ Плана управления", "Разработал" };
            ReadAddPanel(a, b);
            ComboBoxProekt.IsEditable = false;
            DataGridFiles.ItemsSource = null;
            DataGridFiles.Items.Refresh();
            ButtonAddFiles.IsEnabled = false;
            ButtonReadFiles.IsEnabled = false;
            ButtonRemoveFiles.IsEnabled = false;
            RowDetail.Height = new GridLength(value: 45, type: GridUnitType.Pixel);
            RowFiles.Height = new GridLength(value: 40, type: GridUnitType.Pixel);
            ButtonAddDetail.Visibility = Visibility.Visible;
            ButtonReadDetail.Visibility = Visibility.Visible;
            ButtonRemoveDetail.Visibility = Visibility.Visible;
            ButtonReadDetail.IsEnabled = true;
            ButtonRemoveDetail.IsEnabled = true;
        }

        //Кнопка отображения документов
        private void Button_service_Click(object sender, RoutedEventArgs e)
        {
            TextBlock_type.Text = "Документы";
            TreeviewSet();
            int[] a = { 60, 60, 0, 0, 0, 0, 60, 0, 0, 60 };
            string[] b = { "Добавить документ", "Изменить документ", "Удалить документ", "Номер документа", "Короткое описание", "Задание", "Проект", "№ Плана управления", "Разработал" };
            ReadAddPanel(a, b);
            DataGridFiles.ItemsSource = null;
            DataGridFiles.Items.Refresh();
            ButtonAddFiles.IsEnabled = false;
            ButtonReadFiles.IsEnabled = false;
            ButtonRemoveFiles.IsEnabled = false;
            RowDetail.Height = new GridLength(value: 45, type: GridUnitType.Pixel);
            RowFiles.Height = new GridLength(value: 40, type: GridUnitType.Pixel);
            ButtonAddDetail.Visibility = Visibility.Visible;
            ButtonReadDetail.Visibility = Visibility.Visible;
            ButtonRemoveDetail.Visibility = Visibility.Visible;
            ButtonReadDetail.IsEnabled = true;
            ButtonRemoveDetail.IsEnabled = true;
        }

        /// <summary>
        /// Метод редактирующий форму добавления\редактирования\удаления 
        /// </summary>
        /// <param name="a">Массив из 10 параметров Height ячеек данных(rowindex.Height, rowname.Height, rowtask.Height, rowproject.Height, rownpu.Height, rowrazrab.Height, rowdir.Height, rowdata.Height, rowcurrent.Height, rowbutton.Height)</param>
        /// <param name="b">Массив из 9 параметров(ButtonAddDetailInDB.ToolTip, ButtonReadDetailInDB.ToolTip, ButtonRemoveDetailInDB.ToolTip, TextBlockIndex.Text, TextBlockName.Text, TextBlockTask.Text, TextBlockProject.Text, TextBlockNPU.Text, TextBlockRazrab.Text)</param>
        public void ReadAddPanel(int[] a, string[] b)
        {
            rowindex.Height = new GridLength(value: a[0], type: GridUnitType.Pixel);
            rowname.Height = new GridLength(value: a[1], type: GridUnitType.Pixel);
            rowtask.Height = new GridLength(value: a[2], type: GridUnitType.Pixel);
            rowproject.Height = new GridLength(value: a[3], type: GridUnitType.Pixel);
            rownpu.Height = new GridLength(value: a[4], type: GridUnitType.Pixel);
            rowrazrab.Height = new GridLength(value: a[5], type: GridUnitType.Pixel);
            rowdir.Height = new GridLength(value: a[6], type: GridUnitType.Pixel);
            rowdata.Height = new GridLength(value: a[7], type: GridUnitType.Pixel);
            rowcurrent.Height = new GridLength(value: a[8], type: GridUnitType.Pixel);
            rowbutton.Height = new GridLength(value: a[9], type: GridUnitType.Pixel);
            ButtonAddDetailInDB.ToolTip = b[0];
            ButtonReadDetailInDB.ToolTip = b[1];
            ButtonRemoveDetailInDB.ToolTip = b[2];
            TextBlockIndex.Text = b[3];
            TextBlockName.Text = b[4];
            TextBlockTask.Text = b[5];
            TextBlockProject.Text = b[6];
            TextBlockNPU.Text = b[7];
            TextBlockRazrab.Text = b[8];
        }

        // Кнопка добавления директории файла в меню добавления задания\документов\графиков
        private void ButtonAddFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.ShowDialog();
            string dir = openFile.FileName;
            if (dir != "")
            {
                TextBoxDirFile.Text = dir;
            }
        }

        // Кнопка открытия активных заданий
        private void CurrentTask_Click(object sender, RoutedEventArgs e)
        {
            Window1 windowTask = new Window1();
            windowTask.Show();
        }

        private void TechnologyList_Click(object sender, RoutedEventArgs e)
        {
            Window2 window = new Window2();
            window.Show();
        }

        private void Button_update_Click(object sender, RoutedEventArgs e)
        {
            State.stateTreeView = true;
            TreeviewSet();
            State.UpdateMessageColl();
            ChatBox.Items.Clear();
            FillChatBox();
        }

        private void TextBox_Search_KeyUp(object sender, KeyEventArgs e)
        {
            TreeViewDet.Items.Clear();
            if (TextBox_Search.Text != "" && TextBox_Search.Text != " ")
            {
                switch (TextBlock_type.Text)
                {
                    case "Детали":
                        foreach (Detail item in State.detailColl.Where(item => item.DetailName.Contains(TextBox_Search.Text) || item.Index.Contains(TextBox_Search.Text)))
                        {
                            TextBlock IT2 = new TextBlock();
                            IT2.Text = item.DetailName + '|' + item.Index;
                            TreeViewDet.Items.Add(IT2);
                        }
                        break;
                    case "Приспособления":
                        foreach (Device item in State.deviceColl.Where(item => item.DeviceIndex.Contains(TextBox_Search.Text) || item.DeviceName.Contains(TextBox_Search.Text)))
                        {
                            TextBlock IT2 = new TextBlock();
                            IT2.Text = item.DeviceIndex;
                            TreeViewDet.Items.Add(IT2);
                        }
                        break;
                    case "Задания":
                        foreach (TaskDet item in State.taskColl.Where(item => item.TaskName.Contains(TextBox_Search.Text)))
                        {
                            TextBlock IT2 = new TextBlock();
                            IT2.Text = item.TaskName;
                            TreeViewDet.Items.Add(IT2);
                        }
                        break;
                    case "Проекты":
                        foreach (Project item in State.projectColl.Where(item => item.ProjectName.Contains(TextBox_Search.Text)))
                        {
                            TextBlock IT2 = new TextBlock();
                            IT2.Text = item.ProjectName;
                            TreeViewDet.Items.Add(IT2);
                        }
                        break;
                    case "Графики":
                        foreach (Graphics item in State.graphicsColl.Where(item => item.NameGrap.Contains(TextBox_Search.Text)))
                        {
                            TextBlock IT2 = new TextBlock();
                            IT2.Text = item.NameGrap;
                            TreeViewDet.Items.Add(IT2);
                        }
                        break;
                    case "Документы":
                        foreach (Services item in State.servicesColl.Where(item => item.NameServ.Contains(TextBox_Search.Text)))
                        {
                            TextBlock IT2 = new TextBlock();
                            IT2.Text = item.NameServ;
                            TreeViewDet.Items.Add(IT2);
                        }
                        break;
                    default: break;
                }
            }
            else { TreeviewSet(); }
        }

        private void RegistryTask_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(@"\\Ts-03\users\OGK\Реестр Заданий\РЕЕСТР ЗАДАНИЙ.xlsx");
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            if (ChatString.Text != "" && ChatString.Text != null)
            {
                SendChatBox();
                ChatString.Clear();
            }
        }

        private void ChatString_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (ChatString.Text != "" && ChatString.Text != null)
                {
                    SendChatBox();
                    ChatString.Clear();
                }
            }
        }

        private void Chat_expand_Click(object sender, RoutedEventArgs e)
        {
            if (RowChat.Height != new GridLength(value: 300, type: GridUnitType.Pixel))
            {
                RowChat.Height = new GridLength(value: 300, type: GridUnitType.Pixel);
            }
            else RowChat.Height = new GridLength(value: 50, type: GridUnitType.Pixel);
            State.UpdateMessageColl();
            FillChatBox();
            Chat_expand.Foreground = new SolidColorBrush(Colors.White);
        }
    }
}
