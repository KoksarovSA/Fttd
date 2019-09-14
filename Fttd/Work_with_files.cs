using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Windows;

namespace Fttd
{
    internal class Table
    {
        public string Name { get; set; }
        public string Type { get; set; }
    }

    /// <summary>
    /// Класс для работы с базой данных Microsoft Access
    /// </summary>
    internal class Dbaccess
    {
        public Dbaccess()
        {
            Querydata = new List<string[]>();
        }

        public List<string[]> Querydata { get; private set; }

        /// <summary>
        /// Метод добавляющий данные из базы данных в список 
        /// </summary>
        /// <param name="query">Запрос к базе данных</param>
        public void Db2select(string query = "")
        {
            using (OleDbConnection con1 = new OleDbConnection())
            {
                try
                {
                    Querydata.Clear();
                    con1.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + State.DirDb + ";Jet OLEDB:Database Password=derpassword";
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
                        Querydata.Add(vs);
                    }

                }
                catch (Exception e) { MessageBox.Show("Проверьте правильность запроса." + e, "Ошибка"); }
                con1.Close();
            }


        }

        /// <summary>
        /// Метод добавляющий данные из базы данных в список 
        /// </summary>
        /// <param name="query">Запрос к базе данных</param>
        public void Dbselect(string query = "")
        {
            using (OleDbConnection con = new OleDbConnection())
            {
                try
                {
                    Querydata.Clear();
                    con.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + State.DirDb + ";Jet OLEDB:Database Password=derpassword";
                    con.Open();
                    OleDbCommand cmd = new OleDbCommand();
                    cmd.Connection = con;
                    cmd.CommandText = "" + query + "";
                    OleDbDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        string[] vs = new string[reader.FieldCount];
                        for (int i = 0; i < reader.FieldCount; ++i)
                        { vs[i] = reader[i].ToString(); }
                        Querydata.Add(vs);
                    }

                }
                catch (Exception e) { MessageBox.Show("Проверьте правильность запроса." + e, "Ошибка"); }
                con.Close();
            }
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
            using (OleDbConnection con = new OleDbConnection())
            {
                try
                {
                    Querydata.Clear();
                    con.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + State.DirDb + ";Jet OLEDB:Database Password=derpassword";
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
                        Querydata.Add(vs);
                    }
                }
                catch (Exception e) { MessageBox.Show("Проверьте правильность запроса." + e, "Ошибка"); }
                con.Close();
            }
        }

        /// <summary>
        /// Метод добавляющий данные в таблицу базы данных
        /// </summary>
        /// <param name="tab">Название таблицы в базе</param>
        /// <param name="list1">Список названий столбцов через запятую, куда будут добавляться данные</param>
        /// <param name="list2">Список данных через запятую(количество данных должно совпадать с количеством переданных столбцов в list1)</param>
        public void Dbinsert(string tab, string list1, string list2)
        {
            using (OleDbConnection con = new OleDbConnection())
            {
                try
                {
                    Querydata.Clear();
                    con.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + State.DirDb + ";Jet OLEDB:Database Password=derpassword";
                    con.Open();
                    OleDbCommand cmd = new OleDbCommand();
                    cmd.Connection = con;
                    cmd.CommandText = "INSERT INTO [" + tab + "] (" + list1 + ") VALUES (" + list2 + ")";
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e) { MessageBox.Show("Проверьте правильность запроса." + e, "Ошибка"); }
                con.Close();
            }
        }

        /// <summary>
        /// Метод добавляющий bool данные в таблицу базы данных
        /// </summary>
        /// <param name="tab">Название таблицы в базе</param>
        /// <param name="list1">Список названий столбцов через запятую, куда будут добавляться данные</param>
        /// <param name="list2">Список данных через запятую(количество данных должно совпадать с количеством переданных столбцов в list1)</param>
        public void Dbinsertbool(string tab, string list1, bool list2)
        {
            using (OleDbConnection con = new OleDbConnection())
            {
                try
                {
                    Querydata.Clear();
                    con.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + State.DirDb + ";Jet OLEDB:Database Password=derpassword";
                    con.Open();
                    OleDbCommand cmd = new OleDbCommand();
                    cmd.Connection = con;
                    cmd.CommandText = "INSERT INTO [" + tab + "] (" + list1 + ") VALUES (" + list2 + ")";
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e) { MessageBox.Show("Проверьте правильность запроса." + e, "Ошибка"); }
                con.Close();
            }
        }

        /// <summary>
        /// Метод создающий таблицы в базе данных
        /// </summary>
        /// <param name="tab">Название таблицы</param>
        /// <param name="list">Параметры создаваемых столбцов</param>
        public void Dbincreate(string tab, string list)
        {
            using (OleDbConnection con = new OleDbConnection())
            {
                try
                {
                    con.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + State.DirDb + ";Jet OLEDB:Database Password=derpassword";
                    con.Open();
                    OleDbCommand cmd = new OleDbCommand();
                    cmd.Connection = con;
                    cmd.CommandText = "CREATE TABLE [" + tab + "] (" + list + ")";
                    cmd.ExecuteNonQuery();
                }
                catch (Exception e) { MessageBox.Show("Файл с таким индексом уже существует." + e, "Ошибка"); }
                con.Close();
            }
        }

        /// <summary>
        /// Метод принимающий запрос в базу данных для транзакрии не возвращающей значения
        /// </summary>
        /// <param name="query">Запрос</param>     
        public void DbRead(string query)
        {
            using (OleDbConnection con = new OleDbConnection())
            {
                try
                {
                    Querydata.Clear();
                    con.ConnectionString = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + State.DirDb + ";Jet OLEDB:Database Password=derpassword";
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
}
