using CourseWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWPF.Commands.Coords {
    public class DeleteRowCommand : BaseCommand {
        private CoordsViewModel _viewModel;

        public DeleteRowCommand(CoordsViewModel viewModel) {
            _viewModel = viewModel;
            _viewModel.PropertyChanged += _viewModel_PropertyChanged;
            _viewModel.DataTable.RowChanged += (sender, args) => InvokeCanExecuteChanged();
        }

        private void _viewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(CoordsViewModel.DataTable)) {
                if (_viewModel.DataTable is not null)
                    _viewModel.DataTable.RowChanged += (sender, args) => InvokeCanExecuteChanged();
                InvokeCanExecuteChanged();
            }
        }

        public override bool CanExecute(object? parameter) {
            if (_viewModel.DataTable is null)
                return false;

            return _viewModel.DataTable.Rows.Count > 2 && base.CanExecute(parameter);
        }

        public override void Execute(object? parameter) {
            int count = _viewModel.DataTable.Rows.Count;
            if (count > 2)
                _viewModel.DataTable.Rows.RemoveAt(count - 1);
        }
    }
}
