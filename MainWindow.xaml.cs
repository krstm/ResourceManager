using ResourceManager.Entities;
using ResourceManager.Helpers;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml;

namespace ResourceManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static List<ResxFile> resxFiles = new List<ResxFile>();
        private static bool istbxSearchKeyTextChangedEventEnabled = true;
        private static bool istbxSearchWordingTextChangedEventEnabled = true;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AppSettingsHelper.LoadWindowSize(this);
            var fileList = AppSettingsHelper.LoadStringListFromJsonFile();

            foreach (var filePath in fileList)
            {
                cbxFiles.Items.Add(filePath);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            AppSettingsHelper.SaveWindowSize(this);
            var fileList = resxFiles.Select(x => x.FilePath).ToList();
            AppSettingsHelper.SaveStringListToJsonFile(fileList);
        }

        private void tbxFilePathGotFocus(object sender, RoutedEventArgs e)
        {
            cbxFiles.Visibility = Visibility.Visible;
        }

        private void cbxFilesSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbxFiles.SelectedIndex != -1)
            {
                string selectedItem = cbxFiles.SelectedItem as string;
                if (selectedItem != null)
                {
                    tbxFilePath.Text = selectedItem;
                    cbxFiles.Visibility = Visibility.Collapsed;
                }
            }
        }
        public bool ImportFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath) || Path.GetExtension(filePath) != ".resx")
            {
                return false;
            }

            var filePaths = resxFiles.Select(x => x.FilePath).ToHashSet();

            if (filePaths.Contains(filePath))
            {
                return false;
            }

            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(filePath);
            int count = 2;

            string fileName = AddAndSearchHelper.RemovePunctuation(fileNameWithoutExt);
            var fileNames = resxFiles.Select(x => x.FileName).ToHashSet();
            while (fileNames.Contains(fileName))
            {
                fileName = fileNameWithoutExt + "_" + count.ToString();
                count++;
            }

            try
            {
                if (ResxHelper.HasDuplicateKeys(filePath))
                {
                    ResxHelper.RemoveDuplicateResources(filePath);
                }

                var keysAndWordings = ResxHelper.ReadResXFile(filePath);

                resxFiles.Add(new ResxFile() { FileName = fileName, FilePath = filePath, KeysAndWordings = keysAndWordings });
                AddRadioButtonToStackPanel(fileName);

                AddAndSearchHelper.LoadResxFilesToDataGrid(dtgResxDatas, resxFiles);

                AppSettingsHelper.SetEqualColumnWidths(dtgResxDatas);

                return true;
            }
            catch (System.Exception)
            {

                return false;
            }
        }

        private void btnAddResx_Click(object sender, RoutedEventArgs e)
        {
            var isImported = ImportFile(tbxFilePath.Text);
            if (isImported)
            {
                ControlsHelper.UpdateButton((Button)sender, false);
            }
        }

        private void tbxSearchWording_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (istbxSearchWordingTextChangedEventEnabled)
            {
                istbxSearchKeyTextChangedEventEnabled = false;
                tbxSearchKey.Text = string.Empty;
                istbxSearchKeyTextChangedEventEnabled = true;
                AddAndSearchHelper.SearchByValue(dtgResxDatas, tbxSearchWording.Text);
                AppSettingsHelper.SetEqualColumnWidths(dtgResxDatas);
            }
        }

        private void tbxSearchKey_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (istbxSearchKeyTextChangedEventEnabled)
            {
                istbxSearchWordingTextChangedEventEnabled= false;
                tbxSearchWording.Text = string.Empty;
                istbxSearchWordingTextChangedEventEnabled = true;
                AddAndSearchHelper.SearchByKey(dtgResxDatas, tbxSearchKey.Text);
                AppSettingsHelper.SetEqualColumnWidths(dtgResxDatas);
            }
        }

        public void AddRadioButtonToStackPanel(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return;
            }

            var radioButton = new RadioButton
            {
                Content = content,
                Margin = new Thickness(0, 0, 20, 0),
            };

            spRadioButtons.Children.Add(radioButton);
        }

        public string GetSelectedRadioButtonContent()
        {
            foreach (var child in spRadioButtons.Children)
            {
                if (child is RadioButton radioButton && radioButton.IsChecked == true)
                {
                    return radioButton.Content.ToString();
                }
            }

            return null;
        }


        private void btnKeyWordingAddOrUpdate_Click(object sender, RoutedEventArgs e)
        {
            var selectedFile = GetSelectedRadioButtonContent();
            if (selectedFile == null)
            {
                return;
            }
            var filePath = resxFiles.Where(x => x.FileName == selectedFile).Select(x => x.FilePath).FirstOrDefault();
            if (filePath == null)
            {
                return;
            }
            var isUpdate = ResxHelper.AddResource(filePath, tbxAddKey.Text, tbxAddWording.Text);
            ControlsHelper.UpdateButton(btnKeyWordingAddOrUpdate, isUpdate);
        }

        private void dtgResxDatas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AppSettingsHelper.SetEqualColumnWidths(dtgResxDatas);
        }

        private void dtgResxDatas_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DataGrid dataGrid = sender as DataGrid;

            if (dataGrid != null && dataGrid.SelectedItems != null && dataGrid.SelectedItems.Count == 1)
            {
                var selectedRow = dataGrid.SelectedItem as DataRowView;
                var headerName = dataGrid.CurrentColumn.Header.ToString();

                if (selectedRow != null && headerName != "Key")
                {
                    string keyColumnValue = selectedRow["Key"].ToString();
                    string selectedColumnValue = selectedRow[dataGrid.CurrentColumn.DisplayIndex].ToString();

                    tbxAddKey.Text = keyColumnValue;
                    tbxAddWording.Text = selectedColumnValue;

                    foreach (RadioButton radioButton in spRadioButtons.Children)
                    {
                        if (radioButton.Content.ToString() == headerName)
                        {
                            radioButton.IsChecked = true;
                            break;
                        }
                    }
                }
            }
        }
    }
}
