using CourseWPF.Model;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CourseWPF.Commands
{
    public class ChangeImageCommand : BaseCommand {
        private Project _project;
        public ChangeImageCommand(Project project) {
            _project = project;
        }

        public override void Execute(object? parameter) {
            var dialog = new OpenFileDialog() {
                Filter = "*.png|*.png"
            };

            if (!(dialog.ShowDialog() ?? false))
                return;

            if (!File.Exists(dialog.FileName)) {
                MessageBox.Show("Файл не найден", "Ошибка ;(", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var newFile = dialog.FileName;

            if (_project.ImagePath == newFile || _project.SavePath == "") {
                _project.InvokeImageChanged();
                return;
            }

            string newPath;
            string newFileName = newFile.Split(new char[] { '/', '\\' }).Last();

            if (_project.SavePath.EndsWith("/") || _project.SavePath.EndsWith("\\"))
                newPath = $"{_project.SavePath}{newFileName}";
            else
                newPath = $"{_project.SavePath}/{newFileName}";


            _project.ImagePath = "";
            _project.InvokeImageChanged();

            CleanDirFromImages(_project.SavePath);
            try { File.Copy(newFile, newPath); } catch (Exception) { }

            _project.ImagePath = newPath;
            _project.InvokeImageChanged();

            // Select file
            // Load png
            // Delete old file

            // Save png
            // Update value
        }

        public static void CleanDirFromImages(string directory) {
            foreach (var file in Directory.GetFiles(directory)) {
                if (file.EndsWith(".png"))
                    File.Delete(file);
            }
        }
    }
}
