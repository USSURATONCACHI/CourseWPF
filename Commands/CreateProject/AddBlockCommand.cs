using CourseWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWPF.Commands.CreateProject {
    internal class AddBlockCommand : BaseCommand {
        private CreateProjectViewModel _viewModel;

        public AddBlockCommand(CreateProjectViewModel vm) {
            _viewModel = vm;
            _viewModel.PropertyChanged += _viewModel_PropertyChanged;
        }

        private void _viewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(CreateProjectViewModel.BlocksCount))
                InvokeCanExecuteChanged();
        }

        public override bool CanExecute(object? parameter) {
            return  _viewModel.BlocksCount < CreateProjectViewModel.MaxBlocks && base.CanExecute(parameter);
        }

        public override void Execute(object? parameter) {
            _viewModel.AvailableBlocks.Add(CreateProjectViewModel.GetBlockName(_viewModel.AvailableBlocks.Count));
        }
    }
}
