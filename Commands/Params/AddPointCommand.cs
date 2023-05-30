using CourseWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWPF.Commands.Params {
    internal class AddPointCommand : BaseCommand {
        private ParamsViewModel _viewModel;

        public AddPointCommand(ParamsViewModel vm) {
            _viewModel = vm;
        }
        
        public override void Execute(object? parameter) {
            int pointId = _viewModel.Points.Count + 1;

            _viewModel.Points.Add(new PointViewModel() {
                PointId = pointId,
                X = 0,
                Y = 0,
                BlockId = -1,
                DeleteCommand = new DeletePointCommand(_viewModel, pointId - 1)
            });

            _viewModel.HasChanges = true;
        }
    }
}
