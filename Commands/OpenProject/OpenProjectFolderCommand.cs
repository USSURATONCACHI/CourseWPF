using CourseWPF.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourseWPF.Commands.OpenProject
{
    class OpenProjectFolderCommand : BaseCommand
    {
        private OpenProjectViewModel _viewModel;
        public OpenProjectFolderCommand(OpenProjectViewModel vm)
        {
            _viewModel = vm;
        }

        public override void Execute(object? parameter)
        {
            throw new NotImplementedException();
        }
    }
}
