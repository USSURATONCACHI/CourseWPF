using CourseWPF.Stores;
using CourseWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWPF.Commands.Main {
    internal class CloseProjectCommand : BaseCommand {
        private NavigationStore _navigationStore;
        private ProjectStore _projectStore;

        public CloseProjectCommand(NavigationStore ns, ProjectStore ps) {
            _navigationStore = ns;
            _projectStore = ps;
            _projectStore.ProjectChanged += InvokeCanExecuteChanged;
        }

        public override bool CanExecute(object? parameter) {
            return _projectStore.Project is not null && base.CanExecute(parameter);
        }

        public override void Execute(object? parameter) {
            _navigationStore.CurrentViewModel = new OpenProjectViewModel(_navigationStore, _projectStore);
        }
    }
}
