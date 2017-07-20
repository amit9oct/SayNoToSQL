using Microsoft.VisualStudio.TestTools.UnitTesting;
using SNTSBackend.Parser;
using System.Data;
using System.Diagnostics;

namespace SNTSBackend.Tests
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void CsvToDataTableConversionTest() {
            DataTable table = CSVToDatatableParser.Parse(@"TestCases\Parser\Table.csv");
            Debug.Assert(table.Columns.Count == 4);
            Debug.Assert(table.Rows.Count == 5);
            Debug.Assert((string)table.Rows[2]["Name"] == "Amitayush");
            Debug.Assert((string)table.Rows[2]["Uni"] == "Pilani");
            Debug.Assert((double)table.Rows[2]["Age"] == 23);
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
            Debug.Assert(desiredTable.Columns.Count == transformedTable.Columns.Count);
            Debug.Assert(desiredTable.Rows.Count == transformedTable.Rows.Count);
            Debug.Assert((string) desiredTable.Rows[4]["Name"] == (string) transformedTable.Rows[4]["Name"]);
            Debug.Assert((string) desiredTable.Rows[4]["Uni"] == (string) transformedTable.Rows[4]["Uni"]);
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
            Debug.Assert(desiredTable.Columns.Count == transformedTable.Columns.Count);
            Debug.Assert(desiredTable.Rows.Count == transformedTable.Rows.Count);
            Debug.Assert((string)desiredTable.Rows[4]["Name"] == (string)transformedTable.Rows[4]["Name"]);
            Debug.Assert((string)desiredTable.Rows[4]["Uni"] == (string)transformedTable.Rows[4]["Uni"]);
        }

        [TestMethod]
        public void ComparatorTest() {
            DataTable table = CSVToDatatableParser.Parse(@"TestCases\Semantics\TableInput.csv");
            DataColumn column = new DataColumn("Age", typeof(double));
            DataTable transformedTable = Semantics.Semantics.Comparator(column, new DataTable[] { table }, "==", 22);
            DataTable desiredTable = CSVToDatatableParser.Parse(@"TestCases\Semantics\TableOutputCondition.csv");
            Debug.Assert(desiredTable.Columns.Count == transformedTable.Columns.Count);
            Debug.Assert(table.Rows.Count != transformedTable.Rows.Count);
            Debug.Assert((string)desiredTable.Rows[1]["Name"] == (string)transformedTable.Rows[1]["Name"]);
            Debug.Assert((string)desiredTable.Rows[1]["Uni"] == (string)transformedTable.Rows[1]["Uni"]);
            Debug.Assert((double)desiredTable.Rows[1]["Age"] == (double)transformedTable.Rows[1]["Age"]);
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
            Debug.Assert(unionTable.Rows.Count == 3);
            Debug.Assert(intersectTable.Rows.Count == 1);

        }

        [TestMethod]
        public void SelectWithoutWhereSynthesisTest() {
            DataTable inputTable = CSVToDatatableParser.Parse(@"TestCases\Synthesis\SelectWithoutWhere-Input.csv");
            DataTable outputTable = CSVToDatatableParser.Parse(@"TestCases\Synthesis\SelectWithoutWhere-Output.csv");
            var programNode = Learner.Instance.LearnSQL(inputTable, outputTable);
            DataTable outputLearnt = Learner.Instance.Invoke(programNode, inputTable);
            Debug.Assert(outputLearnt.Columns.Count == outputTable.Columns.Count);
            Debug.Assert(outputLearnt.Rows.Count == outputTable.Rows.Count);
            Debug.Assert((string)outputLearnt.Rows[4]["Name"] == (string)outputTable.Rows[4]["Name"]);
            Debug.Assert((string)outputLearnt.Rows[4]["Uni"] == (string)outputTable.Rows[4]["Uni"]);
        }
    }

}
