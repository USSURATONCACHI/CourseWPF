using CourseWPF.ViewModel;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Windows;

namespace CourseWPF.Commands.CreateProject.Points {
    public class OpenPointsDbCommand : BaseCommand {

        private CreateProjectViewModel _viewmodel;

        public OpenPointsDbCommand(CreateProjectViewModel vm) {
            _viewmodel = vm;
            _viewmodel.PropertyChanged += _viewmodel_PropertyChanged;
        }

        private void _viewmodel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(CreateProjectViewModel.PointsTableName) ||
                e.PropertyName == nameof(CreateProjectViewModel.AvailablePoints))
                InvokeCanExecuteChanged();
        }

        public override bool CanExecute(object? parameter) {
            return _viewmodel.PointsTableName != "" && 
                _viewmodel.AvailablePoints.Count > 0 &&
                base.CanExecute(parameter);
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

            try {
                using (SQLiteConnection conn = new($"Data Source={dialog.FileName};Version=3;")) {
                    conn.Open();

                    List<(double, double, int)> points = new();
                    List<int> blockIds = new();

                    string query = $"SELECT X, Y, [Блок] FROM [{_viewmodel.PointsTableName}]";
                    using SQLiteCommand command = new(query, conn);
                    using SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.HasRows && reader.Read()) {
                        double x = reader.GetDouble(0);
                        double y = reader.GetDouble(1);
                        int blockId = reader.GetInt32(2);
                        points.Add((x, y, blockId));

                        if (blockId >= 0 && !blockIds.Contains(blockId))
                            blockIds.Add(blockId);
                    }

                    blockIds.Sort();

                    for (int i = 0; i < points.Count && i < _viewmodel.AvailablePoints.Count; i++) {
                        var point = points[i];

                        _viewmodel.AvailablePoints[i] = new CreateProjectViewModel.Point() {
                            PointId = i + 1,
                            X = point.Item1,
                            Y = point.Item2,
                            BlockId = blockIds.IndexOf(point.Item3),
                        };
                    }

                    for (int i = _viewmodel.BlocksCount; i < 30 && i < blockIds.Count; i++)
                        _viewmodel.AddBlock.Execute(null);

                    for (int i = _viewmodel.BlocksCount - 1; i >= blockIds.Count; i--)
                        _viewmodel.RemoveBlock.Execute(null);
                }
            } catch (Exception e) {
                MessageBox.Show(e.Message, "Не удалось открыть файл", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
    }
}
