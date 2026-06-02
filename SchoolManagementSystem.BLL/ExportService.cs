using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;

namespace SchoolManagementSystem.BLL
{
    public class ExportService
    {
        // =========================
        // 🔵 Export DataTable Generic
        // =========================
        public void ExportToExcel(DataTable table, string filePath, string sheetName = "Report")
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add(sheetName);

            // Load Data
            worksheet.Cell(1, 1).InsertTable(table);

            worksheet.Columns().AdjustToContents();

            workbook.SaveAs(filePath);
        }

        // =========================
        // 🔵 Export List<T> Generic
        // =========================
        public void ExportToExcel<T>(List<T> data, string filePath, string sheetName = "Report")
        {
            var dt = ToDataTable(data);
            ExportToExcel(dt, filePath, sheetName);
        }

        // =========================
        // 🔵 Convert List → DataTable
        // =========================
        private DataTable ToDataTable<T>(List<T> items)
        {
            var table = new DataTable(typeof(T).Name);

            var props = typeof(T).GetProperties();

            foreach (var prop in props)
                table.Columns.Add(prop.Name);

            foreach (var item in items)
            {
                var values = new object[props.Length];

                for (int i = 0; i < props.Length; i++)
                    values[i] = props[i].GetValue(item) ?? "";

                table.Rows.Add(values);
            }

            return table;
        }
    }
}