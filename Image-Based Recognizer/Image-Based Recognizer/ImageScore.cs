using System;
using System.Collections.Generic;
using System.Text;
using Utilities;

namespace ImageRecognizer
{
    [Serializable]
    public class ImageScore
    {
        #region Member Variables

        /// <summary>
        /// Ex: AND_02_T_RPT_003
        /// </summary>
        string m_FullSymbolName;

        /// <summary>
        /// Ex: AND
        /// </summary>
        string m_SymbolType;

        /// <summary>
        /// Ex: Gate
        /// </summary>
        string m_SymbolClass;

        /// <summary>
        /// Ex: 01
        /// </summary>
        string m_UserName;

        /// <summary>
        /// Ex: TabletPC
        /// </summary>
        PlatformUsed m_PlatformUsed;

        /// <summary>
        /// Ex: Complete
        /// </summary>
        SymbolCompleteness m_Completeness;

        /// <summary>
        /// Ex: Repeat
        /// </summary>
        DrawingTask m_DrawingTask;

        /// <summary>
        /// Combined score of 4 distance metrics
        /// </summary>
        double m_FusionScore;

        /// <summary>
        /// Hausdorf Distance
        /// </summary>
        double m_HausdorfScore;

        /// <summary>
        /// Modified Hausdorf Distance
        /// </summary>
        double m_ModifiedHausdorfScore;

        /// <summary>
        /// Yule Coefficient
        /// </summary>
        double m_YuleScore;

        /// <summary>
        /// Tanimoto Coefficient
        /// </summary>
        double m_TanimotoScore;

        BitmapSymbol m_Template;

        #endregion

        #region Constructors

        public ImageScore(SymbolRank result)
        {
            m_FusionScore = result.Distance;
            m_HausdorfScore = result.Symbol.HausdorffDistance;
            m_ModifiedHausdorfScore = result.Symbol.ModifiedHausdorffDistance;
            m_TanimotoScore = result.Symbol.TanimotoCoefficient;
            m_YuleScore = result.Symbol.YuleCoefficient;

            m_Completeness = result.Symbol.Completeness;
            m_DrawingTask = result.Symbol.DrawTask;
            m_FullSymbolName = result.Symbol.Name;
            m_PlatformUsed = result.Symbol.Platform;
            m_SymbolClass = result.Symbol.SymbolClass;
            m_SymbolType = result.Symbol.SymbolType;
            m_UserName = result.Symbol.UserName;

            m_Template = result.Symbol;
        }

        #endregion

        public static Dictionary<int, ImageScore> SortTopMatches(Dictionary<ImageScore, int> topMatches)
        {
            double score1 = 1.0;
            double score2 = 1.0;// 0.9;
            double score3 = 1.0;// 0.8;
            double score4 = 1.0;// 0.7;

            Dictionary<string, double> shapeWeights = new Dictionary<string, double>();
            shapeWeights.Add("AND", score2);
            shapeWeights.Add("NAND", score3);
            shapeWeights.Add("OR", score2);
            shapeWeights.Add("NOR", score3);
            shapeWeights.Add("XOR", score3);
            shapeWeights.Add("XNOR", score4); 
            shapeWeights.Add("NOT", score2);
            shapeWeights.Add("NOTBUBBLE", score1);
            shapeWeights.Add("BUBBLE", score1);
            shapeWeights.Add("LabelBox", score3);
            shapeWeights.Add("Female", score1);
            shapeWeights.Add("Male", score1);            

            #region Max/Min calculation
            double Haussmin = double.MaxValue;
            double Haussmax = double.MinValue;
            double ModHaussmin = double.MaxValue;
            double ModHaussmax = double.MinValue;
            double Tanimotomin = double.MaxValue;
            double Tanimotomax = double.MinValue;
            double Yulemin = double.MaxValue;
            double Yulemax = double.MinValue;

            foreach (KeyValuePair<ImageScore, int> entry in topMatches)
            {
                ImageScore score = entry.Key;

                Haussmin = Math.Min(Haussmin, score.HausdorfScore);
                Haussmax = Math.Max(Haussmax, score.HausdorfScore);
                ModHaussmin = Math.Min(ModHaussmin, score.ModifiedHausdorfScore);
                ModHaussmax = Math.Max(ModHaussmax, score.ModifiedHausdorfScore);
                Tanimotomin = Math.Min(Tanimotomin, score.TanimotoScore);
                Tanimotomax = Math.Max(Tanimotomax, score.TanimotoScore);
                Yulemin = Math.Min(Yulemin, score.YuleScore);
                Yulemax = Math.Max(Yulemax, score.YuleScore);
            }
            #endregion

            SortedList<double, ImageScore> sortedResults = new SortedList<double, ImageScore>(topMatches.Count);

            foreach (KeyValuePair<ImageScore, int> entry in topMatches)
            {
                ImageScore score = entry.Key;

                double h = score.HausdorfScore;
                double mh = score.ModifiedHausdorfScore;
                double t = score.TanimotoScore;
                double y = score.YuleScore;

                // Weight it so that larger shapes get better scores.
                if (shapeWeights.ContainsKey(score.SymbolType))
                {
                    h *= shapeWeights[score.SymbolType];
                    mh *= shapeWeights[score.SymbolType];
                    y *= shapeWeights[score.SymbolType];
                    t *= shapeWeights[score.SymbolType];
                }

                //*shapeWeights[score.SymbolType];
                double distance = Vote(Haussmin, Haussmax, h)
                    + Vote(ModHaussmin, ModHaussmax, mh);
                    //+ Vote(Tanimotomin, Tanimotomax, t)
                    //+ Vote(Yulemin, Yulemax, y);

                while (sortedResults.ContainsKey(distance))
                    distance += 0.00000001;

                sortedResults.Add(distance, entry.Key);
            }

            Dictionary<int, ImageScore> results = new Dictionary<int, ImageScore>(sortedResults.Count);
            foreach (KeyValuePair<double, ImageScore> entry in sortedResults)
                results.Add(topMatches[entry.Value], entry.Value);

            return results;
        }

        private static double Vote(double min, double max, double val)
        {
            double EPSILON = 10e-6;
            if (Math.Abs(max - min) < EPSILON) return double.PositiveInfinity;

            return (val - min) / (max - min) * 100.0;
        }

        #region Getters

        public string UserName
        {
            get { return m_UserName; }
        }

        public string FullSymbolName
        {
            get { return m_FullSymbolName; }
        }

        public string SymbolType
        {
            get { return m_SymbolType; }
        }

        public string SymbolClass
        {
            get { return m_SymbolClass; }
        }

        public SymbolCompleteness Completeness
        {
            get { return m_Completeness; }
        }

        public PlatformUsed Platform
        {
            get { return m_PlatformUsed; }
        }

        public DrawingTask DrawTask
        {
            get { return m_DrawingTask; }
        }

        public double FusionScore
        {
            get { return m_FusionScore; }
        }

        public double HausdorfScore
        {
            get { return m_HausdorfScore; }
        }

        public double ModifiedHausdorfScore
        {
            get { return m_ModifiedHausdorfScore; }
        }

        public double TanimotoScore
        {
            get { return m_TanimotoScore; }
        }

        public double YuleScore
        {
            get { return m_YuleScore; }
        }

        public BitmapSymbol TemplateSymbol
        {
            get { return m_Template; }
        }

        #endregion
    }
}
