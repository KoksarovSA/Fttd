using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
            if (ConfigurationManager.AppSettings.Get("CheckBoxTreeViewSet") == "true") { CheckBoxTreeViewSet.IsChecked=true; }
            try
            {
                TreeviewSet();
                Param_in param = new Param_in();
                TextBoxBD.Text = param.GetDirDB();
                TextBoxDir.Text = param.GetDirFiles();
                BackupFTTDDB();
                Window1 window = new Window1();
                window.Show();
            }
            catch (Exception e){ MessageBox.Show(e + " Укажите в настройках файл базы данных и базовую директорию хранения файлов.", "Ошибка"); }
        }

        /// <summary>
        /// Метод бэкапит базу данных через 6 дней  
        /// </summary>
        public void BackupFTTDDB()
        {
            Param_in param = new Param_in();
            if (Math.Abs(DateTime.Now.Day - Convert.ToInt32(param.GetFTTDBackup())) > 6)
            {
                Directory.CreateDirectory(Directory.GetParent(param.GetDirDB()).ToString() + "\\backup");
                if(!File.Exists(Directory.GetParent(param.GetDirDB()).ToString() + "\\backup\\backup_from_" + DateTime.Now.ToString("dd.MM.yyyy") + "_" + new DirectoryInfo(param.GetDirDB()).Name)) File.Copy(param.GetDirDB(), Directory.GetParent(param.GetDirDB()).ToString() + "\\backup\\backup_from_" + DateTime.Now.ToString("dd.MM.yyyy") + "_" + new DirectoryInfo(param.GetDirDB()).Name);
                param.SetFTTDBackup(Convert.ToString(DateTime.Now.Day));
            }
        }

        /// <summary>
        /// Метод копирования файлов и добавления данных в БД
        /// </summary>
        /// <param name="dirout">Директория предоставляемая OpenFileDialog</param>
        /// <param name="index">Индекс детали</param>
        /// <param name="name">Название детали</param>
        /// <param name="file_type">Тип файла</param>
        /// <param name="note">Примечание</param>
        public async void CopyFile(string dirout, string index, string name, string newfilename, string file_type = "None", string note = "None")
        {
            Work_with_files work = new Work_with_files(dirout, index, name, file_type, newfilename);
            switch (file_type)
            {
                case "Задание":
                    {
                        string[] vs = work.Dir_file_copy_in.Split('\\');
                        string vs1 = "\\" + vs[vs.Length - 2] + "\\" + vs[vs.Length - 1];
                        Dbaccess dbaccess = new Dbaccess();
                        await Task.Run(() => dbaccess.Dbinsert("stack_files", "[detail_index], [detail_name], [file_name], [file_type], [file_dir], [file_note]", "'" + index + "', '" + name + "', '" + work.File + "', '" + file_type + "', '" + vs1 + "', '" + note + "'"));                        
                        break;
                    }
                default:
                    {
                        if (TextBlock_type.Text == "Приспособления")
                        {
                            if (File.Exists(work.Dir_file_copy_in))
                            {
                                MessageBox.Show(work.Dir_file_copy_in + "Файл уже есть в базе.", "Ошибка");
                            }
                            else
                            {
                                Directory.CreateDirectory(work.Dir_copy_in + "\\" + name + "_" + index + "");
                                await Task.Run(() => File.Copy(work.Dir_file_copy_out, work.Dir_file_copy_in));
                                string[] vs = work.Dir_file_copy_in.Split('\\');
                                string vs1 = "\\" + vs[vs.Length - 2] + "\\" + vs[vs.Length - 1];
                                Dbaccess dbaccess = new Dbaccess();
                                await Task.Run(() => dbaccess.Dbinsert("device_files", "[indexdev], [file_name], [file_type], [file_dir], [file_note]", "'" + index + "', '" + work.File + "', '" + file_type + "', '" + vs1 + "', '" + note + "'"));
                            }
                        }
                        else
                        {
                            if (File.Exists(work.Dir_file_copy_in))
                            {
                                MessageBox.Show(work.Dir_file_copy_in + "Файл уже есть в базе.", "Ошибка");
                            }
                            else
                            {
                                Directory.CreateDirectory(work.Dir_copy_in + "\\" + name + "_" + index + "");
                                File.Copy(work.Dir_file_copy_out, work.Dir_file_copy_in);
                                string[] vs = work.Dir_file_copy_in.Split('\\');
                                string vs1 = "\\" + vs[vs.Length - 2] + "\\" + vs[vs.Length - 1];
                                Dbaccess dbaccess = new Dbaccess();
                                dbaccess.Dbinsert("stack_files", "[detail_index], [detail_name], [file_name], [file_type], [file_dir], [file_note]", "'" + index + "', '" + name + "', '" + work.File + "', '" + file_type + "', '" + vs1 + "', '" + note + "'");
                            }
                        }                      
                        break;
                    }
            }
            SelectedTreeViewItem();
        }

        /// <summary>
        /// Метод заполняет Treeview наименованиями деталей
        /// </summary>
        public void TreeviewSet()
        {
            TreeViewDet.Items.Clear();
            Dbaccess dbaccess = new Dbaccess();
            switch(TextBlock_type.Text)
            {
                case "Детали":
                    if (CheckBoxTreeViewSet.IsChecked == false)
                    {
                        dbaccess.Dbselect("SELECT [detail_index], [detail_name] FROM [detail_db] ORDER BY [detail_name]");
                        ComboBoxIndex.Items.Clear();
                        ComboBoxName.Items.Clear();
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            TextBlock IT2 = new TextBlock();
                            IT2.Text = vs[1] + '|' + vs[0];
                            TreeViewDet.Items.Add(IT2);
                            ComboBoxIndex.Items.Add(vs[0]);
                            ComboBoxName.Items.Add(vs[1]);
                        }
                        dbaccess.Dbselect("SELECT [project] FROM [project] ORDER BY [project]");
                        ComboBoxProekt.Items.Clear();
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            ComboBoxProekt.Items.Add(vs[0]);
                        }
                        dbaccess.Dbselect("SELECT [task] FROM [task] ORDER BY [task]");
                        ComboBoxZad.Items.Clear();
                        ComboBoxTask.Items.Clear();
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            ComboBoxZad.Items.Add(vs[0]);
                            ComboBoxTask.Items.Add(vs[0]);

                        }
                        dbaccess.Dbselect("SELECT DISTINCT [razrabotal] FROM [detail_db] ORDER BY [razrabotal]");
                        ComboBoxRazrab.Items.Clear();
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            ComboBoxRazrab.Items.Add(vs[0]);
                        }
                        dbaccess.Dbselect("SELECT DISTINCT [inventory] FROM [detail_db] ORDER BY [inventory] DESC");
                        ComboBoxNPU.Items.Clear();
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            ComboBoxNPU.Items.Add(vs[0]);
                        }
                    }
                    else if (CheckBoxTreeViewSet.IsChecked == true)
                    {
                        dbaccess.Dbselect("SELECT DISTINCT [project] FROM [project] ORDER BY [project]");
                        ComboBoxProekt.Items.Clear();
                        ComboBoxIndex.Items.Clear();
                        ComboBoxName.Items.Clear();
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            ComboBoxProekt.Items.Add(vs[0]);
                            TreeViewItem ITProekt = new TreeViewItem();
                            ITProekt.Header = vs[0];
                            TreeViewDet.Items.Add(ITProekt);
                            Dbaccess dbaccess2 = new Dbaccess();
                            dbaccess2.Dbselect("SELECT [detail_index], [detail_name], [project] FROM [detail_db] WHERE [project] = '" + vs[0] + "' ORDER BY [detail_name]");
                            for (int j = 0; j < dbaccess2.Querydata.Count; ++j)
                            {
                                string[] vs2 = dbaccess2.Querydata[j];
                                TextBlock ITDet = new TextBlock();
                                ITDet.Text = vs2[1] + '|' + vs2[0];
                                ITProekt.Items.Add(ITDet);
                                ComboBoxIndex.Items.Add(vs2[0]);
                                ComboBoxName.Items.Add(vs2[1]);
                            }
                        }
                        dbaccess.Dbselect("SELECT [task] FROM [task] ORDER BY [task]");
                        ComboBoxZad.Items.Clear();
                        ComboBoxTask.Items.Clear();
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            ComboBoxZad.Items.Add(vs[0]);
                        }
                        dbaccess.Dbselect("SELECT DISTINCT [razrabotal] FROM [detail_db] ORDER BY [razrabotal]");
                        ComboBoxRazrab.Items.Clear();
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            ComboBoxRazrab.Items.Add(vs[0]);
                        }
                        dbaccess.Dbselect("SELECT DISTINCT [inventory] FROM [detail_db] ORDER BY [inventory] DESC");
                        ComboBoxNPU.Items.Clear();
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            ComboBoxNPU.Items.Add(vs[0]);
                        }
                    }
                    break;
                case "Приспособления":
                    dbaccess.Dbselect("SELECT [indexdev] FROM [device] ORDER BY [indexdev]");
                    ComboBoxIndex.Items.Clear();
                    ComboBoxName.Items.Clear();
                    for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                    {
                            string[] vs = dbaccess.Querydata[i];
                            TextBlock IT2 = new TextBlock();
                            IT2.Text = vs[0];
                            TreeViewDet.Items.Add(IT2);
                            ComboBoxIndex.Items.Add(vs[0]);
                    }
                    dbaccess.Dbselect("SELECT DISTINCT [razrabotal] FROM [detail_db] ORDER BY [razrabotal]");
                    ComboBoxRazrab.Items.Clear();
                    for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                    {
                        string[] vs = dbaccess.Querydata[i];
                        ComboBoxRazrab.Items.Add(vs[0]);
                    }
                    break;
                case "Задания":
                    if (CheckBoxTreeViewSet.IsChecked == false)
                    {
                        dbaccess.Dbselect("SELECT [task] FROM [task] ORDER BY [task]");
                        ComboBoxZad.Items.Clear();
                        ComboBoxTask.Items.Clear();
                        data1.Text = "";
                        data2.Text = "";
                        chTaskIsCurrent.IsChecked = false;
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            TextBlock IT2 = new TextBlock();
                            IT2.Text = vs[0];
                            TreeViewDet.Items.Add(IT2);
                            ComboBoxZad.Items.Add(vs[0]);
                            ComboBoxTask.Items.Add(vs[0]);

                        }
                        dbaccess.Dbselect("SELECT [project] FROM [project] ORDER BY [project]");
                        ComboBoxProekt.Items.Clear();
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            ComboBoxProekt.Items.Add(vs[0]);
                        }
                    }
                    else if (CheckBoxTreeViewSet.IsChecked == true)
                    {
                        dbaccess.Dbselect("SELECT [project] FROM [project] ORDER BY [project]");
                        ComboBoxProekt.Items.Clear();
                        data1.Text = "";
                        data2.Text = "";
                        chTaskIsCurrent.IsChecked = false;
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            ComboBoxProekt.Items.Add(vs[0]);
                            TreeViewItem ITProekt = new TreeViewItem();
                            ITProekt.Header = vs[0];
                            TreeViewDet.Items.Add(ITProekt);
                            Dbaccess dbaccess2 = new Dbaccess();
                            dbaccess2.Dbselect("SELECT [task], [project] FROM [task] WHERE [project] = '" + vs[0] + "' ORDER BY [task]");
                            for (int j = 0; j < dbaccess2.Querydata.Count; ++j)
                            {
                                string[] vs2 = dbaccess2.Querydata[j];
                                TextBlock ITDet = new TextBlock();
                                ITDet.Text = vs2[0];
                                ITProekt.Items.Add(ITDet);
                                ComboBoxZad.Items.Add(vs2[0]);
                                ComboBoxTask.Items.Add(vs2[0]);
                            }
                        }
                    }
                    break;
                case "Проекты":
                    dbaccess.Dbselect("SELECT [project] FROM [project] ORDER BY [project]");
                    ComboBoxProekt.Items.Clear();
                    for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                    {
                        string[] vs = dbaccess.Querydata[i];
                        TextBlock IT2 = new TextBlock();
                        IT2.Text = vs[0];
                        TreeViewDet.Items.Add(IT2);
                        ComboBoxProekt.Items.Add(vs[0]);
                    }
                    break;
                case "Графики":
                    if (CheckBoxTreeViewSet.IsChecked == false)
                    {
                        dbaccess.Dbselect("SELECT [namegrap] FROM [graphics] ORDER BY [namegrap]");
                        ComboBoxName.Items.Clear();
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            TextBlock IT2 = new TextBlock();
                            IT2.Text = vs[0];
                            TreeViewDet.Items.Add(IT2);
                            ComboBoxName.Items.Add(vs[0]);
                        }
                        dbaccess.Dbselect("SELECT [project] FROM [project] ORDER BY [project]");
                        ComboBoxProekt.Items.Clear();
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            ComboBoxProekt.Items.Add(vs[0]);
                        }
                    }
                    else if (CheckBoxTreeViewSet.IsChecked == true)
                    {
                        dbaccess.Dbselect("SELECT [project] FROM [project] ORDER BY [project]");
                        ComboBoxProekt.Items.Clear();
                        ComboBoxName.Items.Clear();
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            ComboBoxProekt.Items.Add(vs[0]);
                            TreeViewItem ITProekt = new TreeViewItem();
                            ITProekt.Header = vs[0];
                            TreeViewDet.Items.Add(ITProekt);
                            Dbaccess dbaccess2 = new Dbaccess();
                            dbaccess2.Dbselect("SELECT [namegrap], [project] FROM [graphics] WHERE [project] = '" + vs[0] + "' ORDER BY [namegrap]");                            
                            for (int j = 0; j < dbaccess2.Querydata.Count; ++j)
                            {
                                string[] vs2 = dbaccess2.Querydata[j];
                                TextBlock ITDet = new TextBlock();
                                ITDet.Text = vs2[0];
                                ITProekt.Items.Add(ITDet);
                                ComboBoxName.Items.Add(vs2[0]);
                            }
                        }
                    }
                    break;
                case "Служебные":
                    dbaccess.Dbselect("SELECT [nameserv] FROM [service] ORDER BY [nameserv]");
                    ComboBoxIndex.Items.Clear();
                    ComboBoxName.Items.Clear();
                    for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                    {
                        string[] vs = dbaccess.Querydata[i];
                        TextBlock IT2 = new TextBlock();
                        IT2.Text = vs[0];
                        TreeViewDet.Items.Add(IT2);
                        ComboBoxIndex.Items.Add(vs[0]);
                    }
                    break;
                default:break;
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
        }

        //switch (TextBlock_type.Text)
        //    {
        //        case "Детали":
                    
        //            break;
        //        case "Приспособления":
                    
        //            break;
        //        case "Задания":
                    
        //            break;
        //        case "Проекты":
                    
        //            break;
        //        case "Графики":
                    
        //            break;
        //        case "Служебные":
                    
        //            break;
        //        default: break;
        //    }

        /// <summary>
        /// Метод заполняющий DataGridFiles
        /// </summary>
        /// <param name="index">Индекс выбраной в TreeViewDet детали</param>
        public void SetDataGrid(string index)
        {
            switch (TextBlock_type.Text)
            {
                case "Детали":
                    try
                    {
                        ObservableCollection<Table> coll = new ObservableCollection<Table>();
                        Dbaccess dbaccess = new Dbaccess();
                        dbaccess.Dbselect("SELECT [detail_index], [file_name], [file_type] FROM [stack_files] WHERE [detail_index] = '" + index + "'");
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            coll.Add(new Table() { Name = vs[1], Type = vs[2] });
                        }
                        DataGridFiles.ItemsSource = coll;
                        DataGridFiles.Items.Refresh();
                    }
                    catch { }                  
                    break;
                case "Приспособления":
                    try
                    {
                        ObservableCollection<Table> coll = new ObservableCollection<Table>();
                        Dbaccess dbaccess = new Dbaccess();
                        dbaccess.Dbselect("SELECT [indexdev], [file_name], [file_type] FROM [device_files] WHERE [indexdev] = '" + index + "'");
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            coll.Add(new Table() { Name = vs[1], Type = vs[2] });
                        }
                        DataGridFiles.ItemsSource = coll;
                        DataGridFiles.Items.Refresh();
                    }
                    catch { }
                    break;
                case "Задания":
                    try
                    {
                        ObservableCollection<Table> coll = new ObservableCollection<Table>();
                        Dbaccess dbaccess = new Dbaccess();
                        dbaccess.Dbselect("SELECT [task], [dir] FROM [task] WHERE [task] = '" + index + "'");
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            string fn = "";
                            string tp = "";
                            if (vs[1] != "") { fn = new DirectoryInfo(vs[1]).Name; tp = "Задание"; }
                            coll.Add(new Table() { Name = fn, Type = tp });
                        }
                        DataGridFiles.ItemsSource = coll;
                        DataGridFiles.Items.Refresh();
                    }
                    catch { }
                    break;
                case "Проекты":
                    break;
                case "Графики":
                    try
                    {
                        ObservableCollection<Table> coll = new ObservableCollection<Table>();
                        Dbaccess dbaccess = new Dbaccess();
                        dbaccess.Dbselect("SELECT [namegrap], [dir] FROM [graphics] WHERE [namegrap] = '" + index + "'");
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            string fn = "";
                            string tp = "";
                            if (vs[1] != "") { fn = new DirectoryInfo(vs[1]).Name; tp = "График"; }
                            coll.Add(new Table() { Name = fn, Type = tp });
                        }
                        DataGridFiles.ItemsSource = coll;
                        DataGridFiles.Items.Refresh();
                    }
                    catch { }
                    break;
                case "Служебные":
                    try
                    {
                        ObservableCollection<Table> coll = new ObservableCollection<Table>();
                        Dbaccess dbaccess = new Dbaccess();
                        dbaccess.Dbselect("SELECT [nameserv], [dir] FROM [service] WHERE [nameserv] = '" + index + "'");
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            string fn = "";
                            string tp = "";
                            if (vs[1] != "") { fn = new DirectoryInfo(vs[1]).Name; tp = "Служебная"; }
                            coll.Add(new Table() { Name = fn, Type = tp });
                        }
                        DataGridFiles.ItemsSource = coll;
                        DataGridFiles.Items.Refresh();
                    }
                    catch { }
                    break;
                default: break;
            }  
        }

        /// <summary>
        /// Метод получающий данные детали строкой
        /// </summary>
        /// <param name="index">Индекс детали</param>
        /// <returns>Данные детали строкой</returns>
        public string GetNoteDetail(string index)
        {
            switch (TextBlock_type.Text)
            {
                case "Детали":
                    try
                    {
                        Dbaccess dbaccess = new Dbaccess();
                        dbaccess.Dbselect("SELECT [detail_index], [detail_name], [inventory], [number_task], [project], [razrabotal], [data_add] FROM [detail_db] WHERE [detail_index] = '" + index + "'");
                        string x = "";
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            x = "Деталь: " + vs[1] + "\n" + "Индекс: " + vs[0] + "\n" + "Инв.№ " + vs[2] + "\n" + "Проект: " + vs[4] + "\n" + "№ Задания: " + vs[3] + "\n" + "Разработал: " + vs[5] + "\n" + "Дата добавления: " + vs[6];
                            ComboBoxIndex.Text = vs[0];
                            ComboBoxName.Text = vs[1];
                            ComboBoxNPU.Text = vs[2];
                            ComboBoxZad.Text = vs[3];
                            ComboBoxProekt.Text = vs[4];
                            ComboBoxRazrab.Text = vs[5];
                        }
                        return x;
                    }
                    catch
                    {
                        string x = "";
                        return x;
                    }                   
                case "Приспособления":
                    try
                    {
                        Dbaccess dbaccess = new Dbaccess();
                        dbaccess.Dbselect("SELECT [indexdev], [namedev], [razrab], [data_add] FROM [device] WHERE [indexdev] = '" + index + "'");
                        string x = "";
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            x = "Приспособление: " + vs[1] + "\n" + "Индекс: " + vs[0] + "\n" + "Разработал: " + vs[2] + "\n" + "Дата добавления: " + vs[3];
                            ComboBoxIndex.Text = vs[0];
                            ComboBoxName.Text = vs[1];
                            ComboBoxRazrab.Text = vs[3];
                        }
                        return x;
                    }
                    catch
                    {
                        string x = "";
                        return x;
                    }
                case "Задания":
                    try
                    {
                        Dbaccess dbaccess = new Dbaccess();
                        dbaccess.Dbselect("SELECT [task], [project], [iscurrent], [datein], [dateout] FROM [task] WHERE [task] = '" + index + "'");
                        string x = "";
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            string current = "Нет";
                            if (vs[2] == "True") { chTaskIsCurrent.IsChecked = true; current = "Да"; }
                            DateTime dateTime1 = DateTime.Parse(vs[3]);
                            DateTime dateTime2 = DateTime.Parse(vs[4]);
                            x = "Задание: " + vs[0] + "\n" + "Проект: " + vs[1] + "\n" + "Актуальное: " + current + "\n" + "Дата выдачи: " + dateTime1.Date.ToString("dd.MM.yy") + "\n" + "Выполнить до: " + dateTime2.Date.ToString("dd.MM.yy");
                            ComboBoxZad.Text = vs[0];
                            ComboBoxProekt.Text = vs[1];
                            data1.SelectedDate = DateTime.Parse(vs[3]);
                            data2.SelectedDate = DateTime.Parse(vs[4]);
                        }
                        return x;
                    }
                    catch
                    {
                        string x = "";
                        return x;
                    }
                case "Проекты":
                    string y = "";
                    return y;
                case "Графики":
                    try
                    {
                        Dbaccess dbaccess = new Dbaccess();
                        dbaccess.Dbselect("SELECT [namegrap], [project], [data_add] FROM [graphics] WHERE [namegrap] = '" + index + "'");
                        string x = "";
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            x = "График: " + vs[0] + "\n" + "Проект: " + vs[1] + "\n" + "Дата добавления: " + vs[2];
                            ComboBoxName.Text = vs[0];
                            ComboBoxProekt.Text = vs[1];
                        }
                        return x;
                    }
                    catch
                    {
                        string x = "";
                        return x;
                    }
                case "Служебные":
                    try
                    {
                        Dbaccess dbaccess = new Dbaccess();
                        dbaccess.Dbselect("SELECT [nameserv], [note], [data_add] FROM [service] WHERE [nameserv] = '" + index + "'");
                        string x = "";
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            x = "Служебная: " + vs[0] + "\n" + "Короткое описание: " + vs[1] + "\n" + "Дата добавления: " + vs[2];
                            ComboBoxIndex.Text = vs[0];
                            ComboBoxName.Text = vs[1];
                        }
                        return x;
                    }
                    catch
                    {
                        string x = "";
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
            switch (TextBlock_type.Text)
            {
                case "Детали":
                    try
                    {
                        Dbaccess dbaccess = new Dbaccess();
                        dbaccess.Dbselect("SELECT [detail_index], [detail_name], [file_name], [file_type], [file_dir], [file_note], [data_add] FROM [stack_files] WHERE [detail_index] = '" + index + "' AND [file_name] = '" + filename + "'");
                        string x = "";
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
                    catch { string x = ""; return x; } 
                case "Приспособления":
                    try
                    {
                        Dbaccess dbaccess = new Dbaccess();
                        dbaccess.Dbselect("SELECT [indexdev], [file_name], [file_type], [file_dir], [file_note], [data_add] FROM [device_files] WHERE [indexdev] = '" + index + "' AND [file_name] = '" + filename + "'");
                        string x = "";
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
                    catch { string x = ""; return x; }
                case "Задания":
                    try
                    {
                        Dbaccess dbaccess = new Dbaccess();
                        dbaccess.Dbselect("SELECT [task], [dir], [note] FROM [task] WHERE [task] = '" + index + "'");
                        string x = "";
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
                    catch { string x = ""; return x; }
                case "Проекты":
                    string y = "";
                    return y; 
                case "Графики":
                    try
                    {
                        Dbaccess dbaccess = new Dbaccess();
                        dbaccess.Dbselect("SELECT [namegrap], [dir], [data_add] FROM [graphics] WHERE [namegrap] = '" + index + "'");
                        string x = "";
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
                    catch { string x = ""; return x; }
                case "Служебные":
                    try
                    {
                        Dbaccess dbaccess = new Dbaccess();
                        dbaccess.Dbselect("SELECT [nameserv], [dir], [note], [data_add] FROM [service] WHERE [nameserv] = '" + index + "'");
                        string x = "";
                        for (int i = 0; i < dbaccess.Querydata.Count; ++i)
                        {
                            string[] vs = dbaccess.Querydata[i];
                            string fn = "";
                            string tp = "";
                            if (vs[1] != "") { fn = new DirectoryInfo(vs[1]).Name; tp = "Служебная"; }
                            x = "Описание файла\nНазвание файла: " + fn + "\n" + "Тип файла: " + tp + "\n" + "Примечание: " + vs[2] + "\n" + "Дата добавления: " + vs[3];
                        }
                        return x;
                    }
                    catch { string x = ""; return x; }
                default:
                    string z = "";
                    return z;
            }

;
        }

        // Кнопка выход
        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        // Перетаскивание окна
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
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
            string dir = openFile.FileName;
            TextBoxBD.Text = dir;
            Param_in param = new Param_in();
            param.SetDirDB(dir);
            MessageBox.Show("После изменения файла базы данных приложение будет перезапущено.", "Изменение файла базы данных");
            this.Close();
            System.Diagnostics.Process.Start(@"Fttd.exe");
        }

        // Кнопка в настройках добавления базовой директории
        private void ButtonAddDir_Click(object sender, RoutedEventArgs e)
        {
            WinForms.FolderBrowserDialog folder = new WinForms.FolderBrowserDialog();
            folder.ShowDialog();
            string dir = folder.SelectedPath;
            TextBoxDir.Text = dir;
            Param_in param = new Param_in();
            param.SetDirFiles(dir);
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
                case "Задания": x = 390; break;
                case "Проекты": x = 160; break;
                case "Графики": x = 280; break;
                case "Служебные": x = 280; break;
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
                case "Задания": x = 390; break;
                case "Проекты": x = 160; break;
                case "Графики": x = 280; break;
                case "Служебные": x = 280; break;
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
                case "Задания": x = 390; break;
                case "Проекты": x = 160; break;
                case "Графики": x = 280; break;
                case "Служебные": x = 280; break;
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
                switch (TextBlock_type.Text)
                {
                    case "Детали":
                        if (ComboBoxIndex.Text != "")
                        {
                            Dbaccess dbaccess = new Dbaccess();
                            dbaccess.Dbinsert("detail_db", "[detail_index], [detail_name], [inventory], [number_task], [project], [razrabotal]", "'" + ComboBoxIndex.Text + "', '" + ComboBoxName.Text + "', '" + ComboBoxNPU.Text + "', '" + ComboBoxZad.Text + "', '" + ComboBoxProekt.Text + "', '" + ComboBoxRazrab.Text + "'");
                            TreeviewSet();
                        }
                        else { MessageBox.Show("Ведите название детали", "Ошибка"); }
                        break;
                    case "Приспособления":
                        if (ComboBoxIndex.Text != "")
                        {
                            Dbaccess dbaccess = new Dbaccess();
                            dbaccess.Dbinsert("device", "[indexdev], [namedev], [razrab]", "'" + ComboBoxIndex.Text + "', '" + ComboBoxName.Text + "', '" + ComboBoxRazrab.Text + "'");
                            TreeviewSet();
                        }
                        else { MessageBox.Show("Ведите название приспособления", "Ошибка"); }
                        break;
                    case "Задания":
                        if (ComboBoxZad.Text != "" && TextBoxDirFile.Text != "" && ComboBoxProekt.Text != "")
                        {
                            if (ComboBoxZad.Items.Contains(ComboBoxZad.Text))
                            {
                                    MessageBox.Show("Укажите новое задание", "Ошибка"); 
                            }
                            else
                            {
                                try
                                {
                                    Work_with_files work = new Work_with_files(TextBoxDirFile.Text, "", "", "Задание", new DirectoryInfo(TextBoxDirFile.Text).Name);
                                    if (File.Exists(work.Dir_file_copy_in))
                                    {
                                        MessageBox.Show(work.Dir_file_copy_in + "Файл уже есть в базе, он будет привязан к заданию.", "Ошибка");
                                    }
                                    else
                                    {
                                        Directory.CreateDirectory(work.Dir_copy_in + "\\Задания");
                                        File.Copy(work.Dir_file_copy_out, work.Dir_file_copy_in);
                                    }
                                    Dbaccess dbaccess = new Dbaccess();
                                    dbaccess.DbRead("INSERT INTO [task] ([task], [project], [dir], [datein], [dateout], [iscurrent]) VALUES ('" + ComboBoxZad.Text + "', '" + ComboBoxProekt.Text + "', '" + TextBoxDirFile.Text + "', '" + data1.Text + "', '" + data2.Text + "', " + chTaskIsCurrent.IsChecked.Value + ")");
                                    TreeviewSet();
                                }
                                catch (Exception ex){ MessageBox.Show(ex + "", "Ошибка"); }
                            }
                        }
                        else { MessageBox.Show("Ведите номер задания, выберите проект и укажите файл","Ошибка"); }
                        break;
                    case "Проекты":
                        if (ComboBoxProekt.Text != "")
                        {
                            Dbaccess dbaccess = new Dbaccess();
                            dbaccess.Dbinsert("project", "[project]", "'" + ComboBoxProekt.Text + "'");
                            TreeviewSet();
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
                                    Work_with_files work = new Work_with_files(TextBoxDirFile.Text, "", "", "График", new DirectoryInfo(TextBoxDirFile.Text).Name);
                                    if (File.Exists(work.Dir_file_copy_in))
                                    {
                                        MessageBox.Show(work.Dir_file_copy_in + "Файл уже есть в базе, он будет привязан к заданию.", "Ошибка");
                                    }
                                    else
                                    {
                                        Directory.CreateDirectory(work.Dir_copy_in + "\\Графики");
                                        File.Copy(work.Dir_file_copy_out, work.Dir_file_copy_in);
                                    }
                                    Dbaccess dbaccess = new Dbaccess();
                                    dbaccess.Dbinsert("graphics", "[namegrap], [project], [dir]", "'" + ComboBoxName.Text + "', '" + ComboBoxProekt.Text + "', '" + TextBoxDirFile.Text + "'");
                                    TreeviewSet();
                                }
                                catch (Exception ex) { MessageBox.Show(ex + "", "Ошибка"); }
                            }
                        }
                        else { MessageBox.Show("Ведите название графика, выберите проект и укажите файл", "Ошибка"); }
                        break;
                    case "Служебные":
                        if (ComboBoxIndex.Text != "" && ComboBoxName.Text != "" && TextBoxDirFile.Text != "")
                        {
                            if (ComboBoxIndex.Items.Contains(ComboBoxIndex.Text))
                            {
                                MessageBox.Show("Укажите новую служебную", "Ошибка");
                            }
                            else
                            {
                                try
                                {
                                    Work_with_files work = new Work_with_files(TextBoxDirFile.Text, "", "", "Служебная", new DirectoryInfo(TextBoxDirFile.Text).Name);
                                    if (File.Exists(work.Dir_file_copy_in))
                                    {
                                        MessageBox.Show(work.Dir_file_copy_in + "Файл уже есть в базе, он будет привязан к заданию.", "Ошибка");
                                    }
                                    else
                                    {
                                        Directory.CreateDirectory(work.Dir_copy_in + "\\Служебные");
                                        File.Copy(work.Dir_file_copy_out, work.Dir_file_copy_in);
                                    }
                                    Dbaccess dbaccess = new Dbaccess();
                                    dbaccess.Dbinsert("service", "[nameserv], [note], [dir]", "'" + ComboBoxIndex.Text + "', '" + ComboBoxName.Text + "', '" + TextBoxDirFile.Text + "'");
                                    TreeviewSet();
                                }
                                catch (Exception ex) { MessageBox.Show(ex + "", "Ошибка"); }
                            }
                        }
                        else { MessageBox.Show("Ведите номер служебной, краткое описание и укажите файл", "Ошибка"); }
                        break;
                    default: break;
                }               
            }
            catch(Exception ex) { MessageBox.Show(ex +"", "Ошибка"); }
        }

        // Кнопка изменения детали
        private void ButtonReadDetailInDB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (TextBlock_type.Text)
                {
                    case "Детали":
                        if (ComboBoxIndex.Text != "")
                        {
                            Dbaccess dbaccess = new Dbaccess();
                            dbaccess.DbRead("UPDATE [detail_db] SET [detail_name] = '" + ComboBoxName.Text + "', [number_task] = '" + ComboBoxZad.Text + "', [project] = '" + ComboBoxProekt.Text + "', [razrabotal] = '" + ComboBoxRazrab.Text + "' WHERE [detail_index] = '" + ComboBoxIndex.Text + "' AND [inventory] = '" + ComboBoxNPU.Text + "'");
                            TreeviewSet();
                        }
                        else { MessageBox.Show("Выберите деталь", "Ошибка"); }
                        break;
                    case "Приспособления":
                        if (ComboBoxIndex.Text != "")
                        {                      
                            Dbaccess dbaccess = new Dbaccess();
                            dbaccess.DbRead("UPDATE [device] SET [namedev] = '" + ComboBoxName.Text + "', [razrabotal] = '" + ComboBoxRazrab.Text + "' WHERE [indexdev] = '" + ComboBoxIndex.Text + "'");
                            TreeviewSet();
                        }
                        else { MessageBox.Show("Ведите название приспособления", "Ошибка"); }
                        break;
                    case "Задания":
                        if (ComboBoxZad.Text != "")
                        {
                            Dbaccess dbaccess = new Dbaccess();
                            dbaccess.DbRead("UPDATE [task] SET [project] = '" + ComboBoxProekt.Text + "', [dir] = '" + TextBoxDirFile.Text + "' WHERE [task] = '" + ComboBoxZad.Text + "'");
                            TreeviewSet();
                        }
                        else { MessageBox.Show("Выберите задание", "Ошибка"); }
                        break;
                    case "Проекты":
                        MessageBox.Show("Проект изменить нельзя", "Ошибка"); 
                        break;
                    case "Графики":
                        if (ComboBoxName.Text != "")
                        {
                            Dbaccess dbaccess = new Dbaccess();
                            dbaccess.DbRead("UPDATE [graphics] SET [project] = '" + ComboBoxProekt.Text + "', [dir] = '" + TextBoxDirFile.Text + "' WHERE [namegrap] = '" + ComboBoxName.Text + "'");
                            TreeviewSet();
                        }
                        else { MessageBox.Show("Выберите график", "Ошибка"); }
                        break;
                    case "Служебные":
                        if (ComboBoxIndex.Text != "")
                        {
                            Dbaccess dbaccess = new Dbaccess();
                            dbaccess.DbRead("UPDATE [service] SET [note] = '" + ComboBoxName.Text + "', [dir] = '" + TextBoxDirFile.Text + "' WHERE [nameserv] = '" + ComboBoxIndex.Text + "'");
                            TreeviewSet();
                        }
                        else { MessageBox.Show("Выберите служебную", "Ошибка"); }
                        break;
                    default: break;
                }
            }
            catch { }
        }

        // Кнопка удаления детали
        private void ButtonRemoveDetailInDB_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                switch (TextBlock_type.Text)
                {
                    case "Детали":
                        if (ComboBoxIndex.Text != "")
                        {
                            Dbaccess dbaccess = new Dbaccess();
                            dbaccess.DbRead("DELETE FROM [detail_db] WHERE [detail_index] = '" + ComboBoxIndex.Text + "' AND [inventory] = '" + ComboBoxNPU.Text + "'");
                            TreeviewSet();
                        }
                        else { MessageBox.Show("Выберите деталь", "Ошибка"); }
                        break;
                    case "Приспособления":
                        if (ComboBoxIndex.Text != "")
                        {
                            Dbaccess dbaccess = new Dbaccess();
                            dbaccess.DbRead("DELETE FROM [device] WHERE [indexdev] = '" + ComboBoxIndex.Text + "'");
                            TreeviewSet();
                        }
                        else { MessageBox.Show("Выберите приспособление", "Ошибка"); }
                        break;
                    case "Задания":
                        MessageBox.Show("Задание можно удалить только через администратора", "Ошибка");
                        break;
                    case "Проекты":
                        MessageBox.Show("Проект можно удалить только через администратора", "Ошибка");
                        break;
                    case "Графики":
                        if (ComboBoxName.Text != "")
                        {
                            Dbaccess dbaccess = new Dbaccess();
                            dbaccess.DbRead("DELETE FROM [graphics] WHERE [namegrap] = '" + ComboBoxName.Text + "'");
                            TreeviewSet();
                        }
                        else { MessageBox.Show("Выберите график", "Ошибка"); }
                        break;
                    case "Служебные":
                        if (ComboBoxIndex.Text != "")
                        {
                            Dbaccess dbaccess = new Dbaccess();
                            dbaccess.DbRead("DELETE FROM [service] WHERE [nameserv] = '" + ComboBoxIndex.Text + "'");
                            TreeviewSet();                            
                        }
                        else { MessageBox.Show("Выберите служебную", "Ошибка"); }
                        break;
                    default: break;
                }
            }
            catch { }
        }

        // Кнопка добавления директории файла
        private void ButtonDirFiles_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog();
            openFile.ShowDialog();
            string dir = openFile.FileName;
            if (dir !="")
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
                switch (TextBlock_type.Text)
                {
                    case "Детали":
                        try
                        {
                            string textItem = (TreeViewDet.SelectedItem as TextBlock).Text;
                            string[] name = textItem.Split('|');
                            CopyFile(TextBoxFiles.Text, name[1], name[0], TextBoxNameFiles.Text, ComboBoxTypeFiles.Text, TextBoxNote.Text);
                        }
                        catch { }
                        break;
                    case "Приспособления":
                        try
                        {
                            string textItem = (TreeViewDet.SelectedItem as TextBlock).Text;
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
                switch (TextBlock_type.Text)
                {
                    case "Детали":
                        try
                        {
                            string textItem = (TreeViewDet.SelectedItem as TextBlock).Text;
                            string[] name = textItem.Split('|');
                            Dbaccess dbaccess = new Dbaccess();
                            dbaccess.DbRead("UPDATE [stack_files] SET [file_type] = '" + ComboBoxTypeFiles.Text + "', [file_note] = '" + TextBoxNote.Text + "' WHERE [detail_index] = '" + name[1] + "' AND [file_dir] = '" + TextBoxFiles.Text + "'");
                        }
                        catch { }
                        break;
                    case "Приспособления":
                        try
                        {
                            string textItem = (TreeViewDet.SelectedItem as TextBlock).Text;
                            Dbaccess dbaccess = new Dbaccess();
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
                switch (TextBlock_type.Text)
                {
                    case "Детали":
                        try
                        {
                            string textItem = (TreeViewDet.SelectedItem as TextBlock).Text;
                            string[] name = textItem.Split('|');
                            Dbaccess dbaccess = new Dbaccess();
                            dbaccess.DbRead("DELETE FROM [stack_files] WHERE [detail_index] = '" + name[1] + "' AND [file_dir] = '" + TextBoxFiles.Text + "'");
                            if (ComboBoxTypeFiles.Text == "Задание")
                            { MessageBox.Show("Файл задания может быть привязан к другим деталям. Привязка к данной детали будет удалена.", "Удаление"); }
                            else { Param_in param = new Param_in(); File.Delete(@"" + param.GetDirFiles() + "\\" + TextBoxFiles.Text + ""); MessageBox.Show("Файл успешно удалён.", "Удаление"); }
                        }
                        catch { }
                        break;
                    case "Приспособления":
                        try
                        {
                            string textItem = (TreeViewDet.SelectedItem as TextBlock).Text;
                            Dbaccess dbaccess = new Dbaccess();
                            dbaccess.DbRead("DELETE FROM [device_files] WHERE [indexdev] = '" + textItem + "' AND [file_dir] = '" + TextBoxFiles.Text + "'");
                            if (ComboBoxTypeFiles.Text == "Задание")
                            { MessageBox.Show("Файл задания может быть привязан к другим деталям. Привязка к данной детали будет удалена.", "Удаление"); }
                            else { Param_in param = new Param_in(); File.Delete(@"" + param.GetDirFiles() + "\\" + TextBoxFiles.Text + ""); MessageBox.Show("Файл успешно удалён.", "Удаление"); }
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
                            Param_in param = new Param_in();
                            Process.Start(@"" + param.GetDirFiles() + "\\" + TextBoxFiles.Text + "");
                        }
                        catch { }
                        break;
                    case "Приспособления":
                        try
                        {
                            TextBlockPF.Text = GetNoteFiles(textItem, textDataGrid);
                            Param_in param = new Param_in();
                            Process.Start(@"" + param.GetDirFiles() + "\\" + TextBoxFiles.Text + "");
                        }
                        catch { }
                        break;
                    case "Задания":
                        try
                        {
                            TextBlockPF.Text = GetNoteFiles(textItem, textDataGrid);
                            Param_in param = new Param_in();
                            Process.Start(@"" + param.GetDirFiles() + "\\Задания\\" + textDataGrid + "");
                        }
                        catch { }
                        break;
                    case "Проекты":
                        break;
                    case "Графики":
                        try
                        {
                            TextBlockPF.Text = GetNoteFiles(textItem, textDataGrid);
                            Param_in param = new Param_in();
                            Process.Start(@"" + param.GetDirFiles() + "\\Графики\\" + textDataGrid + "");
                        }
                        catch { }
                        break;
                    case "Служебные":
                        try
                        {
                            TextBlockPF.Text = GetNoteFiles(textItem, textDataGrid);
                            Param_in param = new Param_in();
                            Process.Start(@"" + param.GetDirFiles() + "\\Служебные\\" + textDataGrid + "");
                        }
                        catch { }
                        break;
                    default: break;
                }
            }
            catch { }

        }

        //    //var bc = new BrushConverter();
        //    //grid0.Background = (Brush)bc.ConvertFrom("#FF7AB9D1");
        //    //grid1.Background = (Brush)bc.ConvertFrom("#FF157599");
        //    //grid2.Background = (Brush)bc.ConvertFrom("#FF7AB9D1");
        //    //grid3.Background = (Brush)bc.ConvertFrom("#FF7AB9D1");
        //    //DataGridFiles.Background = (Brush)bc.ConvertFrom("#FF7AB9D1");
        //    //DataGridFiles.AlternatingRowBackground = (Brush)bc.ConvertFrom("#FF7AB9D1");



        // Чекбокс отображения меню включен(отображение по проектам) 
        private void CheckBoxTreeViewSet_Checked(object sender, RoutedEventArgs e)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var entry = config.AppSettings.Settings["CheckBoxTreeViewSet"];
            if (entry == null) config.AppSettings.Settings.Add("CheckBoxTreeViewSet", "true");
            else config.AppSettings.Settings["CheckBoxTreeViewSet"].Value = "true";
            config.Save(ConfigurationSaveMode.Modified);
        }

        // Чекбокс отображения меню выключен(отображение по деталям) 
        private void CheckBoxTreeViewSet_Unchecked(object sender, RoutedEventArgs e)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            var entry = config.AppSettings.Settings["CheckBoxTreeViewSet"];
            if (entry == null) config.AppSettings.Settings.Add("CheckBoxTreeViewSet", "false");
            else config.AppSettings.Settings["CheckBoxTreeViewSet"].Value = "false";
            config.Save(ConfigurationSaveMode.Modified);
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
            string task = ComboBoxTask.SelectedItem.ToString();
            Dbaccess dbaccess = new Dbaccess();
            dbaccess.Dbselect("SELECT [task], [dir] FROM [task] WHERE [task] = '"+ task + "'");
            for (int i = 0; i < dbaccess.Querydata.Count; ++i)
            {
                string[] vs = dbaccess.Querydata[i];
                TextBoxFiles.Text = (vs[1]);
                if(TextBoxFiles.Text != "") TextBoxNameFiles.Text = new DirectoryInfo(vs[1]).Name;
            }
        }

        //Действие при изменении чекбокса в настройках отображения по проектам
        private void CheckBoxTreeViewSet_Click(object sender, RoutedEventArgs e)
        {
            TreeviewSet();
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
            RowDetail.Height = new GridLength(value: 40, type: GridUnitType.Pixel);
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
            RowDetail.Height = new GridLength(value: 40, type: GridUnitType.Pixel);
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
            int[] a = { 0, 0, 60, 60, 0, 0, 60, 80, 30, 60 };
            string[] b = { "Добавить задание", "Изменить задание", "Удалить задание", "Индекс детали", "Название детали", "Задание", "Проект", "№ Плана управления", "Разработал" };
            ReadAddPanel(a, b);
            ComboBoxProekt.IsEditable = false;
            ComboBoxZad.IsEditable = true;
            DataGridFiles.ItemsSource = null;
            DataGridFiles.Items.Refresh();
            ButtonAddFiles.IsEnabled = false;
            ButtonReadFiles.IsEnabled = false;
            ButtonRemoveFiles.IsEnabled = false;
            RowDetail.Height = new GridLength(value: 40, type: GridUnitType.Pixel);
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
            RowDetail.Height = new GridLength(value: 40, type: GridUnitType.Pixel);
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
            RowDetail.Height = new GridLength(value: 40, type: GridUnitType.Pixel);
            ButtonAddDetail.Visibility = Visibility.Visible;
            ButtonReadDetail.Visibility = Visibility.Visible;
            ButtonRemoveDetail.Visibility = Visibility.Visible;
            ButtonReadDetail.IsEnabled = true;
            ButtonRemoveDetail.IsEnabled = true;
        }

        //Кнопка отображения служебных
        private void Button_service_Click(object sender, RoutedEventArgs e)
        {
            TextBlock_type.Text = "Служебные";
            TreeviewSet();
            int[] a = { 60, 60, 0, 0, 0, 0, 60, 0, 0, 60 };
            string[] b = { "Добавить служебную", "Изменить служебную", "Удалить служебную", "Номер служебной", "Короткое описание", "Задание", "Проект", "№ Плана управления", "Разработал" };
            ReadAddPanel(a, b);
            DataGridFiles.ItemsSource = null;
            DataGridFiles.Items.Refresh();
            ButtonAddFiles.IsEnabled = false;
            ButtonReadFiles.IsEnabled = false;
            ButtonRemoveFiles.IsEnabled = false;
            RowDetail.Height = new GridLength(value: 40, type: GridUnitType.Pixel);
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

        // Кнопка добавления директории файла
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
    }
}
