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
using Utilities;

namespace InkForm
{

    public enum PenAction { Draw, Select };
    
    public class InkPanel : Panel
    {
        #region Member Variables

        InkOverlay m_InkOverlay;

        System.Drawing.Graphics g;

        PenAction m_CurrentPenAction;

        //Clusterer.Clusterer m_Clusterer;

        InkTool m_InkTool;


        public enum DisplayOptions { None, Classifier, Clusters, TopComplete, Top };

        DisplayOptions m_DisplayOption;

        #endregion

        public InkPanel() : base()
        {
            // Set basic panel properties
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.White;
            this.BorderStyle = BorderStyle.Fixed3D;

            // Initialize Ink and Sketch
            this.InitPanel();
            this.m_InkTool = new InkTool(this);
            
        }

        public InkPanel(Sketch.Sketch sketch)
            : base()
        {

            // Set basic panel properties
            this.AutoScroll = true;
            this.BackColor = System.Drawing.Color.White;
            this.BorderStyle = BorderStyle.Fixed3D;

            // Initialize Ink and Sketch
            this.InitPanel();
            this.m_InkTool = new InkTool(this);
            
        }

        private void InitPanel()
        {
            this.MouseDoubleClick += new MouseEventHandler(InkPanel_MouseDoubleClick);
            g = this.CreateGraphics();

            m_InkOverlay = new InkOverlay();
            m_InkOverlay.AttachedControl = this;
            m_InkOverlay.Enabled = true;
            m_InkOverlay.DesiredPacketDescription = new Guid[3] { PacketProperty.X, PacketProperty.Y, PacketProperty.NormalPressure };

            m_InkOverlay.Stroke += new InkCollectorStrokeEventHandler(m_InkOverlay_Stroke);
            m_InkOverlay.StrokesDeleted += new InkOverlayStrokesDeletedEventHandler(m_InkOverlay_StrokesDeleted);
            m_InkOverlay.MouseDown += new InkCollectorMouseDownEventHandler(m_InkOverlay_MouseDown);
            m_InkOverlay.CursorInRange += new InkCollectorCursorInRangeEventHandler(m_InkOverlay_CursorInRange);

            m_DisplayOption = DisplayOptions.None;

        }

        void m_InkOverlay_CursorInRange(object sender, InkCollectorCursorInRangeEventArgs e)
        {
            if (m_CurrentPenAction == PenAction.Select)
            {
                m_InkOverlay.Cursor = System.Windows.Forms.Cursors.Arrow;
            }
            else if (m_CurrentPenAction == PenAction.Draw)
            {
                m_InkOverlay.Cursor = System.Windows.Forms.Cursors.Default;
            }
        }

        public void SetPenEnabled(bool value)
        {
            m_InkOverlay.Enabled = value;
        }

        void m_InkOverlay_MouseDown(object sender, CancelMouseEventArgs e)
        {
        }

        #region events

        //public event FeaturizationFinished FeaturizingDone;

        //protected virtual void OnFeaturizingCompleted(EventArgs e)
        //{
        //    if (FeaturizingDone != null)
        //        FeaturizingDone(this, e);
        //}

        void m_InkOverlay_StrokesDeleted(object sender, EventArgs e)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        void m_InkOverlay_Stroke(object sender, InkCollectorStrokeEventArgs e)
        {
            if (m_CurrentPenAction == PenAction.Draw)
            {
                try
                {
                    //Console.WriteLine("Stroke {0} finished at {1}", m_Clusterer.Classifier.StrokeCount + 1, ((ulong)DateTime.Now.ToFileTime() - 116444736000000000) / 10000);
                    //Console.WriteLine();
                    //Console.WriteLine("Stroke {4} finish @ {0}:{1}:{2}.{3}",
                        //DateTime.Now.Hour.ToString(), DateTime.Now.Minute.ToString(),
                        //DateTime.Now.Second.ToString(), DateTime.Now.Millisecond.ToString(),
                        //m_InkTool.Clusterer.Classifier.StrokeCount + 1);
                    //m_InkTool.Clusterer.AddStroke(e.Stroke);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("InkPanel Stroke: " + ex.Message);
                    //throw ex;
                }
            }
        }

        //void m_Clusterer_FeaturizingDone(object sender, EventArgs e)
        //{
        //    OnFeaturizingCompleted(e);
        //}

        //void m_Clusterer_StrokeClassificationDone(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        StrokeClassifierResult result = m_Clusterer.Classifier.Result;
        //        if (result == null)
        //            return;

