
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SNTSBackend.Semantics
{
    public static class Semantics {
        public static string[] CmpGen = { "<=", ">=", "==", "!=", "<", ">" };

        public static string[] LogicGen = { "AND", "OR" };

        public static string GetCmpSymbol(int index) {
            return CmpGen[index];
        }

        public static string GetLogicSymbol(int index) {
            return LogicGen[index];
        }

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
            var outputTable = SelectWithoutWhere(columnArray, new DataTable[] { table });
            return outputTable;
        }

        public static DataTable Logical(DataTable cmpStatement, DataTable condition, string logicSymbol)
        {
            IEnumerable<DataRow> rows;
            switch(logicSymbol)
            {
                case "AND":
                    rows = cmpStatement.AsEnumerable().Intersect(condition.AsEnumerable(), new DataTableCustomComparator());
                    break;
                case "OR":
                    rows = cmpStatement.AsEnumerable().Union(condition.AsEnumerable(), new DataTableCustomComparator());
                    break;
                default:
                    rows = null;
                    break;

            }
            return CreateOutputTableFromEnumerable(rows);
        }

    public static DataTable Comparator(DataColumn column,DataTable[] tableList,string cmpSymbol,object constValue){
            // Picks a value from the set of values present in the column
            DataRow[] rows;
            DataTable table = tableList[0];
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
            {
                rows = table.Select(column.ColumnName + mappedCompSymbol + constValue.ToString());
            }
            else
            {
                rows = table.Select(column.ColumnName + mappedCompSymbol + "'" + constValue.ToString() + "'");
            }
            return CreateOutputTableFromRows(rows);
        }
        public static DataTable CreateOutputTableFromRows(DataRow[] rows)
        {
            if(rows == null || rows.Length == 0)
            {
                return new DataTable();
            }
            else
            {
                return rows.CopyToDataTable();
            }
        }
        public static DataTable CreateOutputTableFromEnumerable(IEnumerable<DataRow> rows)
        {
            if (rows == null || rows.Count() == 0)
            { 
                return new DataTable();
            }
            else
            {
                return rows.CopyToDataTable();
            }
        } 
    } 
    

}
    