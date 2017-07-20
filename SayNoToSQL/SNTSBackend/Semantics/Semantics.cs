
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SNTSBackend.Semantics
{
    public static class Semantics {
        public static DataTable SelectWithoutWhere(DataColumn[] columnArray, DataTable[] tableArray) {
            var columnsInTable = tableArray.Select(t => t.Columns.Cast<DataColumn>().ToArray()).ToArray();
            var columnNamesDict = columnArray.ToDictionary(c => c.ColumnName, c => c);
            var displayTable = new DataTable();
            var displayColums = new List<DataColumn>();
            foreach (var columns in columnsInTable) {
                foreach (var column in columns) {
                    //Check the data type as well as the name of the column
                    if (columnNamesDict.ContainsKey(column.ColumnName) &&
                        columnNamesDict[column.ColumnName].DataType == column.DataType) {
                        var newColumn = new DataColumn(column.ColumnName);
                        newColumn.DataType = column.DataType;
                        displayTable.Columns.Add(newColumn);
                        displayColums.Add(newColumn);
                    }
                }
            }
            //Currently supports one table
            foreach (DataRow row in tableArray[0].Rows) {
                DataRow newRow = displayTable.NewRow();
                foreach(var cols in displayColums) {
                    newRow[cols.ColumnName] = row[cols.ColumnName];
                }
                displayTable.Rows.Add(newRow);
            }
            return displayTable;  
        }

        public static DataTable SelectWithWhere(DataColumn[] columnArray, DataTable table)
        {
            var columnsInTable = table.Columns.Cast<DataColumn>().ToArray();
            HashSet<DataColumn> columnArrayHash = new HashSet<DataColumn>();
            columnArray.Select(c => columnArrayHash.Add(c));
            var displayTable = new DataTable();
            var displayColums = new List<DataColumn>();
            foreach (var column in columnsInTable)
            {
                    if (columnArrayHash.Contains(column))
                    {
                        displayTable.Columns.Add(column);
                        displayColums.Add(column);
                    }
            }
            foreach (DataRow row in table.Rows)
            {
                DataRow newRow = displayTable.NewRow();
                foreach (var cols in displayColums)
                {
                    newRow[cols.ColumnName] = row[cols.ColumnName];
                }
                displayTable.Rows.Add(newRow);
            }
            return displayTable;
        }

        //public static DataTable Logical(DataTable cmpStatement, @recurse[5] condition, logicSymbol)

        public static DataTable Comparator(DataColumn column,DataTable tableList,string cmpSymbol,object constValue){
            // Picks a value from the set of values present in the column
            var outputTable = new DataTable();
            DataTable table = tableList;//[0];
            string mappedCompSymbol;
            switch (cmpSymbol)
            {
                case "==": mappedCompSymbol = "=";
                    break;
                case "!=": mappedCompSymbol = "<>";
                    break;
                default: mappedCompSymbol = cmpSymbol;
                    break;
            }
            if (column.DataType == typeof(double))
            {   // Krishnan does not know C# but wrote this anyway
                outputTable = table.Select(column.ColumnName + mappedCompSymbol + constValue.ToString()).CopyToDataTable();
            }else
            {
                outputTable = table.Select(column.ColumnName + mappedCompSymbol + constValue.ToString()).CopyToDataTable();
            }
            ShowTable(outputTable);
            return outputTable;
                /*
                switch (cmpSymbol)
                {
                    case "==":;
                        break;
                    case "<=":;
                        break;
                    case ">=":
                        table.Select(column.ColumnName + ">=" + constValue.ToString());
                        break;
                    //case ">": break;
                    //case "<": break;
                    //case "!=": break;

                }*/
            }
        private static void ShowTable(DataTable table)
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
    }
    

}

        /* For the inverse:
            var x = table.Rows.Cast<DataRow>().Select(r => r[column.ColumnName]).Distinct().ToArray();
        */
    