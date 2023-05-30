using CourseWPF.Services;
using CourseWPF.Stores;
using CourseWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CourseWPF.Commands.OpenProject
{
    class OpenProjectCommand : BaseCommand
    {
        private NavigationStore _navigationStore;
        private ProjectStore _projectStore;
        private string _name;
        public OpenProjectCommand(NavigationStore ns, ProjectStore ps, string name) {
            _navigationStore = ns;
            _projectStore = ps;
            _name = name;
        }
        public override void Execute(object? parameter) {
            var projectPath = DataManager.Instance.GetFullProjectPath(_name);

            try {
                var project = ProjectLoader.LoadFromPath(projectPath);

                _projectStore.Project = project;
                _navigationStore.CurrentViewModel = new TabsViewModel(_navigationStore, project);
            } catch (ProjectLoader.ProjectLoaderException e) {
                MessageBox.Show(e.Message, "Failed to open project", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
