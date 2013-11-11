using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using ImageRecognizer;
using Clusters;
using Sketch;
using Utilities;

namespace ImageRecognizerWrapper
{
    [Serializable]
    public class ImageDefinitions : ISerializable
    {
        #region Member Variables

        List<BitmapSymbol> m_ImageDefinitions;

        Dictionary<string, List<BitmapSymbol>> m_Class2ImageDefinitions;

        Dictionary<PlatformUsed, List<BitmapSymbol>> m_Platform2ImageDefinitions;

        #endregion

        #region Constructors

        public ImageDefinitions()
        {
            m_ImageDefinitions = new List<BitmapSymbol>();
            m_Class2ImageDefinitions = new Dictionary<string, List<BitmapSymbol>>();
            m_Platform2ImageDefinitions = new Dictionary<PlatformUsed, List<BitmapSymbol>>();
        }

        public ImageDefinitions(List<BitmapSymbol> definitions)
        {
            m_ImageDefinitions = definitions;
            m_Class2ImageDefinitions = new Dictionary<string, List<BitmapSymbol>>();
            m_Platform2ImageDefinitions = new Dictionary<PlatformUsed, List<BitmapSymbol>>();
            foreach (BitmapSymbol symbol in m_ImageDefinitions)
            {
                string key = symbol.SymbolClass;
                if (key != null)
                {
                    if (!m_Class2ImageDefinitions.ContainsKey(key))
                    {
                        List<BitmapSymbol> symbols = new List<BitmapSymbol>();
                        symbols.Add(symbol);
                        m_Class2ImageDefinitions.Add(key, symbols);
                    }
                    else
                        m_Class2ImageDefinitions[key].Add(symbol);
                }

                PlatformUsed platform = symbol.Platform;
                if (!m_Platform2ImageDefinitions.ContainsKey(platform))
                {
                    List<BitmapSymbol> symbols = new List<BitmapSymbol>();
                    symbols.Add(symbol);
                    m_Platform2ImageDefinitions.Add(platform, symbols);
                }
                else
                    m_Platform2ImageDefinitions[platform].Add(symbol);
            }
        }

        #endregion

        public List<BitmapSymbol> GetDefinitions(string className, PlatformUsed platform)
        {
            List<BitmapSymbol> listFromClass;
            bool success = m_Class2ImageDefinitions.TryGetValue(className, out listFromClass);
            if (!success)
                return new List<BitmapSymbol>();

            List<BitmapSymbol> listFromPlatform;
            bool success2 = m_Platform2ImageDefinitions.TryGetValue(platform, out listFromPlatform);
            if (!success2)
                return new List<BitmapSymbol>();

            List<BitmapSymbol> combined = new List<BitmapSymbol>(listFromPlatform.Count);

            foreach (BitmapSymbol symbol in listFromClass)
                if (listFromPlatform.Contains(symbol))
                    combined.Add(symbol);

            return combined;
        }

        public void AddDefinition(BitmapSymbol symbol)
        {
            if (!m_ImageDefinitions.Contains(symbol))
                m_ImageDefinitions.Add(symbol);
        }

        public void AddDefinition(List<BitmapSymbol> symbols)
        {
            foreach (BitmapSymbol symbol in symbols)
                AddDefinition(symbol);
        }

        /*public Dictionary<string, List<SymbolRank>> Recognize(Cluster cluster, string className, PlatformUsed platform)
        {
            return Recognize(cluster.Strokes, className, platform);
        }

        public Dictionary<string, List<SymbolRank>> Recognize(List<Substroke> strokes, string className, PlatformUsed platform)
        {
            List<BitmapSymbol> relevantDefinitions = GetDefinitions(className, platform);
            BitmapSymbol unknownSymbol = new BitmapSymbol(strokes);
            unknownSymbol.Process();

            Dictionary<string, List<SymbolRank>> completeSymbolResults;
            List<string> shortResults = unknownSymbol.FindSimilarity_and_Rank(relevantDefinitions, out completeSymbolResults);

            return completeSymbolResults;
        }*/

        public ClusterScore Recognize(Cluster cluster)
        {
            PlatformUsed platform = PlatformUsed.TabletPC;
            if (cluster.Strokes.Count > 0 && cluster.Strokes[0].XmlAttrs.Source == "Wacom")
                platform = PlatformUsed.Wacom;

            //Dictionary<string, List<SymbolRank>> results = Recognize(cluster, cluster.ClassName, platform);
            List<ImageScore> imgScores = Recognize(cluster, cluster.ClassName, platform);
            ClusterScore score = new ClusterScore(imgScores);
            return score;
        }

        public List<ImageScore> Recognize(Cluster cluster, string className, PlatformUsed platform)
        {
            return Recognize(cluster.Strokes, className, platform);
        }

        public List<ImageScore> Recognize(List<Substroke> strokes, string className, PlatformUsed platform)
        {
            List<BitmapSymbol> relevantDefinitions = GetDefinitions(className, platform);
            BitmapSymbol unknownSymbol = new BitmapSymbol(strokes);
            unknownSymbol.Process();

            List<double> origins = new List<double>();
            origins.Add(0.0);
            if (className != "Label")
                origins.Add(90.0);

            return unknownSymbol.FindBestMatches(relevantDefinitions, 5, 15.0, origins);
        }

        public List<BitmapSymbol> Definitions
        {
            get { return m_ImageDefinitions; }
        }

        public static ImageDefinitions LoadDefinitionsFromFile(string filename)
        {
            System.IO.Stream stream = System.IO.File.Open(filename, System.IO.FileMode.Open);
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            ImageDefinitions defns = (ImageDefinitions)bformatter.Deserialize(stream);
            stream.Close();

            return defns;
        }

        public static void SaveDefinitionsToFile(string filename, ImageDefinitions defns)
        {
            System.IO.Stream stream = System.IO.File.Create(filename);
            System.Runtime.Serialization.Formatters.Binary.BinaryFormatter bformatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            bformatter.Serialize(stream, defns);
            stream.Close();
        }

        #region Serialization

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        /// <param name="context"></param>
        public ImageDefinitions(SerializationInfo info, StreamingContext context)
        {
            m_ImageDefinitions = (List<BitmapSymbol>)info.GetValue("Definitions", typeof(List<BitmapSymbol>));
            m_Class2ImageDefinitions = (Dictionary<string, List<BitmapSymbol>>)info.GetValue("Class2Definitions", typeof(Dictionary<string, List<BitmapSymbol>>));
            m_Platform2ImageDefinitions = (Dictionary<PlatformUsed, List<BitmapSymbol>>)info.GetValue("Platform2Definitions", typeof(Dictionary<PlatformUsed, List<BitmapSymbol>>));
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Definitions", m_ImageDefinitions);
            info.AddValue("Class2Definitions", m_Class2ImageDefinitions);
            info.AddValue("Platform2Definitions", m_Platform2ImageDefinitions);
        }

        #endregion
    }
}
