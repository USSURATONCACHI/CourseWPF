using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWPF.Commands {
    public class CallbackCommand : BaseCommand {
        private  Action<object?> _callback;
        
        public CallbackCommand(Action<object?> callback) {
            _callback = callback;
        }

        public override void Execute(object? parameter) {
            _callback.Invoke(parameter);
        }
    }
}
