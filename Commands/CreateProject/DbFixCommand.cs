using CourseWPF.ViewModel;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace CourseWPF.Commands.CreateProject {
    internal class DbFixCommand : BaseCommand {
        private CreateProjectViewModel _viewModel;

        public DbFixCommand(CreateProjectViewModel vm) {
            _viewModel = vm;
            _viewModel.PropertyChanged += _viewModel_PropertyChanged;
        }

        private void _viewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e) {
            if (e.PropertyName == nameof(CreateProjectViewModel.AvailableDbTables) ||
                e.PropertyName == nameof(CreateProjectViewModel.SelectedTableIndex) ||
                e.PropertyName == nameof(CreateProjectViewModel.DatabasePath))
                InvokeCanExecuteChanged();
        }

        public override bool CanExecute(object? parameter) {
            return File.Exists(_viewModel.DatabasePath) &&
                _viewModel.SelectedTableIndex < _viewModel.AvailableDbTables.Count &&
                _viewModel.SelectedTableIndex >= 0 &&
                _viewModel.AvailableDbTables[_viewModel.SelectedTableIndex] != "" &&
                base.CanExecute(parameter);
        }

        public override void Execute(object? parameter) {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = false;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "db_fix.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            string tableName = _viewModel.AvailableDbTables[_viewModel.SelectedTableIndex];
            startInfo.ArgumentList.Add(_viewModel.DatabasePath);
            startInfo.ArgumentList.Add(tableName);

            try {
                using Process? exeProcess = Process.Start(startInfo);

                if (exeProcess is null)
                    throw new Exception("Failed to create process");

                exeProcess.WaitForExit();

                if (exeProcess.ExitCode == 0)
                    MessageBox.Show("Успех", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("db_fix завершился с ошибкой", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            } catch (Exception e) {
                MessageBox.Show(e.Message, "Возникло исключение", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
