using CourseWPF.Model;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWPF.Services {
    public class ProjectLoader {
        public class ProjectLoaderException : Exception {
            public ProjectLoaderException(string? message) : base(message) { }
        }

        public static Project LoadFromPath(string folderPath) {
            List<List<double>> epochs;
            List<(double, double)> points;
            List<List<int>> blocks;
            double trustFactor, errorFactor;
            string imagePath = "";

            string? dbPath = FindDatabaseFilepath(folderPath);

            foreach (var file in Directory.GetFiles(folderPath)) {
                if (file.EndsWith(".png")) {
                    imagePath = file;
                    break;
                }
            }

            if (dbPath is null)
                throw new ProjectLoaderException($"No databases are found");

            using (SQLiteConnection connection = new($"Data Source={dbPath};Version=3;")) {
                connection.Open();
                epochs = LoadEpochs(connection);
                (blocks, points) = LoadBlocksAndPoints(connection);

                //Debug.WriteLine($"Blocks: [{String.Join(", ", blocks.Select(b => $"[{String.Join(", ", b)}]"))}]");

                trustFactor = (double) 
                    new SQLiteCommand("SELECT `Значение` FROM `Параметры` Where `Имя` = 'Коэффициент доверия';", connection)
                    .ExecuteScalar();

                errorFactor = (double)
                    new SQLiteCommand("SELECT `Значение` FROM `Параметры` Where `Имя` = 'Погрешность';", connection)
                    .ExecuteScalar();
            }

            return new Project(epochs, points, blocks, trustFactor, errorFactor) { SavePath = folderPath, ImagePath = imagePath };
        }

        private static (List<List<int>>, List<(double, double)>) LoadBlocksAndPoints(SQLiteConnection connection) {
            SQLiteCommand command = new("SELECT X, Y, Блок FROM `Схема объекта` ORDER BY `ID`;", connection);

            List<(double, double)> points = new();
            List<int> blockIds = LoadLocalBlockIds(connection);
            List<List<int>> blocks = blockIds.Select(id => new List<int>()).ToList();

            using (SQLiteDataReader reader = command.ExecuteReader()) {
                if (reader.HasRows) {
                    while (reader.Read()) {
                        // Add point to corresponding block
                        var blockId = blockIds.IndexOf(reader.GetInt32(2));
                        if (blockId >= 0)
                            blocks[blockId].Add(points.Count);

                        // Save point
                        points.Add((reader.GetDouble(0), reader.GetDouble(1)));
                    }
                }
            }
            return (blocks, points);
        }

        private static List<int> LoadLocalBlockIds(SQLiteConnection connection) {
            List<int> blockIds = new();

            SQLiteCommand command = new("SELECT DISTINCT [Блок] FROM [Схема объекта];", connection);
            using (SQLiteDataReader reader = command.ExecuteReader()) {
                if (reader.HasRows)
                    while (reader.Read()) {
                        int value = reader.GetInt32(0);
                        if (value >= 0)
                            blockIds.Add(value);
                    }
            }
            blockIds.Sort();
            return blockIds;
        }

        private static List<List<double>> LoadEpochs(SQLiteConnection connection) {
            SQLiteCommand command = new("SELECT * FROM `Данные` ORDER BY `Эпоха`;", connection);
            List<List<double>> result = new();
            uint pointsCount = 0;

            using (SQLiteDataReader reader = command.ExecuteReader()) {
                if (reader.HasRows) {
                    while (reader.Read()) {
                        pointsCount = (uint) (reader.FieldCount - 1);
                        double[] epoch = new double[pointsCount];

                        for (int i = 1; i <= pointsCount; i++) {
                            epoch[i - 1] = reader.GetFieldValue<double>(i);
                            //Debug.WriteLine($"Value: {reader.GetFieldValue<double>(i)}");
                        }

                        result.Add(epoch.ToList());
                    }
                }
            }

            return result;
        }


        public static string? FindDatabaseFilepath(string path) {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
            if (!Directory.Exists(path))
                throw new ProjectLoaderException($"Directory {path} does not exists");

            string? database_path = null;
            foreach (var _file in Directory.EnumerateFiles(path)) {
                if (_file.EndsWith(".sqlite")) {
                    if (database_path is null)
                        database_path = _file;
                    else
                        throw new ProjectLoaderException($"Multiple databases are found");
                }
            }

            if (!File.Exists(database_path))
                return null;

            return database_path;
        }
    }
}
