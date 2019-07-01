using System;
using System.IO;
using System.Linq;

namespace Fttd.Entities
{
    class AllFiles
    {
        public AllFiles()
        {
        }

        public AllFiles(string file_type, string file_dir_out)
        {
            File_type = file_type ?? throw new ArgumentNullException(nameof(file_type));
            File_dir_out = file_dir_out ?? throw new ArgumentNullException(nameof(file_dir_out));
        }

        public AllFiles(string detail_name, string detail_index, string file_type, string file_dir_out, string file_note)
        {
            Detail_name = detail_name ?? throw new ArgumentNullException(nameof(detail_name));
            Detail_index = detail_index ?? throw new ArgumentNullException(nameof(detail_index));
            File_note = file_note ?? throw new ArgumentNullException(nameof(file_note));
            File_type = file_type ?? throw new ArgumentNullException(nameof(file_type));
            File_dir_out = file_dir_out ?? throw new ArgumentNullException(nameof(file_dir_out));
        }

        public string Detail_name { get; set; }
        public string Detail_index { get; set; }
        public string File_note { get; set; }
        public string Project
        {
            get
            {
                string pr = State.detailColl.First(item => item.Index == Detail_index).Project ?? null;
                return pr;
            }
        }
        public string File_name
        {
            get
            {
                if (File_dir_out != null && File_dir_out != "") { return new DirectoryInfo(File_dir_out).Name; }
                else System.Windows.Forms.MessageBox.Show("Укажите файл"); return null;
            }
        }
        public string File_type { get; set; }
        public string File_dir_in
        {
            get
            {
                string dir;
                switch (File_type)
                {
                    case "Задания": dir = @"" + Param_in.DirFiles + "\\" + File_type + "\\" + File_name; break;
                    case "Графики": dir = @"" + Param_in.DirFiles + "\\" + File_type + "\\" + File_name; break;
                    case "Документы": dir = @"" + Param_in.DirFiles + "\\" + File_type + "\\" + File_name; break;
                    case "Приспособления": dir = @"" + Param_in.DirFiles + "\\" + File_type + "\\" + Detail_index + "\\" + File_name; break;
                    default:
                        {
                            if (File_dir_out.Contains("Задания"))
                            {
                                dir = File_dir_out;
                            }
                            else dir = @"" + Param_in.DirFiles + "\\" + Project + "\\" + Detail_name + "_" + Detail_index + "\\" + File_name;
                            break;
                        }
                }
                return dir;
            }
        }
        public string File_dir_in_short
        {
            get
            {
                string dir;
                switch (File_type)
                {
                    case "Задания": dir = @"\\" + File_type + "\\" + File_name; break;
                    case "Графики": dir = @"\\" + File_type + "\\" + File_name; break;
                    case "Документы": dir = @"\\" + File_type + "\\" + File_name; break;
                    case "Приспособления": dir = @"\\" + File_type + "\\" + Detail_index + "\\" + File_name; break;
                    default:
                        {
                            if (File_dir_out.Contains("Задания"))
                            {
                                dir = File_dir_out;
                            }
                            else dir = @"\\" + Project + "\\" + Detail_name + "_" + Detail_index + "\\" + File_name; break;
                        }
                }
                return dir;
            }
        }
        public string File_dir_out { get; set; }

        public bool Copy_file()
        {
            if (File.Exists(File_dir_in))
            {
                System.Windows.Forms.MessageBox.Show(File_dir_in + "Файл уже есть в базе, он будет привязан к заданию.", "Ошибка");
                return false;
            }
            else
            {
                if (File_type == "Детали" && File_dir_out.Contains("Задания"))
                {
                    return true;
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(File_dir_in));
                    File.Copy(File_dir_out, File_dir_in);
                    return true;
                }

            }
        }
    }
}
