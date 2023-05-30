using CourseWPF.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWPF.Stores {
    public class PredictionStore {
        public event Action? TrustFactorChanged, FullRefresh;
        public event Action<int>? EpochAdded, EpochRemoved, EpochChanged;
        
        private Project project;
        private Func<Project, Prediction> getter;

        public Prediction Prediction => getter(project);
        public Project Project => project;

        private int? _blockId;

        public int? BlockId {
            get => _blockId;
            set { 
                _blockId = value; 
                InvokeFullRefresh();
            }
        }


        public PredictionStore(
            Project projectIn, 
            Func<Project, Prediction> getterIn, 
            int? blockId,
            bool refreshOnErrorFactor    
        ) {
            project = projectIn;
            getter = getterIn;
            _blockId = blockId;

            project.TrustFactorChanged += () => TrustFactorChanged?.Invoke();

            if (refreshOnErrorFactor)
                project.ErrorFactorChanged += InvokeFullRefresh;

            project.EpochAdded += id => EpochAdded?.Invoke(id);
            project.EpochRemoved += id => EpochRemoved?.Invoke(id);
            project.EpochChanged += id => EpochChanged?.Invoke(id);

            project.PointAdded += id => {
                if (BlockId is null) InvokeFullRefresh();
            };
            project.PointRemoved += id => {
                if (BlockId is null) InvokeFullRefresh();
            };
            project.BlockChanged += id => {
                if (BlockId is not null && id == BlockId)
                    InvokeFullRefresh();
            };
        }

        public void InvokeFullRefresh() => FullRefresh?.Invoke();
    }
}
