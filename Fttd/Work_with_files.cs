using System.Configuration;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Data.OleDb;

namespace Fttd
{
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
        private static string dirDb;
        private static string dirFiles;

        public static string DirDb
        {
            get { return dirDb; }
            set { dirDb = value; }
        }

        public static string DirFiles
        {
            get { return dirFiles; }
            set { dirFiles = value; }
        }

        /// <summary>
        /// Метод для получения последнего числа бэкапа
        /// </summary>
        /// <returns>Возвращает число месяца последнего бэкапа</returns>
        public static string GetFTTDBackup()
        {
            string day = ConfigurationManager.AppSettings.Get("FTTDBackup");
            return day;
        }

        /// <summary>
        /// Метод записи последнего числа бэкапа
        /// </summary>
        /// <param name="txt_dir">Число месяца последнего бэкапа</param>
        public static void SetFTTDBackup(string day)
        {
            var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            var entry = config.AppSettings.Settings["FTTDBackup"];
            if (entry == null)
                config.AppSettings.Settings.Add("FTTDBackup", day);
            else
                config.AppSettings.Settings["FTTDBackup"].Value = day;

            config.Save(ConfigurationSaveMode.Modified);
        }

        /// <summary>
        /// Метод для получения директории файла базы данных базовой директории файлов
        /// </summary>
        /// <returns>Возвращает директорию файла базы данных</returns>
        public static void GetDirDB()
        {
            string dirDb = ConfigurationManager.AppSettings.Get("DirDB");
            DirDb = dirDb;
        }

        /// <summary>
        /// Метод записи в конфигурацию программы директорию файла базы данных
        /// </summary>
        /// <param name="txt_dir">Директория файла базы данных</param>
        public static void SetDirDB(string txt_dir)
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
        public static void GetDirFiles()
        {
            string dirDirFiles = ConfigurationManager.AppSettings.Get("DirFiles");
            DirFiles = dirDirFiles;
        }
        
        /// <summary>
        /// Метод изменения базовой директории в конфигурации программы
        /// </summary>
        /// <param name="txt_dir">Базовая директория папки</param>
        public static void SetDirFiles(string txt_dir)
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
    class Dbaccess
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
        public void Db2select(string query = "")
        {
            OleDbConnection con1 = new OleDbConnection();
            try
            {
                querydata.Clear();
                con1.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Param_in.DirDb + ";Jet OLEDB:Database Password=derpassword";
                con1.Open();
                OleDbCommand cmd1 = new OleDbCommand();
                cmd1.Connection = con1;
                cmd1.CommandText = "" + query + "";
                OleDbDataReader reader1 = cmd1.ExecuteReader();
                while (reader1.Read())
                {
                    string[] vs = new string[reader1.FieldCount];
                    for (int i = 0; i < reader1.FieldCount; ++i)
                    { vs[i] = reader1[i].ToString(); }
                    querydata.Add(vs);
                }

            }
            catch (Exception e) { MessageBox.Show("Проверьте правильность запроса." + e, "Ошибка"); }
            con1.Close();
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
                con.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Param_in.DirDb + ";Jet OLEDB:Database Password=derpassword";
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
                con.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Param_in.DirDb + ";Jet OLEDB:Database Password=derpassword";
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
        public void Dbinsert(string tab, string list1, string list2)
        {
            OleDbConnection con = new OleDbConnection();
            try
            {
                querydata.Clear();
                con.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Param_in.DirDb + ";Jet OLEDB:Database Password=derpassword";
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
        /// Метод добавляющий bool данные в таблицу базы данных
        /// </summary>
        /// <param name="tab">Название таблицы в базе</param>
        /// <param name="list1">Список названий столбцов через запятую, куда будут добавляться данные</param>
        /// <param name="list2">Список данных через запятую(количество данных должно совпадать с количеством переданных столбцов в list1)</param>
        public void Dbinsertbool(string tab, string list1, bool list2)
        {
            OleDbConnection con = new OleDbConnection();
            try
            {
                querydata.Clear();
                con.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Param_in.DirDb + ";Jet OLEDB:Database Password=derpassword";
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
                con.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Param_in.DirDb + ";Jet OLEDB:Database Password=derpassword";
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
                con.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + Param_in.DirDb + ";Jet OLEDB:Database Password=derpassword";
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
