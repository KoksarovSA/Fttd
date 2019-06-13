using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public AllFiles(string detail_name, string detail_index, string project, string file_type, string file_dir_out)
        {
            Detail_name = detail_name ?? throw new ArgumentNullException(nameof(detail_name));
            Detail_index = detail_index ?? throw new ArgumentNullException(nameof(detail_index));
            Project = project ?? throw new ArgumentNullException(nameof(project));
            File_type = file_type ?? throw new ArgumentNullException(nameof(file_type));
            File_dir_out = file_dir_out ?? throw new ArgumentNullException(nameof(file_dir_out));
        }

        public string Detail_name { get; set; }
        public string Detail_index { get; set; }
        public string Project { get; set; }
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
                    case "Задание": dir = @"" + Param_in.DirFiles + "\\" + Project + "\\" + File_type + "\\" + File_name; break;
                    case "График": dir = @"" + Param_in.DirFiles + "\\" + File_type + "\\" + File_name; break;
                    case "Служебная": dir = @"" + Param_in.DirFiles + "\\" + File_type + "\\" + File_name; break;
                    case "Приспособления": dir = @"" + Param_in.DirFiles + "\\" + File_type + "\\" + File_name; break;
                    default: dir = @"" + Param_in.DirFiles + "\\" + Project + "\\" + Detail_name + "_" + Detail_index + "\\" + File_name; break;
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
                    case "Задание": dir = @"\\" + Project + "\\" + File_type + "\\" + File_name; break;
                    case "График": dir = @"\\" + File_type + "\\" + File_name; break;
                    case "Служебная": dir = @"\\" + File_type + "\\" + File_name; break;
                    case "Приспособления": dir = @"\\" + File_type + "\\" + File_name; break;
                    default: dir = @"\\" + Project + "\\" + Detail_name + "_" + Detail_index + "\\" + File_name; break;
                }
                return dir;
            }
        }
        public string File_dir_out { get; set; }

        public void Copy_file()
        {
            if (File.Exists(File_dir_in))
            {
                System.Windows.Forms.MessageBox.Show(File_dir_in + "Файл уже есть в базе, он будет привязан к заданию.", "Ошибка");
            }
            else
            {
                Directory.CreateDirectory(Path.GetDirectoryName(File_dir_in));
                File.Copy(File_dir_out, File_dir_in);
            }
        }
    }
}
