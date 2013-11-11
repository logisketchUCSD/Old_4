using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Microsoft.Ink;
using Utilities;
using Utilities.Neuro;
using Clusterer;
using StrokeClassifier;
using StrokeGrouper;
using ImageRecognizerWrapper;
using Featurefy;

namespace InkForm
{
    public partial class Sketcher : Form
    {
        #region Member Variables

        private InkPanel m_InkPanel;

        private string m_SettingsFilename;

        private string m_BaseDirectory;

        #endregion

        public Sketcher()
        {
            InitializeComponent();

            m_InkPanel = new InkPanel();
            m_InkPanel.Dock = DockStyle.Fill;
            tabSketch.Controls.Add(m_InkPanel);

            InitializeWithSettings();
            SetCurrentPenAction();
        }

        

        private void InitializeWithSettings()
        {
            try
            {
                string dir = Directory.GetCurrentDirectory();
                dir = dir.Substring(0, dir.IndexOf("\\Code\\") + 6);
                dir += "Recognition\\InkForm\\InkForm";
                //DirectoryInfo info = Directory.GetParent(dir);
                //info = Directory.GetParent(info.FullName);
                //I don't like absolute paths but when this is used in CircuitSimulatorUI the currentDirectory changes and its a different
                //relative path
                //dir = "C:\\Documents and Settings\\Guest\\My Documents\\Sketch\\Code\\Recognition\\InkForm\\InkForm";//info.FullName;
                m_BaseDirectory = dir;
                m_SettingsFilename = dir + "\\" + "settings.txt";
                
                Dictionary<string, string> filenames = m_InkPanel.ReadSettings(m_SettingsFilename);


                string filename;
                Stream stream = File.Open(m_SettingsFilename, FileMode.Open);
                stream.Close();
                BinaryFormatter bformatter = new BinaryFormatter();
                Domain domain;
                NeuralNetwork strokeNetwork, gatesNetwork, labelsNetwork, wiresNetwork;
                ImageRecognizerWrapper.ImageDefinitions imageDefinitions;


                bool found = filenames.TryGetValue("Domain", out filename);
                if (found)
                {
                    stream = File.Open(filename, FileMode.Open);
                    domain = (Domain)bformatter.Deserialize(stream);
                    stream.Close();
                }
                else
                {
                    MessageBox.Show("No default domain specified! (Key = Domain)");
                    domain = new Domain();
                }

                found = filenames.TryGetValue("StrokeClassificationNetwork", out filename);
                if (found)
                    strokeNetwork = NeuralNetwork.LoadNetworkFromFile(filename);
                else
                {
                    MessageBox.Show("No default stroke classification network specified! (Key = StrokeClassificationNetwork)");
                    strokeNetwork = new NeuralNetwork(new NeuralNetworkInfo());
                }

                found = filenames.TryGetValue("GrouperNetworkGates", out filename);
                if (found)
                    gatesNetwork = NeuralNetwork.LoadNetworkFromFile(filename);
                else
                {
                    MessageBox.Show("No default gates grouping network specified! (Key = GrouperNetworkGates)");
                    gatesNetwork = new NeuralNetwork(new NeuralNetworkInfo());
                }

                found = filenames.TryGetValue("GrouperNetworkLabels", out filename);
                if (found)
                    labelsNetwork = NeuralNetwork.LoadNetworkFromFile(filename);
                else
                {
                    MessageBox.Show("No default labels grouping network specified! (Key = GrouperNetworkLabels)");
                    labelsNetwork = new NeuralNetwork(new NeuralNetworkInfo());
                }

                found = filenames.TryGetValue("GrouperNetworkWires", out filename);
                if (found)
                    wiresNetwork = NeuralNetwork.LoadNetworkFromFile(filename);
                else
                {
                    MessageBox.Show("No default wires grouping network specified! (Key = GrouperNetworkWires)");
                    wiresNetwork = new NeuralNetwork(new NeuralNetworkInfo());
                }

                Dictionary<string, NeuralNetwork> grouperNetworks = new Dictionary<string, NeuralNetwork>();
                grouperNetworks.Add("Gate", gatesNetwork);
                grouperNetworks.Add("Label", labelsNetwork);
                grouperNetworks.Add("Wire", wiresNetwork);

                Dictionary<string, double[]> avgsAndStdDevs;
                Dictionary<string, bool> classifierFeatures, grouperFeatures;

                found = filenames.TryGetValue("FeatureAvgsAndStdDevs", out filename);
                if (found)
                    avgsAndStdDevs = General.GetAvgsForSoftMax(filename);
                else
                {
                    MessageBox.Show("No file containing Avgs and Std Devs specified! (Key = FeatureAvgsAndStdDevs)");
                    avgsAndStdDevs = new Dictionary<string, double[]>();
                }

                found = filenames.TryGetValue("FeaturesSingle", out filename);
                if (found)
                    classifierFeatures = StrokeClassifierSettings.ReadFeatureListFile(filename);
                else
                {
                    MessageBox.Show("No file containing List of Single Stroke Features to Use specified! (Key = FeaturesSingle)");
                    classifierFeatures = new Dictionary<string, bool>();
                }

                found = filenames.TryGetValue("FeaturesGroup", out filename);
                if (found)
                    grouperFeatures = StrokeClassifierSettings.ReadFeatureListFile(filename);
                else
                {
                    MessageBox.Show("No file containing List of Grouper Features to Use specified! (Key = FeaturesGroup)");
                    grouperFeatures = new Dictionary<string, bool>();
                }

                found = filenames.TryGetValue("ImageDefinitions", out filename);
                if (found)
                    imageDefinitions = ImageRecognizerWrapper.ImageDefinitions.LoadDefinitionsFromFile(filename);
                else
                {
                    MessageBox.Show("No file containing Image Definitions to Use specified! (Key = ImageDefinitions)");
                    imageDefinitions = new ImageRecognizerWrapper.ImageDefinitions();
                }

                StrokeClassifierSettings classifierSettings = new StrokeClassifierSettings(domain, strokeNetwork, classifierFeatures);
                StrokeGrouperSettings grouperSettings = new StrokeGrouperSettings(domain, grouperFeatures, grouperNetworks);
                ImageRecognizerSettings imageSettings = new ImageRecognizerSettings("All", PlatformUsed.TabletPC, domain, imageDefinitions);

                ClustererSettings clustererSettings = new ClustererSettings(classifierSettings, grouperSettings, imageSettings);
                clustererSettings.AvgsAndStdDevs = avgsAndStdDevs;

                m_InkPanel.InkTool.InitializeClusterer(domain, clustererSettings);
                m_InkPanel.InkTool.FeaturizingDone += new FeaturizationFinished(m_InkPanel_FeaturizingDone);
            }
            catch (Exception e)
            {
                Console.WriteLine("Sketcher InitializeSettings: " + e.Message);
                //throw e;
            }
        }

