
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
    }
}
