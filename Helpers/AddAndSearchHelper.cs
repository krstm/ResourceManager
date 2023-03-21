using ResourceManager.Entities;
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

        public static void LoadResxFilesToDataGrid(DataGrid dataGrid, List<ResxFile> files)
        {
            var keys = files.SelectMany(x => x.KeysAndWordings.Keys).ToHashSet();
            var table = new DataTable();

            table.Columns.Add("Key", typeof(string));

            foreach (var file in files)
            {
                table.Columns.Add(file.FileName, typeof(string));
            }

            foreach (var key in keys)
            {
                var row = table.NewRow();
                row["Key"] = key;

                foreach (var file in files)
                {
                    if (file.KeysAndWordings.TryGetValue(key, out var value))
                    {
                        row[file.FileName] = value;
                    }
                }
                table.Rows.Add(row);
            }

            dataGrid.ItemsSource = table.DefaultView;
        }

        public static void SearchByKey(DataGrid dataGrid, string searchText)
        {
            var view = (DataView)dataGrid.Tag ?? (DataView)dataGrid.ItemsSource;
            var table = view.ToTable();

            if (table.Columns.Count < 1)
            {
                throw new ArgumentException("DataGrid must have at least one column.");
            }

            if (string.IsNullOrWhiteSpace(searchText))
            {
                dataGrid.ItemsSource = view;
                return;
            }

            var filterExpression = $"Key like '%{searchText}%'";

            var rows = table.Select(filterExpression);

            var resultTable = table.Clone();
            foreach (var row in rows)
            {
                resultTable.ImportRow(row);
            }

            dataGrid.ItemsSource = resultTable.DefaultView;

            dataGrid.Tag = view;
        }

        public static void SearchByValue(DataGrid dataGrid, string searchText)
        {
            var view = (DataView)dataGrid.Tag ?? (DataView)dataGrid.ItemsSource;
            var table = view.ToTable();

            if (table.Columns.Count < 1)
            {
                throw new ArgumentException("DataGrid must have at least one column.");
            }

            if (string.IsNullOrWhiteSpace(searchText))
            {
                dataGrid.ItemsSource = view;
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

            dataGrid.ItemsSource = resultTable.DefaultView;

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
    }
}