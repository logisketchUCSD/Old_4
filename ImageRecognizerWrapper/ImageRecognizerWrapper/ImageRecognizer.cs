using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Threading;
using Clusters;
using Sketch;
using Featurefy;
using ImageRecognizer;
using Utilities;

namespace ImageRecognizerWrapper
{
    public delegate void RecognitionCompletedEventHandler(object sender, RunWorkerCompletedEventArgs e);

    public class ImageRecognizer
    {
        #region Member Variables

        ImageRecognizerSettings m_Settings;

        ImageRecognizerResults m_Results;

        FeatureSketch m_FeatureSketch;

        List<Cluster> m_OriginalClusters;

        Dictionary<Cluster, List<Cluster>> m_OrigCluster2Children;

        Dictionary<Cluster, Dictionary<string, List<SymbolRank>>> m_Cluster2FullImageResult;

        BackgroundWorker m_BWorker;

        public bool m_RecognitionComplete;

        #endregion

        #region Constructors

        public ImageRecognizer(FeatureSketch featureSketch, ImageRecognizerSettings settings)
        {
            m_Settings = settings;
            m_FeatureSketch = featureSketch;
            m_Results = new ImageRecognizerResults();
            m_OriginalClusters = new List<Cluster>();
            m_OrigCluster2Children = new Dictionary<Cluster, List<Cluster>>();
            m_Cluster2FullImageResult = new Dictionary<Cluster, Dictionary<string, List<SymbolRank>>>();
            m_RecognitionComplete = false;

            m_BWorker = new BackgroundWorker();
            m_BWorker.WorkerSupportsCancellation = true;
            m_BWorker.WorkerReportsProgress = true;
            m_BWorker.DoWork += new DoWorkEventHandler(m_BWorker_DoWork);
            m_BWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(m_BWorker_RunWorkerCompleted);
        }


        #endregion

        #region Events

        public event RecognitionCompletedEventHandler RecognitionCompleted;

        protected virtual void OnRecognitionCompleted(RunWorkerCompletedEventArgs e)
        {
            if (RecognitionCompleted != null)
                RecognitionCompleted(this, e);
        }

        #endregion

        #region Adding

        public void AddDefinition(BitmapSymbol symbol)
        {
            m_Settings.ImageDefinitions.AddDefinition(symbol);
        }

        public void AddDefinition(List<BitmapSymbol> symbols)
        {
            m_Settings.ImageDefinitions.AddDefinition(symbols);
        }

        public void AddClusters(Dictionary<Cluster, List<Cluster>> clustersAndChildren)
        {
            foreach (KeyValuePair<Cluster, List<Cluster>> pair in clustersAndChildren)
            {
                if (!m_OrigCluster2Children.ContainsKey(pair.Key))
                {
                    List<Cluster> clusters = new List<Cluster>(pair.Value.Count);
                    foreach (Cluster c in pair.Value)
                        clusters.Add(c);

                    m_OrigCluster2Children.Add(pair.Key, clusters);
                }

                if (!m_OriginalClusters.Contains(pair.Key))
                    m_OriginalClusters.Add(pair.Key);
            }
        }

        public void AddClusters(List<Cluster> originalClusters)
        {
            foreach (Cluster c in originalClusters)
                if (!m_OriginalClusters.Contains(c))
                    m_OriginalClusters.Add(c);
        }

        #endregion

        #region Recognize

        /*public Dictionary<Cluster, Dictionary<string, List<SymbolRank>>> RecognizeOriginalClusters()
        {
            Dictionary<Cluster, Dictionary<string, List<SymbolRank>>> results = new Dictionary<Cluster, Dictionary<string, List<SymbolRank>>>(m_OriginalClusters.Count);
            foreach (Cluster c in m_OriginalClusters)
            {
                string source = c.Strokes[0].XmlAttrs.Source;
                if (!m_Cluster2FullImageResult.ContainsKey(c))
                {
                    PlatformUsed platform = PlatformUsed.TabletPC;
                    if (c.Strokes.Count > 0 && c.Strokes[0].XmlAttrs.Source == "Wacom")
                        platform = PlatformUsed.Wacom;
                    Dictionary<string, List<SymbolRank>> completeResultsForCluster = m_Settings.ImageDefinitions.Recognize(c, c.ClassName, platform);
                    m_Cluster2FullImageResult.Add(c, completeResultsForCluster);
                    results.Add(c, completeResultsForCluster);
                }
                else
                    results.Add(c, m_Cluster2FullImageResult[c]);
            }

            return results;
        }

        public Dictionary<Cluster, Dictionary<string, List<SymbolRank>>> RecognizeChildrenOnly(Dictionary<Cluster, List<Cluster>> original2Children)
        {
            Dictionary<Cluster, Dictionary<string, List<SymbolRank>>> results = new Dictionary<Cluster, Dictionary<string, List<SymbolRank>>>(original2Children.Count * 100);
            foreach (KeyValuePair<Cluster, List<Cluster>> entry in original2Children)
            {
                Cluster original = entry.Key;
                List<Cluster> children = entry.Value;

                string source = "New";
                if (original.Strokes.Count > 0)
                    source = original.Strokes[0].XmlAttrs.Source;

                PlatformUsed platform = PlatformUsed.TabletPC;
                if (original.Strokes.Count > 0 && original.Strokes[0].XmlAttrs.Source == "Wacom")
                    platform = PlatformUsed.Wacom;

                foreach (Cluster c in children)
                {
                    if (c != original && !m_Cluster2FullImageResult.ContainsKey(c))
                    {
                        Dictionary<string, List<SymbolRank>> completeResultsForCluster = m_Settings.ImageDefinitions.Recognize(c, c.ClassName, platform);

                        m_Cluster2FullImageResult.Add(c, completeResultsForCluster);

                        if (!results.ContainsKey(c))
                            results.Add(c, completeResultsForCluster);
                    }
                    else if (!results.ContainsKey(c) && m_Cluster2FullImageResult.ContainsKey(c))
                        results.Add(c, m_Cluster2FullImageResult[c]);
                }
            }

            return results;
        }*/

