using CourseWPF.Model;
using CourseWPF.Stores;
using CourseWPF.View;
using CourseWPF.ViewModel.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace CourseWPF.ViewModel {
    public class TabsViewModel : ViewModelBase {
        private NavigationStore _navigationStore;
        private Project _project;

        public SchemeViewModel SchemeViewModel { get; }
        public ParamsViewModel ParamsViewModel { get; }
        public CoordsViewModel CoordsViewModel { get; }
        public BlockViewModel Level1ViewModel { get; }
        public BlockViewModel Level2ViewModel { get; }
        public PredictionViewModel Level4ViewModel { get; }
        //public Level4ViewModel Level4ViewModel2 { get; }
        private PredictionStore Level4PredictionStore { get; }

        private int _selectedLevel2Block;
        public int SelectedLevel2Block {
            get => _selectedLevel2Block;
            set {
                _selectedLevel2Block = value;
                bool inBounds = value >= 0 && value < _project.Blocks.Count;
                Level2ViewModel.BlockId = inBounds ? value : null;
                OnPropertyChanged(nameof(SelectedLevel2Block));
            }
        }

        private int _selectedLevel4Point;
        public int SelectedLevel4Point {
            get => _selectedLevel4Point;
            set {
                _selectedLevel4Point = value;
                Level4PredictionStore.InvokeFullRefresh();
                OnPropertyChanged(nameof(SelectedLevel4Point));
            }
        }
        public IEnumerable<string> AvailablePoints { get => Enumerable.Range(1, _project.PointsCount).Select(x => x.ToString()); } 


        public TabsViewModel(NavigationStore ns, Project project) {
            _navigationStore = ns;
            _project = project;

            SchemeViewModel = new(project);
            ParamsViewModel = new(project);
            CoordsViewModel = new(project);
            //Level4ViewModel2 = new(project);
            Level1ViewModel = new(project, "Обобщенный анализ (ур.1)", null);
            Level2ViewModel = new(project, "Поблочный анализ (ур.2)", 0);
            Level4PredictionStore = new PredictionStore(project, 
                project => project.Level4Points[Math.Clamp(SelectedLevel4Point, 0, project.Level4Points.Count - 1)], null, false);
            Level4ViewModel = new(Level4PredictionStore, "Z");

            _project.PointAdded += _ => OnPropertyChanged(nameof(AvailablePoints));
            _project.PointRemoved += pointId => {
                if (SelectedLevel4Point >= pointId)
                    SelectedLevel4Point--;
                OnPropertyChanged(nameof(AvailablePoints));
            };
        }
    }
}
