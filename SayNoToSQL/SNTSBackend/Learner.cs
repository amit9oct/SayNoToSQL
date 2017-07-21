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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTSBackend
{
    public class Learner {
        public Grammar Grammar { get; }

        public static Learner Instance = new Learner();
        public Semantics.RankingScore staticScorer;
        private Learner() {
            Grammar = Utils.Utils.LoadGrammar(@"Semantics\SQL.grammar");
            staticScorer = new Semantics.RankingScore(Grammar);
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
            if (scorer != null)
            {
                //If there is a ranking function then find the best program.
                int nProgs = consistentPrograms.AllElements.Count();
                if (nProgs > 10)
                {
                    nProgs = 10;
                }
                return consistentPrograms.TopK(scorer, k: nProgs).ToArray();
                

            }
            else
            {
                return consistentPrograms.AllElements.ToArray();
            }
        }

        public ProgramNode LearnSQL(DataTable inputTable, DataTable outputTable) {
            var spec = SpecFromStateOutput(inputTable, outputTable);
            var programNode = LearnAll(spec, staticScorer, new Semantics.WitnessFunctions(Grammar));
            return programNode.First();
        }

        public ProgramNode[] LearnSQLAll(DataTable inputTable, DataTable outputTable)
        {
            var spec = SpecFromStateOutput(inputTable, outputTable);
            return LearnAll(spec, staticScorer, new Semantics.WitnessFunctions(Grammar));
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
