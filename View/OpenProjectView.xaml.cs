using CourseWPF.Services;
using CourseWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CourseWPF.View
{
    /// <summary>
    /// Логика взаимодействия для OpenProjectView.xaml
    /// </summary>
    public partial class OpenProjectView : UserControl
    {
        public OpenProjectView()
        {
            InitializeComponent();
        }
        private void OpenCurrentProject() {
            if (ProjectsList.SelectedItem is OpenProjectViewModel.ProjectItem item)
                if (item.Click is not null && item.Click.CanExecute(null))
                    item.Click.Execute(null);
        }

        public void ProjectsListDoubleClick(object sender, MouseButtonEventArgs e) => OpenCurrentProject();
        private void OpenProjectButtonClick(object sender, RoutedEventArgs e) => OpenCurrentProject();

        private void DeleteSaveButtonClick(object sender, RoutedEventArgs e) {
            if (ProjectsList.SelectedItem is OpenProjectViewModel.ProjectItem item) {
                string projectName = item.Name;
                var path = DataManager.Instance.GetFullProjectPath(projectName);

                try {
                    Directory.Delete(path, true);
                } catch (DirectoryNotFoundException) {}

#pragma warning disable CS8602 // Разыменование вероятной пустой ссылки.
                (DataContext as OpenProjectViewModel).RefreshCommand.Execute(null);
#pragma warning restore CS8602 // Разыменование вероятной пустой ссылки.
            }
        }
    }
}
