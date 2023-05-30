using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace CourseWPF.Model {
    public class Project {
        public event Action<int>? BlockChanged, BlockAdded, BlockRemoved;
        public event Action<int>? PointChanged, PointAdded, PointRemoved;
        public event Action<int>? EpochChanged, EpochAdded, EpochRemoved;
        public event Action? TrustFactorChanged, ErrorFactorChanged, ImageChanged;


        public List<List<double>> Epochs;
        public List<(double, double)> Points;
        public List<List<int>> Blocks;

        private double _errorFactor, _trustFactor;
        public double TrustFactor {
            get => _trustFactor;
            set => SetTrustFactor(value);
        }
        public double ErrorFactor {
            get => _errorFactor;
            set => SetErrorFactor(value);
        }

        public string SavePath { get; set; }
        public string ImagePath { get; set; } = "";

        public FullBlockData Level1Data;
        public List<FullBlockData> Level2Datas;
        public List<Prediction> Level4Points;

        public int EpochsCount => Epochs.Count;
        public int PointsCount => Points.Count;

        public Project(
            List<List<double>> epochs,
            List<(double, double)> points,
            List<List<int>> blocks,

            double trustFactor,
            double errorFactor
        ) {
            Epochs = epochs;
            Points = points;
            Blocks = blocks;
            _trustFactor = trustFactor;
            _errorFactor = errorFactor;

            Level1Data = new FullBlockData(Epochs, TrustFactor, ErrorFactor);
            Level2Datas = new();
            Level4Points = new();

            for (int blockId = 0; blockId < Blocks.Count; blockId++)
                Level2Datas.Add(new FullBlockData(GetBlockEpochs(blockId), TrustFactor, ErrorFactor));

            for (int pointId = 0; pointId < Points.Count; pointId++)
                Level4Points.Add(new Prediction(GetPointEpochs(pointId), trustFactor));
        }

        // Setters
        public void SetErrorFactor(double value) {
            _errorFactor = value;

            Level1Data.ErrorFactor = value;
            foreach (var data in Level2Datas)
                data.ErrorFactor = value;

            ErrorFactorChanged?.Invoke();
        }
        public void SetTrustFactor(double value) {
            _trustFactor = value;

            Level1Data.TrustFactor = value;
            foreach (var data in Level2Datas)
                data.TrustFactor = value;
            foreach (var prediction in Level4Points)
                prediction.TrustFactor = value;

            TrustFactorChanged?.Invoke();
        }

        // Points manipulations
        public void AddPoint(int pos) {
            Points.Insert(pos, (0, 0));

            foreach (var epoch in Epochs)
                epoch.Insert(pos, 0);

            Level1Data.AddPoint(pos, GetPointEpochs(pos));
            ShiftBlocksPoints(pos, 1);
            Level4Points.Insert(pos, new Prediction(GetPointEpochs(pos), TrustFactor));
            PointAdded?.Invoke(pos);
        }
        // Assumes that no changes to data itself are made
        private void ShiftBlocksPoints(int startPoint, int shift) {
            foreach ((var block, var blockId) in Blocks.Select((b, i) => (b, i))) {
                bool anyChanges = false;

                for (int i = 0; i < block.Count; i++)
                    if (block[i] >= startPoint) {
                        block[i] += shift;
                        anyChanges = true;
                    }

                if (anyChanges)
                    BlockChanged?.Invoke(blockId);
            }
        }

        public void RemovePoint(int pos) {
            Points.RemoveAt(pos);

            foreach (var epoch in Epochs)
                epoch.RemoveAt(pos);

            Level1Data.RemovePoint(pos);
            ShiftBlocksPoints(pos, -1);
            Level4Points.RemoveAt(pos);

            PointRemoved?.Invoke(pos);
        }
        public void SetPointPos(int pointId, (double, double) value) {
            Points[pointId] = value;
            PointChanged?.Invoke(pointId);
        }
        public (double, double) GetPointPos(int pointId) => Points[pointId];
        public IReadOnlyCollection<(double, double)> GetAllPointsPoses() => Points;
        public int? GetPointBlockId(int pointId) {
            foreach ((var block, var index) in Blocks.Select((b, i) => (b, i)))
                if (block.Contains(pointId))
                    return index;

            return null;
        }
        public void SetPointBlockId(int pointId, int? blockId) {
            if (blockId is int nonnull && Blocks[nonnull].Contains(pointId))
                return;

            foreach (int curBlockId in Enumerable.Range(0, Blocks.Count))
                BlockRemovePoint(curBlockId, pointId);

            if (blockId is int nonnull1)
                BlockAddPoint(nonnull1, pointId);
        }
        public IEnumerable<double> GetPointEpochs(int pointId) => Epochs.Select(epoch => epoch[pointId]);


        // Epochs manipulations
        public void AddEpoch(int epochId, IEnumerable<double> heights) {
            Util.Assert(heights.Count() == PointsCount, "SetEpoch - heights count should be equal to points count");

            Epochs.Insert(epochId, heights.ToList());
            Level1Data.AddEpoch(epochId, heights);

            // Update all level2 blocks
            foreach ((var data, var blockId) in Level2Datas.Select((d, i) => (d, i))) {
                var blockLocalHeights = Util.FilterDataByPointsIds(heights, GetBlock(blockId));
                data.AddEpoch(epochId, blockLocalHeights);
            }

            FullUpdateLevel4();

            EpochAdded?.Invoke(epochId);
        }
        public void RemoveEpoch(int epochId) {
            Epochs.RemoveAt(epochId);

            Level1Data.RemoveEpoch(epochId);
            foreach (var data in Level2Datas)
                data.RemoveEpoch(epochId);
            FullUpdateLevel4();

            EpochRemoved?.Invoke(epochId);
        }

        public void SetEpochValue(int epochId, int pointId, double value) {
            Epochs[epochId][pointId] = value;

            Level1Data.SetEpoch(epochId, GetEpoch(epochId));

            for (int blockId = 0; blockId < Blocks.Count; blockId++) {
                var blockPointInOrder = Util.FilterDataByPointsIds(GetAllPointsIds(), GetBlock(blockId));
                var localPointId = blockPointInOrder.ToList().IndexOf(pointId);

                if (localPointId != -1) {
                    Level2Datas[blockId].SetEpochValue(epochId, localPointId, value);
                }
            }

            UpdateLevel4(pointId);

            EpochChanged?.Invoke(epochId);
        }
        public void SetEpoch(int epochId, IEnumerable<double> heights) {
            Util.Assert(heights.Count() == PointsCount, "SetEpoch - heights count should be equal to points count");

            foreach ((var height, var i) in heights.Select((h, i) => (h, i)))
                Epochs[epochId][i] = height;

            Level1Data.SetEpoch(epochId, GetEpoch(epochId));

            // Send data to all level2s
            for (int blockId = 0; blockId < Blocks.Count; blockId++)
                Level2Datas[blockId].SetEpoch(
                    epochId,
                    Util.FilterDataByPointsIds(Epochs[epochId], GetBlock(blockId))
                );
            FullUpdateLevel4();

            EpochChanged?.Invoke(epochId);
        }
        public IReadOnlyCollection<double> GetEpoch(int epochId) => Epochs[epochId];
        public IReadOnlyCollection<IReadOnlyCollection<double>> GetAllEpochs() => Epochs;
        public IEnumerable<IEnumerable<double>> GetEpochsByPoints(IEnumerable<int> points) {
            return Epochs.Select(epoch => Util.FilterDataByPointsIds(epoch, points));
        }


        // Block manipulations
        public IReadOnlyCollection<int> GetBlock(int blockId) => Blocks[blockId];
        public IReadOnlyCollection<IReadOnlyCollection<int>> GetAllBlocks() => Blocks;
        public void SetBlock(int blockId, IEnumerable<int>? points = null) {
            if (points is null)
                points = new List<int>();

            Blocks[blockId] = points.ToList();
            // Fully reinit
            Level2Datas[blockId] = new FullBlockData(GetBlockEpochs(blockId), TrustFactor, ErrorFactor);

            BlockChanged?.Invoke(blockId);
        }
        public void AddBlock(int pos, IEnumerable<int>? points = null) {
            if (points is null)
                points = new List<int>();
            Blocks.Insert(pos, points.ToList());
            Level2Datas.Insert(pos, new FullBlockData(GetBlockEpochs(pos), TrustFactor, ErrorFactor));
            BlockAdded?.Invoke(pos);
        }
        public void RemoveBlock(int blockId) {
            Blocks.RemoveAt(blockId);
            Level2Datas.RemoveAt(blockId);
            BlockRemoved?.Invoke(blockId);
        }
        public IEnumerable<int> GetAllPointsIds() => Enumerable.Range(0, PointsCount);
        public IEnumerable<int> GetNonBlocksPoints() {
            var allPoints = GetAllPointsIds();

            foreach (var block in Blocks)
                allPoints = allPoints.Except(block);

            return allPoints;
        }
        public IEnumerable<IEnumerable<double>> GetBlockEpochs(int blockId) => GetEpochsByPoints(Blocks[blockId]);

        public void BlockAddPoint(int blockId, int point) {
            if (Blocks[blockId].Contains(point))
                return;

            Blocks[blockId].Add(point);
            Blocks[blockId].Sort();

            Level2Datas[blockId].AddPoint(
                Blocks[blockId].IndexOf(point), 
                GetAllEpochs().Select( epoch => epoch.ElementAt(point) )    
            );
            BlockChanged?.Invoke(blockId);
        }
        public void BlockRemovePoint(int blockId, int point) {
            int index = Blocks[blockId].IndexOf(point);
            if (index < 0)
                return;

            Blocks[blockId].Remove(point);
            Level2Datas[blockId].RemovePoint(index);
            BlockChanged?.Invoke(blockId);
        }


        private void FullUpdateLevel4() {
            for (int pointId = 0; pointId < PointsCount; pointId++)
                UpdateLevel4(pointId);
        }
        private void UpdateLevel4(int pointId) => 
            Level4Points[pointId].UpdateData(GetPointEpochs(pointId));

        public void InvokeImageChanged() => ImageChanged?.Invoke();
    }
}
