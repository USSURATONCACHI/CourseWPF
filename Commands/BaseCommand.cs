﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CourseWPF.Commands {
    public abstract class BaseCommand : ICommand {
        public event EventHandler? CanExecuteChanged;

        public virtual bool CanExecute(object? parameter) => true;
        public abstract void Execute(object? parameter);

        public virtual void InvokeCanExecuteChanged() {
            CanExecuteChanged?.Invoke(this, new());
        }
    }
}
