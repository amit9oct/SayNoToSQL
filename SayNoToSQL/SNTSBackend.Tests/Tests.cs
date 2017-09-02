using Microsoft.VisualStudio.TestTools.UnitTesting;
using SNTSBackend.Parser;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace SNTSBackend.Tests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void CsvToDataTableConversionTest() {
            DataTable table = CSVToDatatableParser.Parse(@"TestCases\Parser\Table.csv");
            Assert.IsTrue(table.Columns.Count == 3);
            Assert.IsTrue(table.Rows.Count == 5);
            Assert.IsTrue((string)table.Rows[2]["Name"] == "Amitayush");
            Assert.IsTrue((string)table.Rows[2]["Uni"] == "Pilani");
            Assert.IsTrue((double)table.Rows[2]["Age"] == 23);
        }

        [TestMethod]
        public void CsvToDataTableConversionOutputTest()
        {
            DataTable table = CSVToDatatableParser.Parse(@"TestCases\Parser\Table.csv");
            DataTable outputTable = CSVToDatatableParser.Parse(@"TestCases\Parser\TableOutput.csv");
            Assert.AreEqual(table.Rows[2][0],outputTable.Rows[1][0]);
            
        }

        [TestMethod]
        public void SelectWithoutWhereTest() {
            DataTable table = CSVToDatatableParser.Parse(@"TestCases\Semantics\TableInput.csv");
            DataColumn[] columns = {
                                    new DataColumn("Name",typeof(string)),
                                    new DataColumn("Uni",typeof(string))
                                 };
            DataTable transformedTable = Semantics.Semantics.SelectWithoutWhere(columns, new DataTable[] { table });
            DataTable desiredTable = CSVToDatatableParser.Parse(@"TestCases\Semantics\TableOutput.csv");
            Assert.IsTrue(desiredTable.Columns.Count == transformedTable.Columns.Count);
            Assert.IsTrue(desiredTable.Rows.Count == transformedTable.Rows.Count);
            Assert.IsTrue((string) desiredTable.Rows[4]["Name"] == (string) transformedTable.Rows[4]["Name"]);
            Assert.IsTrue((string) desiredTable.Rows[4]["Uni"] == (string) transformedTable.Rows[4]["Uni"]);
        }

        [TestMethod]
        public void SelectWithWhereTest()
        {
            DataTable table = CSVToDatatableParser.Parse(@"TestCases\Semantics\TableInput.csv");
            DataColumn[] columns = {
                                    new DataColumn("Name",typeof(string)),
                                    new DataColumn("Uni",typeof(string))
                                 };
            DataTable transformedTable = Semantics.Semantics.SelectWithWhere(columns, table);
            DataTable desiredTable = CSVToDatatableParser.Parse(@"TestCases\Semantics\TableOutput.csv");
            Assert.IsTrue(desiredTable.Columns.Count == transformedTable.Columns.Count);
            Assert.IsTrue(desiredTable.Rows.Count == transformedTable.Rows.Count);
            Assert.IsTrue((string)desiredTable.Rows[4]["Name"] == (string)transformedTable.Rows[4]["Name"]);
            Assert.IsTrue((string)desiredTable.Rows[4]["Uni"] == (string)transformedTable.Rows[4]["Uni"]);
        }

        [TestMethod]
        public void ComparatorTest() {
            DataTable table = CSVToDatatableParser.Parse(@"TestCases\Semantics\TableInput.csv");
            DataColumn column = new DataColumn("Age", typeof(double));
            DataTable transformedTable = Semantics.Semantics.Comparator(column, new DataTable[] { table }, "==", 22);
            DataTable desiredTable = CSVToDatatableParser.Parse(@"TestCases\Semantics\TableOutputCondition.csv");
            Assert.IsTrue(desiredTable.Columns.Count == transformedTable.Columns.Count);
            Assert.IsTrue(table.Rows.Count != transformedTable.Rows.Count);
            Assert.IsTrue((string)desiredTable.Rows[1]["Name"] == (string)transformedTable.Rows[1]["Name"]);
            Assert.IsTrue((string)desiredTable.Rows[1]["Uni"] == (string)transformedTable.Rows[1]["Uni"]);
            Assert.IsTrue((double)desiredTable.Rows[1]["Age"] == (double)transformedTable.Rows[1]["Age"]);
        }

        [TestMethod]
        public void LogicalTest()
        {
            DataTable table = CSVToDatatableParser.Parse(@"TestCases\Semantics\TableInput.csv");
            DataColumn column = new DataColumn("Age", typeof(double));
            DataTable firstTable = Semantics.Semantics.Comparator(column, new DataTable[] { table }, "==", 22);
            column = new DataColumn("Uni", typeof(string));
            DataTable secondTable = Semantics.Semantics.Comparator(column, new DataTable[] { table }, "==", "Pilani");
            DataTable unionTable = Semantics.Semantics.Logical(firstTable, secondTable, "OR");
            DataTable intersectTable = Semantics.Semantics.Logical(firstTable, secondTable, "AND");
            Assert.IsTrue(unionTable.Rows.Count == 3);
            Assert.IsTrue(intersectTable.Rows.Count == 1);

        }

        [TestMethod]
        public void SelectWithoutWhereSynthesisTest() {
            DataTable inputTable = CSVToDatatableParser.Parse(@"TestCases\Synthesis\SelectWithoutWhere-Input.csv");
            DataTable outputTable = CSVToDatatableParser.Parse(@"TestCases\Synthesis\SelectWithoutWhere-Output.csv");
            var generatedPrograms = Learner.Instance.LearnSQLTopK(inputTable, outputTable,10);

            Assert.IsTrue(generatedPrograms.Length >= 1);
            foreach (var programNode in generatedPrograms)
            {
                DataTable outputLearnt = Learner.Instance.Invoke(programNode, inputTable);
                Assert.IsTrue(outputLearnt.Columns.Count == outputTable.Columns.Count);
                Assert.IsTrue(outputLearnt.Rows.Count == outputTable.Rows.Count);
                Assert.IsTrue(Utils.Utils.EqualColumns(outputLearnt, outputTable));
            }
        }

        [TestMethod]
        public void SelectWithWhereSynthesisTest1()
        {
            DataTable inputTable = CSVToDatatableParser.Parse(@"TestCases\Synthesis\SelectWithWhere-Input.csv");
            DataTable outputTable = CSVToDatatableParser.Parse(@"TestCases\Synthesis\SelectWithWhere-Output.csv");
            var generatedPrograms = Learner.Instance.LearnSQLAll(inputTable, outputTable);


            Assert.IsTrue(generatedPrograms.Length >= 1);
            foreach (var programNode in generatedPrograms)
            {
                Console.Write(programNode + " = " + programNode.GetFeatureValue(Learner.Instance.QueryRanker));
                DataTable outputLearnt = Learner.Instance.Invoke(programNode, inputTable);
                Assert.IsTrue(outputLearnt.Columns.Count == outputTable.Columns.Count);
                Assert.IsTrue(outputLearnt.Rows.Count == outputTable.Rows.Count);
                Assert.IsTrue(Utils.Utils.EqualColumns(outputLearnt, outputTable));
            }
        }

        [TestMethod]
        public void SelectWithWhereSynthesisTest2()
        {
            DataTable inputTable = CSVToDatatableParser.Parse(@"TestCases\Synthesis\SelectWithWhere1-Input.csv");
            DataTable outputTable = CSVToDatatableParser.Parse(@"TestCases\Synthesis\SelectWithWhere1-Output.csv");
            var generatedPrograms = Learner.Instance.LearnSQLAll(inputTable, outputTable);

            Assert.IsTrue(generatedPrograms.Length >= 1);
            foreach (var programNode in generatedPrograms)
            {
                DataTable outputLearnt = Learner.Instance.Invoke(programNode, inputTable);
                Assert.IsTrue(outputLearnt.Columns.Count == outputTable.Columns.Count);
                Assert.IsTrue(outputLearnt.Rows.Count == outputTable.Rows.Count);
                Assert.IsTrue(Utils.Utils.EqualColumns(outputLearnt, outputTable));
            }
        }

        [TestMethod]
        public void SelectWithWhereSynthesisTest3()
        { 
            DataTable inputTable = CSVToDatatableParser.Parse(@"TestCases\Synthesis\SelectWithWhere-Input.csv");
            DataTable outputTable = CSVToDatatableParser.Parse(@"TestCases\Synthesis\SelectWithWhere-Output.csv");
            var programNode = Learner.Instance.LearnSQL(inputTable, outputTable);
            DataTable outputLearnt = Learner.Instance.Invoke(programNode, inputTable);
            Assert.IsTrue(outputLearnt.Columns.Count == outputTable.Columns.Count);
            Assert.IsTrue(outputLearnt.Rows.Count == outputTable.Rows.Count);
            Assert.IsTrue((string)outputLearnt.Rows[0]["Name"] == (string)outputTable.Rows[0]["Name"]);
            Assert.IsTrue((string)outputLearnt.Rows[1]["Name"] == (string)outputTable.Rows[1]["Name"]);
        }

        [TestMethod]
        public void SelectWithWhereSynthesisTest4()
        {
            DataTable inputTable = CSVToDatatableParser.Parse(@"TestCases\Synthesis\SelectWithWhere1-Input.csv");
            DataTable outputTable = CSVToDatatableParser.Parse(@"TestCases\Synthesis\SelectWithWhere1-Output.csv");
            var programNode = Learner.Instance.LearnSQL(inputTable, outputTable);
            DataTable outputLearnt = Learner.Instance.Invoke(programNode, inputTable);
            Assert.IsTrue(outputLearnt.Columns.Count == outputTable.Columns.Count);
            Assert.IsTrue(outputLearnt.Rows.Count == outputTable.Rows.Count);
            Assert.IsTrue((string)outputLearnt.Rows[0]["Name"] == (string)outputTable.Rows[0]["Name"]);
            Assert.IsTrue((string)outputLearnt.Rows[1]["Name"] == (string)outputTable.Rows[1]["Name"]);
        }

        [TestMethod]
        public void SelectWithWhereNotEqualSynthesisTest()
        {
            DataTable inputTable = CSVToDatatableParser.Parse(@"TestCases\Synthesis\SelectWithWhere\SWW-NotEqual-Input.csv");
            DataTable outputTable = CSVToDatatableParser.Parse(@"TestCases\Synthesis\SelectWithWhere\SWW-NotEqual-Output.csv");
            var generatedPrograms = Learner.Instance.LearnSQLTopK(inputTable, outputTable, 10);

            Assert.IsTrue(generatedPrograms.Length >= 1);
            foreach (var programNode in generatedPrograms)
            {
                DataTable outputLearnt = Learner.Instance.Invoke(programNode, inputTable);
                Assert.IsTrue(outputLearnt.Columns.Count == outputTable.Columns.Count);
                Assert.IsTrue(outputLearnt.Rows.Count == outputTable.Rows.Count);
                Assert.IsTrue(Utils.Utils.EqualColumns(outputLearnt, outputTable));
            }
        }

        [TestMethod]
        public void SelectWithWhereEqualSynthesisTest()
        {
            DataTable inputTable = CSVToDatatableParser.Parse(@"TestCases\Synthesis\SelectWithWhere\SWW-Equal-Input.csv");
            DataTable outputTable = CSVToDatatableParser.Parse(@"TestCases\Synthesis\SelectWithWhere\SWW-Equal-Output.csv");
            var generatedPrograms = Learner.Instance.LearnSQLAll(inputTable, outputTable);

            Assert.IsTrue(generatedPrograms.Length >= 1);
            foreach (var programNode in generatedPrograms)
            {
                DataTable outputLearnt = Learner.Instance.Invoke(programNode, inputTable);
                Assert.IsTrue(outputLearnt.Columns.Count == outputTable.Columns.Count);
                Assert.IsTrue(outputLearnt.Rows.Count == outputTable.Rows.Count);
                Assert.IsTrue(Utils.Utils.EqualColumns(outputLearnt, outputTable));
            }
        }


        [TestMethod]
        public void SelectWithWhereLessEqualSynthesisTest()
        {
            DataTable inputTable = CSVToDatatableParser.Parse(@"TestCases\Synthesis\SelectWithWhere\SWW-LessEqual-Input.csv");
            DataTable outputTable = CSVToDatatableParser.Parse(@"TestCases\Synthesis\SelectWithWhere\SWW-LessEqual-Output.csv");
            var generatedPrograms = Learner.Instance.LearnSQLAll(inputTable, outputTable);

            Assert.IsTrue(generatedPrograms.Length >= 1);
            foreach (var programNode in generatedPrograms)
            {
                DataTable outputLearnt = Learner.Instance.Invoke(programNode, inputTable);
                Assert.IsTrue(outputLearnt.Columns.Count == outputTable.Columns.Count);
                Assert.IsTrue(outputLearnt.Rows.Count == outputTable.Rows.Count);
                Assert.IsTrue(Utils.Utils.EqualColumns(outputLearnt, outputTable));
            }
        }
        
        [TestMethod]
        public void GeneratePowerSetTest()
        {
            DataTable inputTable = new DataTable();
            DataTable[] outputs = Utils.Utils.GeneratePowerSet(inputTable);
            Assert.IsTrue(outputs.Length == Math.Pow(2, inputTable.Rows.Count)-1);
        }

        [TestMethod]
        public void LogicalSynthesisTest()
        {
            DataTable inputTable = CSVToDatatableParser.Parse(@"TestCases\Synthesis\WithLogic-Input.csv");
            DataTable outputTable = CSVToDatatableParser.Parse(@"TestCases\Synthesis\WithLogic-Output.csv");
            var generatedPrograms = Learner.Instance.LearnSQLAll(inputTable, outputTable);

            Assert.IsTrue(generatedPrograms.Length >= 1);
            foreach (var programNode in generatedPrograms)
            {
                DataTable outputLearnt = Learner.Instance.Invoke(programNode, inputTable);
                Assert.IsTrue(outputLearnt.Columns.Count == outputTable.Columns.Count);
                Assert.IsTrue(outputLearnt.Rows.Count == outputTable.Rows.Count);
                Assert.IsTrue(Utils.Utils.EqualColumns(outputLearnt, outputTable));
            }
        }

        [TestMethod]
        public void DataTableCrossTest()
        {
            DataTable table1 = CSVToDatatableParser.Parse(@"TestCases\Synthesis\WithLogic-Input.csv");
            DataTable table2 = CSVToDatatableParser.Parse(@"TestCases\Synthesis\WithLogic-Output.csv");
            DataTable crossTable = Utils.Utils.CrossTable(table1, table2);
            Assert.IsTrue(crossTable.Columns.Count == table1.Columns.Count + table2.Columns.Count);
            Assert.IsTrue(crossTable.Rows.Count == table1.Rows.Count * table2.Rows.Count);
            table1 = CSVToDatatableParser.Parse(@"TestCases\Synthesis\WithLogic-Input.csv");
            table2 = CSVToDatatableParser.Parse(@"TestCases\Synthesis\SelectWithoutWhere-Output.csv");
            crossTable = Utils.Utils.CrossTable(table1, table2);
            Assert.IsTrue(crossTable.Columns.Count == table1.Columns.Count + table2.Columns.Count);
            Assert.IsTrue(crossTable.Rows.Count == table1.Rows.Count * table2.Rows.Count);
        }

        [TestMethod]
        public void DataTableCrossMultiple()
        {
            DataTable table1 = CSVToDatatableParser.Parse(@"TestCases\Synthesis\WithLogic-Input.csv");
            DataTable table2 = CSVToDatatableParser.Parse(@"TestCases\Synthesis\SelectWithWhere-Output.csv");
            DataTable table3 = CSVToDatatableParser.Parse(@"TestCases\Synthesis\SelectWithoutWhere-Output.csv");
            var cross = Semantics.Semantics.CrossMultiplyTables(new[] { table1, table2, table3 });
            Assert.IsTrue(cross.Columns.Count == table1.Columns.Count + table2.Columns.Count + table3.Columns.Count);
            Assert.IsTrue(cross.Rows.Count == table1.Rows.Count * table2.Rows.Count * table3.Rows.Count);
        }

        [TestMethod]
        public void PrimaryKeyFailTest()
        {
            DataTable inputTable = CSVToDatatableParser.Parse(@"TestCases\Synthesis\PrimaryKeyFail-Input.csv");
            DataTable outputTable = CSVToDatatableParser.Parse(@"TestCases\Synthesis\PrimaryKeyFail-Output.csv");
            var generatedPrograms = Learner.Instance.LearnSQLAll(inputTable, outputTable);

            Assert.IsTrue(generatedPrograms.Length >= 1);
            foreach (var programNode in generatedPrograms)
            {
                DataTable outputLearnt = Learner.Instance.Invoke(programNode, inputTable);
                Assert.IsTrue(outputLearnt.Columns.Count == outputTable.Columns.Count);
                Assert.IsTrue(outputLearnt.Rows.Count == outputTable.Rows.Count);
                Assert.IsTrue(Utils.Utils.EqualColumns(outputLearnt, outputTable));
            }
        }

        [TestMethod]
        public void NCRTest()
        {
            var str = new[] {"A","B","C","D"};
            var _4c2 = Utils.Utils.NChooseR(str, 2).ToArray();
            Assert.AreEqual(6,_4c2.Count());
        }

        [TestMethod]
        public void CrossDataRowTest()
        {
            DataTable table1 = CSVToDatatableParser.Parse(@"TestCases\Synthesis\DataRowCross1.csv");
            DataTable table2 = CSVToDatatableParser.Parse(@"TestCases\Synthesis\DataRowCross2.csv");
            DataTable table3 = CSVToDatatableParser.Parse(@"TestCases\Synthesis\DataRowCross3.csv");
            var rows1 = new DataRow[table1.Rows.Count]; 
            table1.Rows.CopyTo(rows1,0);
            var rows2 = new DataRow[table2.Rows.Count];
            table2.Rows.CopyTo(rows2, 0);
            var rows3 = new DataRow[table3.Rows.Count];
            table3.Rows.CopyTo(rows3, 0);
            var cross = Utils.Utils.GetAllPossibleTables(
                    new[] {rows1, rows2, rows3}.ToList(),
                    new[] { 1, 2, 1}.ToList(),
                    table1);
            Assert.AreEqual(table1.Rows.Count * 
                            table2.Rows.Count * 
                            table3.Rows.Count, cross.Count());
        }
    }

}
