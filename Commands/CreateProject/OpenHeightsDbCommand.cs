using CourseWPF.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CourseWPF.Commands.CreateProject {
    public class OpenHeightsDbCommand : BaseCommand {
        private CreateProjectViewModel _viewmodel;

        public OpenHeightsDbCommand(CreateProjectViewModel vm) {
            _viewmodel = vm;
        }

        public override void Execute(object? parameter) {
            var dialog = new OpenFileDialog() {
                Filter = "*.sqlite|*.sqlite"
            };

            if (!(dialog.ShowDialog() ?? false))
                return;

            if (!File.Exists(dialog.FileName)) {
                MessageBox.Show("Файл не найден", "Ошибка ;(", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Run db fix

            try {
                using (SQLiteConnection conn = new($"Data Source={dialog.FileName};Version=3;")) {
                    conn.Open();
                    var tables = GetTables(conn);
                    _viewmodel.AvailableDbTables = new ObservableCollection<string>(tables);
                    _viewmodel.SelectedTableIndex = tables.Count > 1 ? 1 : 0;
                    _viewmodel.DatabasePath = dialog.FileName;
                }
            } catch (Exception e) {
                MessageBox.Show(e.Message, "Не удалось открыть файл", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        private List<string> GetTables(SQLiteConnection conn) {
            string query = "SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY 1;";
            List<string> result = new();

            using (SQLiteCommand command = new(query, conn)) {
                using (SQLiteDataReader reader = command.ExecuteReader()) {
                    while (reader.HasRows && reader.Read())
                        result.Add(reader.GetString(0));
                }
            }

            return result;
        }
    }
}
