using CourseWPF.Stores;
using CourseWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWPF.Commands.CreateProject {
    internal class ReturnCommand : BaseCommand {
        //private CreateProjectViewModel _viewModel;
        private NavigationStore _navigationStore;
        private ProjectStore _projectStore;

        public ReturnCommand(NavigationStore ns, ProjectStore ps) {
            _navigationStore = ns;
            _projectStore = ps;
        }

        public override void Execute(object? parameter) {
            _navigationStore.CurrentViewModel = new OpenProjectViewModel(_navigationStore, _projectStore);
        }
    }
}
