using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ResourceManager.Helpers
{
    public static class ControlsHelper
    {
        public static void DisableControls(Window window, bool isEnabled)
        {
            foreach (Control control in FindVisualChildren<Control>(window))
            {
                control.IsEnabled = isEnabled;
            }
        }

        private static IEnumerable<T> FindVisualChildren<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                    if (child is T)
                    {
                        yield return (T)child;
                    }
                    foreach (T grandChild in FindVisualChildren<T>(child))
                    {
                        yield return grandChild;
                    }
                }
            }
        }

        public static async void UpdateButton(Button button, bool isUpdate)
        {
            if (button.Tag == null)
            {
                object oldContent = button.Content;
                if (isUpdate)
                {
                    button.Background = new SolidColorBrush(Color.FromRgb(255, 140, 0));
                    button.Content = "Güncellendi";
                }
                else
                {
                    button.Background = Brushes.Green;
                    button.Content = "Eklendi";
                }

                button.Tag = true;

                await Task.Delay(500);

                button.Background = Brushes.LightGray;
                button.Content = oldContent;

                button.MouseEnter += (s, e) =>
                {
                    button.Background = Brushes.Gray;
                    button.Content = oldContent;
                };
                button.MouseLeave += (s, e) =>
                {
                    button.Background = Brushes.LightGray;
                    button.Content = oldContent;
                };

                button.Tag = null;
            }
        }

        public static void SetEqualColumnWidths(DataGrid dataGrid)
        {
            if (dataGrid != null)
            {
                double columnWidth = dataGrid.ActualWidth / dataGrid.Columns.Count;

                foreach (var column in dataGrid.Columns)
                {
                    column.Width = new DataGridLength(columnWidth, DataGridLengthUnitType.Pixel);
                }
            }
        }
    }
}