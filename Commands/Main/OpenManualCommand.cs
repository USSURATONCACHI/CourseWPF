using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWPF.Commands.Main {
    public class OpenManualCommand : BaseCommand {
        public OpenManualCommand() { }

        public override void Execute(object? parameter) {
            var window = new Manual();
            window.Show();
        }
    }
}
