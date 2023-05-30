using CourseWPF.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CourseWPF.Services.ProjectLoader;

namespace CourseWPF.Services {
    public class ProjectSaver {

        public static void SaveProject(Project project, string folderPath) {
            string? dbPath = ProjectLoader.FindDatabaseFilepath(folderPath);

            bool createNew = false;
            if (dbPath is null) {
                createNew = true;
                dbPath = $"{folderPath}/data.sqlite";
                SQLiteConnection.CreateFile(dbPath);
            }

            using (SQLiteConnection connection = new($"Data Source={dbPath};Version=3;")) {
                connection.Open();
                using var transaction = connection.BeginTransaction();

                if (createNew)
                    CreateTables(connection);

                CheckEpochsColumns(connection, project.PointsCount);
                // RemoveExtraEpochs(connection, project.EpochsCount);
                InsertEpochs(connection, project.GetAllEpochs());

                SaveParameter(connection, "Коэффициент доверия", project.TrustFactor);
                SaveParameter(connection, "Погрешность", project.ErrorFactor);

                RemoveExtraPoints(connection, project.PointsCount);
                InsertPoints(connection, project);

                transaction.Commit();
            }
        }

        private static void InsertEpochs(SQLiteConnection connection, IEnumerable<IEnumerable<double>> epochs) {
            new SQLiteCommand($"DELETE FROM [Данные];", connection).ExecuteNonQuery();

            var colsCount = epochs.First().Count();
            var cols = String.Join(",", Enumerable.Range(1, colsCount).Select(col => $"[{col}]"));
            var values = epochs.Select((epoch, epochId) => $"({epochId}, {String.Join(", ", epoch)})");
            string queryText = $"INSERT INTO [Данные] (Эпоха, {cols}) VALUES {String.Join(", ", values)};";

            new SQLiteCommand(queryText, connection).ExecuteNonQuery();
        }

        /*private static void RemoveExtraEpochs(SQLiteConnection connection, int maxEpochs) {
            new SQLiteCommand($"DELETE FROM [Данные] WHERE [Эпоха] >= {maxEpochs} OR [Эпоха] < 0;", connection)
                .ExecuteNonQuery();
        }*/

        private static void CheckEpochsColumns(SQLiteConnection connection, int columnsNeeded) {
            var presentColumns = new List<int>();
            var command = new SQLiteCommand("PRAGMA table_info(Данные);", connection);
            using (SQLiteDataReader reader = command.ExecuteReader()) {
                if (reader.HasRows)
                    while (reader.Read()) {
                        string colName = reader.GetString("name");

                        try {
                            int colId = int.Parse(colName.Trim());
                            presentColumns.Add(colId);
                        } catch (FormatException) { }
                    }
            }

            // We might be unable to delete columns in SQlite, but we will try ;(
            var columnsToDelete = presentColumns.Where(colId => colId < 1 || colId > columnsNeeded);
            var columnsToAdd = Enumerable.Range(1, columnsNeeded).Except(presentColumns);

            // Add needed columns
            foreach (var col in columnsToAdd)
                new SQLiteCommand($"ALTER TABLE Данные ADD COLUMN [{col}] REAL;", connection).ExecuteNonQuery();

            // Remove extra columns
            foreach (var col in columnsToDelete)
                new SQLiteCommand($"ALTER TABLE Данные DROP COLUMN [{col}];", connection).ExecuteNonQuery();
        }

        private static void InsertPoints(SQLiteConnection connection, Project project) {
            // Delete old data
            var cmd = new SQLiteCommand($"DELETE FROM [Схема объекта];", connection);
            var reader = cmd.ExecuteNonQuery();

            var queryText = $"INSERT INTO [Схема объекта] (ID, X, Y, [Блок]) VALUES ";
            for (int point = 0; point < project.PointsCount; point++) {
                if (point > 0)
                    queryText += ", ";

                (var x, var y) = project.GetPointPos(point);
                queryText += $"({point}, {x}, {y}, {project.GetPointBlockId(point) ?? -1})";
            }
            queryText += ";";

            var affected = new SQLiteCommand(queryText, connection).ExecuteNonQuery();
            Debug.WriteLine($"Insert points: affected {affected}");
        }

        private static void RemoveExtraPoints(SQLiteConnection connection, int totalPoints) {
            new SQLiteCommand($"DELETE FROM [Схема объекта] WHERE ID >= {totalPoints} OR ID < 0;", connection)
                .ExecuteNonQuery();
        }

        private static void SaveParameter(SQLiteConnection connection, string name, double value) {
            var command = new SQLiteCommand(
                $"UPDATE  `Параметры` SET `Значение` = {value} WHERE `Имя` = '{name}';", connection);
            command.ExecuteNonQuery();
        }

        private static void CreateTables(SQLiteConnection connection) {
            string queryText = @$"
CREATE TABLE Данные (Эпоха INT UNIQUE);

CREATE TABLE Параметры (Имя TEXT UNIQUE, Значение REAL);

INSERT INTO Параметры (Имя, Значение) VALUES (""Погрешность"", 0.0004), (""Коэффициент доверия"", 0.9);

CREATE TABLE [Схема объекта] (ID INT UNIQUE, X REAL, Y REAL, Блок INT);";
            new SQLiteCommand("CREATE TABLE Данные (Эпоха INT UNIQUE);", connection).ExecuteNonQuery();
            new SQLiteCommand("CREATE TABLE Параметры (Имя TEXT UNIQUE, Значение REAL);", connection).ExecuteNonQuery();
            new SQLiteCommand("INSERT INTO Параметры (Имя, Значение) VALUES (\"Погрешность\", 0.0004), (\"Коэффициент доверия\", 0.9);", connection).ExecuteNonQuery();
            new SQLiteCommand("CREATE TABLE [Схема объекта] (ID INT UNIQUE, X REAL, Y REAL, Блок INT);", connection).ExecuteNonQuery();
        }
    }
}