        //        m_ClassifierColors = new Dictionary<Sketch.Substroke, System.Drawing.Color>();
        //        foreach (Stroke stroke in m_InkOverlay.Ink.Strokes)
        //        {
        //            Sketch.Substroke s = m_Clusterer.GetSubstroke(stroke);
        //            if (s != null)
        //            {
        //                string classification = result.GetClassification(s);
        //                if (classification != null)
        //                {
        //                    if (m_Domain.Class2Color.ContainsKey(classification))
        //                    {
        //                        if (!m_ClassifierColors.ContainsKey(s))
        //                            m_ClassifierColors.Add(s, m_Domain.Class2Color[classification]);
        //                        else
        //                            m_ClassifierColors[s] = m_Domain.Class2Color[classification];
        //                    }
        //                    else
        //                        Console.WriteLine("Domain does not have class: {0}", classification);
        //                }
        //            }
        //        }


        //        //Console.WriteLine("Completed Stroke Classification event at {0}", ((ulong)DateTime.Now.ToFileTime() - 116444736000000000) / 10000);
        //        this.RefreshPanel();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("InkPanel ClassificationDone: " + ex.Message);
        //        //throw ex;
        //    }
        //}

        //void m_Clusterer_FinalClustersDone(object sender, EventArgs e)
        //{
        //    m_TopMatchColors = m_Clusterer.TopMatchColors;
        //    m_TopCompleteMatchColors = m_Clusterer.TopCompleteMatchColors;
        //}

        //void m_Clusterer_InitialClustersDone(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        //Console.WriteLine("Initial Clusters Finished at {0}", ((ulong)DateTime.Now.ToFileTime() - 116444736000000000) / 10000);
        //        m_ClusterColors = m_Clusterer.ClusterColors;

        //        RefreshPanel();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine("InkPanel InitialClustersDone: " + ex.Message);
        //        //throw ex;
        //    }
        //}

        void InkPanel_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (m_CurrentPenAction == PenAction.Draw)
                return;

            System.Drawing.Point pt = e.Location;
            m_InkOverlay.Renderer.PixelToInkSpace(this.g, ref pt);
            /*
            List<Clusters.Cluster> entry = m_InkTool.Clusterer.GetSomeClusters(pt);
            if (entry.Count == 0)
                return;

            VisualizeSearch visualizeForm = new VisualizeSearch(entry);
            visualizeForm.Show();
            visualizeForm.moveChildren();
            */
        }

        #endregion

        #region Getters

        public PenAction CurrentPenAction
        {
            get { return m_CurrentPenAction; }
            set { m_CurrentPenAction = value; }
        }

        public InkTool InkTool
        {
            get { return m_InkTool; }
        }

        public InkOverlay InkOverlay
        {
            get { return m_InkOverlay; }
        }

        //public Clusterer.Clusterer Clusterer
        //{
        //    get { return m_Clusterer; }
        //}

        //public Domain Domain
        //{
        //    get { return m_Domain; }
        //    set { m_Domain = value; }
        //}

        #endregion

        #region Panel operations

        public void RefreshPanel()
        {
            if (m_DisplayOption == DisplayOptions.None)
            {
                foreach (Stroke stroke in m_InkOverlay.Ink.Strokes)
                    stroke.DrawingAttributes.Color = System.Drawing.Color.Black;

                this.Refresh();
            }
            else if (m_DisplayOption == DisplayOptions.Classifier)
            {
                /*foreach (Stroke stroke in m_InkOverlay.Ink.Strokes)
                {
                    Sketch.Substroke s = m_InkTool.Clusterer.GetSubstroke(stroke);
                    if (s != null && m_InkTool.ClassifierColors.ContainsKey(s))
                        stroke.DrawingAttributes.Color = m_InkTool.ClassifierColors[s];
                }*/

                this.Refresh();
            }
            else if (m_DisplayOption == DisplayOptions.Clusters)
            {
                /*foreach (Stroke stroke in m_InkOverlay.Ink.Strokes)
                {
                    Sketch.Substroke s = m_InkTool.Clusterer.GetSubstroke(stroke);
                    if (s != null && m_InkTool.ClusterColors.ContainsKey(s))
                    {
                        if (m_InkTool.Clusterer.Classifier.Result.GetClassification(s) != "Wire")
                            stroke.DrawingAttributes.Color = m_InkTool.ClusterColors[s];
                        else
                            stroke.DrawingAttributes.Color = System.Drawing.Color.Black;
                    }
                    else
                        stroke.DrawingAttributes.Color = System.Drawing.Color.Black;

                }*/

                this.Refresh();
            }
            else if (m_DisplayOption == DisplayOptions.TopComplete)
            {
                /*foreach (Stroke stroke in m_InkOverlay.Ink.Strokes)
                {
                    Sketch.Substroke s = m_InkTool.Clusterer.GetSubstroke(stroke);
                    if (s != null && m_InkTool.TopCompleteMatchColors.ContainsKey(s))
                        stroke.DrawingAttributes.Color = m_InkTool.TopCompleteMatchColors[s];
                    else
                        stroke.DrawingAttributes.Color = System.Drawing.Color.Black;
                }*/

                this.Refresh();
            }
            else if (m_DisplayOption == DisplayOptions.Top)
            {
                /*foreach (Stroke stroke in m_InkOverlay.Ink.Strokes)
                {
                    Sketch.Substroke s = m_InkTool.Clusterer.GetSubstroke(stroke);
                    if (s != null && m_InkTool.TopMatchColors.ContainsKey(s))
                        stroke.DrawingAttributes.Color = m_InkTool.TopMatchColors[s];
                    else
                        stroke.DrawingAttributes.Color = System.Drawing.Color.Black;
                }*/

                this.Refresh();
            }
        }

