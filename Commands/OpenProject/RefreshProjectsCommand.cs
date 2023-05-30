using CourseWPF.Services;
using CourseWPF.Stores;
using CourseWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWPF.Commands.OpenProject
{
    class RefreshProjectsCommand : BaseCommand
    {
        private OpenProjectViewModel _viewModel;
        private readonly NavigationStore _navigationStore;
        private readonly ProjectStore _projectStore;

        public RefreshProjectsCommand(OpenProjectViewModel vm, NavigationStore ns, ProjectStore ps) {
            _viewModel = vm;
            this._navigationStore = ns;
            this._projectStore = ps;
        }

        public override void Execute(object? parameter)
        {
            var items = DataManager.Instance.GetAvailableProjects()
                .Select(projectName => new OpenProjectViewModel.ProjectItem() {
                    Name = projectName,
                    Click = new OpenProjectCommand(_navigationStore, _projectStore, projectName)
                });

            _viewModel.Items = new(items);
        }
    }
}
