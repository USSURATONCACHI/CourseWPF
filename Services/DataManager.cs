using CourseWPF.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWPF.Services {
    class DataManager {
        private static readonly string DATA_FOLDER = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string APP_FOLDER = "PointsAnalysis";

        public static DataManager Instance = new DataManager();

        public DataManager() { }

        public string[] GetAvailableProjects() {
            var entries = this.GetDirEntries();
            if (entries.Count() == 0)
                return new string[0];

            List<string> projects = new List<string>();
            foreach (var entry in entries) {
                //Debug.WriteLine("===== ENTRY: {entry}");
                projects.Add(entry.Split(new char[] { '/', '\\' }).Last());
            }
            return projects.ToArray();
        }

        public string GetFullProjectPath(string name) {
            return DATA_FOLDER + "/" + APP_FOLDER + "/" + name;
        }

        public void CreateProject(Project p) {
            throw new NotImplementedException();
        }

        private IEnumerable<string> GetDirEntries() {
            string path = DATA_FOLDER + "/" + APP_FOLDER;

            if (!Directory.Exists(path))
                return Array.Empty<string>();

            return Directory.EnumerateDirectories(path);
        }
    }
}