        public void ChangeDisplayOption(DisplayOptions option)
        {
            if (option == m_DisplayOption)
                return;

            m_DisplayOption = option;
            RefreshPanel();
        }

        #endregion

        //public void InitializeClusterer(Domain domain, ClustererSettings settings)
        //{
        //    m_Domain = domain;
        //    m_ClustererSettings = settings;
        //    m_Clusterer = new Clusterer.Clusterer(m_ClustererSettings);
        //    m_Clusterer.InitialClustersDone += new InitialClustersCompleted(m_Clusterer_InitialClustersDone);
        //    m_Clusterer.FinalClustersDone += new FinalClustersCompleted(m_Clusterer_FinalClustersDone);
        //    m_Clusterer.StrokeClassificationDone += new StrokeClassificationCompleted(m_Clusterer_StrokeClassificationDone);
        //    m_Clusterer.FeaturizingDone += new FeaturizationCompleted(m_Clusterer_FeaturizingDone);

        //    m_Clusterer.CurrentUser = "New";
        //    m_Clusterer.CurrentPlatform = "Tablet";
        //}

        public bool LoadXMLSketch(string filename, string settingsFilename, string baseDirectory)
        {
            Sketch.Sketch sketch = new ConverterXML.ReadXML(filename).Sketch;

            string userName = Path.GetFileNameWithoutExtension(filename).Substring(0, 2);
            m_InkTool.Clusterer.CurrentUser = new User(userName);
            if (filename.Contains("_T_"))
                m_InkTool.Clusterer.CurrentPlatform = PlatformUsed.TabletPC;
            else if (filename.Contains("_P_"))
                m_InkTool.Clusterer.CurrentPlatform = PlatformUsed.Wacom;

            Console.WriteLine("Loaded Sketch at {0}", ((ulong)DateTime.Now.ToFileTime() - 116444736000000000) / 10000);

            bool result = m_InkTool.ClassifySketch(sketch, settingsFilename, baseDirectory);

            if (result)
                this.Refresh();

            return result;
        }

        



        public Dictionary<string, string> ReadSettings(string settingsFilename)
        {
            Dictionary<string, string> entries = new Dictionary<string,string>();
            Dictionary<string, string> filenames = new Dictionary<string, string>();
            StreamReader reader = new StreamReader(settingsFilename);

            string line;

            while ((line = reader.ReadLine()) != null)
            {
                if (line != "" && line[0] != '%' && line.Contains("="))
                {
                    int index = line.IndexOf("=");
                    string key = line.Substring(0, index - 1);
                    string value = line.Substring(index + 2, line.Length - (index + 2));

                    if (!entries.ContainsKey(key))
                        entries.Add(key, value);
                }
            }

            reader.Close();

            if (entries.ContainsKey("Directory"))
            {
                string dir = Path.GetDirectoryName(settingsFilename);
                dir += "\\" + entries["Directory"];
                entries.Remove("Directory");

                foreach (KeyValuePair<string, string> pair in entries)
                {
                    if (!pair.Key.Contains("Directory"))
                        filenames.Add(pair.Key, dir + pair.Value);
                    else
                        filenames.Add(pair.Key, "\\" + pair.Value);
                }
            }
            else
                MessageBox.Show("No Directory specified! (Key = Directory)");

            return filenames;
        }

    }
}
