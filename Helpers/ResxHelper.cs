using ResourceManager.Entities;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;

namespace ResourceManager.Helpers
{
    public static class ResxHelper
    {
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

        public static Encoding GetEncoding(string filePath)
        {
            using (var reader = new StreamReader(filePath, Encoding.Default, true))
            {
                if (reader.Peek() >= 0)
                {
                    reader.Read();
                }

                return reader.CurrentEncoding;
            }
        }

        public static Dictionary<string, string> ReadResXFile(string filePath)
        {
            Dictionary<string, string> resources = new Dictionary<string, string>();

            XElement resxFile = XElement.Load(filePath);

            foreach (XElement element in resxFile.Elements("data"))
            {
                string key = element.Attribute("name").Value;
                string wording = element.Element("value").Value;

                resources.Add(key, wording);
            }

            return resources;
        }

        public static void RemoveDuplicateResources(string filePath)
        {
            Hashtable resourceTable = new Hashtable();
            XmlDocument resxDoc = new XmlDocument();
            resxDoc.Load(filePath);
            foreach (XmlNode node in resxDoc.SelectNodes("root/data"))
            {
                string key = node.Attributes["name"].Value;
                if (!resourceTable.ContainsKey(key))
                {
                    resourceTable.Add(key, node);
                }
                else
                {
                    node.ParentNode.RemoveChild(node);
                }
            }

            using (StreamWriter streamWriter = new StreamWriter(filePath, false, GetEncoding(filePath)))
            {
                resxDoc.Save(streamWriter);
                streamWriter.Flush();
                streamWriter.Close();
            }
        }

        public static List<string> GetDuplicateKeys(string filePath)
        {
            XmlDocument resxDoc = new XmlDocument();
            resxDoc.Load(filePath);

            var allKeys = new HashSet<string>();
            var duplicateKeys = new HashSet<string>();

            foreach (XmlNode node in resxDoc.SelectNodes("root/data"))
            {
                string key = node.Attributes["name"].Value;

                if (!allKeys.Add(key))
                {
                    duplicateKeys.Add(key);
                }
            }

            return new List<string>(duplicateKeys);
        }

        public static Dictionary<string, List<string>> FindDistinctDifferentNameDuplicateKeys(string filePath)
        {
            XmlDocument resxDoc = new XmlDocument();
            resxDoc.Load(filePath);

            var distinctDuplicateKeys = new Dictionary<string, List<string>>();

            foreach (XmlNode node in resxDoc.SelectNodes("root/data"))
            {
                string key = node.Attributes["name"].Value;
                string value = node.InnerText;

                if (!distinctDuplicateKeys.ContainsKey(key))
                {
                    distinctDuplicateKeys.Add(key, new List<string> { value });
                }
                else
                {
                    if (!distinctDuplicateKeys[key].Contains(value))
                    {
                        distinctDuplicateKeys[key].Add(value);
                    }
                }
            }

            var differentNameDuplicateKeys = distinctDuplicateKeys.Where(kvp => kvp.Value.Count > 1)
                                                                   .ToDictionary(kvp => kvp.Key,
                                                                                 kvp => kvp.Value.Distinct().ToList());

            return differentNameDuplicateKeys;
        }
    }
}