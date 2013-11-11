using System;
using System.Collections.Generic;
using System.Text;
using ImageRecognizer;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using Utilities;

namespace ImageRecognizerWrapper
{
    [Serializable]
    public class ImageRecognizerSettings
    {
        #region Member Variables

        /// <summary>
        /// User name or number as string
        /// </summary>
        string m_UserName;

        /// <summary>
        /// Platform used for drawing - either "Tablet" or "Wacom"
        /// </summary>
        PlatformUsed m_Platform;

        /// <summary>
        /// Domain in which the symbols are found
        /// </summary>
        Domain m_Domain;

        ImageDefinitions m_ImageDefinitions;

        #endregion

        public ImageRecognizerSettings()
        {
            m_UserName = "New";
            m_Platform = PlatformUsed.TabletPC;
            m_Domain = new Domain();
            m_ImageDefinitions = new ImageDefinitions();
        }

        public ImageRecognizerSettings(string User, PlatformUsed platformUsed, Domain domain, ImageDefinitions definitions)
        {
            m_UserName = User;
            m_Platform = platformUsed;
            m_Domain = domain;
            m_ImageDefinitions = definitions;
        }

        public static ImageRecognizerSettings MakeImageRecognizerSettings(Dictionary<string, string> filenames)
        {
            string filename;
            Domain domain;
            Stream stream;
            BinaryFormatter bformatter = new BinaryFormatter();
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

            found = filenames.TryGetValue("ImageDefinitions", out filename);
            if (found)
                imageDefinitions = ImageRecognizerWrapper.ImageDefinitions.LoadDefinitionsFromFile(filename);
            else
            {
                MessageBox.Show("No file containing Image Definitions to Use specified! (Key = ImageDefinitions)");
                imageDefinitions = new ImageRecognizerWrapper.ImageDefinitions();
            }

            return new ImageRecognizerSettings("All", PlatformUsed.TabletPC, domain, imageDefinitions);
        }

        public string UserName
        {
            get { return m_UserName; }
            set { m_UserName = value; }
        }

        public PlatformUsed Platform
        {
            get { return m_Platform; }
            set { m_Platform = value; }
        }

        public Domain Domain
        {
            get { return m_Domain; }
            set { m_Domain = value; }
        }

        public ImageDefinitions ImageDefinitions
        {
            get { return m_ImageDefinitions; }
            set { m_ImageDefinitions = value; }
        }
    }
}
