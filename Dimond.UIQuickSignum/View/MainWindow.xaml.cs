using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Dimond.QuickSignum;
using UIQuickSignum.ViewModel;

namespace UIQuickSignum
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
            UiQuickSignumViewModel = new UiQuickSignumViewModel();

			InitializeComponent();

            Closing += (sender, args) => UiQuickSignumViewModel.Dispose();
		}

        public UiQuickSignumViewModel UiQuickSignumViewModel { get; set; }

	    private void DataGrid_Documents_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
	    {
	        e.Handled = true;
	    }
	}
}
