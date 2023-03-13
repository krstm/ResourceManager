using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace ResourceManager.Helpers
{
    public static class ResxHelper
    {
        private static Encoding GetEncoding(string filePath)
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

            using (StreamWriter streamWriter = new StreamWriter(filePath, false, GetEncoding(filePath)))
            {
                resxDoc.Save(streamWriter);
                streamWriter.Flush();
                streamWriter.Close();
            }

            return resourceExists;
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
        public static bool HasDuplicateKeys(string filePath)
        {
            XmlDocument resxDoc = new XmlDocument();
            resxDoc.Load(filePath);

            var allKeys = new HashSet<string>();

            foreach (XmlNode node in resxDoc.SelectNodes("root/data"))
            {
                string key = node.Attributes["name"].Value;

                if (!allKeys.Add(key))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
