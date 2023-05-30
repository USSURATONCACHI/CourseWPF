using CourseWPF.Commands.Coords;
using CourseWPF.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;

namespace CourseWPF.ViewModel {
	public class CoordsViewModel : ViewModelBase {

		public ApplyCommand CommandApply { get; }
		public ICommand CommandReset { get; }
		public ICommand CommandAddRow { get; }
		public ICommand CommandDeleteRow { get; }

        private Project _project;

		private DataTable _dataTable;
		public DataTable DataTable {
			get => _dataTable;
			set {
				_dataTable = value;

				if (_dataTable is not null) {
					_dataTable.RowDeleted += (sender, args) => OnPropertyChanged(nameof(CanDeleteRows));
					_dataTable.RowChanged += (sender, args) => OnPropertyChanged(nameof(CanDeleteRows));
				}

                OnPropertyChanged(nameof(DataTable));
				OnPropertyChanged(nameof(CanDeleteRows));
            }
		}

		public Action<object, DataGridCellEditEndingEventArgs> EditingEnded;


		private bool _hasChanges = false;
		public bool HasChanges {
			get => _hasChanges;
			set {
				_hasChanges = value;
				OnPropertyChanged(nameof(HasChanges));
				OnPropertyChanged(nameof(HasChangesText));
                OnPropertyChanged(nameof(TotalChangesText));
            }
		}
		public string HasChangesText => HasChanges ? "*" : "";

		private bool _hasForeignChanges = false;
		public bool HasForeignChanges {
			get => _hasForeignChanges;
			set {
				_hasForeignChanges = value;
				OnPropertyChanged(nameof(HasForeignChanges));
				OnPropertyChanged(nameof(HasForeignChangesText));
				OnPropertyChanged(nameof(TotalChangesText));
            }
        }
        public string HasForeignChangesText => HasForeignChanges ? "*" : "";
        public string TotalChangesText => HasForeignChangesText + HasChangesText;

		public bool CanDeleteRows {
			get => (DataTable.Rows?.Count ?? 0) > 2;
        }


		private bool IgnoreUpdates = false;

        public CoordsViewModel(Project project) {
            _project = project;

			_dataTable = new();
            _dataTable.RowDeleted += (sender, args) => OnPropertyChanged(nameof(CanDeleteRows));
            _dataTable.RowChanged += (sender, args) => OnPropertyChanged(nameof(CanDeleteRows));
            Reset();

			Action<int> onChange = i => {
				if (HasChanges)
					HasForeignChanges = true;
				else
					Reset();
            };
            _project.PointAdded += onChange;
			_project.PointRemoved += onChange;
			_project.EpochChanged += onChange;
			_project.EpochAdded += onChange;
			_project.EpochRemoved += onChange;

            DataTable.RowChanged += DataTable_RowChanged;
            DataTable.RowDeleted += DataTable_RowDeleted;
			EditingEnded = (sender, args) => {
				HasChanges = true;

				var correct = IsDataCorrect(args);
                CommandApply?.SetAvailable(correct);
            };

			CommandApply = new ApplyCommand(this, project);
			CommandReset = new ResetCommand(this);
			CommandAddRow = new AddRowCommand(this);
			CommandDeleteRow = new DeleteRowCommand(this);
        }

        public bool IsDataCorrect(DataGridCellEditEndingEventArgs change) {
            TextBox? el = change.EditingElement as TextBox;

            bool result = true;

			for (int rowIndex = 0; rowIndex < DataTable.Rows.Count; rowIndex++) {
				DataRow row = DataTable.Rows[rowIndex];
				foreach (DataColumn col in DataTable.Columns) {
					if (col.ToString() == "Эпоха") continue;
					string str;

					if (rowIndex == change.Row.GetIndex() && col.Caption == change.Column.Header.ToString())
						str = el?.Text ?? "";
					else
						str = row[col].ToString() ?? "";

					if (str == "") str = "0";
                    result = result && double.TryParse(str.Replace(".", ","), out double parsed);
                }
			}

            return result;
		}

		public void Reset() {
            DataTable.Columns.Clear();
			DataTable.Rows.Clear();
            DataTable.Columns.Add("Эпоха").ReadOnly = true;

            for (int i = 0; i < _project.PointsCount; i++)
                DataTable.Columns.Add($"{i + 1}");

            var epochs = _project.GetAllEpochs();

            for (int i = 0; i < epochs.Count(); i++) {
                var epoch = epochs.ElementAt(i);

                var newRow = DataTable.NewRow();
                newRow[0] = i;
                for (int j = 0; j < epoch.Count; j++)
                    newRow[j + 1] = epoch.ElementAt(j);

                DataTable.Rows.Add(newRow);
            }

			HasChanges = false;
			HasForeignChanges = false;
			RefreshTable();
		}

		private void RefreshTable() {
            var tmp = DataTable;
#pragma warning disable CS8625 // Литерал, равный NULL, не может быть преобразован в ссылочный тип, не допускающий значение NULL.
            DataTable = null;
#pragma warning restore CS8625 // Литерал, равный NULL, не может быть преобразован в ссылочный тип, не допускающий значение NULL.
            DataTable = tmp;
        }


		private void UpdateIds() {
			var col = DataTable.Columns["Эпоха"];
			if (col is null)
				return;

			IgnoreUpdates = true;
            col.ReadOnly = false;

			for (int i = 0; i < DataTable.Rows.Count; i++)
				DataTable.Rows[i][col] = i;

            col.ReadOnly = true;
			IgnoreUpdates = false;
        }

		private void DataTable_RowDeleted(object sender, DataRowChangeEventArgs e) {
			if (IgnoreUpdates)
				return;
			UpdateIds();
			HasChanges = true;
        }
		private void DataTable_RowChanged(object sender, DataRowChangeEventArgs e) {
            if (IgnoreUpdates)
                return;
            UpdateIds();
            HasChanges = true;
        }

        public class TableRow : INotifyPropertyChanged {
            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged(string name) =>
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

            private string _title = "";
			public string Title {
				get => _title;
				set {
					_title = value;
					OnPropertyChanged(nameof(Title));
				}
			}

			private ObservableCollection<double> _values;
			public ObservableCollection<double> Values {
				get => _values;
				set {
					_values = value;
					OnPropertyChanged(nameof(Values));
				}
			}

		}
    }
}
