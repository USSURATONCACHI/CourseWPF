using MdXaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CourseWPF {
    /// <summary>
    /// Логика взаимодействия для Manual.xaml
    /// </summary>
    public partial class Manual : Window {

        public string ManualText { get; set; }
        public Manual() {
            ManualText = File.ReadAllText("D:\\MyFiles\\dungeon 4\\Дока\\docs.md");
            InitializeComponent();
        }
    }
}
