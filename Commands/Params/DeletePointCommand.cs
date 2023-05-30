using CourseWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWPF.Commands.Params {
    public class DeletePointCommand : BaseCommand {
        private ParamsViewModel _viewModel;
        private int _pointId;

        public DeletePointCommand(ParamsViewModel viewModel, int pointId) {
            _viewModel = viewModel;
            _pointId = pointId;
            _viewModel.Points.CollectionChanged += (sender, args) => InvokeCanExecuteChanged();
            _viewModel.PropertyChanged += _viewModel_PropertyChanged;
        }

        private void _viewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(ParamsViewModel.Points)) {
                _viewModel.Points.CollectionChanged += (sender, args) => InvokeCanExecuteChanged();
                InvokeCanExecuteChanged();
            }
        }

        public override bool CanExecute(object? parameter) {
            return _viewModel.Points.Count > 1 && base.CanExecute(parameter);
        }

        public override void Execute(object? parameter) {
            _viewModel.Points.RemoveAt(_pointId);

            foreach (var point in _viewModel.Points.Skip(_pointId)) {
                point.PointId--;

                if (point.DeleteCommand is not null)
                    point.DeleteCommand._pointId--;
            }
            
            _viewModel.HasChanges = true;
        }
    }
}
