
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SNTSBackend.Semantics
{
    public static class Semantics {
        public static DataTable SelectWithoutWhere(DataColumn[] columnArray, DataTable[] tableArray) {
            var columnsInTable = tableArray.Select(t => t.Columns.Cast<DataColumn>().ToArray()).ToArray();
            HashSet<DataColumn> columnArrayHash = new HashSet<DataColumn>();
            columnArray.Select(c => columnArrayHash.Add(c));
            var displayTable = new DataTable();
            var displayColums = new List<DataColumn>();
            foreach (var columns in columnsInTable) {
                foreach (var column in columns) {
                    if (columnArrayHash.Contains(column)) {
                        displayTable.Columns.Add(column);
                        displayColums.Add(column);
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
