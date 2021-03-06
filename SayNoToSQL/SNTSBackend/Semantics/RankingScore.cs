﻿using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.ProgramSynthesis;
using Microsoft.ProgramSynthesis.AST;
using Microsoft.ProgramSynthesis.Extraction.Text.Semantics;
using Microsoft.ProgramSynthesis.Features;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace SNTSBackend.Semantics
{
    public class RankingScore : Feature<double>
    {
        public RankingScore(Grammar grammar) : base(grammar, "RankingScore") { }

        protected override double GetFeatureValueForVariable(VariableNode variable) => 0;

        [FeatureCalculator(nameof(Semantics.SelectWithoutWhere))]
        public static double Score_SelectWithoutWhere(double colAr, double taAr, double taNames) => 10;

        [FeatureCalculator(nameof(Semantics.SelectWithWhere))]
        public static double Score_SelectWithWhere(double colAr, double condition, double tableNames) => 10 + condition;

        [FeatureCalculator(nameof(Semantics.Comparator))]
        public static double Score_Comparator(double colAr, double taAr, double cmpSymbol, double constVal) => cmpSymbol;

        [FeatureCalculator(nameof(Semantics.Logical))]
        public static double Score_Logical(double cmpStat, double condition, double logicSymbol) => condition + cmpStat + logicSymbol;

        [FeatureCalculator("cmpSymbol", Method = CalculationMethod.FromLiteral)]
        public static double Score_cmpSymbol(string cmpSymbol) {
            switch (cmpSymbol)
            {
                case "!=":
                    return -0.4;
                case ">":
                case "<":
                    return -0.3;
                // case "AND": return -0.2;
                case ">=":
                case "<=":
                    return -0.2;
                
                default:
                    return 0;
            }
        }

        [FeatureCalculator("logicSymbol", Method = CalculationMethod.FromLiteral)]
        public static double Score_logicSymbol(string logicSymbol) {
            switch (logicSymbol)
            {
                case "OR": return -0.3;
             // case "AND": return -0.2;
                default:
                    return 0;
            }
        }

        [FeatureCalculator("columnArray", Method = CalculationMethod.FromLiteral)]
        public static double Score_columnArray(DataColumn[] columnArray) => 0;

        [FeatureCalculator("column", Method = CalculationMethod.FromLiteral)]
        public static double Score_column(DataColumn column) => 0;

        [FeatureCalculator("constValue", Method = CalculationMethod.FromLiteral)]
        public static double Score_constValue(object constValue) => 0;

        [FeatureCalculator("tableNames", Method = CalculationMethod.FromLiteral)]
        public static double Score_tableArray(string[] tableNames) => 0.0;

    }
}
