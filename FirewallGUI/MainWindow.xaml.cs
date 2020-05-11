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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;

namespace FirewallGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string inputFileNames { get; set; }

        public string outputFileName { get; set; }

        string loneFieldOption { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnAddFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            if(fileDialog.ShowDialog() == true)
            {
                addedFileList.Text = File.ReadAllText(fileDialog.FileName);
                inputFileNames += fileDialog.FileName + ',';
            }
        }

        private void startMerge_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
