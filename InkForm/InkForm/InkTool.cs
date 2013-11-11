/*************************************************************************************************
 * This InkTool is intended to allow the functionality of the InkPanel to be easily added to other
 * panels.  Right now the InkTool is not entirely seperable unfortunately.
 * The main refactoring was splitting up InkPanel.LoadXML and extracting out what is now 
 * InkTool.ClassifySketch.  All of the code was written by Eric Peterson.
 *
 * More comments need to be added in the future
 **************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Featurefy;
using Microsoft.Ink;
using StrokeClassifier;
using Clusterer;
using Clusters;
using Utilities;

namespace InkForm
{
    public delegate void FeaturizationFinished(object sender, EventArgs e);

    public class InkTool
    {
        #region Member Variables
        // TODO add descriptions

        /// <summary>
        /// The inkPanel this subscribes too
        /// </summary>
        //System.Windows.Forms.Panel inkPanel;
        InkPanel inkPanel; // needs InkOverlay

        Clusterer.Clusterer m_Clusterer;

        ClustererSettings m_ClustererSettings;

        Domain m_Domain;

        Dictionary<Sketch.Substroke, System.Drawing.Color> m_ClusterColors;

        Dictionary<Sketch.Substroke, System.Drawing.Color> m_TopCompleteMatchColors;

        Dictionary<Sketch.Substroke, System.Drawing.Color> m_TopMatchColors;

        Dictionary<Sketch.Substroke, System.Drawing.Color> m_ClassifierColors;



        #endregion

        #region Constructors

        public InkTool(InkPanel IP)
            : base()
        {
            inkPanel = IP;


            m_ClusterColors = new Dictionary<Sketch.Substroke, System.Drawing.Color>();
            m_TopCompleteMatchColors = new Dictionary<Sketch.Substroke, System.Drawing.Color>();
            m_TopMatchColors = new Dictionary<Sketch.Substroke, System.Drawing.Color>();
            m_ClassifierColors = new Dictionary<Sketch.Substroke, System.Drawing.Color>();
        }

        public InkTool(InkPanel IP, Domain domain, ClustererSettings settings)
            : base()
        {
            inkPanel = IP;
            m_Domain = domain;
            m_ClustererSettings = settings;

            m_ClusterColors = new Dictionary<Sketch.Substroke, System.Drawing.Color>();
            m_TopCompleteMatchColors = new Dictionary<Sketch.Substroke, System.Drawing.Color>();
            m_TopMatchColors = new Dictionary<Sketch.Substroke, System.Drawing.Color>();
            m_ClassifierColors = new Dictionary<Sketch.Substroke, System.Drawing.Color>();
        }

        #endregion

        #region Subscription

        public void InitializeClusterer()
        {
            

            m_Clusterer = new Clusterer.Clusterer(m_ClustererSettings);


            m_Clusterer.InitialClustersDone += new InitialClustersCompleted(m_Clusterer_InitialClustersDone);
            m_Clusterer.FinalClustersDone += new FinalClustersCompleted(m_Clusterer_FinalClustersDone);
            //m_Clusterer.StrokeClassificationDone += new StrokeClassificationCompleted(m_Clusterer_StrokeClassificationDone);
            m_Clusterer.FeaturizingDone += new FeaturizationCompleted(m_Clusterer_FeaturizingDone);

            m_Clusterer.CurrentUser = new User();
            m_Clusterer.CurrentPlatform = PlatformUsed.TabletPC;
        }

        public void InitializeClusterer(Domain domain, ClustererSettings settings)
        {
            m_Domain = domain;
            m_ClustererSettings = settings;

            InitializeClusterer();
        }

        public void InitializeClusterer(ClustererSettings settings, FeatureSketch featureSketch, Dictionary<int, Sketch.Substroke> mStroke2Substroke)
        {
            m_Clusterer = new Clusterer.Clusterer(settings, featureSketch);
            m_Clusterer.InitialClustersDone += new InitialClustersCompleted(m_Clusterer_InitialClustersDone);
            m_Clusterer.FinalClustersDone += new FinalClustersCompleted(m_Clusterer_FinalClustersDone);
            //m_Clusterer.StrokeClassificationDone += new StrokeClassificationCompleted(m_Clusterer_StrokeClassificationDone);
            
        }

  
        #endregion

        #region Events
        // TODO add comments

        public event FeaturizationFinished FeaturizingDone;
       
        protected virtual void OnFeaturizingCompleted(EventArgs e)
        {
            if (FeaturizingDone != null)
                FeaturizingDone(this, e);
        }

        void m_Clusterer_FeaturizingDone(object sender, EventArgs e)
        {
            OnFeaturizingCompleted(e);
        }

        void m_Clusterer_FinalClustersDone(object sender, EventArgs e)
        {
            //m_TopMatchColors = m_Clusterer.TopMatchColors;
            //m_TopCompleteMatchColors = m_Clusterer.TopCompleteMatchColors;
        }

        void m_Clusterer_InitialClustersDone(object sender, EventArgs e)
        {
            try
            {
                //Console.WriteLine("Initial Clusters Finished at {0}", ((ulong)DateTime.Now.ToFileTime() - 116444736000000000) / 10000);
                //m_ClusterColors = m_Clusterer.ClusterColors;

                inkPanel.RefreshPanel();
            }
            catch (Exception ex)
            {
                Console.WriteLine("InkPanel InitialClustersDone: " + ex.Message);
                //throw ex;
            }
        }



        #endregion


        #region Getters

        public Dictionary<Sketch.Substroke, System.Drawing.Color> ClusterColors
        {
            get { return m_ClusterColors; }
        }

        public Dictionary<Sketch.Substroke, System.Drawing.Color> TopCompleteMatchColors
        {
            get { return m_TopCompleteMatchColors; }
        }

        public Dictionary<Sketch.Substroke, System.Drawing.Color> TopMatchColors
        {
            get { return m_TopMatchColors; }
        }

        public Dictionary<Sketch.Substroke, System.Drawing.Color> ClassifierColors
        {
            get { return m_ClassifierColors; }
        }

        public Clusterer.Clusterer Clusterer
        {
            get { return m_Clusterer; }
            set { m_Clusterer = value; }
        }

        public Domain Domain
        {
            get { return m_Domain; }
            set { m_Domain = value; }
        }

        #endregion

        /// <summary>
        /// This function takes in a list of scored clusters and creates a new sketch which has been updated to reflect 
        /// those changes
        /// </summary>
        /// <param name="scored"></param>
        /// <returns></returns>
        public Sketch.Sketch MakeSketch(List<Cluster> scored)
        {
            try
            {
                //List<Cluster> scored = (List<Cluster>)iTool.Clusterer.ImageRecognizer.Results.ScoredClusters;
                Sketch.Sketch rSketch = new Sketch.Sketch();
                foreach (Cluster group in scored)
                {
                    Sketch.XmlStructs.XmlShapeAttrs xml = new Sketch.XmlStructs.XmlShapeAttrs();
                    if (group.HasBeenScored && group.Score.TopMatch != null)
                    {
                        xml.Name = group.Score.TopMatch.SymbolType;
                        xml.Type = group.Score.TopMatch.SymbolType;
                    }
                    else
                    {
                        xml.Name = group.ClassName;
                        xml.Type = group.ClassName;
                    }

                    Sketch.Shape s = new Sketch.Shape(group.Strokes, xml);
                    rSketch.AddShape(s);

                    //foreach (Sketch.Substroke sub in group.Strokes)
                    //{
                    //if (sub.Labels.Length == 0)
                    //    rSketch.AddLabel(sub, group.Score.TopMatch.SymbolType);
                    //}
                }

                return rSketch;
            }
            catch (Exception ex)
            {
                Console.WriteLine("ImageRecognizerCompleted: {0}", ex.Message);
                return new Sketch.Sketch();
            }
        }

        /// <summary>
        /// This takes a sketch and initializes the clusterer to use that sketch
        /// </summary>
        /// <param name="sketch"></param>
        /// <param name="settingsFilename"></param>
        /// <param name="dir"></param>
        /// <returns></returns>
        public bool ClassifySketch(Sketch.Sketch sketch, string settingsFilename, String dir)
        {
            //Sketch.Sketch rSketch = new Sketch.Sketch();
            //rSketch.XmlAttrs = sketch.XmlAttrs;


            //foreach (Sketch.Shape shape in sketch.Shapes)
            //{
            //    Sketch.Shape rShape = new Sketch.Shape();
            //    rShape.XmlAttrs = shape.XmlAttrs;
            //    foreach (Sketch.Substroke stroke in shape.Substrokes)
            //    {
            //        Sketch.Substroke rStroke = Featurefy.Compute.DeHook(stroke);
            //        if (this.Clusterer.CurrentPlatform == "Wacom")
            //            rStroke.XmlAttrs.Source = "Wacom";

            //        rShape.AddSubstroke(rStroke);
            //    }

            //    rSketch.AddShape(rShape);
            //}

            //sketch = rSketch;
            

            /*// Dehook strokes
            for (int i = 0; i < sketch.Substrokes.Length; i++)
            {
                Sketch.Substroke s = Compute.DeHook(sketch.Substrokes[i]);
                if (filename.Contains("_P"))
                {
                    sketch.Substrokes[i].XmlAttrs.Source = "Wacom";
                    s.XmlAttrs.Source = "Wacom";
                    s.ParentStroke.XmlAttrs.Source = "Wacom";
                }
                else
                {
                }
                //This doesn't actually work...!!!
                sketch.Substrokes[i] = s;
                
            }*/

            inkPanel.InkOverlay.Ink.DeleteStrokes();
            Dictionary<int, Sketch.Substroke> mStroke2Substroke = new Dictionary<int, Sketch.Substroke>();
            foreach (Sketch.Substroke s in sketch.Substrokes)
            {
                inkPanel.InkOverlay.Ink.CreateStroke(s.PointsAsSysPoints);
                ulong[] times = new ulong[s.Points.Length];
                for (int i = 0; i < s.Points.Length; i++)
                    times[i] = s.Points[i].Time;

                inkPanel.InkOverlay.Ink.Strokes[inkPanel.InkOverlay.Ink.Strokes.Count - 1].ExtendedProperties.Add(General.theTimeGuid, (object)times);

                int sKey = inkPanel.InkOverlay.Ink.Strokes[inkPanel.InkOverlay.Ink.Strokes.Count - 1].Id;
                if (!mStroke2Substroke.ContainsKey(sKey))
                    mStroke2Substroke.Add(sKey, s);
            }
            
            FeatureSketch featureSketch = new FeatureSketch(sketch, this.Clusterer.Settings.ClassifierSettings.FeaturesOn, this.Clusterer.Settings.GrouperSettings.FeaturesOn, this.Clusterer.Settings.AvgsAndStdDevs);
            //Console.WriteLine("Feature Sketch finished at {0}", ((ulong)DateTime.Now.ToFileTime() - 116444736000000000) / 10000);

            ClustererSettings settings = this.Clusterer.Settings;

            if (this.Clusterer.CurrentUser.Name != "New")
            {
                try
                {
                    Dictionary<string, string> directories = inkPanel.ReadSettings(settingsFilename);

                    if (directories.ContainsKey("DirectoryStrokeNetworks"))
                    {
                        string strokeNetworkName = dir + "\\" + directories["DirectoryStrokeNetworks"] + "Network" + settings.CurrentUser + ".nn";
                        settings.ClassifierSettings.Network = Utilities.Neuro.NeuralNetwork.LoadNetworkFromFile(strokeNetworkName);
                    }
                    else
                        MessageBox.Show("No stroke network directory given! (Key = DirectoryStrokeNetworks)");

                    if (directories.ContainsKey("DirectoryGateNetworks"))
                    {
                        string gateNetworkName = dir + "\\" + directories["DirectoryGateNetworks"] + "Network" + settings.CurrentUser + ".nn";
                        settings.GrouperSettings.Networks["Gate"] = Utilities.Neuro.NeuralNetwork.LoadNetworkFromFile(gateNetworkName);
                    }
                    else
                        MessageBox.Show("No gate network directory given! (Key = DirectoryGateNetworks)");

                    if (directories.ContainsKey("DirectoryLabelNetworks"))
                    {
                        string labelNetworkName = dir + "\\" + directories["DirectoryLabelNetworks"] + "Network" + settings.CurrentUser + ".nn";
                        settings.GrouperSettings.Networks["Label"] = Utilities.Neuro.NeuralNetwork.LoadNetworkFromFile(labelNetworkName);
                    }
                    else
                        MessageBox.Show("No label network directory given! (Key = DirectoryLabelNetworks)");

                    if (directories.ContainsKey("DirectoryWireNetworks"))
                    {
                        string wireNetworkName = dir + "\\" + directories["DirectoryWireNetworks"] + "Network" + settings.CurrentUser + ".nn";
                        settings.GrouperSettings.Networks["Wire"] = Utilities.Neuro.NeuralNetwork.LoadNetworkFromFile(wireNetworkName);
                    }
                    else
                        MessageBox.Show("No wire network directory given! (Key = DirectoryWireNetworks)");

                    if (directories.ContainsKey("DirectoryImageDefinitions"))
                    {
                        string imgDefName = dir + "\\" + directories["DirectoryImageDefinitions"] + "ImageDefinitions" + settings.CurrentUser + ".imdf";
                        //this.Clusterer.ImageRecognizer.AllImageDefinitions = ImageRecognizerWrapper.ImageDefinitions.LoadDefinitionsFromFile(imgDefName);
                    }
                    else
                        MessageBox.Show("No stroke network directory given! (Key = DirectoryStrokeNetworks)");
                }
                catch (Exception e)
                {
                    MessageBox.Show("Error duing try in ClassifySketch: " + e.Message);
                    return false;
                }
            }
            InitializeClusterer(settings, featureSketch, mStroke2Substroke);
            settings.Save("C:\\Documents and Settings\\eric\\My Documents\\Trunk\\Code\\Recognition\\Clusterer\\Clusterer");

            featureSketch.CheckDone();

            return true;
        }


    }
}
