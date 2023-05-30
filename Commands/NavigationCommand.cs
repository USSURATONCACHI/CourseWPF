using CourseWPF.Stores;
using CourseWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWPF.Commands {
    internal class NavigationCommand : BaseCommand {
        private NavigationStore _navigationStore;
        private Func<object?, ViewModelBase> _generator;
        public NavigationCommand(NavigationStore ns, Func<object?, ViewModelBase> generator) {
            _navigationStore = ns;
            _generator = generator;
        }

        public override void Execute(object? parameter) {
            _navigationStore.CurrentViewModel = _generator(parameter);
        }
    }
}
