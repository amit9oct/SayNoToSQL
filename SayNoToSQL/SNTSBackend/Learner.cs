using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.AST;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Learning.Logging;
using Microsoft.ProgramSynthesis.Learning.Strategies;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.VersionSpace;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTSBackend
{
    public class Learner {
        public Grammar Grammar { get; private set; }

        public void SetGrammar(string filename) {
            Grammar = Utils.Utils.LoadGrammar(filename);
        }

        public static Learner Instance = new Learner();

        private Learner() {
           if(File.Exists(@"Semantics\SQL.grammar"))
                Grammar = Utils.Utils.LoadGrammar(@"Semantics\SQL.grammar");
        }

        private ProgramNode[] LearnAll(Spec spec, Feature<double> scorer, DomainLearningLogic witnessFunctions) {
            var engine = new SynthesisEngine(Grammar, new SynthesisEngine.Config {
                Strategies = new ISynthesisStrategy[] {
                    new EnumerativeSynthesis(),
                    new DeductiveSynthesis(witnessFunctions)
                },
                UseThreads = false,
                LogListener = new LogListener(),
            });
            ProgramSet consistentPrograms = engine.LearnGrammar(spec);
            engine.Configuration.LogListener.SaveLogToXML("learning.log.xml");

            //See if there is a ranking function.
            if (scorer != null) { 
                //If there is a ranking function then find the best program.
                ProgramNode bestProgram = consistentPrograms.TopK(scorer).FirstOrDefault();
                if (bestProgram == null)
                {
                    Utils.Utils.WriteColored(ConsoleColor.Red, "No program :(");
                    return null;
                }
                var score = bestProgram.GetFeatureValue(scorer);
                Utils.Utils.WriteColored(ConsoleColor.Cyan, $"[score = {score:F3}] {bestProgram}");
                return new ProgramNode[] { bestProgram };
            }
            return consistentPrograms.AllElements.ToArray();
        }

        public ProgramNode LearnSQL(DataTable inputTable, DataTable outputTable) {
            var spec = SpecFromStateOutput(inputTable, outputTable);
            var programNode = LearnAll(spec, null, new Semantics.WitnessFunctions(Grammar));
            return programNode.First();
        }

        public ProgramNode[] LearnSQLAll(DataTable inputTable, DataTable outputTable)
        {
            var spec = SpecFromStateOutput(inputTable, outputTable);
            return LearnAll(spec, null, new Semantics.WitnessFunctions(Grammar));
        }

        public DataTable Invoke(ProgramNode programNode, DataTable inputTable) {
            return (DataTable)programNode.Invoke(StateFromInput(inputTable));
        }

        private Spec SpecFromStateOutput(DataTable inputTable, DataTable outputTable) {
            Dictionary<State, object> exampleDict = new Dictionary<State, object>();
            //hack should have more tables
            exampleDict.Add(StateFromInput(inputTable), outputTable);
            return new ExampleSpec(exampleDict);
        }

        private State StateFromInput(DataTable dataTable) {
            return StateFromInput(new DataTable[] { dataTable });
        }

        private State StateFromInput(DataTable[] dataTables) {
            return State.Create(Grammar.InputSymbol, dataTables);
        }
    }
}
