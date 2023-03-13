using ResourceManager.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace ResourceManager.Helpers
{
    public static class AddAndSearchHelper
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
