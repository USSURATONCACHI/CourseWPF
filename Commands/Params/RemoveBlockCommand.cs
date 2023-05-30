using CourseWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWPF.Commands.Params {
    public class RemoveBlockCommand  : BaseCommand {
        private ParamsViewModel _viewModel;

        public RemoveBlockCommand(ParamsViewModel vm) {
            _viewModel = vm;
            _viewModel.PropertyChanged += _viewModel_PropertyChanged;
        }

        private void _viewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(ParamsViewModel.BlocksCount))
                InvokeCanExecuteChanged();
        }

        public override bool CanExecute(object? parameter) {
            return _viewModel.BlocksCount > 0 && base.CanExecute(parameter);
        }

        public override void Execute(object? parameter) {
            _viewModel.BlocksCount--;
            _viewModel.HasChanges = true;
        }
    }
}
