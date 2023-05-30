using CourseWPF.Commands;
using CourseWPF.Commands.OpenProject;
using CourseWPF.Stores;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CourseWPF.ViewModel
{
    public class OpenProjectViewModel : ViewModelBase
    {
        public class ProjectItem {
            public string Name { get; set; } = "<name?>";
            public ICommand? Click { get; set; }
        }


        private ObservableCollection<ProjectItem> _items;
        public ObservableCollection<ProjectItem> Items {
            get => _items;
            set {
                _items = value;
                OnPropertyChanged(nameof(Items));
            }
        }

        public ICommand NewProjectCommand { get; }
        public ICommand OpenProjectFolderCommand { get; }
        public ICommand RefreshCommand { get; }


        public OpenProjectViewModel(NavigationStore ns, ProjectStore ps) {
            _items = new();

            RefreshCommand = new RefreshProjectsCommand(this, ns, ps);
            RefreshCommand.Execute(null); // Load available projects

            NewProjectCommand = new NewProjectCommand(ns, ps);
            OpenProjectFolderCommand = new OpenProjectFolderCommand(this);
        }
    }
}
