using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;

namespace SNTSBackend.Parser
{
    public static class DatatableToCSVWriter
    {
        public static void Write(DataTable table, String outputFilePath)
        {
            List<String> fields = new List<string>();
            foreach(DataColumn col in table.Columns)
            {
                fields.Add(col.ColumnName);
            }
            using (StreamWriter sw = new StreamWriter(outputFilePath))
            {
                sw.WriteLine(String.Join(",", fields));
            }
            foreach (DataRow row in table.Rows)
            {
                fields = new List<string>();
                foreach (DataColumn col in table.Columns)
                {
                    fields.Add(row[col].ToString());
                }
                using (StreamWriter sw = File.AppendText(outputFilePath))
                {
                    sw.WriteLine(String.Join(",", fields));
                }
            }
        }
    }
}
