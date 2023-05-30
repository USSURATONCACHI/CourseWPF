using CourseWPF.Services;
using CourseWPF.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWPF.Commands.Main {
    internal class SaveProjectCommand : BaseCommand {
        private ProjectStore _projectStore;

        public SaveProjectCommand(ProjectStore ps) {
            _projectStore = ps;
            _projectStore.ProjectChanged += InvokeCanExecuteChanged;
        }

        public override bool CanExecute(object? parameter) {
            return _projectStore.Project is not null &&
                _projectStore.Project.SavePath != null &&
                _projectStore.Project.SavePath != "" &&
                base.CanExecute(parameter);
        }

        public override void Execute(object? parameter) {
            if (_projectStore.Project is null) return;

            ProjectSaver.SaveProject(_projectStore.Project, _projectStore.Project.SavePath);
        }
    }
}
