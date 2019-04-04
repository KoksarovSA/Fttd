using System.Configuration;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Data.OleDb;

namespace Fttd
{
    class Work_with_files : Param_in
    {
        private string dir_file_copy_out;
        private string dir_file_copy_in;
        private string dir_copy_in;
        private string file;

        public string Dir_file_copy_out
        {
            get { return dir_file_copy_out; }
            set { dir_file_copy_out = value; }
        }

        public string Dir_file_copy_in
        {
            get { return dir_file_copy_in; }
            set { dir_file_copy_in = value; }
        }

        public string Dir_copy_in
        {
            get { return dir_copy_in; }
            set { dir_copy_in = value; }
        }

        public string File
        {
            get { return file; }
            set { file = value; }
        }

        /// <summary>
        /// Конструктор класса Work_with_files
        /// </summary>
        /// <param name="dirout">Директория файла (откуда)</param>
        /// <param name="index">Индекс деталм</param>
        /// <param name="name">Имя детали</param>
        public Work_with_files(string dirout, string index, string name, string file_type, string newfilename)
        {

            Dir_copy_in = @"" + GetDirFiles() + "";
            Dir_file_copy_out = dirout;
            file = newfilename;
            switch (file_type)
            {
                case "Задание": Dir_file_copy_in = Dir_copy_in + "\\Задания\\" + File; break;
                case "График": Dir_file_copy_in = Dir_copy_in + "\\Графики\\" + File; break;
                case "Служебная": Dir_file_copy_in = Dir_copy_in + "\\Служебные\\" + File; break;
                default: Dir_file_copy_in = Dir_copy_in + "\\" + name + "_" + index + "\\" + File; break;
            }
        }
    }

    class Table
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    /// <summary>
    /// Класс изменяющий конфигурационный файл программы
    /// </summary>
    class Param_in
    {
        /// <summary>
        /// Метод для получения последнего числа бэкапа
        /// </summary>
        /// <returns>Возвращает число месяца последнего бэкапа</returns>
        public string GetFTTDBackup()
        {
            string dirDb = ConfigurationManager.AppSettings.Get("FTTDBackup");
            return dirDb;
        }

        /// <summary>
        /// Метод записи последнего числа бэкапа
        /// </summary>
        /// <param name="txt_dir">Число месяца последнего бэкапа</param>
        public void SetFTTDBackup(string txt_dir)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            var entry = config.AppSettings.Settings["FTTDBackup"];
            if (entry == null)
                config.AppSettings.Settings.Add("FTTDBackup", txt_dir);
            else
                config.AppSettings.Settings["FTTDBackup"].Value = txt_dir;

            config.Save(ConfigurationSaveMode.Modified);
        }

        /// <summary>
        /// Метод для получения директории файла базы данных базовой директории файлов
        /// </summary>
        /// <returns>Возвращает директорию файла базы данных</returns>
        public string GetDirDB()
        {
            string dirDb = ConfigurationManager.AppSettings.Get("DirDB");
            return dirDb;
        }

        /// <summary>
        /// Метод записи в конфигурацию программы директорию файла базы данных
        /// </summary>
        /// <param name="txt_dir">Директория файла базы данных</param>
        public void SetDirDB(string txt_dir)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            var entry = config.AppSettings.Settings["DirDB"];
            if (entry == null)
                config.AppSettings.Settings.Add("DirDB", txt_dir);
            else
                config.AppSettings.Settings["DirDB"].Value = txt_dir;

