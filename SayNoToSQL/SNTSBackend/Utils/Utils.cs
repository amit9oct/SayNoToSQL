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

        public static bool EqualColumns(DataTable t1, DataTable t2, string columnName)
        {
            var c1= t1.AsEnumerable().Select(r => r.Field< object >(columnName)).ToList();
            c1.Sort();
            var c2 = t2.AsEnumerable().Select(r => r.Field<object>(columnName)).ToList();
            c2.Sort();
            return c1.Equals(c2);
        }

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
                return objects.First();
            }
            var subObjects = objects.Skip(1).ToArray();
            var subCartesianProduct = CartesianProduct(subObjects, Cross);
            return Cross(objects.First(), subCartesianProduct);
        }

        public static DataTable CrossTable(DataTable table1, DataTable table2)
        {
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
