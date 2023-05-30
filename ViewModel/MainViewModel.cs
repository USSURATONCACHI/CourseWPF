using CourseWPF.Commands;
using CourseWPF.Commands.Main;
using CourseWPF.Stores;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CourseWPF.ViewModel {
    internal class MainViewModel : ViewModelBase {
        private readonly NavigationStore _navigationStore;
        public ViewModelBase CurrentViewModel => _navigationStore.CurrentViewModel;


        public ICommand OpenAbout { get; }
        public ICommand OpenManual { get; }
        public ICommand Save { get; }
        public ICommand Quit { get; }
        public ICommand GoToNewProject { get; }
        public ICommand GoToOpenProject { get; }
        public ICommand CloseProject { get; }

        public MainViewModel(NavigationStore ns, ProjectStore ps) {
            _navigationStore = ns;
            _navigationStore.CurrentViewModelChanged += OnCurrentViewModelChanged;

            OpenAbout = new OpenInfoCommand();
            OpenManual = new OpenManualCommand();
            CloseProject = new CloseProjectCommand(ns, ps);
            Save = new SaveProjectCommand(ps);
            Quit = new QuitCommand();
            GoToNewProject = new NavigationCommand(_navigationStore, arg => new CreateProjectViewModel(ns, ps));
            GoToOpenProject = new NavigationCommand(_navigationStore, arg => new OpenProjectViewModel(ns, ps));
        }

        private void OnCurrentViewModelChanged() {
            OnPropertyChanged(nameof(CurrentViewModel));
        }
    }
}
