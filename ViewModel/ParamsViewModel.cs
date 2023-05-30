using CourseWPF.Commands;
using CourseWPF.Commands.Params;
using CourseWPF.Model;
using CourseWPF.Stores;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CourseWPF.ViewModel {
    public class ParamsViewModel : ViewModelBase {
		private bool _isResponsive = true;
		public bool IsResponsive {
			get => _isResponsive;
			set {
				_isResponsive = value;
				OnPropertyChanged(nameof(IsResponsive));
			}
		}
		private double _trustFactor;
		public double TrustFactor {
			get => _trustFactor;
			set {
                if (_trustFactor == value)
                    return;

				_trustFactor = value;
				OnPropertyChanged(nameof(TrustFactor));
                HasChanges = true;
			}
		}

		private double _errorFactor;
		public double ErrorFactor {
			get => _errorFactor;
			set {
                if (_errorFactor == value)
                    return;

                _errorFactor = value;
				OnPropertyChanged(nameof(ErrorFactor));

                HasChanges = true;
			}
		}

		private int _blocksCount;
		public int BlocksCount {
			get => _blocksCount;
			set {
				_blocksCount = value;
				_availableBlocks = Enumerable.Range(0, _blocksCount)
					.Select(index => (char)('А' + index))
					.Select(cchar => $"{cchar}");
				OnPropertyChanged(nameof(BlocksCount));
				OnPropertyChanged(nameof(AvailableBlocks));
            }
		}

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

        private IEnumerable<string> _availableBlocks;
		public IEnumerable<string> AvailableBlocks {
			get => _availableBlocks;
		}

		private ObservableCollection<PointViewModel> _points;
		public ObservableCollection<PointViewModel> Points {
			get => _points;
			set {
				_points = value;
				OnPropertyChanged(nameof(Points));
			}
		}

        public ICommand CommandApply { get; }
        public ICommand CommandReset { get; }
        public ICommand CommandAddBlock { get; }
        public ICommand CommandRemoveBlock { get; }
        public ICommand CommandAddPoint { get; }

        private Project _project;
        public ParamsViewModel(Project project) {
            _project = project;

            _points = new() { };
            _availableBlocks = new List<string>();

            TrustFactor = 0.9;
            ErrorFactor = 0.0004;
            BlocksCount = 2;

            CommandApply = new ParamsApplyCommand(this, _project);
            CommandReset = new ParamsResetCommand(this, _project);
            CommandAddBlock = new AddBlockCommand(this);
            CommandRemoveBlock = new RemoveBlockCommand(this);
            CommandAddPoint = new AddPointCommand(this);

            CommandReset.Execute(null);


            _project.TrustFactorChanged += _project_TrustFactorChanged;
            _project.ErrorFactorChanged += _project_ErrorFactorChanged;

            _project.PointChanged += _project_PointChanged;
            _project.PointAdded += _project_PointAdded;
            _project.PointRemoved += _project_PointRemoved;

            _project.BlockChanged += _project_BlockChanged;
            _project.BlockAdded += _project_BlockAdded;
            _project.BlockRemoved += _project_BlockRemoved;

            HasChanges = false;
            HasForeignChanges = false;
            IsResponsive = true;
        }

        private void _project_BlockRemoved(int blockId) {
			if (!IsResponsive)
				return;

            if (HasChanges)
                HasForeignChanges = true;
            else {
                foreach (var point in _points)
                    if (point.BlockId == blockId)
                        point.BlockId = -1;
                    else if (point.BlockId > blockId)
                        point.BlockId--;

                BlocksCount--;
            }
        }

        private void _project_BlockAdded(int blockId) {
            if (!IsResponsive)
                return;

            if (HasChanges)
                HasForeignChanges = true;
            else {
                foreach (var point in _points)
                    if (point.BlockId >= blockId)
                        point.BlockId++;

                BlocksCount++;
            }
        }

        private void _project_BlockChanged(int blockId) {
            if (!IsResponsive)
                return;

            if (HasChanges)
                HasForeignChanges = true;
            else {
                foreach (int pointId in _project.GetBlock(blockId))
                    _points[pointId].BlockId = blockId;
            }
        }

        private void _project_ErrorFactorChanged() {
            if (!IsResponsive)
                return;

            if (HasChanges)
                HasForeignChanges = true;
            else
                ErrorFactor = _project.ErrorFactor;
        }

        private void _project_TrustFactorChanged() {
            if (!IsResponsive)
                return;

            if (HasChanges)
                HasForeignChanges = true;
            else
                TrustFactor = _project.TrustFactor;
        }

        private void _project_PointChanged(int pointId) {
            if (!IsResponsive)
                return;

            if (HasChanges)
                HasForeignChanges = true;
            else
                (_points[pointId].X, _points[pointId].Y) = _project.GetPointPos(pointId);
        }

        private void _project_PointAdded(int pointId) {
            if (!IsResponsive)
                return;

            if (HasChanges)
                HasForeignChanges = true;
            else {

                _points.Insert(pointId, new() {
                    PointId = pointId,
                    X = _project.GetPointPos(pointId).Item1,
                    Y = _project.GetPointPos(pointId).Item2,
                    BlockId = _project.GetPointBlockId(pointId) ?? -1
                });
                foreach (var point in _points.Skip(pointId + 1))
                    point.PointId++;
            }        
        }

        private void _project_PointRemoved(int pointId) {
            if (!IsResponsive)
                return;

            if (HasChanges)
                HasForeignChanges = true;
            else {
                _points.RemoveAt(pointId);

                foreach (var point in _points.Skip(pointId))
                    point.PointId--;
            }
        }
    }
}
