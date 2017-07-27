using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.AST;
using Microsoft.ProgramSynthesis.Learning;
using Microsoft.ProgramSynthesis.Learning.Logging;
using Microsoft.ProgramSynthesis.Learning.Strategies;
using Microsoft.ProgramSynthesis.Specifications;
using Microsoft.ProgramSynthesis.VersionSpace;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SNTSBackend
{
    public class Learner {
        public Grammar Grammar { get; private set; }
        public static bool GrammarNotCompiled = true;
        private static object lockObject = new object();
        public Dictionary<string, ProgramNode> ProgramNodeDict { get; private set; }
        public void SetGrammar(string filename) {
            Grammar = Utils.Utils.LoadGrammar(filename);
            Learner.GrammarNotCompiled = false;
        }
        private static Learner _instance = null;
        public static Learner Instance
        {
            get
            {
                lock (lockObject)
                {
                    if (_instance == null)
                    {
                        _instance = new Learner();
                    }
                    return _instance;
                }
            }
        }

        private Learner() {
            if (File.Exists(@"Semantics\SQL.grammar"))
            {
                Grammar = Utils.Utils.LoadGrammar(@"Semantics\SQL.grammar");
            }
            ProgramNodeDict = new Dictionary<string, ProgramNode>();
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
            return LearnAll(spec, null, new Semantics.WitnessFunctions(Grammar)).Take(10).ToArray();
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

        public string Query(ProgramNode pNode) {
            switch (pNode.Symbol.ToString()) {
                case "select":
                    if(pNode.GrammarRule.ToString() == "SelectWithWhere") {
                        var columns = Query(pNode.Children[0]);
                        var tables = Query(pNode.Children[2]);
                        var condition = Query(pNode.Children[1]);
                        return $"SELECT {columns} FROM {tables} WHERE {condition}";
                    }
                    else if(pNode.GrammarRule.ToString() == "SelectWithoutWhere") {
                        var columns = Query(pNode.Children[0]);
                        var tables = Query(pNode.Children[2]);
                        return $"SELECT {columns} FROM {tables}";
                    }
                    break;
                case "sql":
                    return Query(pNode.Children[0]);
                case "condition":
                    if (pNode.GrammarRule.ToString() == "~convert_condition_cmpStatement") {
                        return Query(pNode.Children[0]);
                    }
                    else if (pNode.GrammarRule.ToString() == "Logical") {
                        var cmp = Query(pNode.Children[0]);
                        var other = Query(pNode.Children[1]);
                        var logicalSymb = (string)(pNode.Children[2] as LiteralNode).Value;
                        return $"({cmp} {logicalSymb} {other})";
                    }
                    break;
                case "cmpStatement":
                    {
                        var column = (DataColumn)(pNode.Children[0] as LiteralNode).Value;
                        var columnName = column.ColumnName;
                        object constValue = (pNode.Children[3] as LiteralNode).Value;
                        var symb = (string)(pNode.Children[2] as LiteralNode).Value;
                        return $"{columnName} {symb} {constValue}";
                    }
                case "columnArray":
                    {
                        var columns = (DataColumn[])(pNode as LiteralNode).Value;
                        var newColumns = columns.Where(c => c.ColumnName != "PrimaryKey").Select(c => c.ColumnName);
                        return string.Join(", ", newColumns);
                    }
                case "tableArray":
                    {
                        var tables = (DataTable[])(pNode as LiteralNode).Value;
                        var newTables = tables.Select(t => t.TableName);
                        return string.Join(", ", newTables);
                    }
                case "tableNames":
                    {
                        var tableNames = (string[])(pNode as LiteralNode).Value;
                        return string.Join(",",tableNames);
                    }
            }
            return pNode.ToString();
        }
    }
}
