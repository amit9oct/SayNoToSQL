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
        internal DisjunctiveExamplesSpec WitnessSelectWithoutWhere(GrammarRule rule, ExampleSpec spec)
        {

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

        /* Inverse for :
         *  Comparator(DataColumn column, DataTable tableList, string cmpSymbol, object constValue)
         * */
        [WitnessFunction(nameof(Semantics.Comparator), 3)]
        internal DisjunctiveExamplesSpec WitnessComparatorConstValue(GrammarRule rule, ExampleSpec spec)
        {
            /* Inverse for the constValue field */
            var ppExamples = new Dictionary<State, IEnumerable<object>>();

            foreach (State input in spec.ProvidedInputs)
            {
                DataTable outputTable = (DataTable)spec.Examples[input];

                var allPossibleSolutions = new List<object>();
                string cmpSymbol = (string)input[rule.Body[2]]; // Get the comparison symbol
                DataColumn column = (DataColumn)input[rule.Body[0]]; // Column
                if (column.DataType == typeof(string))
                {
                    switch (cmpSymbol)
                    {
                        case "==":
                            var valuesInColumn =
                                outputTable.Rows.Cast<DataRow>().Select(t => t[column.ColumnName]).Distinct();
                            // TODO: Get a linq guy to look this up
                            if (valuesInColumn.Count() == 1)
                            {
                                allPossibleSolutions.Add(valuesInColumn.First());
                            }
                            // else, Keep it as empty

                            break;
                        default:
                            // TODO: Unsupported datatype
                            break;
                    }

                }
                else if (column.DataType == typeof(double))
                {

                    switch (cmpSymbol)
                    {
                        case "==":
                            var valuesInColumn =
                                 outputTable.Rows.Cast<DataRow>().Select(t => t[column.ColumnName]).Distinct();
                            // TODO: Get a linq guy to look this up
                            if (valuesInColumn.Count() == 1)
                            {
                                allPossibleSolutions.Add(valuesInColumn.First());
                            }
                            // else, Keep it as empty
                            break;

                        case ">=":
                            var minValueInColumn = outputTable.Rows.Cast<DataRow>().Select(t => t[column.ColumnName]).Min();
                            allPossibleSolutions.Add((object)minValueInColumn);
                            break;

                        case "<=":
                            var maxValueInColumn = outputTable.Rows.Cast<DataRow>().Select(t => t[column.ColumnName]).Max();
                            allPossibleSolutions.Add((object)maxValueInColumn);
                            break;

                        default:
                            // TODO: Unsupported datatype
                            break;
                    }
                }

                ppExamples[input] = allPossibleSolutions;

            }

            return DisjunctiveExamplesSpec.From(ppExamples);
        }


        [WitnessFunction(nameof(Semantics.Comparator), 0)]
        internal DisjunctiveExamplesSpec WitnessComparatorColumn(GrammarRule rule, ExampleSpec spec)
        {
            /* Inverse for the column field */
            var ppExamples = new Dictionary<State, IEnumerable<object>>();

            foreach (State input in spec.ProvidedInputs)
            {
                DataTable outputTable = (DataTable)spec.Examples[input];
                DataTable[] tableList = (DataTable[])input[rule.Body[1]]; // Single table hack
                var dataColumnArrayArray= tableList.SelectMany(t => t.Columns.Cast<DataColumn>().ToArray()).ToArray();
                ppExamples[input] = dataColumnArrayArray; 

            }

            return DisjunctiveExamplesSpec.From(ppExamples);
        }



    } // End class
}