        #endregion

        #region Background / Threading

        public void Recognize(List<Cluster> clusters)
        {
            if (m_BWorker.IsBusy)
            {
                m_BWorker.CancelAsync();
                Thread.Sleep(100);
            }

            if (m_BWorker.IsBusy)
            {
                Console.WriteLine("Couldn't enter BWorker because it was busy");
                return;
            }
            else
            {
                m_RecognitionComplete = false;
                m_BWorker.RunWorkerAsync(clusters);
            }
        }

        void m_BWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            OnRecognitionCompleted(e);
        }

        public ImageRecognizerResults RecognizeST(List<Cluster> clusters)
        {
            try
            {
                List<Cluster> scoredClusters = new List<Cluster>(clusters.Count);
                m_Results = new ImageRecognizerResults();

                for (int i = 0; i < clusters.Count; i++)
                {
                    if (!scoredClusters.Contains(clusters[i]))
                    {
                        ClusterScore score = AllImageDefinitions.Recognize(clusters[i]);
                        clusters[i].Score = score;
                        scoredClusters.Add(clusters[i]);
                    }
                    else
                        Console.WriteLine("Gotcha");
                }

                m_Results.ScoredClusters = scoredClusters;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ImageRecognizer DoWork SingleThread: {0}", ex.Message);
            }
            m_RecognitionComplete = true;

            return m_Results;
        }

        public ImageScore RecognizeST(Sketch.Shape shape, Dictionary<Substroke, List<SubstrokeDistance>> distances)
        {
            Cluster cluster = new Cluster(shape.SubstrokesL, shape.XmlAttrs.Classification, distances);

            m_RecognitionComplete = true;

            return AllImageDefinitions.Recognize(cluster).TopMatch;
        }


        void m_BWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                List<Cluster> clusters = (List<Cluster>)e.Argument;
                List<Cluster> scoredClusters = new List<Cluster>(clusters.Count);
                m_Results = new ImageRecognizerResults();

                for (int i = 0; i < clusters.Count; i++)
                {
                    if (m_BWorker.CancellationPending)
                    {
                        e.Result = scoredClusters;
                        m_Results.ScoredClusters = scoredClusters;
                        //e.Cancel = true;
                        return;
                    }

                    if (!scoredClusters.Contains(clusters[i]))
                    {
                        ClusterScore score = AllImageDefinitions.Recognize(clusters[i]);
                        clusters[i].Score = score;
                        scoredClusters.Add(clusters[i]);
                    }
                    else
                        Console.WriteLine("Gotcha");

                    int progress = i / clusters.Count;
                    m_BWorker.ReportProgress(progress);
                }

                e.Result = scoredClusters;
                m_Results.ScoredClusters = scoredClusters;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ImageRecognizer DoWork: {0}", ex.Message);
            }
            m_RecognitionComplete = true;
        }

        public bool CancelRecognition()
        {
            m_BWorker.CancelAsync();

            /*if (m_BWorker.IsBusy)
            {
                Console.WriteLine("Slept for 110 ms");
                Thread.Sleep(110); // Give the BWorker time to cancel
            }*/

            return !m_BWorker.IsBusy;
        }

        public bool BWorkerIsBusy
        {
            get { return m_BWorker.IsBusy; }
        }

        #endregion

        #region Getters

        public ImageRecognizerSettings Settings
        {
            get { return m_Settings; }
            set { m_Settings = value; }
        }

        public ImageRecognizerResults Results
        {
            get { return m_Results; }
        }

        public FeatureSketch FeatureSketch
        {
            get { return m_FeatureSketch; }
        }

        public ImageDefinitions AllImageDefinitions
        {
            get { return m_Settings.ImageDefinitions; }
            set { m_Settings.ImageDefinitions = value; }
        }

        #endregion

    }
}
