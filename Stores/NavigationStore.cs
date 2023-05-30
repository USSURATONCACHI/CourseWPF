using CourseWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWPF.Stores {
    public class NavigationStore {
		public event Action? CurrentViewModelChanged;

#pragma warning disable CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
        private ViewModelBase _currentViewModel;
#pragma warning restore CS8618 // Поле, не допускающее значения NULL, должно содержать значение, отличное от NULL, при выходе из конструктора. Возможно, стоит объявить поле как допускающее значения NULL.
        public ViewModelBase CurrentViewModel {
			get => _currentViewModel;
			set {
				_currentViewModel = value;
				OnCurrentViewModelChanged();
				//OnPropertyChanged(nameof(CurrentViewModel));
			}
		}

		private void OnCurrentViewModelChanged() {
			CurrentViewModelChanged?.Invoke();
		}
	}
}
