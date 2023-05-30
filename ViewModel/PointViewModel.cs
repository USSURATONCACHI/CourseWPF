using CourseWPF.Commands.Params;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CourseWPF.ViewModel {
    public class PointViewModel : ViewModelBase {
		private double _x;
        private double _y;
        private int _blockId;

		private int _pointId;
		private DeletePointCommand? _deleteCommand;
		public DeletePointCommand? DeleteCommand {
			get => _deleteCommand;
			set {
				_deleteCommand = value;
				OnPropertyChanged(nameof(DeleteCommand));
			}
		}
		public int PointId {
			get => _pointId;
			set {
				_pointId = value;
				OnPropertyChanged(nameof(PointId));
			}
		}

		public double X {
			get => _x;
			set {
				_x = value;
				OnPropertyChanged(nameof(X));
			}
		}

		public double Y {
			get => _y;
			set {
				_y = value;
				OnPropertyChanged(nameof(Y));
			}
		}

		public int BlockId {
			get => _blockId;
			set {
				_blockId = value;
				OnPropertyChanged(nameof(BlockId));
			}
		}
	}
}
