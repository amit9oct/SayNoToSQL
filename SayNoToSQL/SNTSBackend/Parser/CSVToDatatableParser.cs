using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic.FileIO;

namespace SNTSBackend.Parser
{
    class CSVToDatatableParser
    { 
        public DataTable parse(String inputFile, Boolean headerPresent)
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
                    Type type = detectType(firstLine[i]);
                    cols.Add(new DataColumn(headers[i], type));
                }
                table.Columns.AddRange(cols.ToArray());

                DataRow row = table.NewRow();
                table.Rows.Add(createRow(row, headers, firstLine));
                while (!parser.EndOfData)
                {
                    string[] fields = parser.ReadFields();
                    row = table.NewRow();
                    table.Rows.Add(createRow(row, headers, fields));
                }
            }
            return table;
        }
        public Type detectType(String field)
        {
            Type type = typeof(String);
            Double res = 0;
            if (Double.TryParse(field, out res))
            {
                type = typeof(Double);
            }
            return type;
        }
        public DataRow createRow(DataRow row, String[] headers, String[] fields)
        {
            for(int i = 0; i < headers.Length; i++)
            {
                row[headers[i]] = fields[i];
            }
            return row;
        }

    }
}
