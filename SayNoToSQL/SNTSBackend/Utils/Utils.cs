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
        public static Grammar LoadGrammar(string grammarFile)
        {
            var compilationResult = DSLCompiler.ParseGrammarFromFile(grammarFile);
            if (compilationResult.HasErrors)
            {
                WriteColored(ConsoleColor.Magenta, compilationResult.TraceDiagnostics);
                throw new Exception("Grammar file has compilation errors!!");
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
        public static ProgramNode GetBestProgram(ProgramNode[] generatedPrograms, DataTable initialTable)
        {
            int count = Int32.MaxValue;
            int tempCount = 0;
            ProgramNode selectedProgramNode = generatedPrograms[0];
            foreach (var programNode in generatedPrograms)
            {
                DataTable temp = Learner.Instance.Invoke(programNode, initialTable);

                if (count > programNode.ToString().Replace("==", "=").Replace(">=", "=").Replace("<=", "=").Length)
                {
                    Console.WriteLine(programNode.ToString());
                    if (temp.Rows.Count > tempCount)
                    {
                        count = programNode.ToString().Length;
                        selectedProgramNode = programNode;
                    }
                    tempCount = temp.Rows.Count;

                }
            }
            return selectedProgramNode;

        }


    }
}
