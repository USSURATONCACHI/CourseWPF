using CourseWPF.Stores;
using CourseWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWPF.Commands.OpenProject
{
    class NewProjectCommand : BaseCommand
    {
        private NavigationStore _navigationStore;
        private ProjectStore _projectStore;
        public NewProjectCommand(NavigationStore ns, ProjectStore ps) {
            _navigationStore = ns;
            _projectStore = ps;
        }

        public override void Execute(object? parameter)
        {
            _navigationStore.CurrentViewModel = new CreateProjectViewModel(_navigationStore, _projectStore);
        }
    }
}
