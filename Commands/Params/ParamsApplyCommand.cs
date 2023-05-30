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
    internal class ParamsApplyCommand : BaseCommand
    {
        private ParamsViewModel _paramsViewModel;
        private Project _project;

        public ParamsApplyCommand(ParamsViewModel vm, Project project)
        {
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

        public override void Execute(object? parameter)
        {
            _paramsViewModel.IsResponsive = false;

            Project project = _project;
            project.SetTrustFactor(_paramsViewModel.TrustFactor);
            project.SetErrorFactor(_paramsViewModel.ErrorFactor);

            for (int i = project.Blocks.Count; i < _paramsViewModel.BlocksCount; i++)
                project.AddBlock(i);

            for (int i = project.Blocks.Count - 1; i >= _paramsViewModel.BlocksCount; i--)
                project.RemoveBlock(i);


            var vmPoints = _paramsViewModel.Points;
            List<List<int>> blocks = Enumerable
                .Range(0,_paramsViewModel.BlocksCount)
                .Select( i => new List<int>() )
                .ToList();

            foreach ((var point, int pointId) in vmPoints.Select((p, i) => (p, i)))
            {
                if (pointId >= project.PointsCount)
                    project.AddPoint(pointId);

                if (project.GetPointPos(pointId) != (point.X, point.Y))
                    project.SetPointPos(pointId, (point.X, point.Y));

                int? actualBlockId = point.BlockId >= 0 && point.BlockId < _paramsViewModel.BlocksCount ? point.BlockId : null;
                if (actualBlockId is int abi)
                    blocks[abi].Add(pointId);
            }

            // Update blocks
            for (int i = 0; i < blocks.Count; i++) {
                var projectBlock = project.GetBlock(i);
                bool eq = projectBlock.Count == blocks[i].Count;

                foreach (int p in projectBlock)
                    eq = eq && blocks[i].Contains(p);

                if (!eq)
                    project.SetBlock(i, blocks[i]);
            }

            for (int i = project.PointsCount - 1; i >= vmPoints.Count; i--)
                project.RemovePoint(i);


            _paramsViewModel.IsResponsive = true;
            _paramsViewModel.HasChanges = false;
            _paramsViewModel.HasForeignChanges = false;
        }
    }
}
