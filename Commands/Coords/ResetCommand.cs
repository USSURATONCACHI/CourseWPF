using CourseWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWPF.Commands.Coords {
    public class ResetCommand : BaseCommand {
        private CoordsViewModel _viewModel;

        public ResetCommand(CoordsViewModel viewModel) {
            _viewModel = viewModel;
        }

        public override void Execute(object? parameter) => _viewModel.Reset();
    }
}
