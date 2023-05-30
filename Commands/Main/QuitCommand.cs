using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CourseWPF.Commands.Main {
    public class QuitCommand : BaseCommand {
        public QuitCommand() { }
        public override void Execute(object? parameter) {
            Application.Current.Shutdown();
        }
    }
}
