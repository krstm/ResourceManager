using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace ResourceManager.Helpers
{
    public static class AppSettingsHelper
    {
        private static readonly string _settingsFilePath = "appsettings.json";
        private static readonly string _fileListFilePath = "filelist.json";

        public static void LoadWindowSize(Window window)
        {
            if (File.Exists(_settingsFilePath))
            {
                string json = File.ReadAllText(_settingsFilePath);
                dynamic settings = JsonSerializer.Deserialize<JsonElement>(json).GetProperty("Settings");
                double width = settings.GetProperty("Width").GetDouble();
                double height = settings.GetProperty("Height").GetDouble();
                double left = settings.GetProperty("Left").GetDouble();
                double top = settings.GetProperty("Top").GetDouble();
                if (width > 0 && height > 0)
                {
                    window.Width = width;
                    window.Height = height;
                    window.Left = left;
                    window.Top = top;
                }
            }
            else
            {
                SaveWindowSize(window);
            }
        }

        public static void SaveWindowSize(Window window)
        {
            dynamic settings = new
            {
                Settings = new
                {
                    Width = window.Width,
                    Height = window.Height,
                    Left = window.Left,
                    Top = window.Top
                }
            };
            string json = JsonSerializer.Serialize(settings);
            File.WriteAllText(_settingsFilePath, json);
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

        public static void SaveStringListToJsonFile(List<string> stringList)
        {
            List<string> existingList = LoadStringListFromJsonFile();
            IEnumerable<string> newItems = stringList.Except(existingList);
            List<string> updatedList = existingList.Concat(newItems).ToList();
            dynamic settings = new
            {
                Settings = new
                {
                    StringList = updatedList
                }
            };
            string json = JsonSerializer.Serialize(settings);
            File.WriteAllText(_fileListFilePath, json);
        }

        public static List<string> LoadStringListFromJsonFile()
        {
            if (File.Exists(_fileListFilePath))
            {
                string json = File.ReadAllText(_fileListFilePath);
                dynamic settings = JsonSerializer.Deserialize<JsonElement>(json).GetProperty("Settings");
                List<string> stringList = new List<string>();
                if (settings.TryGetProperty("StringList", out JsonElement jsonElement))
                {
                    foreach (JsonElement element in jsonElement.EnumerateArray())
                    {
                        stringList.Add(element.GetString());
                    }
                }
                return stringList;
            }
            else
            {
                return new List<string>();
            }
        }
    }
}