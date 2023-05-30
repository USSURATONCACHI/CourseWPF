using CourseWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CourseWPF.Commands.CreateProject {
    public class LoadTableCommand : BaseCommand {
        private CreateProjectViewModel _viewModel;

        public LoadTableCommand(CreateProjectViewModel vm) {
            _viewModel = vm;
            _viewModel.PropertyChanged += _viewModel_PropertyChanged;
        }

        private void _viewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(CreateProjectViewModel.AvailableDbTables) ||
                e.PropertyName == nameof(CreateProjectViewModel.SelectedTableIndex) ||
                e.PropertyName == nameof(CreateProjectViewModel.DatabasePath))
                InvokeCanExecuteChanged();
        }

        public override bool CanExecute(object? parameter) {
            return File.Exists(_viewModel.DatabasePath) &&
                _viewModel.SelectedTableIndex < _viewModel.AvailableDbTables.Count &&
                _viewModel.SelectedTableIndex >= 0 &&
                _viewModel.AvailableDbTables[_viewModel.SelectedTableIndex] != "" &&
                base.CanExecute(parameter);
        }

        public override void Execute(object? parameter) {
            string filename = _viewModel.DatabasePath;
            string tableName = _viewModel.AvailableDbTables[_viewModel.SelectedTableIndex];


            if (!File.Exists(filename)) {
                MessageBox.Show("Файл не найден", "Ошибка ;(", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try {
                List<List<double>> values = new();
                int pointsCount = 0;

                using (SQLiteConnection conn = new($"Data Source={filename};Version=3;")) {
                    conn.Open();
                    string query = $"SELECT * FROM [{tableName}] ORDER BY [Эпоха];";

                    using SQLiteCommand command = new(query, conn);
                    using SQLiteDataReader reader = command.ExecuteReader();
                    while (reader.HasRows && reader.Read()) {
                        pointsCount = reader.FieldCount - 1;

                        values.Add(new List<double>());
                        
                        for (int i = 0; i < pointsCount; i++)
                            values.Last().Add(reader.GetDouble(i + 1));
                    }

                    if (pointsCount <= 0)
                        throw new Exception("Таблица не содержит данных");

                    // =====
                    DataTable table = new();
                    table.Columns.Add(new DataColumn() { ColumnName = "Эпоха", ReadOnly = true });
                    for (int i = 0; i < pointsCount; i++)
                        table.Columns.Add(new DataColumn() { ColumnName = $"{i + 1}", ReadOnly = true }); ; ;

                    foreach ((var epoch, int epochId) in values.Select((e, i) => (e, i))) {
                        var newRow = table.NewRow();
                        newRow[0] = epochId;

                        foreach ((var z, int i) in epoch.Select((z, i) => (z, i)))
                            newRow[i + 1] = z;

                        table.Rows.Add(newRow);
                    }
                    _viewModel.HeightsTable = table;
                    _viewModel.RawEpochsData = values;

                    IEnumerable<CreateProjectViewModel.Point> points = 
                        Enumerable.Range(1, pointsCount)
                            .Select(pointId => new CreateProjectViewModel.Point { PointId = pointId, X = 0, Y = 0, BlockId = -1 });

                    // ObservableCollection's undefined behaviour, idk?
                    do {
                        _viewModel.AvailablePoints = new ObservableCollection<CreateProjectViewModel.Point>(points.ToList());
                    } while (_viewModel.AvailablePoints.Count != points.Count());
                }
            } catch (Exception e) {
                MessageBox.Show(e.Message, "Не удалось открыть файл", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
    }
}
