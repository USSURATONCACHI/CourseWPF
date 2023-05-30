using CourseWPF.Model;
using CourseWPF.Services;
using CourseWPF.Stores;
using CourseWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CourseWPF.Commands.CreateProject {
    internal class CreateProjectCommand : BaseCommand {
        private CreateProjectViewModel _viewModel;
        private NavigationStore _navigationStore;
        private ProjectStore _projectStore;

        public CreateProjectCommand(CreateProjectViewModel vm, NavigationStore ns, ProjectStore ps) {
            _viewModel = vm;
            _navigationStore = ns;
            _projectStore = ps;
            _viewModel.PropertyChanged += _viewModel_PropertyChanged;
        }

        private void _viewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            List<string> names = new() {
                nameof(CreateProjectViewModel.AvailablePoints),
                nameof(CreateProjectViewModel.ProjectName),
                nameof(CreateProjectViewModel.HeightsTable)
            };

            if (names.Contains(e.PropertyName ?? "<null>"))
                InvokeCanExecuteChanged();
        }

        public override bool CanExecute(object? parameter) {
            return _viewModel.AvailablePoints.Count > 0 &&
                _viewModel.HeightsTable.Columns.Count > 0 &&
                _viewModel.HeightsTable.Rows.Count > 0 &&
                _viewModel.ProjectName != "" &&
                base.CanExecute(parameter);
        }

        public override void Execute(object? parameter) {
            if (_viewModel.RawEpochsData is null)
                throw new Exception("Unreachable");

            string projectName = _viewModel.ProjectName;
            var path = DataManager.Instance.GetFullProjectPath(projectName);

            if (Directory.Exists(path)) {
                MessageBox.Show("Проект с таким названием уже существует. Пожалуйста, выберите новое название", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            List<List<double>> epochs = _viewModel.RawEpochsData;
            List<List<int>> blocks = _viewModel.AvailableBlocks.Select(_ => new List<int>()).ToList();
            List<(double, double)> points = _viewModel.AvailablePoints.Select(point => (point.X, point.Y)).ToList();
            double trustFactor = _viewModel.TrustFactor;
            double errorFactor = _viewModel.ErrorFactor;

            for (int i = 0; i < _viewModel.AvailablePoints.Count; i++) {
                CreateProjectViewModel.Point point = _viewModel.AvailablePoints[i];
                bool inBounds = point.BlockId >= 0 && point.BlockId < blocks.Count;

                if (inBounds)
                    blocks[point.BlockId].Add(i);
            }


            try {
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);
            } catch (Exception) {
                MessageBox.Show($"Не удалось создать директорию проекта: {path}", "Ошибка :(", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Project project;
            try {
                project = new Project(epochs, points, blocks, trustFactor, errorFactor) { 
                    SavePath = path,
                };
                ProjectSaver.SaveProject(project, path);
            } catch (Exception e) {
                MessageBox.Show(e.Message, "Не удалось создать проект", MessageBoxButton.OK, MessageBoxImage.Error);

                try { Directory.Delete(path, true); }
                catch (DirectoryNotFoundException) {}

                return;
            }

            _projectStore.Project = project;
            _navigationStore.CurrentViewModel = new TabsViewModel(_navigationStore, project);
        }
    }
}