        void m_InkPanel_FeaturizingDone(object sender, EventArgs e)
        {

        }

        #region Button/Menu Clicks, Check Changes

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void openSketchToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openDlg = new OpenFileDialog();
            openDlg.Title = "Select Sketch to open";
            openDlg.Filter = "XML Sketches (*.xml)|*.xml";

            if (openDlg.ShowDialog() == DialogResult.OK)
            {
                bool success = m_InkPanel.LoadXMLSketch(openDlg.FileName, m_SettingsFilename, m_BaseDirectory);

                if (success)
                {
                    string sketchName = Path.GetFileNameWithoutExtension(openDlg.FileName);
                    this.Text = "Sketcher - " + sketchName;
                }
            }
        }

        private void saveStrokeClassifierToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //m_InkPanel.SaveStrokeClassifier();
        }

        private void openStrokeClassifierToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //m_InkPanel.OpenStrokeClassifier();
        }

        private void DisplayNoneRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            m_InkPanel.ChangeDisplayOption(InkPanel.DisplayOptions.None);
        }

        private void DisplayClassificationRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            m_InkPanel.ChangeDisplayOption(InkPanel.DisplayOptions.Classifier);
        }

        private void DisplayGroupingRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            m_InkPanel.ChangeDisplayOption(InkPanel.DisplayOptions.Clusters);
        }

        private void radioButtonTopComplete_CheckedChanged(object sender, EventArgs e)
        {
            m_InkPanel.ChangeDisplayOption(InkPanel.DisplayOptions.TopComplete);
        }

        private void radioButtonTop_CheckedChanged(object sender, EventArgs e)
        {
            m_InkPanel.ChangeDisplayOption(InkPanel.DisplayOptions.Top);
        }

        #endregion

        #region Features for Single

        private void rawValuesSingleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool successful;
            List<string> filenames = General.SelectOpenMultipleSketches(out successful);
            if (!successful)
                return;

            bool saveSuccessful;
            string saveFilename = General.SelectSaveFile(out saveSuccessful);
            if (!saveSuccessful)
                return;

            StreamWriter writer = new StreamWriter(saveFilename);
            List<string> features = new List<string>();
            foreach (KeyValuePair<string, bool> pair in m_InkPanel.InkTool.Clusterer.FeatureSketch.FeatureListSingle)
                if (pair.Value)
                    features.Add(pair.Key);

            foreach (string featureName in features)
                writer.Write(featureName + ",");
            
            // Write classes?

            writer.WriteLine();

            foreach (string name in filenames)
                writeFeaturesSingle(name, writer, features);

            writer.Close();
        }

        private void writeFeaturesSingle(string sketchFile, StreamWriter writer, List<string> features)
        {
            m_InkPanel.LoadXMLSketch(sketchFile, m_SettingsFilename, m_BaseDirectory);


            Dictionary<string, bool> featuresToUse = m_InkPanel.InkTool.Clusterer.Classifier.FeatureSketch.FeatureListSingle;
            Sketch.Sketch sketch = m_InkPanel.InkTool.Clusterer.Classifier.FeatureSketch.Sketch;

            Dictionary<Sketch.Substroke, double[]> stroke2values;
            double[][] normalizedValues = m_InkPanel.InkTool.Clusterer.Classifier.FeatureSketch.GetValuesSingle(
                out stroke2values, 
                ValuePreparationStage.Normalized);

            foreach (double[] stroke in normalizedValues)
            {
                foreach (double value in stroke)
                    writer.Write("{0} ", value);

                writer.WriteLine();
            }

        }


        private void writeSoftMaxFeaturesSingle(string sketchFile, StreamWriter writer, List<string> features, Dictionary<string, double[]> SoftmaxNormalizers)
        {
            m_InkPanel.LoadXMLSketch(sketchFile, m_SettingsFilename, m_BaseDirectory);

            Dictionary<string, bool> featuresToUse = m_InkPanel.InkTool.Clusterer.Classifier.FeatureSketch.FeatureListSingle;
            Sketch.Sketch sketch = m_InkPanel.InkTool.Clusterer.Classifier.FeatureSketch.Sketch;

            foreach (Sketch.Substroke stroke in sketch.Substrokes)
            {
                foreach (string feature in features)
                    if (featuresToUse.ContainsKey(feature) && featuresToUse[feature])
                        writeSoftMaxFeatureSingle(writer, feature, stroke, SoftmaxNormalizers);

                if (General.IsGate(stroke) || General.IsOther(stroke))
                    writer.WriteLine("1 0 0");
                else if (General.IsConnector(stroke))
                    writer.WriteLine("0 1 0");
                else if (General.IsLabel(stroke))
                    writer.WriteLine("0 0 1");
                else
                    writer.WriteLine("0 0 0");
            }
        }

        private void writeSoftMaxFeatureSingle(StreamWriter writer, string featureName, Sketch.Substroke stroke, Dictionary<string, double[]> SoftmaxNormalizers)
        {
            FeatureStroke featureStroke = m_InkPanel.InkTool.Clusterer.Classifier.FeatureSketch.StrokeFeatures[stroke];

            double value = Featurefy.Compute.GetSoftmaxNormalizedValue(featureStroke, featureName, SoftmaxNormalizers);

            writer.Write(value.ToString() + " ");
        }


        private void usingSoftmaxSingleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool successful;
            List<string> filenames = General.SelectOpenMultipleSketches(out successful);
            if (!successful)
                return;

            bool saveSuccessful;
            string saveDirectory = General.SelectDirectorySaveFeatureFile(out saveSuccessful);
            if (!saveSuccessful)
                return;

            Dictionary<string, double[]> avgs = m_InkPanel.InkTool.Clusterer.Settings.AvgsAndStdDevs;
            if (avgs == null)
                return;

            
            List<string> features = new List<string>();
            foreach (KeyValuePair<string, bool> pair in m_InkPanel.InkTool.Clusterer.FeatureSketch.FeatureListSingle)
                if (pair.Value)
                    features.Add(pair.Key);

            foreach (string name in filenames)
            {
                string nameShort = Path.GetFileNameWithoutExtension(name);
                string saveFilename = saveDirectory + "\\features" + nameShort.Substring(0, 2) + ".txt";
                bool exists = File.Exists(saveFilename);
                StreamWriter writer = new StreamWriter(saveFilename, true);
                if (!exists)
                {
                    writer.WriteLine("{0}", features.Count);
                    writer.WriteLine("{0}", m_InkPanel.InkTool.Clusterer.Classifier.Settings.Domain.Classes.Count);
                }
                writeSoftMaxFeaturesSingle(name, writer, features, avgs);
                writer.Close();
            }


        }

        private void normalizedInARFFFormatToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool successful;
            List<string> filenames = General.SelectOpenMultipleSketches(out successful);
            if (!successful)
                return;

            bool saveSuccessful;
            string saveFile = General.SelectSaveFile(out saveSuccessful, 
                "File to save all features to", 
                "Weka file format (*.arff)|*.arff");
            if (!saveSuccessful)
                return;

            List<string> features = new List<string>();
            foreach (KeyValuePair<string, bool> pair in m_InkPanel.InkTool.Clusterer.FeatureSketch.FeatureListSingle)
                if (pair.Value)
                    features.Add(pair.Key);

            StreamWriter writer = new StreamWriter(saveFile, false);

            writer.WriteLine("% Digital Circuits");
            writer.WriteLine("% Stroke Classification Features");
            writer.WriteLine("% {0}", DateTime.Now.ToString());
            writer.WriteLine();

            writer.WriteLine("@RELATION digital_circuits");
            writer.WriteLine();

            foreach (string feature in features)
                writer.WriteLine("@ATTRIBUTE '{0}' NUMERIC", feature);

            writer.WriteLine("@ATTRIBUTE class {Gate,Label,Wire}");
            writer.WriteLine();

            writer.WriteLine("@DATA");

            foreach (string name in filenames)
            {
                string nameShort = Path.GetFileNameWithoutExtension(name);
                writeARFFfileSingle(name, writer, features);
            }

            writer.Close();
        }

        private void writeARFFfileSingle(string sketchFile, StreamWriter writer, List<string> features)
        {
            m_InkPanel.LoadXMLSketch(sketchFile, m_SettingsFilename, m_BaseDirectory);

            Dictionary<string, bool> featuresToUse = m_InkPanel.InkTool.Clusterer.Classifier.FeatureSketch.FeatureListSingle;
            Sketch.Sketch sketch = m_InkPanel.InkTool.Clusterer.Classifier.FeatureSketch.Sketch;

            foreach (Sketch.Substroke stroke in sketch.Substrokes)
            {
                foreach (string feature in features)
                    if (featuresToUse.ContainsKey(feature) && featuresToUse[feature])
                        writeARFFfileSingle(writer, feature, stroke);

                if (General.IsGate(stroke) || General.IsOther(stroke))
                    writer.WriteLine("Gate");
                else if (General.IsConnector(stroke))
                    writer.WriteLine("Wire");
                else if (General.IsLabel(stroke))
                    writer.WriteLine("Label");
                else
                    writer.WriteLine("?");
            }
        }

        private void writeARFFfileSingle(StreamWriter writer, string featureName, Sketch.Substroke stroke)
        {
            FeatureStroke featureStroke = m_InkPanel.InkTool.Clusterer.Classifier.FeatureSketch.StrokeFeatures[stroke];
            double value = double.NaN;

            try
            {
                value = featureStroke.Features[featureName].NormalizedValue;
            }
            catch
            {
            }
            //double value = Compute.GetSoftmaxNormalizedValue(featureStroke, featureName, SoftmaxNormalizers);
            if (value != double.NaN)
                writer.Write(value.ToString() + ",");
            else
                writer.Write("?,");
        }

        #endregion

        #region Features for Pairwise

        private void rawValuesPairToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool successful;
            List<string> filenames = General.SelectOpenMultipleSketches(out successful);
            if (!successful)
                return;

            bool saveSuccessful;
            string saveFilename = General.SelectSaveFile(out saveSuccessful);
            if (!saveSuccessful)
                return;
            
            List<string> features = new List<string>();
            foreach (KeyValuePair<string, bool> pair in m_InkPanel.InkTool.Clusterer.Classifier.FeatureSketch.FeatureListPair)
                if (pair.Value)
                    features.Add(pair.Key);

            Dictionary<string, StreamWriter> writersPerClass = new Dictionary<string, StreamWriter>();
            foreach (string name in m_InkPanel.InkTool.Clusterer.Classifier.Settings.Domain.Classes)
            {
                string filenameShort = Path.GetFileNameWithoutExtension(saveFilename);
                string modified = filenameShort + "." + name;
                string fName = saveFilename.Replace(filenameShort, modified);
                StreamWriter writer = new StreamWriter(fName);

                foreach (string featureName in features)
                    writer.Write(featureName + ",");

                writer.WriteLine();

                writersPerClass.Add(name, writer);
            }

            foreach (string name in filenames)
                writeFeaturesPair(name, writersPerClass, features);

            foreach (KeyValuePair<string, StreamWriter> pair in writersPerClass)
                pair.Value.Close();
        }

        private void writeFeaturesPair(string sketchFile, Dictionary<string, StreamWriter> writersPerClass, List<string> features)
        {
            m_InkPanel.LoadXMLSketch(sketchFile, m_SettingsFilename, m_BaseDirectory);

            Dictionary<string, bool> featuresToUse = m_InkPanel.InkTool.Clusterer.Classifier.FeatureSketch.FeatureListPair;
            Dictionary<Sketch.Substroke, string> classifications = new Dictionary<Sketch.Substroke, string>();
            Domain dom = m_InkPanel.InkTool.Clusterer.Classifier.Settings.Domain;
            Dictionary<string, string> shape2Class = dom.Shape2Class;

            foreach (Sketch.Substroke substroke in m_InkPanel.InkTool.Clusterer.FeatureSketch.Sketch.Substrokes)
            {
                string className;
                bool success = shape2Class.TryGetValue(substroke.FirstLabel, out className);
                if (success)
                    classifications.Add(substroke, className);
            }

            Dictionary<string, Dictionary<FeatureStrokePair, double[]>> pair2values;
            Dictionary<string, double[][]> valuesPerClass = m_InkPanel.InkTool.Clusterer.FeatureSketch.GetValuesPairwise(out pair2values, classifications, ValuePreparationStage.Normalized);

            foreach (KeyValuePair<string, double[][]> pair in valuesPerClass)
            {
                StreamWriter writer;
                bool success = writersPerClass.TryGetValue(pair.Key, out writer);
                if (success)
                {
                    foreach (double[] strokePair in pair.Value)
                    {
                        foreach (double value in strokePair)
                            writer.Write("{0} ", value);

                        writer.WriteLine();
                    }
                }
            }
        }

        private void usingSoftmaxPairToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool successful;
            List<string> filenames = General.SelectOpenMultipleSketches(out successful);
            if (!successful)
                return;

            bool saveSuccessful;
            string saveDirectory = General.SelectDirectorySaveFeatureFile(out saveSuccessful);
            if (!saveSuccessful)
                return;

            Dictionary<string, double[]> avgs = m_InkPanel.InkTool.Clusterer.Settings.AvgsAndStdDevs;
            if (avgs == null)
                return;


            List<string> features = new List<string>();
            foreach (KeyValuePair<string, bool> pair in m_InkPanel.InkTool.Clusterer.FeatureSketch.FeatureListPair)
                if (pair.Value)
                    features.Add(pair.Key);

            foreach (string name in filenames)
                writeSoftMaxFeaturesPair(name, features, avgs, saveDirectory);
        }

        private void writeSoftMaxFeaturesPair(string sketchFile, List<string> features, Dictionary<string, double[]> avgs, string saveDirectory)
        {
            m_InkPanel.LoadXMLSketch(sketchFile, m_SettingsFilename, m_BaseDirectory);

            Dictionary<string, StreamWriter> writersPerClass = new Dictionary<string, StreamWriter>();

            string nameShort = Path.GetFileNameWithoutExtension(sketchFile);
            string saveFilename = saveDirectory + "\\features" + nameShort.Substring(0, 2);
            string[] extensions = new string[3] { "Gate", "Wire", "Label" };
            for (int i = 0; i < extensions.Length; i++)
            {
                string fName = saveFilename + "." + extensions[i] + ".txt";
                bool exists = File.Exists(fName);
                StreamWriter writer = new StreamWriter(fName, true);
                if (!exists)
                {
                    writer.WriteLine("{0}", features.Count);
                    writer.WriteLine("1");
                }
                writersPerClass.Add(extensions[i], writer);
            }

            Dictionary<string, bool> featuresToUse = m_InkPanel.InkTool.Clusterer.Classifier.FeatureSketch.FeatureListPair;
            Dictionary<Sketch.Substroke, string> classifications = new Dictionary<Sketch.Substroke, string>();
            Domain dom = m_InkPanel.InkTool.Clusterer.Classifier.Settings.Domain;
            Dictionary<string, string> shape2Class = dom.Shape2Class;

            foreach (Sketch.Substroke substroke in m_InkPanel.InkTool.Clusterer.FeatureSketch.Sketch.Substrokes)
            {
                string className;
                bool success = shape2Class.TryGetValue(substroke.FirstLabel, out className);
                if (success)
                    classifications.Add(substroke, className);
            }

            Dictionary<string, Dictionary<FeatureStrokePair, double[]>> pair2valuesPerClass;
            Dictionary<string, double[][]> valuesPerClass = m_InkPanel.InkTool.Clusterer.FeatureSketch.GetValuesPairwise(out pair2valuesPerClass, classifications, ValuePreparationStage.Normalized);

            foreach (KeyValuePair<string, Dictionary<FeatureStrokePair, double[]>> pair2values in pair2valuesPerClass)
            {
                string className = pair2values.Key;
                StreamWriter writer;
                bool success = writersPerClass.TryGetValue(className, out writer);
                if (success)
                {
                    foreach (KeyValuePair<FeatureStrokePair, double[]> current in pair2values.Value)
                    {
                        FeatureStrokePair strokePair = current.Key;
                        double[] values = current.Value;
                        int n = 0;
                        foreach (double value in values)
                        {
                            string key = className + "_" + features[n];
                            n++;
                            double[] normalizers = avgs[key];
                            double mean = normalizers[0];
                            double stdDev = normalizers[1];
                            double softmaxValue = Featurefy.Compute.GetSoftmaxNormalizedValue(value, mean, stdDev);
                            writer.Write("{0} ", softmaxValue);
                        }

                        // Write class
                        if (strokePair.StrokeA.ParentShapes.Count > 0 && strokePair.StrokeB.ParentShapes.Count > 0)
                        {
                            if (strokePair.StrokeA.ParentShapes[0] == strokePair.StrokeB.ParentShapes[0])
                                writer.WriteLine("1");
                            else
                                writer.WriteLine("0");
                        }
                        else
                            writer.WriteLine();
                    }
                }

                writer.Close();
            }
        }

        private void writeSoftMaxFeaturePair(StreamWriter writer, string featureName, Sketch.Substroke stroke, Dictionary<string, double[]> SoftmaxNormalizers)
        {
            FeatureStroke featureStroke = m_InkPanel.InkTool.Clusterer.Classifier.FeatureSketch.StrokeFeatures[stroke];

            double value = Featurefy.Compute.GetSoftmaxNormalizedValue(featureStroke, featureName, SoftmaxNormalizers);

            writer.Write(value.ToString() + " ");
        }

        #endregion

        private void selectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            drawToolStripMenuItem.Checked = false;
            selectToolStripMenuItem.Checked = true;
            SetCurrentPenAction();
        }

        private void drawToolStripMenuItem_Click(object sender, EventArgs e)
        {
            selectToolStripMenuItem.Checked = false;
            drawToolStripMenuItem.Checked = true;
            SetCurrentPenAction();
        }

        private void SetCurrentPenAction()
        {
            if (drawToolStripMenuItem.Checked)
            {
                m_InkPanel.CurrentPenAction = PenAction.Draw;
                m_InkPanel.SetPenEnabled(true);
            }
            else
            {
                m_InkPanel.CurrentPenAction = PenAction.Select;
                m_InkPanel.SetPenEnabled(false);
            }
        }

        public InkPanel InkPanel
        {
            get { return this.m_InkPanel; }
        }

        public string BaseDirectory
        {
            get { return m_BaseDirectory; }
        }

        public string SettingsFilename
        {
            get { return m_SettingsFilename; }
        }

    }
}