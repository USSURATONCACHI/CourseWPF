using CourseWPF.Model;
using CourseWPF.Stores;
using CourseWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWPF.Commands.Params
{
    internal class ParamsResetCommand : BaseCommand
    {
        private ParamsViewModel _paramsViewModel;
        private Project _project;

        public ParamsResetCommand(ParamsViewModel vm, Project project) {
            _paramsViewModel = vm;
            _project = project;

            _paramsViewModel.PropertyChanged += _paramsViewModel_PropertyChanged;
        }

        private void _paramsViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(ParamsViewModel.IsResponsive))
                InvokeCanExecuteChanged();
        }

        public override bool CanExecute(object? parameter)
        {
            return _paramsViewModel.IsResponsive && base.CanExecute(parameter);
        }

        public override void Execute(object? parameter) {
            _paramsViewModel.IsResponsive = false;

            Project project = _project;

            _paramsViewModel.TrustFactor = project.TrustFactor;
            _paramsViewModel.ErrorFactor = project.ErrorFactor;
            _paramsViewModel.BlocksCount = project.GetAllBlocks().Count();

            var vmPoints = _paramsViewModel.Points;

            for (int i = 0; i < project.PointsCount; i++)
            {
                if (i >= vmPoints.Count) {
                    PointViewModel newPoint = new();
                    newPoint.PropertyChanged += (sender, args) => { _paramsViewModel.HasChanges = true; };
                    vmPoints.Add(newPoint);
                }

                var point = vmPoints[i];
                point.PointId = i + 1;
                (point.X, point.Y) = project.GetPointPos(i);
                point.BlockId = project.GetPointBlockId(i) ?? -1;
                point.DeleteCommand = new DeletePointCommand(_paramsViewModel, i);
                
            }

            for (int i = vmPoints.Count - 1; i >= project.PointsCount; i--)
                vmPoints.RemoveAt(i);


            _paramsViewModel.IsResponsive = true;
            _paramsViewModel.HasChanges = false;
            _paramsViewModel.HasForeignChanges = false;
        }
    }
}
