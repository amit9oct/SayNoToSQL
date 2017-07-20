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
    }
}
