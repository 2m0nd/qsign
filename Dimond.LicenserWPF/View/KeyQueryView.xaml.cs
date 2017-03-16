using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Dimond.LicenserWPF.ViewModel;

namespace Dimond.LicenserWPF.View
{
    /// <summary>
    /// Interaction logic for KeyQueryView.xaml
    /// </summary>
    public partial class KeyQueryView : UserControl
    {
        public KeyQueryView()
        {
            InitializeComponent();
            DataContext = new KeyQueryViewModel();
        }
    }
}
