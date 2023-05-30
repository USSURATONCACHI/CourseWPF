using CourseWPF.Model;
using CourseWPF.Stores;
using CourseWPF.ViewModel.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWPF.ViewModel {
    public class BlockViewModel : ViewModelBase {
        public StateViewModel StateViewModel { get; }
        public PredictionViewModel ModulePredictionVm { get; }
        public PredictionViewModel ModuleHighPredictionVm { get; }
        public PredictionViewModel ModuleLowPredictionVm { get; }
        public PredictionViewModel AnglePredictionVm { get; }
        public PredictionViewModel AngleHighPredictionVm { get; }
        public PredictionViewModel AngleLowPredictionVm { get; }

        private string _title;
        public string Title {
            get => _title;
            set {
                _title = value;
                OnPropertyChanged(nameof(Title));
            }
        }

        private int? _blockId;
        public int? BlockId {
            get => _blockId;
            set {
                _blockId = value;
                OnPropertyChanged(nameof(BlockId));
                _blockStateStore.BlockId = _blockId;
                foreach (var ps in _predictionsStores)
                    ps.BlockId = _blockId;
            }
        }

        private BlockStateStore _blockStateStore;
        private List<PredictionStore> _predictionsStores;


        public BlockViewModel(
            Project project, 
            string title,
            int? blockId
        ) {
            _title = title;
            _blockId = blockId;

            Func<Project, FullBlockData> dataGetter = project => {
                if (_blockId is int reallyBlockId && _blockId < project.Level2Datas.Count)
                    return project.Level2Datas[reallyBlockId];
                else
                    return project.Level1Data;
            };

            _blockStateStore = new BlockStateStore(project, project => dataGetter(project).BlockState, blockId);
            _predictionsStores = new() {
                new PredictionStore(project, project => dataGetter(project).ModulePrediction, blockId, false),
                new PredictionStore(project, project => dataGetter(project).ModuleHighPrediction, blockId, true),
                new PredictionStore(project, project => dataGetter(project).ModuleLowPrediction, blockId, true),
                new PredictionStore(project, project => dataGetter(project).AnglePrediction, blockId, false),
                new PredictionStore(project, project => dataGetter(project).AngleHighPrediction, blockId, true),
                new PredictionStore(project, project => dataGetter(project).AngleLowPrediction, blockId, true)
            };

            StateViewModel =            new(_blockStateStore);
            ModulePredictionVm =        new(_predictionsStores[0], "M");
            ModuleHighPredictionVm =    new(_predictionsStores[1], "M+");
            ModuleLowPredictionVm =     new(_predictionsStores[2], "M-");
            AnglePredictionVm =         new(_predictionsStores[3], "A");
            AngleHighPredictionVm =     new(_predictionsStores[4], "A+");
            AngleLowPredictionVm =      new(_predictionsStores[5], "A-");
        }
    }
}
