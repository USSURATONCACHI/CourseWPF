using CourseWPF.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace CourseWPF.Stores {
    public class BlockStateStore {

        public event Action? TrustFactorChanged, ErrorFactorChanged, FullRefresh;
        public event Action<int>? EpochAdded, EpochRemoved, EpochChanged;

        private Project project;
        private Func<Project, BlockState> getter;

        private int? _blockId;

        public int? BlockId {
            get => _blockId;
            set {
                if (_blockId == value)
                    return;
                _blockId = value;
                FullRefresh?.Invoke();
            }
        }


        public BlockState BlockState => getter(project);

        public BlockStateStore(
            Project projectIn, 
            Func<Project, BlockState> getterIn,
            int? blockId
        ) {
            project = projectIn;
            getter = getterIn;
            BlockId = blockId;

            project.TrustFactorChanged += () => TrustFactorChanged?.Invoke();
            project.ErrorFactorChanged += () => ErrorFactorChanged?.Invoke();

            project.EpochAdded += id => EpochAdded?.Invoke(id);
            project.EpochRemoved += id => EpochRemoved?.Invoke(id);
            project.EpochChanged += id => EpochChanged?.Invoke(id);


            project.PointAdded += id => {
                if (BlockId is null)
                    FullRefresh?.Invoke();
            };

            project.PointRemoved += id => {
                if (BlockId is null)
                    FullRefresh?.Invoke();
            };

            project.BlockChanged += id => {
                if (BlockId is not null)
                    if (id == BlockId)
                        FullRefresh?.Invoke();
            };
        }
    }
}
