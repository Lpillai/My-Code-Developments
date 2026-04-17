using OfficeOpenXml;
using RuntimeVariables;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace SharedScrpits
{
    public class Convertor_Share
    {
        public byte[] ToByteArray(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }


        public DataTable ToDataTable(ExcelWorksheet pmSheet)
        {
            DataTable table = new DataTable();

            foreach (var firstRowCell in pmSheet.Cells[1, 1, 1, pmSheet.Dimension.End.Column])
                table.Columns.Add(firstRowCell.Text);

            for (var rowNumber = 2; rowNumber <= pmSheet.Dimension.End.Row; rowNumber++)
            {
                if (pmSheet.Cells[rowNumber, 2].Value != null && pmSheet.Cells[rowNumber, 3].Value != null)
                {
                    var row = pmSheet.Cells[rowNumber, 1, rowNumber, pmSheet.Dimension.End.Column];
                    var newRow = table.NewRow();
                    foreach (var cell in row)
                        newRow[cell.Start.Column - 1] = cell.Text;

                    table.Rows.Add(newRow);
                }
            }

            return table;
        }


        public IEnumerable<SelectListItem> ToDropDownOptions(List<CodeValue> pmList)
        {
            var selectList = new List<SelectListItem>();

            foreach (var element in pmList)
            {
                selectList.Add(new SelectListItem
                {
                    Value = element.Code,
                    Text = element.Value
                });
            }

            return selectList;
        }

    }
}
