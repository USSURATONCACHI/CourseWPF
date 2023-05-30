using CourseWPF.Model;
using CourseWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWPF.Commands.Coords {
    public class AddRowCommand : BaseCommand {
        private CoordsViewModel _viewModel;

        public AddRowCommand(CoordsViewModel viewModel) {
            _viewModel = viewModel;
        }

        public override void Execute(object? parameter) {
            var rows = _viewModel.DataTable.Rows;
            var cols = _viewModel.DataTable.Columns;

            DataRow newRow;
            if (rows.Count < 3) {
                newRow = _viewModel.DataTable.NewRow();
                for (int i = 0; i < _viewModel.DataTable.Columns.Count; i++)
                    newRow[i] = 1.0;
            } else {
                List<double> deltasSum = Enumerable.Range(0,cols.Count).Select(i => 0.0).ToList();
                List<double> lastValues = Enumerable.Range(0,cols.Count).Select(i => 0.0).ToList();

                int deltasCount = 0;
                for (int rowId = 0; rowId < rows.Count - 1; rowId++) {
                    DataRow row = _viewModel.DataTable.Rows[rowId];
                    DataRow nextRow = _viewModel.DataTable.Rows[rowId + 1];

                    int colId = 0;
                    foreach (DataColumn col in cols) {
                        if (col.Caption != "Эпоха") {
                            var value = double.Parse((row[col]?.ToString() ?? "0").Replace(",", "."));
                            var nextValue = double.Parse((nextRow[col]?.ToString() ?? "0").Replace(",", "."));

                            deltasSum[colId] += Math.Abs(nextValue - value);
                            lastValues[colId] = nextValue;

                            colId++;
                        }
                    }

                    deltasCount++;
                }

                newRow = _viewModel.DataTable.NewRow();
                for (int i = 0; i < _viewModel.DataTable.Columns.Count - 1; i++) {
                    newRow[i + 1] = Math.Round(lastValues[i] + (deltasSum[i] / ((double) deltasCount - 1)), 4);
                }
            }

            _viewModel.DataTable.Rows.Add(newRow);
        }
    }
}
