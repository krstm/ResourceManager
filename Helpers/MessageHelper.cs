using System.Collections.Generic;
using System.Text;
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

        public static string GetDistinctDifferentNameDuplicateKeysMessage(Dictionary<string, List<string>> distinctDifferentNameDuplicateKeys)
        {
            StringBuilder messageBuilder = new StringBuilder();
            messageBuilder.AppendLine("Aşağıdaki key'lerin farklı değerleri vardır:");
            messageBuilder.AppendLine();

            foreach (KeyValuePair<string, List<string>> kvp in distinctDifferentNameDuplicateKeys)
            {
                messageBuilder.AppendLine($"Key: {kvp.Key}");

                foreach (string value in kvp.Value)
                {
                    messageBuilder.AppendLine($" - Value: {value}");
                }

                messageBuilder.AppendLine();
            }

            return messageBuilder.ToString();
        }
    }
}