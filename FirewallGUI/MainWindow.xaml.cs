using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using FirewallLibrary;

namespace FirewallGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string inputFileNames { get; set; }

        private string outputFileName { get; set; }

        private string loneFieldOption { get; set; }

        public MainWindow()
        {
            InitializeComponent();
        }
        
        // Opens a windows file explorer box
        private void btnAddFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            if(fileDialog.ShowDialog() == true)
            {
                string fileName = fileDialog.FileName;
                inputFileNames += fileName + ',';
                FileListTextBlock.Text += fileDialog.SafeFileName;
                addedFilePreview.Text = File.ReadAllText(fileName);
            }

            
        }
        // Starts file processing on click
        private async void btnStartMerge_Click(object sender, RoutedEventArgs e)
        {
            inputFileNames = inputFileNames.TrimEnd(',');

            outputFileName = outputFileTextBox.Text;

            Task fileProcessing;

            if (mergeOptionCheckBox.IsChecked == true)
            {
                loneFieldOption = mergeFieldOptionCbox.Text;
                fileProcessing = FileHandler.ProcessFilesAsync(inputFileNames, outputFileName, loneFieldOption);
            }
            else
            {
                fileProcessing = FileHandler.ProcessFilesAsync(inputFileNames, outputFileName);
            }
            Console.WriteLine("Processing file, this shouldn't take long.");

            await fileProcessing.ConfigureAwait(false);

            aboveButtonTextBlock.Text = "Files processed";
            btnStartMerge.Content = "Restart";
            fileOutputPreview.Text = File.ReadAllText(outputFileName);
        }
        //Ensure we have an input file and output file before allowing operations
        private void outputFileTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if(outputFileTextBox.Text != "" && inputFileNames != "")
            {
                btnStartMerge.IsEnabled = true;
            }
        }
    }
}
