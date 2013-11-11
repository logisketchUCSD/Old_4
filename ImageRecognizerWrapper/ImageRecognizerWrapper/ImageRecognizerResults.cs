using System;
using System.Collections.Generic;
using System.Text;
using ImageRecognizer;
using Clusters;

namespace ImageRecognizerWrapper
{
    public class ImageRecognizerResults
    {
        List<Cluster> m_ScoredClusters;

        public ImageRecognizerResults()
        {
            m_ScoredClusters = new List<Cluster>();
        }

        /// <summary>
        ///  Add a newly scored cluster to the list of clusters
        /// </summary>
        /// <param name="newScored"></param>
        public void Add(Cluster newScored)
        {
            m_ScoredClusters.Add(newScored);
        }

        /// <summary>
        /// Reset the list of scored clusters
        /// </summary>
        public void Reset()
        {
            m_ScoredClusters.Clear();
        }

        /// <summary>
        /// Getter and Setter for the list of scoredclusters
        /// </summary>
        public List<Cluster> ScoredClusters
        {
            get { return m_ScoredClusters; }
            set { m_ScoredClusters = value; }
        }

    }
}
