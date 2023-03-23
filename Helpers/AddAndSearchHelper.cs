using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Xml;

namespace ResourceManager.Helpers
{
    public static class AddAndSearchHelper
    {
        public static bool AddResource(string filePath, string key, string value)
        {
            XmlDocument resxDoc = new XmlDocument();
            resxDoc.Load(filePath);

            bool resourceExists = false;
            foreach (XmlNode node in resxDoc.SelectNodes("root/data"))
            {
                if (node.Attributes["name"].Value == key)
                {
                    node.SelectSingleNode("value").InnerText = value;
                    resourceExists = true;
                    break;
                }
            }

            if (!resourceExists)
            {
                XmlNode newNode = resxDoc.CreateNode(XmlNodeType.Element, "data", null);
                XmlAttribute nameAttribute = resxDoc.CreateAttribute("name");
                nameAttribute.Value = key;
                newNode.Attributes.Append(nameAttribute);
                XmlAttribute spaceAttribute = resxDoc.CreateAttribute("xml", "space", "http://www.w3.org/XML/1998/namespace");
                spaceAttribute.Value = "preserve";
                newNode.Attributes.Append(spaceAttribute);
                XmlNode valueNode = resxDoc.CreateNode(XmlNodeType.Element, "value", null);
                valueNode.InnerText = value;
                newNode.AppendChild(valueNode);
                resxDoc.SelectSingleNode("root").AppendChild(newNode);
            }

            using (StreamWriter streamWriter = new StreamWriter(filePath, false, ResxHelper.GetEncoding(filePath)))
            {
                resxDoc.Save(streamWriter);
                streamWriter.Flush();
                streamWriter.Close();
            }

            return resourceExists;
        }

        public static void SearchByKey(DataGrid dataGrid, string searchText)
        {
            var view = (DataView)dataGrid.Tag ?? (DataView)dataGrid.ItemsSource;
            var table = view.ToTable();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                dataGrid.ItemsSource = view;
                dataGrid.Tag = view;
                return;
            }

            var filterExpression = $"Key like '%{searchText}%'";

            var rows = table.Select(filterExpression);

            var resultTable = table.Clone();
            foreach (var row in rows)
            {
                resultTable.ImportRow(row);
            }

            if (resultTable.Rows.Count == 0)
            {
                dataGrid.ItemsSource = CreateNotFoundRow(table).DefaultView;
            }
            else
            {
                dataGrid.ItemsSource = resultTable.DefaultView;
            }

            dataGrid.Tag = view;
        }

        public static void SearchByValue(DataGrid dataGrid, string searchText)
        {
            var view = (DataView)dataGrid.Tag ?? (DataView)dataGrid.ItemsSource;
            var table = view.ToTable();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                dataGrid.ItemsSource = view;
                dataGrid.Tag = view;
                return;
            }

            var rows = table.AsEnumerable().Where(row =>
            {
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    if (row[i].ToString().IndexOf(searchText, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        return true;
                    }
                }
                return false;
            });

            var resultTable = table.Clone();
            foreach (var row in rows)
            {
                resultTable.ImportRow(row);
            }

            if (resultTable.Rows.Count == 0)
            {
                dataGrid.ItemsSource = CreateNotFoundRow(table).DefaultView;
            }
            else
            {
                dataGrid.ItemsSource = resultTable.DefaultView;
            }

            dataGrid.Tag = view;
        }

        public static string RemovePunctuation(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var sb = new StringBuilder();
                foreach (var c in value)
                {
                    if (!char.IsPunctuation(c))
                    {
                        sb.Append(c);
                    }
                }
                value = sb.ToString();
            }
            return value;
        }

        public static string GetNonAlphanumericChars(string input)
        {
            HashSet<char> nonAlphanumericChars = new HashSet<char>();

            foreach (char c in input)
            {
                if ((c < 'a' || c > 'z') && (c < 'A' || c > 'Z') && (c < '0' || c > '9'))
                {
                    nonAlphanumericChars.Add(c);
                }
            }

            StringBuilder sb = new StringBuilder();
            foreach (char c in nonAlphanumericChars)
            {
                sb.Append(string.Format("'{0}',", c));
            }

            if (sb.Length > 0)
            {
                sb.Length--;
            }

            return sb.ToString();
        }

        private static DataTable CreateNotFoundRow(DataTable table)
        {
            var notFoundTable = table.Clone();
            var notFoundRow = notFoundTable.NewRow();

            for (int i = 0; i < notFoundTable.Columns.Count; i++)
            {
                notFoundRow[i] = string.Empty;
            }

            notFoundTable.Rows.Add(notFoundRow);
            return notFoundTable;
        }

        public static void AddOrUpdateDictionaryEntry(Dictionary<string, string> dictionary, string key, string wording)
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = wording;
            }
            else
            {
                dictionary.Add(key, wording);
            }
        }
    }
}