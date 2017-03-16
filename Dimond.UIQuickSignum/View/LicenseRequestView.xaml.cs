using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using UIQuickSignum.ViewModel;

namespace UIQuickSignum.View
{
    /// <summary>
    /// Interaction logic for LicenseRequestView.xaml
    /// </summary>
    public partial class LicenseRequestView : Window
    {
        public LicenseRequestViewModel ViewModel { get; set; }

        public LicenseRequestView(string serialNumber)
        {
            InitializeComponent();
            ViewModel = new LicenseRequestViewModel(this, serialNumber);

            DataContext = ViewModel;
        }

        public string OpenRequestDialog()
        {
            this.Topmost = true;
            this.ShowDialog();
            if (!string.IsNullOrWhiteSpace(ViewModel.LicenseKey) && DialogResult.Value)
                return ViewModel.LicenseKey;
            return string.Empty;
        }
    }
}
