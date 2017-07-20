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

            foreach (State input in spec.ProvidedInputs)

            {

                var tableList = (DataTable[])input[rule.Body[1]];
                var inputTable = tableList[0];
                var desiredOutput = (DataTable)spec.Examples[input];
                var allPossibleSolutions = new List<object>();
                allPossibleSolutions.Add(desiredOutput.Columns.Cast<DataColumn>().ToArray());

                ppExamples[input] = allPossibleSolutions;

            }

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
                string logicalOperator = (string)logicalOperatorSpec.Examples[logicalOperatorSpec.ProvidedInputs.First()];
                switch (logicalOperator)
                {
                    case "AND":
                        allPossibleSolutions = GetAndInputTable1(inputTable, outputTable);
                        break;
                    case "OR":
                        var transformedOutputTable = inputTable.AsEnumerable().Except(outputTable.AsEnumerable()).CopyToDataTable();
                        allPossibleSolutions = GetAndInputTable1(inputTable, transformedOutputTable);
                        for(int i = 0; i < allPossibleSolutions.Count; i++)
                        {
                            allPossibleSolutions[i] = inputTable.AsEnumerable().Except(allPossibleSolutions[i].AsEnumerable()).CopyToDataTable();
                        }
                        break;
                }
                ppExamples[input] = allPossibleSolutions;
            }
            return DisjunctiveExamplesSpec.From(ppExamples);
        }

        [WitnessFunction(nameof(Semantics.Logical), 1, DependsOnParameters = new[] { 0 , 2 })]
        internal DisjunctiveExamplesSpec WitnessInputTable1(GrammarRule rule, DisjunctiveExamplesSpec spec, ExampleSpec inputTable1Spec, ExampleSpec logicalOperatorSpec)
        {
            var ppExamples = new Dictionary<State, IEnumerable<object>>();


            foreach (State input in spec.ProvidedInputs)
            {
                DataTable outputTable = (DataTable)spec.DisjunctiveExamples[input].First();
                DataTable inputTable = ((DataTable[])input[rule.Grammar.InputSymbol])[0];
                DataTable inputTable1 = (DataTable)inputTable1Spec.Examples[inputTable1Spec.ProvidedInputs.First()];
                var allPossibleSolutions = new List<DataTable>();
                string logicalOperator = (string)logicalOperatorSpec.Examples[logicalOperatorSpec.ProvidedInputs.First()];
                switch (logicalOperator)
                {
                    case "AND":
                        allPossibleSolutions = GetAndInputTable2(inputTable,inputTable1,outputTable);
                        break;
                    case "OR":
                        var transformedOutputTable = inputTable.AsEnumerable().Except(outputTable.AsEnumerable()).CopyToDataTable();
                        var transformInputTable1 = inputTable.AsEnumerable().Except(inputTable1.AsEnumerable()).CopyToDataTable();

                        allPossibleSolutions = GetAndInputTable2(inputTable, transformInputTable1, transformedOutputTable);
                        for (int i = 0; i < allPossibleSolutions.Count; i++)
                        {
                            allPossibleSolutions[i] = inputTable.AsEnumerable().Except(allPossibleSolutions[i].AsEnumerable()).CopyToDataTable();
                        }
                        break;
                }
                ppExamples[input] = allPossibleSolutions;
            }
            return DisjunctiveExamplesSpec.From(ppExamples);
        }

        static List<DataTable> GetAndInputTable2(DataTable initialTable, DataTable inputTable1, DataTable outputTable)
        {
            List<DataTable> allPossibleSolutions = new List<DataTable>();
            DataTable input1Skeleton = outputTable.Copy();
            DataTable remainingRowsNotInInput1 = initialTable.AsEnumerable().Except(inputTable1.AsEnumerable()).CopyToDataTable();

            DataTable[] powerSetOfRemainingRows = Utils.Utils.GeneratePowerSet(remainingRowsNotInInput1);
            foreach (DataTable powerSetEntry in powerSetOfRemainingRows)
            {
                DataTable skeletonCopy = input1Skeleton.Copy();
                allPossibleSolutions.Add(skeletonCopy.AsEnumerable().Union(powerSetEntry.AsEnumerable()).CopyToDataTable());
            }
            return allPossibleSolutions;

        }

        static List<DataTable> GetAndInputTable1(DataTable inputTable, DataTable outputTable)
        {
            List<DataTable> allPossibleSolutions = new List<DataTable>();
            DataTable input1Skeleton = outputTable.Copy();
            DataTable remainingRows = inputTable.AsEnumerable().Except(outputTable.AsEnumerable()).CopyToDataTable();

            DataTable[] powerSetOfRemainingRows = Utils.Utils.GeneratePowerSet(remainingRows);
            foreach (DataTable powerSetEntry in powerSetOfRemainingRows)
            {
                DataTable skeletonCopy = input1Skeleton.Copy();
                allPossibleSolutions.Add(skeletonCopy.AsEnumerable().Union(powerSetEntry.AsEnumerable()).CopyToDataTable());
            }
            return allPossibleSolutions;
        }
    }
}
