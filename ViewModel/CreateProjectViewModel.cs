using CourseWPF.Commands.CreateProject;
using CourseWPF.Commands.CreateProject.Points;
using CourseWPF.Services;
using CourseWPF.Stores;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Shapes;

namespace CourseWPF.ViewModel
{
    public class CreateProjectViewModel : ViewModelBase
    {



        public static readonly int MaxBlocks = 30;
        public static string GetBlockName(int id) {
            const string names = "АБВГДЕЖЗИКЛМНОПРСТУФХЦЧШЩЬыЪЭЮЯ";
            return names.ElementAt(id).ToString();
        }

        private ObservableCollection<string> _availableBlocks = new();
        public ObservableCollection<string> AvailableBlocks {
            get => _availableBlocks;
            set {
                _availableBlocks = value;
                _availableBlocks.CollectionChanged += (sender, args) => {
                    if (sender == _availableBlocks)
                        OnPropertyChanged(nameof(BlocksCount));
                };
                OnPropertyChanged(nameof(AvailableBlocks));
                OnPropertyChanged(nameof(BlocksCount));
                _availableBlocks.CollectionChanged += (sender, args) => {
                    if (_availableBlocks == value)
                        OnPropertyChanged(nameof(BlocksCount));
                };
            }
        }

        private ObservableCollection<Point> _availablePoints;
        public ObservableCollection<Point> AvailablePoints {
            get => _availablePoints;
            set {
                _availablePoints = value;
                OnPropertyChanged(nameof(AvailablePoints));
            }
        }
        public int BlocksCount { get => AvailableBlocks.Count(); }

        public ICommand AddBlock { get; }
        public ICommand RemoveBlock { get; }
        public ICommand Create { get; }
        public ICommand Return { get; }
        public ICommand OpenHeightsDb { get;  }
        public ICommand LoadDbTable { get; }
        public ICommand RunDbFix { get; }

        public ICommand OpenPointsDb { get; }


        private ObservableCollection<string> _availableDbTables;
        public ObservableCollection<string> AvailableDbTables {
            get => _availableDbTables;
            set {
                _availableDbTables = value;
                OnPropertyChanged(nameof(AvailableDbTables));
            }
        }

        private int _selectedTableIndex;
        public int SelectedTableIndex {
            get => _selectedTableIndex;
            set {
                _selectedTableIndex = value;
                OnPropertyChanged(nameof(SelectedTableIndex));
            }
        }

        private string _databasePath;
        public string DatabasePath {
            get => _databasePath;
            set {
                _databasePath = value;
                OnPropertyChanged(nameof(DatabasePath));
            }
        }

        public List<List<double>>? RawEpochsData;
        private DataTable _heightsTable;
        public DataTable HeightsTable {
            get => _heightsTable;
            set {
                _heightsTable = value;
                OnPropertyChanged(nameof(HeightsTable));
            }
        }

        private string _pointsTableName;
        public string PointsTableName {
            get => _pointsTableName;
            set {
                _pointsTableName = value;
                OnPropertyChanged(nameof(PointsTableName));
            }
        }

        private double _trustFactor;
        public double TrustFactor {
            get => _trustFactor;
            set {
                _trustFactor = value;
                OnPropertyChanged(nameof(TrustFactor));
            }
        }

        private double _errorFactor;
        public double ErrorFactor {
            get => _errorFactor;
            set {
                _errorFactor = value;
                OnPropertyChanged(nameof(ErrorFactor));
            }
        }

        private string _projectName;
        public string ProjectName {
            get => _projectName;
            set {
                _projectName = value;
                OnPropertyChanged(nameof(ProjectName));
            }
        }

        public CreateProjectViewModel(NavigationStore ns, ProjectStore ps) {
            _projectName = GenerateRandomName();
            _availableDbTables = new();
            _availablePoints = new();
            _availableBlocks = new();

            var currentBlocks = _availableBlocks;
            _availableBlocks.CollectionChanged += (sender, args) => {
                if (_availableBlocks == currentBlocks)
                    OnPropertyChanged(nameof(BlocksCount));
            };

            _heightsTable = new();
            _databasePath = "";
            _pointsTableName = "Схема объекта";
            _errorFactor = 0.0004;
            _trustFactor = 0.9;

            AddBlock = new AddBlockCommand(this);
            RemoveBlock = new RemoveBlockCommand(this);
            Create = new CreateProjectCommand(this, ns, ps);
            Return = new ReturnCommand(ns, ps);
            OpenHeightsDb = new OpenHeightsDbCommand(this);
            LoadDbTable = new LoadTableCommand(this);
            RunDbFix = new DbFixCommand(this);

            OpenPointsDb = new OpenPointsDbCommand(this);
        }


        private static Random RANDOM = new();

        public static string GenerateRandomName() {
            int len = RANDOM_WORDS.Length;

            string name, path;

            int i = 0;
            do {
                name = RANDOM_WORDS[RANDOM.Next(len)] + " " + RANDOM_WORDS[RANDOM.Next(len)];
                path = DataManager.Instance.GetFullProjectPath(name);
                i++;

                if (i > len * len * len)  // If all names are taken (very low probability)
                    return "Новый проект";
            } while (Directory.Exists(path));

            return name;
        }

        public static readonly string[] RANDOM_WORDS = new string[] {
            "pursion",
            "pluminclastott",
            "frivessile",
            "syndumster",
            "nezzy",
            "raez",
            "ensalex",
            "burgination",
            "deferts",
            "exultion",
            "wesy",
            "onama",
            "dimenting",
            "wardon",
            "hirew",
            "facuoup",
            "ewtu",
            "corniprome",
            "kepplier",
            "litionom",
            "absole",
            "dikort",
            "rendann",
            "tationous",
            "ihowton",
            "appectedies",
            "atrovel",
            "ceath",
            "derous",
            "pote",
            "climpower",
            "miccopriuos",
            "belleciarcle",
            "ferras",
            "ounters",
            "foragenue",
            "dipolinestigns",
            "hospelece",
            "hirew",
            "ception",
            "evenost",
            "koppons",
            "fuvertment",
            "quarropt",
            "mosquen",
            "tardonts",
            "commenquaded",
            "strizzes",
            "qurt",
            "ejecroict"
        };

        public class Point : INotifyPropertyChanged {
            private int _pointId;
            public int PointId {
                get => _pointId;
                set {
                    _pointId = value;
                    OnPropertyChanged(nameof(PointId));
                }
            }

            private double _x;
            public double X
            {
                get => _x;
                set
                {
                    _x = value;
                    OnPropertyChanged(nameof(X)); 
                }
            }
            private double _y;
            public double Y {
                get => _y;
                set {
                    _y = value;
                    OnPropertyChanged(nameof(Y));
                }
            }

            private int _blockId;
            public int BlockId {
                get => _blockId;
                set {
                    _blockId = value;
                    OnPropertyChanged(nameof(BlockId));
                }
            }

            public event PropertyChangedEventHandler? PropertyChanged;
            public void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
