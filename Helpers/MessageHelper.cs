using System.Windows;

namespace ResourceManager.Helpers
{
    public class MessageHelper
    {
        public static void ShowErrorMessage(string message)
        {
            MessageBox.Show(message, "Hata", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static bool ShowYesNoMessage(string message)
        {
            MessageBoxResult result = MessageBox.Show(message, "Soru", MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result == MessageBoxResult.Yes;
        }

        public static void ShowInformationMessage(string message)
        {
            MessageBox.Show(message, "Bilgi", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public static void ShowWarningMessage(string message)
        {
            MessageBox.Show(message, "Uyarı", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

    }
}
