using Microsoft.ProgramSynthesis;

using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Rules;
using Microsoft.ProgramSynthesis.Specifications;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SNTSBackend.Semantics
{
    class WitnessFunctions : DomainLearningLogic
    {
            public WitnessFunctions(Grammar grammar) : base(grammar) { }

            [WitnessFunction(nameof(Semantics.SelectWithoutWhere), 0)]
            internal DisjunctiveExamplesSpec WitnessSelectWithoutWhere(GrammarRule rule, ExampleSpec spec) {

                var ppExamples = new Dictionary<State, IEnumerable<object>>();

                foreach (State input in spec.ProvidedInputs) {

                    var tableList = (DataTable[])input[rule.Body[1]];
                    var inputTable = tableList[0];
                    var desiredOutput = (DataTable)spec.Examples[input];
                    var allPossibleSolutions = new List<object>();
                    if (desiredOutput.Rows.Count == inputTable.Rows.Count) {
                        allPossibleSolutions.Add(desiredOutput.Columns.Cast<DataColumn>().ToArray());
                    }
                    ppExamples[input] = allPossibleSolutions;

                }

                return DisjunctiveExamplesSpec.From(ppExamples);

            }

            [WitnessFunction(nameof(Semantics.SelectWithWhere), 1)]
            internal DisjunctiveExamplesSpec WitnessSelectWithWhereCondition(GrammarRule rule, ExampleSpec spec) {
                var ppExamples = new Dictionary<State, IEnumerable<object>>();
                foreach(State input in spec.ProvidedInputs) {
                    var inputTable = ((DataTable[])input[rule.Grammar.InputSymbol])[0];
                    var outputTable = (DataTable)spec.Examples[input];
                    var conditionTable = inputTable.Clone();
                    var allPossibleSolutions = new List<object>();
                    foreach (DataRow row in outputTable.Rows) {
                        //TODO: Make it a Primary Key
                        var completeRow = inputTable.Select("PrimaryKey=" + row["PrimaryKey"]).First();
                        conditionTable.ImportRow(completeRow);
                    }
                    allPossibleSolutions.Add(conditionTable);
                    ppExamples[input] = allPossibleSolutions;
                }
                //Complete the rows
                return DisjunctiveExamplesSpec.From(ppExamples);
            }

            [WitnessFunction(nameof(Semantics.SelectWithWhere), 0, DependsOnParameters = new[] { 1 })]
            internal DisjunctiveExamplesSpec WitnessSelectWithWhereColumnArray(GrammarRule rule, DisjunctiveExamplesSpec spec, ExampleSpec conditionSpec) {
                var ppExamples = new Dictionary<State, IEnumerable<object>>();
                var conditionTable = (DataTable)conditionSpec.Examples.Values.First();
                foreach (State input in spec.ProvidedInputs) {

                    DataTable inputTable = ((DataTable[])input[rule.Grammar.InputSymbol])[0];
                    var desiredOutput = (DataTable)spec.DisjunctiveExamples[input].First(); // conditionTable;
                    var allPossibleSolutions = new List<object>();
                    allPossibleSolutions.Add(desiredOutput.Columns.Cast<DataColumn>().ToArray());

                    ppExamples[input] = allPossibleSolutions;

                }
            //Complete the rows
            return DisjunctiveExamplesSpec.From(ppExamples);
        }
        /*public static DataTable Logical(DataTable cmpStatement, DataTable condition, string logicSymbol)*/
        [WitnessFunction(nameof(Semantics.Logical), 2)]
        internal DisjunctiveExamplesSpec WitnessLogicalSymbol(GrammarRule rule, ExampleSpec spec)
        {
            var ppExamples = new Dictionary<State, IEnumerable<object>>();
            foreach (State input in spec.ProvidedInputs)
            {
                var allPossibleSolutions = Semantics.LogicGen;
                ppExamples[input] = allPossibleSolutions;
            }
            return DisjunctiveExamplesSpec.From(ppExamples);
        }

        [WitnessFunction(nameof(Semantics.Logical), 0, DependsOnParameters = new[] { 2 })]
        internal DisjunctiveExamplesSpec WitnessInputTable1(GrammarRule rule, DisjunctiveExamplesSpec spec, ExampleSpec logicalOperatorSpec)
        {
            var ppExamples = new Dictionary<State, IEnumerable<object>>();
            
            foreach (State input in spec.ProvidedInputs)
            {
                DataTable outputTable = (DataTable)spec.DisjunctiveExamples[input].First();
                DataTable inputTable = ((DataTable[])input[rule.Grammar.InputSymbol])[0];
                var allPossibleSolutions = new List<DataTable>();
                var allPossibleSolutionsCleaned = new List<DataTable>();
                string logicalOperator = (string)logicalOperatorSpec.Examples[logicalOperatorSpec.ProvidedInputs.First()];
                switch (logicalOperator)
                {
                    case "AND":
                        allPossibleSolutions = GetAndInputTable1(inputTable, outputTable);
                        break;
                    case "OR":
                        var transformedOutputTable = Utils.Utils.CreateOutputTableFromEnumerable(inputTable.AsEnumerable().Except(outputTable.AsEnumerable(), new DataTableCustomComparator()));
                        allPossibleSolutions = GetAndInputTable1(inputTable, transformedOutputTable);
                        for (int i = 0; i < allPossibleSolutions.Count; i++)
                        {
                            allPossibleSolutions[i] = Utils.Utils.CreateOutputTableFromEnumerable(inputTable.AsEnumerable().Except(allPossibleSolutions[i].AsEnumerable(), new DataTableCustomComparator()));
                        }
                        break;
                }
                for (int i = 0; i < allPossibleSolutions.Count; i++)
                {
                    if (allPossibleSolutions[i] != null && allPossibleSolutions[i].Rows.Count != 0)
                    {
                        allPossibleSolutionsCleaned.Add(allPossibleSolutions[i]);
                    }
                }
                ppExamples[input] = allPossibleSolutionsCleaned;
            }
            return DisjunctiveExamplesSpec.From(ppExamples);
        }

        [WitnessFunction(nameof(Semantics.Logical), 1, DependsOnParameters = new[] { 0, 2 })]
        internal DisjunctiveExamplesSpec WitnessInputTable2(GrammarRule rule, DisjunctiveExamplesSpec spec, ExampleSpec inputTable1Spec, ExampleSpec logicalOperatorSpec)
        {
            var ppExamples = new Dictionary<State, IEnumerable<object>>();


            foreach (State input in spec.ProvidedInputs)
            {
                DataTable outputTable = (DataTable)spec.DisjunctiveExamples[input].First();
                DataTable inputTable = ((DataTable[])input[rule.Grammar.InputSymbol])[0];
                DataTable inputTable1 = (DataTable)inputTable1Spec.Examples[inputTable1Spec.ProvidedInputs.First()];
                var allPossibleSolutions = new List<DataTable>();
                var allPossibleSolutionsCleaned = new List<DataTable>();
                string logicalOperator = (string)logicalOperatorSpec.Examples[logicalOperatorSpec.ProvidedInputs.First()];
                switch (logicalOperator)
                {
                    case "AND":
                        allPossibleSolutions = GetAndInputTable2(inputTable, inputTable1, outputTable);
                        break;
                    case "OR":
                        var transformedOutputTable = Utils.Utils.CreateOutputTableFromEnumerable(inputTable.AsEnumerable().Except(outputTable.AsEnumerable(), new DataTableCustomComparator()));
                        var transformInputTable1 = Utils.Utils.CreateOutputTableFromEnumerable(inputTable.AsEnumerable().Except(inputTable1.AsEnumerable(), new DataTableCustomComparator()));

                        allPossibleSolutions = GetAndInputTable2(inputTable, transformInputTable1, transformedOutputTable);
                        for (int i = 0; i < allPossibleSolutions.Count; i++)
                        {
                            allPossibleSolutions[i] = Utils.Utils.CreateOutputTableFromEnumerable(inputTable.AsEnumerable().Except(allPossibleSolutions[i].AsEnumerable(), new DataTableCustomComparator()));
                        }
                        break;
                }
                for(int i = 0; i < allPossibleSolutions.Count; i++)
                {
                    if(allPossibleSolutions[i] != null && allPossibleSolutions[i].Rows.Count != 0)
                    {
                        allPossibleSolutionsCleaned.Add(allPossibleSolutions[i]);
                    }
                }
                ppExamples[input] = allPossibleSolutionsCleaned;
            }
            return DisjunctiveExamplesSpec.From(ppExamples);
        }

        static List<DataTable> GetAndInputTable2(DataTable initialTable, DataTable inputTable1, DataTable outputTable)
        {
            List<DataTable> allPossibleSolutions = new List<DataTable>();
            DataTable input1Skeleton = outputTable.Copy();
            DataTable remainingRowsNotInInput1 = Utils.Utils.CreateOutputTableFromEnumerable(initialTable.AsEnumerable().Except(inputTable1.AsEnumerable(), new DataTableCustomComparator()));

            DataTable[] powerSetOfRemainingRows = Utils.Utils.GeneratePowerSet(remainingRowsNotInInput1);
            foreach (DataTable powerSetEntry in powerSetOfRemainingRows)
            {
                DataTable skeletonCopy = input1Skeleton.Copy();
                allPossibleSolutions.Add(Utils.Utils.CreateOutputTableFromEnumerable(skeletonCopy.AsEnumerable().Union(powerSetEntry.AsEnumerable(), new DataTableCustomComparator())));
            }
            return allPossibleSolutions;

        }

        static List<DataTable> GetAndInputTable1(DataTable inputTable, DataTable outputTable)
        {
            List<DataTable> allPossibleSolutions = new List<DataTable>();
            DataTable input1Skeleton = outputTable.Copy();
            DataTable remainingRows = Utils.Utils.CreateOutputTableFromEnumerable(inputTable.AsEnumerable().Except(outputTable.AsEnumerable(), new DataTableCustomComparator()));

            DataTable[] powerSetOfRemainingRows = Utils.Utils.GeneratePowerSet(remainingRows);
            foreach (DataTable powerSetEntry in powerSetOfRemainingRows)
            {
                DataTable skeletonCopy = input1Skeleton.Copy();
                allPossibleSolutions.Add(Utils.Utils.CreateOutputTableFromEnumerable(skeletonCopy.AsEnumerable().Union(powerSetEntry.AsEnumerable(), new DataTableCustomComparator())));
            }
            return allPossibleSolutions;
        }

            /* Inverse for :
             *  Comparator(DataColumn column, DataTable tableList, string cmpSymbol, object constValue)
             * */
            [WitnessFunction(nameof(Semantics.Comparator), 0)]
            internal DisjunctiveExamplesSpec WitnessComparatorColumn(GrammarRule rule, ExampleSpec spec)
            {
                /* Inverse for the column field */
                var ppExamples = new Dictionary<State, IEnumerable<object>>();
                foreach (State input in spec.ProvidedInputs) {
                    DataTable[] tableList = (DataTable[])input[rule.Grammar.InputSymbol]; // Single table hack
                    var dataColumnArrayArray= tableList.SelectMany(t => t.Columns.Cast<DataColumn>().ToArray()).ToArray();
                    ppExamples[input] = dataColumnArrayArray; 

                }
                
                return DisjunctiveExamplesSpec.From(ppExamples);
            }

            [WitnessFunction(nameof(Semantics.Comparator), 2)]
            internal DisjunctiveExamplesSpec WitnessComparatorCmpSymbol(GrammarRule rule, ExampleSpec spec)
            {
                /* Inverse for the constValue field */
                var ppExamples = new Dictionary<State, IEnumerable<object>>();
                foreach (State input in spec.ProvidedInputs) {
                    var allPossibleSolutions = Semantics.CmpGen;
                    ppExamples[input] = allPossibleSolutions;
                }
                return DisjunctiveExamplesSpec.From(ppExamples);
            }

            [WitnessFunction(nameof(Semantics.Comparator), 3, DependsOnParameters = new[] { 0, 2})]
            internal DisjunctiveExamplesSpec WitnessComparatorConstValue(GrammarRule rule, DisjunctiveExamplesSpec spec, ExampleSpec columnSpec, ExampleSpec cmpSymbolSpec) {
                /* Inverse for the constValue field */
                var ppExamples = new Dictionary<State, IEnumerable<object>>();
                
            
                foreach (State input in spec.ProvidedInputs)
                {
                    DataTable outputTable = (DataTable)spec.DisjunctiveExamples[input].First();
                    DataTable inputTable = ((DataTable[])input[rule.Grammar.InputSymbol])[0];
                    var allPossibleSolutions = new List<object>();
                    if(outputTable.Rows.Count == 0) {
                        ppExamples[input] = allPossibleSolutions;
                        continue;
                    }
                    string cmpSymbol = (string)cmpSymbolSpec.Examples[cmpSymbolSpec.ProvidedInputs.First()]; // Get the comparison symbol
                    DataColumn column = (DataColumn)columnSpec.Examples[columnSpec.ProvidedInputs.First()]; // Column

                    bool flag = true ;
                    
                    if (column.DataType == typeof(string))

                    {
                        var mappedCmpSymbol = "";
                        string valueToCompare = "";
                        switch (cmpSymbol)
                        {
                            case "==":
                                mappedCmpSymbol = "=";
                                var valuesInColumn =
                                    outputTable.Rows.Cast<DataRow>().Select(t => t[column.ColumnName]).Distinct();
                                // TODO: Get a linq guy to look this up
                                if (valuesInColumn.Count() != 1)
                                {
                                    flag = false;
                                }
                                else
                                {
                                    valueToCompare = (string)valuesInColumn.First();
                                }
                            // else, Keep it as empty

                            break;
                            default:
                            // TODO: Unsupported datatype
                                flag = false;
                                break;
                        }
                        if (flag)
                        {
                            var countRowsInput = inputTable.Select(column.ColumnName + mappedCmpSymbol + "'" + valueToCompare + "'").Count();
                             var countRowsOutput = outputTable.Select(column.ColumnName + mappedCmpSymbol + "'" + valueToCompare + "'").Count();
                            // var countRowsOutput = outputTable.Rows.Count; // This seems more correct
                            if (flag && countRowsInput == countRowsOutput)
                            {
                                allPossibleSolutions.Add((object)valueToCompare);
                            }
                        }
                    }
                    else if (column.DataType == typeof(double))
                    {
                        double valueToCompare = 0;
                        var mappedCmpSymbol = "";

                        switch (cmpSymbol)
                        {
                            case "==":
                            mappedCmpSymbol = "=";
                            var valuesInColumn =
                                     outputTable.Rows.Cast<DataRow>().Select(t => t[column.ColumnName]).Distinct();
                                // TODO: Get a linq guy to look this up
                                if (valuesInColumn.Count() != 1)
                                {
                                    flag = false;
                                }
                                else
                                {
                                    valueToCompare = (double)valuesInColumn.First();
                                }
                            // else, Keep it as empty
                            break;

                            case ">=":
                            {
                                mappedCmpSymbol = ">=";
                                valueToCompare = (double)outputTable.Rows.Cast<DataRow>().Select(t => t[column.ColumnName]).Min();
                                break;
                            }

                            case "<=":
                            {
                                mappedCmpSymbol = "<=";

                                valueToCompare = (double)outputTable.Rows.Cast<DataRow>().Select(t => t[column.ColumnName]).Max();
                                break;
                            }

                            case "<":
                            {
                                mappedCmpSymbol = "<";
                                var excludedRow1 = inputTable.AsEnumerable().Where(r => 
                                                                                    !outputTable.AsEnumerable()
                                                                                                .Select(x => 
                                                                                                        x[column.ColumnName])
                                                                                                .ToList()
                                                                                                .Contains(r[column.ColumnName])
                                                                               );
                                if (excludedRow1 != null && excludedRow1.Count() != 0) {
                                    valueToCompare = (double)
                                        ((excludedRow1).ToList()).Cast<DataRow>().Select(t => t[column.ColumnName]).Min();
                                    if (valueToCompare <= 
                                        (double)outputTable
                                            .Rows.Cast<DataRow>()
                                                 .Select(t => t[column.ColumnName]).Max()
                                       )
                                    {
                                        flag = false;
                                    }
                                } else
                                {
                                    flag = false;
                                }

                                break;
                            }
                            case ">":
                            mappedCmpSymbol = ">";
                            var excludedRow = inputTable.AsEnumerable().Where(r => !outputTable.AsEnumerable().Select(x
                                    => x[column.ColumnName]).ToList().Contains(r[column.ColumnName]));
                            if (excludedRow !=null && excludedRow.Count() != 0) {
                                valueToCompare = (double)
                                        (excludedRow.ToList()).Cast<DataRow>().Select(t => t[column.ColumnName]).Max();
                                if (valueToCompare >=
                                        (double)outputTable
                                            .Rows.Cast<DataRow>()
                                                 .Select(t => t[column.ColumnName]).Min()
                                       )
                                {
                                    flag = false;
                                }
                            } else
                            {
                                flag = false;
                            }
                                break;
                            case "!=":
                            mappedCmpSymbol = "<>";
                            var excluded2 = inputTable.AsEnumerable().Where(r => 
                                                                            !outputTable.AsEnumerable()
                                                                                        .Select(x => 
                                                                                                x[column.ColumnName]).ToList()
                                                                                                                     .Contains(r[column.ColumnName])
                                                                            );
                            var valuesExcludedInColumn =
                                     ((excluded2).ToList()).Cast<DataRow>().Select(t => t[column.ColumnName]).Distinct();
                                // TODO: Get a linq guy to look this up
                                if (valuesExcludedInColumn.Count() != 1)
                                {
                                    flag = false;
                                }else
                                {
                                    valueToCompare = (double)valuesExcludedInColumn.First();
                                }
                                
                                // else, Keep it as empty
                                break;
                            default:
                                // TODO: Unsupported datatype
                                flag = false;
                                break;
                        }

                        if (flag)
                        {
                            var countRowsInput = inputTable.Select(column.ColumnName + mappedCmpSymbol + valueToCompare).Count();
                            var countRowsOutput = outputTable.Select(column.ColumnName + mappedCmpSymbol + valueToCompare).Count();
                            // This seems more correct // var countRowsOutput = outputTable.Rows.Count;
                            if (flag && countRowsInput == countRowsOutput && (countRowsInput !=0 || outputTable.Rows.Count == 0))
                            {
                                allPossibleSolutions.Add((object)valueToCompare);
                            }
                        }
                }
                    ppExamples[input] = allPossibleSolutions;

                }
                return DisjunctiveExamplesSpec.From(ppExamples);
            }


        } // End class
    }