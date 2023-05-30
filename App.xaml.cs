using CourseWPF.Stores;
using CourseWPF.ViewModel;
using System.Windows;

namespace CourseWPF {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        private readonly NavigationStore _navigationStore;
        private readonly ProjectStore _projectStore;

        public App() {
            _navigationStore = new();
            _projectStore = new();
        }

        protected override void OnStartup(StartupEventArgs e) {
            //var projectPath = DataManager.Instance.GetFullProjectPath("Вариант 20");
            //var project = ProjectLoader.LoadFromPath(projectPath);

            _navigationStore.CurrentViewModel = new OpenProjectViewModel(_navigationStore, _projectStore);
            var mainWindow = new MainWindow() {
                DataContext = new MainViewModel(_navigationStore, _projectStore)
            };
            mainWindow.Show();

            base.OnStartup(e);
        }
    }
}
