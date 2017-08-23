
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SNTSBackend.Semantics
{
    public static class Semantics {
        public static string[] CmpGen = { "<=", ">=", "==", "!=", "<", ">" };

        public static string[] LogicGen = { "AND", "OR" };

        public static DataTable CrossMultiplyTables(DataTable[] tables) => 
            (DataTable)Utils.Utils.CartesianProduct(tables.Cast<object>().ToArray(), 
                                                    (object x, object y) => Utils.Utils.CrossTable((DataTable)x, (DataTable)y));


        public static DataTable SelectWithoutWhere(DataColumn[] columnArray, DataTable[] tableArray, string[] tableNames = null) {
            var columnsInTable = tableArray.Select(t => t.Columns.Cast<DataColumn>().ToArray()).ToArray();
            var columnNamesDict = columnArray.ToDictionary(c => c.ColumnName, c => c);
            if(!(columnNamesDict.ContainsKey("PrimaryKey")))
            {
                columnNamesDict.Add("PrimaryKey", tableArray[0].PrimaryKey[0]);
            }
            
            var displayTable = new DataTable();
            var displayColumns = new List<DataColumn>();
            foreach (var columns in columnsInTable) {
                foreach (var column in columns) {
                    //Check the data type as well as the name of the column
                    if (columnNamesDict.ContainsKey(column.ColumnName) &&
                        columnNamesDict[column.ColumnName].DataType == column.DataType) {
                        var newColumn = new DataColumn(column.ColumnName);
                        newColumn.DataType = column.DataType;
                        displayTable.Columns.Add(newColumn);
                        displayColumns.Add(newColumn);
                    }
                }
            }


            //Currently supports one table
            //adding primary key to new table created
            displayTable.PrimaryKey = new DataColumn[] { displayTable.Columns["PrimaryKey"] };

            foreach (DataRow row in tableArray[0].Rows) {
                DataRow newRow = displayTable.NewRow();
                foreach(var cols in displayColumns) {
                    newRow[cols.ColumnName] = row[cols.ColumnName];
                }
                displayTable.Rows.Add(newRow);
            }
            return displayTable;  
        }

        public static DataTable SelectWithWhere(DataColumn[] columnArray, DataTable table, string[] tableNames = null)
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
            return Utils.Utils.CreateOutputTableFromEnumerable(rows);
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
            return Utils.Utils.CreateOutputTableFromRows(rows);
        }
    } 
    

}
    