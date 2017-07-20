using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualBasic.FileIO;

namespace SNTSBackend.Parser
{
    public static class CSVToDatatableParser
    { 
        public static DataTable Parse(String inputFile, Boolean headerPresent = true)
        {
            DataTable table = new DataTable();
            List<DataColumn> cols = new List<DataColumn>();
            using (TextFieldParser parser = new TextFieldParser(inputFile))
            {
                parser.TextFieldType = FieldType.Delimited;
                //hardcoded - should input
                parser.SetDelimiters(",");
                string[] headers;
                string[] firstLine;

                if(headerPresent)
                {
                    headers = parser.ReadFields(); //read header
                    firstLine = parser.ReadFields(); //read first line of values
                }
                else
                {
                    firstLine = parser.ReadFields(); //read first line of values - to get length
                    headers = Enumerable.Range(0, firstLine.Length).Select(n => n.ToString()).ToArray();
                }
                for(int i = 0; i < firstLine.Length; i++)
                {
                    Type type = DetectType(firstLine[i]);
                    cols.Add(new DataColumn(headers[i], type));
                }
                table.Columns.AddRange(cols.ToArray());

                DataRow row = table.NewRow();
                table.Rows.Add(CreateRow(row, headers, firstLine));
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    row = table.NewRow();
                    table.Rows.Add(CreateRow(row, headers, fields));
                }
            }
            return table;
        }

        public static void ShowTable(DataTable table)
        {
            foreach (DataColumn col in table.Columns)
            {
                Console.Write("{0,-14}", col.ColumnName);
            }
            Console.WriteLine();

            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn col in table.Columns)
                {
                    if (col.DataType.Equals(typeof(DateTime)))
                        Console.Write("{0,-14:d}", row[col]);
                    else if (col.DataType.Equals(typeof(Decimal)))
                        Console.Write("{0,-14:C}", row[col]);
                    else
                        Console.Write("{0,-14}", row[col]);
                }
                Console.WriteLine();
            }
            Console.WriteLine();
        }

        private static Type DetectType(String field)
        {
            Type type = typeof(String);
            Double res = 0;
            if (Double.TryParse(field, out res))
            {
                type = typeof(Double);
            }
            return type;
        }

        private static DataRow CreateRow(DataRow row, String[] headers, String[] fields)
        {
            for(int i = 0; i < headers.Length; i++)
            {
                row[headers[i]] = fields[i];
            }
            return row;
        }

    }
}
