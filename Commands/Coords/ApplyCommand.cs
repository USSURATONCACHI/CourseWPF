using CourseWPF.Model;
using CourseWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace CourseWPF.Commands.Coords {
    public class ApplyCommand : BaseCommand {
        private CoordsViewModel _viewModel;
        private Project _project;
        private bool _isAvailable;

        public ApplyCommand(CoordsViewModel viewmodel, Project project) {
            _viewModel = viewmodel;
            _isAvailable = true;
            _project = project;
            _viewModel.PropertyChanged += _viewModel_PropertyChanged; ;
            _viewModel.DataTable.RowChanged += (sender, args) => InvokeCanExecuteChanged();
        }

        private void _viewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(CoordsViewModel.DataTable)) {
                if (_viewModel.DataTable != null)
                    _viewModel.DataTable.RowChanged += (sender, args) => InvokeCanExecuteChanged();
                InvokeCanExecuteChanged();
            }
        }

        public override bool CanExecute(object? parameter) {
            if (_viewModel.DataTable is null)
                return false;

            return _isAvailable && _viewModel.DataTable.Rows.Count >= 2 && base.CanExecute(parameter);
        }

        public override void Execute(object? parameter) {
            var rows = _viewModel.DataTable.Rows;

            for (int rowId = 0; rowId < rows.Count; rowId++) {
                DataRow row = _viewModel.DataTable.Rows[rowId];
                List<double> values = new();

                foreach (DataColumn col in _viewModel.DataTable.Columns)
                    if (col.Caption != "Эпоха")
                        values.Add(double.Parse((row[col]?.ToString() ?? "0").Replace(",", ".")));

                if (values.Count != _project.PointsCount)
                    throw new Exception($"This is very bad, i hate datagrid. ({values.Count}/{_project.PointsCount})");

                if (_project.Epochs.Count <= rowId) {
                    _project.AddEpoch(rowId, values);
                } else {
                    bool equals = true;
                    foreach ((double a, double b) in _project.GetEpoch(rowId).Zip(values))
                        if (a != b)
                            equals = false;

                    if (!equals)
                        _project.SetEpoch(rowId, values);
                }
            }

            for (int rowId = _project.EpochsCount - 1; rowId >= rows.Count; rowId--)
                _project.RemoveEpoch(rowId);

            _viewModel.HasChanges = false;
            _viewModel.HasForeignChanges = false;
        }

        public void SetAvailable(bool available) {
            if (_isAvailable == available) return;

            _isAvailable = available;
            InvokeCanExecuteChanged();
        }
    }
}