            config.Save(ConfigurationSaveMode.Modified);
        }

        /// <summary>
        /// Метод для получения базовой директории файлов
        /// </summary>
        /// <returns>Возвращает базовую директорию файлов</returns>
        public string GetDirFiles()
        {
            string dirDirFiles = ConfigurationManager.AppSettings.Get("DirFiles");
            return dirDirFiles;
        }
        
        /// <summary>
        /// Метод изменения базовой директории в конфигурации программы
        /// </summary>
        /// <param name="txt_dir">Базовая директория папки</param>
        public void SetDirFiles(string txt_dir)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            var entry = config.AppSettings.Settings["DirFiles"];
            if (entry == null)
                config.AppSettings.Settings.Add("DirFiles", txt_dir);
            else
                config.AppSettings.Settings["DirFiles"].Value = txt_dir;

            config.Save(ConfigurationSaveMode.Modified);
        }
    }

    /// <summary>
    /// Класс для работы с базой данных Microsoft Access
    /// </summary>
    class Dbaccess : Param_in
    {
        /// <summary>
        /// Создание списка для данных из базы
        /// </summary>
        private List<string[]> querydata = new List<string[]>();

        /// <summary>
        /// Свойство querydata
        /// </summary>
        public List<string[]> Querydata
        {
            get { return querydata; }
        }

        /// <summary>
        /// Метод добавляющий данные из базы данных в список 
        /// </summary>
        /// <param name="query">Запрос к базе данных</param>
        public void Dbselect(string query = "")
        {
            OleDbConnection con = new OleDbConnection();
            try
            {
                querydata.Clear();
                con.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + GetDirDB() + ";Jet OLEDB:Database Password=derpassword";
                con.Open();
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = con;
                cmd.CommandText = "" + query + "";
                OleDbDataReader reader = cmd.ExecuteReader();
                while(reader.Read())
                {
                    string[] vs = new string[reader.FieldCount];
                    for (int i = 0; i < reader.FieldCount; ++i)
                    { vs[i] = reader[i].ToString(); }
                    querydata.Add(vs);
                }

            }
            catch (Exception e) { MessageBox.Show("Проверьте правильность запроса." + e, "Ошибка"); }
            con.Close();
        }

        /// <summary>
        /// Метод добавляющий данные из базы данных с условием в список
        /// </summary>
        /// <param name="list">Список названий столбцов через запятую данные которых будут передаваться в список, по умолчанию "*"(все столбцы)</param>
        /// <param name="what">Столбец для поиска условия</param>
        /// <param name="tab">Название таблицы в базе</param>
        /// <param name="where">Условие</param>
        public void Dbselectset(string list = "*", string what = "", string tab = "", string where = "")
        {
            OleDbConnection con = new OleDbConnection();
            try
            {
                querydata.Clear();
                con.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + GetDirDB() + ";Jet OLEDB:Database Password=derpassword";
                con.Open();
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = con;
                cmd.CommandText = "SELECT '" + list + "' FROM '" + tab + "' WHERE '" + what + "'='" + where + "'";
                OleDbDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string[] vs = new string[reader.FieldCount];
                    for (int i = 0; i < reader.FieldCount; ++i)
                    { vs[i] = reader[i].ToString(); }
                    querydata.Add(vs);
                }
            }
            catch (Exception e) { MessageBox.Show("Проверьте правильность запроса." + e, "Ошибка"); }
            con.Close();
        }

        /// <summary>
        /// Метод добавляющий данные в таблицу базы данных
        /// </summary>
        /// <param name="tab">Название таблицы в базе</param>
        /// <param name="list1">Список названий столбцов через запятую, куда будут добавляться данные</param>
        /// <param name="list2">Список данных через запятую(количество данных должно совпадать с количеством переданных столбцов в list1)</param>
        public void Dbinsert(string tab, string list1 = "", string list2 = "")
        {
            OleDbConnection con = new OleDbConnection();
            try
            {
                querydata.Clear();
                con.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + GetDirDB() + ";Jet OLEDB:Database Password=derpassword";
                con.Open();
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = con;
                cmd.CommandText = "INSERT INTO [" + tab + "] (" + list1 + ") VALUES (" + list2 + ")";
                cmd.ExecuteNonQuery();
            }
            catch (Exception e) { MessageBox.Show("Проверьте правильность запроса." + e, "Ошибка"); }
            con.Close();
        }

        /// <summary>
        /// Метод создающий таблицы в базе данных
        /// </summary>
        /// <param name="tab">Название таблицы</param>
        /// <param name="list">Параметры создаваемых столбцов</param>
        public void Dbincreate(string tab, string list)
        {
            OleDbConnection con = new OleDbConnection();
            try
            {
                con.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + GetDirDB() + ";Jet OLEDB:Database Password=derpassword";
                con.Open();
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = con;
                cmd.CommandText = "CREATE TABLE [" + tab + "] (" + list + ")";
                cmd.ExecuteNonQuery();
            }
            catch (Exception e) { MessageBox.Show("Файл с таким индексом уже существует." + e, "Ошибка"); }
            con.Close();
        }

        /// <summary>
        /// Метод принимающий запрос в базу данных для транзакрии не возвращающей значения
        /// </summary>
        /// <param name="query">Запрос</param>
        public void DbRead(string query)
        {
            OleDbConnection con = new OleDbConnection();
            try
            {
                querydata.Clear();
                con.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + GetDirDB() + ";Jet OLEDB:Database Password=derpassword";
                con.Open();
                OleDbCommand cmd = new OleDbCommand();
                cmd.Connection = con;
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
            }
            catch (Exception e) { MessageBox.Show("Проверьте правильность запроса." + e, "Ошибка"); }
            con.Close();
        }
    }
}
