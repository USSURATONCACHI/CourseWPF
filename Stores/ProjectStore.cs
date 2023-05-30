using CourseWPF.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWPF.Stores {
    public class ProjectStore {
		public event Action? ProjectChanged;

		private Project? _project = null;
		public Project? Project {
			get => _project;
			set {
				_project = value;
                ProjectChanged?.Invoke();
			}
		}

		public ProjectStore() {}
	}
}
