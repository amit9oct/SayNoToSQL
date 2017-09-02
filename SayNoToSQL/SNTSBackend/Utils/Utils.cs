using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.AST;
using Microsoft.ProgramSynthesis.Compiler;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Learning.Logging;
using Microsoft.ProgramSynthesis.Learning.Strategies;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.VersionSpace;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SNTSBackend.Utils
{
    public class Utils
    {
        /// <summary>
        ///     To load the grammar file
        /// </summary>
        /// <param name="grammarFile">Location of the grammar file</param>
        /// <returns></returns>
        public static Grammar LoadGrammar(string grammarFile)
        {
            var compilationResult = DSLCompiler.ParseGrammarFromFile(grammarFile);
            if (compilationResult.HasErrors)
            {
                WriteColored(ConsoleColor.Magenta, compilationResult.TraceDiagnostics);
                throw new Exception(compilationResult.Exception.InnerException.Message);
            }
            if (compilationResult.Diagnostics.Count > 0)
            {
                WriteColored(ConsoleColor.Yellow, compilationResult.TraceDiagnostics);
            }

            return compilationResult.Value;
        }

        public static void WriteColored(ConsoleColor color, object obj) => WriteColored(color, () => Console.WriteLine(obj));

        private static void WriteColored(ConsoleColor color, Action write)
        {
            ConsoleColor currentColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            write();
            Console.ForegroundColor = currentColor;
        }

        /// <summary>
        ///    Generates the powerset for the N rows in the table.
        /// </summary>
        /// <param name="table">The table whose powerset has to found</param>
        /// <returns>Array of tables which represents the set</returns>
        public static DataTable[] GeneratePowerSet(DataTable table)
        {
            List<DataTable> outputTables = new List<DataTable>();
            if (table != null)
            {
                int n = table.Rows.Count;

                // Power set contains 2^N subsets.
                int powerSetCount = 1 << n;

                for (int setMask = 1; setMask < powerSetCount; setMask++)
                {
                    DataTable outputTable = table.Clone();
                    for (int i = 0; i < n; i++)
                    {
                        // Checking whether i'th element of input collection should go to the current subset.
                        if ((setMask & (1 << i)) > 0)
                        {
                            outputTable.ImportRow(table.Rows[i]);
                        }
                    }
                    outputTables.Add(outputTable);
                }
            }
            return outputTables.ToArray();
        }

        public static DataTable CreateOutputTableFromRows(DataRow[] rows)
        {
            if (rows == null || rows.Length == 0)
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

        public static bool EqualColumns(DataTable table1, DataTable table2)
        {
            DataTable dt;
            dt = GetDifferentRecords(table1, table2);

            if (dt.Rows.Count == 0)
                return true;
            else
                return false;
        }

        //Found at http://canlu.blogspot.com/2009/05/how-to-compare-two-datatables-in-adonet.html
        public static DataTable GetDifferentRecords(DataTable FirstDataTable, DataTable SecondDataTable)
        {
            //Create Empty Table     
            DataTable ResultDataTable = new DataTable("ResultDataTable");

            //use a Dataset to make use of a DataRelation object     
            using (DataSet ds = new DataSet())
            {
                //Add tables     
                ds.Tables.AddRange(new DataTable[] { FirstDataTable.Copy(), SecondDataTable.Copy() });

                //Get Columns for DataRelation     
                DataColumn[] firstColumns = new DataColumn[ds.Tables[0].Columns.Count];
                for (int i = 0; i < firstColumns.Length; i++)
                {
                    firstColumns[i] = ds.Tables[0].Columns[i];
                }

                DataColumn[] secondColumns = new DataColumn[ds.Tables[1].Columns.Count];
                for (int i = 0; i < secondColumns.Length; i++)
                {
                    secondColumns[i] = ds.Tables[1].Columns[i];
                }

                //Create DataRelation     
                DataRelation r1 = new DataRelation(string.Empty, firstColumns, secondColumns, false);
                ds.Relations.Add(r1);

                DataRelation r2 = new DataRelation(string.Empty, secondColumns, firstColumns, false);
                ds.Relations.Add(r2);

                //Create columns for return table     
                for (int i = 0; i < FirstDataTable.Columns.Count; i++)
                {
                    ResultDataTable.Columns.Add(FirstDataTable.Columns[i].ColumnName, FirstDataTable.Columns[i].DataType);
                }

                //If FirstDataTable Row not in SecondDataTable, Add to ResultDataTable.     
                ResultDataTable.BeginLoadData();
                foreach (DataRow parentrow in ds.Tables[0].Rows)
                {
                    DataRow[] childrows = parentrow.GetChildRows(r1);
                    if (childrows == null || childrows.Length == 0)
                        ResultDataTable.LoadDataRow(parentrow.ItemArray, true);
                }

                //If SecondDataTable Row not in FirstDataTable, Add to ResultDataTable.     
                foreach (DataRow parentrow in ds.Tables[1].Rows)
                {
                    DataRow[] childrows = parentrow.GetChildRows(r2);
                    if (childrows == null || childrows.Length == 0)
                        ResultDataTable.LoadDataRow(parentrow.ItemArray, true);
                }
                ResultDataTable.EndLoadData();
            }

            return ResultDataTable;
        }


        public static DataTable[] GetAllPossibleTables(List<DataRow[]> completeRowsList, List<int> outputTableCount, DataTable inputTable)
        {
            var dataTableList = new List<DataTable>();
            var idx = 0;
            var preCrossDataRows = new List<DataRow[][]>();
            foreach (var dataRows in completeRowsList)
            {
                var possibleCombinations = NChooseR(dataRows, outputTableCount[idx]).ToArray();
                var possibleDataRows = new List<DataRow[]>();
                foreach (var combination in possibleCombinations)
                {
                    var newDataRows = combination.Cast<DataRow>().ToArray();
                    possibleDataRows.Add(newDataRows);
                }
                preCrossDataRows.Add(possibleDataRows.ToArray());
                idx++;
            }
            var crossRows = (NTuples[])CartesianProduct(preCrossDataRows.ToArray(), (object x, object y) => CrossDataRows((DataRow[][])x, (NTuples[])y));
            foreach(var tup in crossRows)
            {
                var newTable = inputTable.Clone();
                foreach(DataRow[] rowsPossible in tup.Tuple)
                {
                    foreach(var row in rowsPossible)
                        newTable.ImportRow(row);
                }
                dataTableList.Add(newTable);
            }
            return dataTableList.ToArray();
        }

        public static IEnumerable<HashSet<object>> NChooseR(IEnumerable<object> inputSet, int r)
        {
            int n = inputSet.Count();
            //Find all nCr subsets of the given set inputSet
            var answer = new HashSet<object>();
            if(r == 0)
            {
                yield return answer;
            }
            int idx = 0;
            foreach (var element in inputSet)
            {
                foreach (var combi in NChooseR(inputSet.Skip(idx+1), r - 1))
                {
                    combi.Add(element);
                    yield return combi;
                }
                idx++;
            }
        }

        public static NTuples[] CrossDataRows(DataRow[][] possibleRows, NTuples[] possibleRowsTuple = null)
        {
            if (possibleRowsTuple == null)
            {
                return possibleRows.Select(r => { var tup = new NTuples(1); tup.AddToTuple(r); return tup; }).ToArray();
            }
            else
            {
                var newPossibleRowTuple = new List<NTuples>();

                foreach(var rows in possibleRows)
                {
                    foreach (var rowTuple in possibleRowsTuple)
                    {
                        var newRowTuple = new NTuples(rowTuple.TupleSize+1);
                        foreach (var tup in rowTuple.Tuple)
                            newRowTuple.AddToTuple(tup);
                        newRowTuple.AddToTuple(rows);
                        newPossibleRowTuple.Add(newRowTuple);
                    }
                }
                return newPossibleRowTuple.ToArray();
            }
        }

        //public static List<DataRow>[] CrossDataRow(DataRow[] rows1, DataRow[] rows2) {
        //    var cross = new List<DataRow>[rows1.Length * rows2.Length];
            
        //}

        public static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }


        /// <summary>
        ///     Function to take cross product of two Clonable objects
        /// </summary>
        /// <param name="objects">The objects whose cross has to be taken</param>
        /// <param name="Cross">A callback method which can defines the crossing two objects</param>
        /// <returns></returns>
        public static object CartesianProduct(object[] objects, Func<object, object, object> Cross)
        {
            int crossDimension = objects.Length;
            if (crossDimension == 1)
            {
                return Cross(objects.First(), null);
            }
            var subObjects = objects.Skip(1).ToArray();
            var subCartesianProduct = CartesianProduct(subObjects, Cross);
            return Cross(objects.First(), subCartesianProduct);
        }

        public static DataTable CrossTable(DataTable table1, DataTable table2 = null)
        {
            if (table2 == null)
                return table1;
            if(table1.TableName == table2.TableName)
            {
                table2.TableName += "I";
            }

            var product = new DataTable($"{table1.TableName}_X_{table2.TableName}");
            var tupList = new List<NTuples>();
            
            foreach(var table in new[] { table1, table2})
            {
                foreach(DataColumn col in table.Columns)
                {
                    DataColumn newCol = new DataColumn($"{table.TableName}.{col.ColumnName}", col.DataType);
                    product.Columns.Add(newCol);
                }
            }

            foreach(DataRow table1Row in table1.Rows)
            {
                foreach(DataRow table2Row in table2.Rows)
                {
                    var tup = new NTuples(2);
                    tup.AddToTuple(table1Row);
                    tup.AddToTuple(table2Row);
                    tupList.Add(tup);
                }
            }

            foreach(var tup in tupList)
            {
                DataRow newRow = product.NewRow();
                foreach (DataRow ele in tup.Tuple)
                {
                    foreach (DataColumn col in ele.Table.Columns)
                    {
                        newRow[$"{ele.Table.TableName}.{col.ColumnName}"] = ele[col.ColumnName];
                    }

                }
                product.Rows.Add(newRow);
            }

            return product;
        }

    }
}
