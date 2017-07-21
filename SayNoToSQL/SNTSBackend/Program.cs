using System;
using System.Data;
using SNTSBackend.Parser;
using Msri.Adp.Common.CommandLine;
using System.Collections.Generic;
using Microsoft.ProgramSynthesis.AST;

namespace SNTSBackend
{
    class Program
    {
        static void Main(string[] args)
        {
            //Console.WriteLine("Moved the tests to the test.");
            //Console.ReadLine();
            WCArguments parsedArgs = new WCArguments();
            if (Msri.Adp.Common.CommandLine.Parser.ParseArgumentsWithUsage(args, parsedArgs))
            {
                String inputFilePath = parsedArgs.inputFilePath;
                String outputFilePath = parsedArgs.outputFilePath;
                String initialFilePath = parsedArgs.initialFilePath;

                DataTable inputTable = CSVToDatatableParser.Parse(inputFilePath);
                DataTable outputTable = CSVToDatatableParser.Parse(outputFilePath, inputTable);
                DataTable initialTable = CSVToDatatableParser.Parse(initialFilePath);
                var generatedPrograms = Learner.Instance.LearnSQLAll(inputTable, outputTable);
                var program = Utils.Utils.GetBestProgram(generatedPrograms, initialTable);

                DataTable outputLearnt = Learner.Instance.Invoke(program, initialTable);
                Console.WriteLine("\nCHOSEN:\t" + program.ToString());
                //Utils.Utils.ShowTable(outputLearnt);
                DatatableToCSVWriter.Write(outputLearnt, inputFilePath.Replace("InputLarge.csv", "FinalOutputLarge.csv"));
            }
        }
    

    }
    class WCArguments
    {
        [Argument(ArgumentType.Required, HelpText = "Input Example Path")]
        public string inputFilePath;
        [Argument(ArgumentType.Required, HelpText = "Output Example Path")]
        public string outputFilePath;
        [Argument(ArgumentType.Required, HelpText = "Entire Data Path")]
        public string initialFilePath;
    }
}