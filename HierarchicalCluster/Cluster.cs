/*
 * File: Cluster.cs
 *
 * Author: Eric Peterson
 * Eric.J.Peterson@gmail.com
 * University of California, Riverside
 * Smart Tools Lab 2009
 * 
 * Use at your own risk.  This code is not maintained and not guaranteed to work.
 * We take no responsibility for any harm this code may cause.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using RecognitionTemplates;

namespace HierarchicalCluster
{
    /// <summary>
    /// Type of linking to use for tree building
    /// </summary>
    public enum ClusterLinking { Single, Complete, Average };

    /// <summary>
    /// This Cluster object represents an agglomerative hierarchical cluster.
    /// </summary>
    [DebuggerDisplay("{m_Center.Name}: {m_Nodes.Count} Nodes, r = {m_Radius.ToString(\"#0.000\"), Depth = {DepthOfCluster}")]
    [Serializable]
    public class Cluster
    {
        #region INTERNALS

        /// <summary>
        /// The most representative RecognitionTemplate in this Cluster.
        /// </summary>
        private RecognitionTemplate m_Center;

        /// <summary>
        /// Distance between the center and the template farthest from the center.
        /// </summary>
        private double m_Radius;

        /// <summary>
        /// All of the templates in this cluster
        /// </summary>
        private List<RecognitionTemplate> m_Nodes;

        /// <summary>
        /// Parent of this node. If this cluster is the root then this value will be null.
        /// </summary>
        private Cluster m_Parent;

        /// <summary>
        /// List of the IMMEDIATE children for this cluster. If this cluster is a leaf node 
        /// then this value will be null.
        /// </summary>
        private List<Cluster> m_Children;

        private const ClusterLinking m_LinkingMethod = ClusterLinking.Complete;

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Constructor for creating leaf node clusters
        /// </summary>
        /// <param name="template">RecognitionTemplate to be the center of this leaf node</param>
        public Cluster(RecognitionTemplate template)
        {
            m_Nodes = new List<RecognitionTemplate>();
            m_Nodes.Add(template);
            m_Center = FindClusterCenter(m_Nodes, out m_Radius);
        }

        /// <summary>
        /// Constructor to use when merging other clusters
        /// </summary>
        /// <param name="templates">all nodes to be in this cluster</param>
        /// <param name="children">List of the immediate children for this cluster</param>
        /// <param name="indices">Lookup table for each node to access the correct distance</param>
        /// <param name="distances">Matrix of all distances between the nodes</param>
        public Cluster(List<RecognitionTemplate> templates, List<Cluster> children, Dictionary<RecognitionTemplate, int> indices, double[,] distances)
        {
            m_Nodes = templates;
            m_Center = FindClusterCenter(m_Nodes, indices, distances, out m_Radius);

            m_Children = children;
            foreach (Cluster child in children)
                child.m_Parent = this;
        }

        #endregion

        #region RECOGNITION

        /// <summary>
        /// Attempts to recognize the unknown RecognitionTemplate using
        /// the templates in this cluster. Returns the score to the best 
        /// matching template. If the distance from the cluster center is 
        /// greater than the cluster's radius times a constant parameter, 
        /// -1.0 is returned indicating that no suitable match was found 
        /// in this cluster.
        /// </summary>
        /// <param name="unknown">Symbol to recognize</param>
        /// <returns>Score with best matching template</returns>
        public double Recognize(RecognitionTemplate unknown)
        {
            return Recognize(unknown, -1.0);
        }

        public Dictionary<double, RecognitionTemplate> RecognizeNBest(RecognitionTemplate unknown, int n)
        {
            SortedList<double, RecognitionTemplate> best = new SortedList<double, RecognitionTemplate>(n);
            best = RecognizeNBest(unknown, best, n);

            n = Math.Min(n, best.Count);
            Dictionary<double, RecognitionTemplate> sorted = new Dictionary<double, RecognitionTemplate>(n);
            List<double> scores = new List<double>(best.Keys);
            scores.Reverse();

            for (int i = 0; i < n; i++)
                sorted.Add(scores[i], best[scores[i]]);

            return sorted;
        }

        private SortedList<double, RecognitionTemplate> RecognizeNBest(RecognitionTemplate unknown, SortedList<double, RecognitionTemplate> best, int n)
        {
            //double bestScore = 0.0;
            //List<double> bestN = new List<double>(best.Keys);
            //for (int i = 0; i < n && i < bestN.Count; i++)
                //bestScore = bestN[i];

            //if (bestScore > ClusteringParameters.SCORE_THRESHOLD)
                //return best;

            if (DepthOfCluster > ClusteringParameters.MAX_DEPTH)
                return best;

            if (!best.ContainsValue(m_Center))
            {
                double score = m_Center.Recognize(unknown);
                double distance = 1.0 - score;

                if (IsLeaf && score > 0.0)
                {
                    while (best.ContainsKey(score))
                        score += 0.000000001;

                    best.Add(score, m_Center);
                    return best;
                }

                if (distance > m_Radius * ClusteringParameters.RADIUS_RATIO)
                {
                    return best;
                }
                
                while (best.ContainsKey(score))
                    score += 0.000000001;

                best.Add(score, m_Center);
                
            }

            if (m_Children != null)
                foreach (Cluster child in m_Children)
                    best = child.RecognizeNBest(unknown, best, n);

            return best;
        }

        /// <summary>
        /// Internal recognition function, recursive.
        /// Checks to make sure the maximum depth has not been exceeded.
        /// Checks whether the best score is higher than the SCORE_THRESHOLD
        /// constant parameter, if it is, the best score is returned.
        /// Checks the unknown against the cluster's children.
        /// </summary>
        /// <param name="unknown">Unknown symbol to recognize</param>
        /// <param name="depth">Current depth in the search</param>
        /// <param name="bestScore">Current best score found</param>
        /// <returns>Best score for this cluster and its children</returns>
        private double Recognize(RecognitionTemplate unknown, double bestScore)
        {
            if (bestScore > ClusteringParameters.SCORE_THRESHOLD)
                return bestScore;

            if (DepthOfCluster > ClusteringParameters.MAX_DEPTH)
                return bestScore;

            double score = m_Center.Recognize(unknown);
            double distance = 1.0 - score;

            if (distance > m_Radius * ClusteringParameters.RADIUS_RATIO)
            {
                if (DepthOfCluster == 0)
                    return bestScore;
                else if (score > bestScore)
                {
                    //PrintToConsole(score);
                    return score;
                }
                else
                    return bestScore;
            }
            else
                bestScore = Math.Max(bestScore, score);

            foreach (Cluster child in m_Children)
            {
                double childScore = child.Recognize(unknown, bestScore);
                if (childScore > bestScore)
                {
                    bestScore = childScore;
                    //PrintToConsole(bestScore);
                }
            }

            return bestScore;
        }

        public static string RecognizeBestFirst(List<Cluster> allClusters, RecognitionTemplate unknown, out double bestScore)
        {
            string bestName = "None";
            RecognitionTemplate bestTemplate = null;
            bestScore = -1.0;
            
            Dictionary<Cluster, double> scores = new Dictionary<Cluster, double>();
            Dictionary<Cluster, double> predictions = new Dictionary<Cluster, double>();

            
            foreach (Cluster c in allClusters)
            {
                double score = c.Center.Recognize(unknown);
                double distance = 1.0 - score;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestName = c.Center.Name;
                    bestTemplate = c.Center;
                }

                scores.Add(c, score);
                if (distance > c.Radius * ClusteringParameters.RADIUS_RATIO)
                    continue;

                if (c.Children != null && c.DepthOfCluster < ClusteringParameters.MAX_DEPTH - 1)
                {
                    predictions.Add(c.Children[0], score);
                    predictions.Add(c.Children[1], score);
                }
            }

            string incorrect = "";
            if (bestScore > ClusteringParameters.SCORE_THRESHOLD)
            {
                /*if (unknown.Name != bestName)
                {
                    double s = 0.0;
                    foreach (KeyValuePair<Cluster, double> kvp in scores)
                        if (kvp.Key.Center.Name == unknown.Name && kvp.Value > s)
                            s = kvp.Value;
                    incorrect = "* " + s.ToString("#0.00");
                }

                Console.WriteLine("1 Unknown: " + unknown.Name.PadLeft(9) + " = " + bestName.PadRight(9) +
                ", Score: " + bestScore.ToString("#0.00") + ", Branches: " + scores.Count.ToString().PadRight(4) + incorrect);*/
                return bestName;
            }


            LinkedList<Cluster> queue = GetStartingQueue(new Dictionary<Cluster, double>(predictions));

            while (queue.Count > 0)
            {
                Cluster current = queue.First.Value;
                queue.RemoveFirst();

                double score = current.Center.Recognize(unknown);
                double distance = 1.0 - score;

                if (score > bestScore)
                {
                    bestScore = score;
                    bestName = current.Center.Name;
                    bestTemplate = current.Center;
                }

                if (bestScore > ClusteringParameters.SCORE_THRESHOLD)
                {
                    /*if (unknown.Name != bestName)
                    {
                        double s = 0.0;
                        foreach (KeyValuePair<Cluster, double> kvp in scores)
                            if (kvp.Key.Center.Name == unknown.Name && kvp.Value > s)
                                s = kvp.Value;
                        incorrect = "* " + s.ToString("#0.00");
                    }
                    Console.WriteLine("2 Unknown: " + unknown.Name.PadLeft(9) + " = " + bestName.PadRight(9) +
                        ", Score: " + bestScore.ToString("#0.00") + ", Branches: " + scores.Count.ToString().PadRight(4) + incorrect);*/
                    return bestName;
                }

                scores.Add(current, score);

                if (distance > current.Radius * ClusteringParameters.RADIUS_RATIO)
                    continue;

                if (current.Children != null && current.DepthOfCluster < ClusteringParameters.MAX_DEPTH - 1)
                {
                    predictions.Add(current.Children[0], score);
                    predictions.Add(current.Children[1], score);
                    LinkedListNode<Cluster> node = null;
                    foreach (Cluster c in queue)
                    {
                        if (score > predictions[c])
                        {
                            node = queue.Find(c);
                            break;
                        }
                    }

                    if (node == null)
                        node = queue.Last;

                    if (queue.Count > 0)
                    {
                        queue.AddBefore(node, current.Children[0]);
                        queue.AddBefore(node, current.Children[1]);
                    }
                    else
                    {
                        queue.AddLast(current.Children[0]);
                        queue.AddLast(current.Children[1]);
                    }
                }
            }

            /*if (unknown.Name != bestName)
            {
                double s = 0.0;
                foreach (KeyValuePair<Cluster, double> kvp in scores)
                    if (kvp.Key.Center.Name == unknown.Name && kvp.Value > s)
                        s = kvp.Value;
                incorrect = "* " + s.ToString("#0.00");
            }
            Console.WriteLine("3 Unknown: " + unknown.Name.PadLeft(9) + " = " + bestName.PadRight(9) +
                ", Score: " + bestScore.ToString("#0.00") + ", Branches: " + scores.Count.ToString().PadRight(4) + incorrect);*/
            return bestName;
        }

        private static LinkedList<Cluster> GetStartingQueue(Dictionary<Cluster, double> predictions)
        {
            Cluster low = null;
            double max = double.MinValue;

            LinkedList<Cluster> queue = new LinkedList<Cluster>();
            while (predictions.Count > 0)
            {
                low = null;
                max = double.MinValue;

                foreach (KeyValuePair<Cluster, double> kvp in predictions)
                {
                    if (kvp.Value > max)
                    {
                        low = kvp.Key;
                        max = kvp.Value;
                    }
                }

                if (low != null)
                {
                    queue.AddLast(low);
                    predictions.Remove(low);
                }
            }

            return queue;
        }

        #endregion

        #region CENTER/DISTANCE FUNCTIONS

        /// <summary>
        /// Finds the cluster center and radius.
        /// </summary>
        /// <param name="nodes">All templates to find distances for</param>
        /// <param name="indices">Indices of templates in the distances matrix</param>
        /// <param name="distances">matrix of all distances between templates (pre-computed)</param>
        /// <param name="radius">Distance between the found center and the farthest away template</param>
        /// <returns>Template that is the most representative of the list of nodes (Center)</returns>
        private RecognitionTemplate FindClusterCenter(List<RecognitionTemplate> nodes, Dictionary<RecognitionTemplate, int> indices, double[,] distances, out double radius)
        {
            SortedList<double, RecognitionTemplate> sums = new SortedList<double, RecognitionTemplate>(indices.Count);
            double min = double.MaxValue;
            RecognitionTemplate center = null;

            for (int i = 0; i < indices.Count; i++)
            {
                RecognitionTemplate current = GetTemplate(indices, i);
                if (!m_Nodes.Contains(current)) continue;

                double sum = 0.0;
                for (int j = 0; j < indices.Count; j++)
                {
                    if (i == j) continue;

                    sum += distances[i, j];
                }
                if (sum < min)
                {
                    min = sum;
                    center = current;
                }
            }

            int ind = indices[center];
            radius = 0.0;
            for (int j = 0; j < indices.Count; j++)
            {
                if (ind == j) continue;
                RecognitionTemplate current = GetTemplate(indices, j);
                if (!m_Nodes.Contains(current)) continue;

                radius = Math.Max(radius, distances[ind, j]);
            }

            return center;
        }

        /// <summary>
        /// Finds the cluster center and radius.
        /// First computes the distance matrix then uses the other FindClusterCenter function.
        /// </summary>
        /// <param name="nodes">All templates to find distances for</param>
        /// <param name="radius">Distance between the found center and the farthest away template</param>
        /// <returns>Template that is the most representative of the list of nodes (Center)</returns>
        private RecognitionTemplate FindClusterCenter(List<RecognitionTemplate> nodes, out double radius)
        {
            Dictionary<RecognitionTemplate, int> indices;
            double[,] distances = GetDistances(nodes, out indices);
            RecognitionTemplate center = FindClusterCenter(m_Nodes, indices, distances, out radius);
            return center;
        }

        /// <summary>
        /// Finds the distances between each template using the RecognitionTemplate's Recognize function
        /// </summary>
        /// <param name="templates">All templates in this cluster</param>
        /// <param name="indices">Lookup table of templates to indices in distances matrix</param>
        /// <returns>Distances matrix</returns>
        private static double[,] GetDistances(List<RecognitionTemplate> templates, out Dictionary<RecognitionTemplate, int> indices)
        {
            indices = new Dictionary<RecognitionTemplate, int>(templates.Count);
            for (int i = 0; i < templates.Count; i++)
                indices.Add(templates[i], i);

            double[,] distances = new double[templates.Count, templates.Count];
            foreach (RecognitionTemplate t1 in templates)
            {
                int i = templates.IndexOf(t1);
                foreach (RecognitionTemplate t2 in templates)
                {
                    int j = templates.IndexOf(t2);

                    // Set the distance between a template and itself as 1.0 (max value)
                    if (i == j)
                        distances[i, j] = 1.0;
                    // If we've already found the complementary value ignore entry
                    else if (distances[j, i] != 0.0)
                        continue;
                    // Compute the distance between the two templates using the Recognize function
                    // The distance is equal to 1.0 - Score.
                    // Set both this matrix entry as well as its complement.
                    else
                    {
                        double d = 1.0 - t1.Recognize(t2);
                        distances[i, j] = d;
                        distances[j, i] = d;
                    }
                }
            }

            return distances;
        }

        private static double[,] GetClusterDistances(List<Cluster> clusters,
            double[,] distances, Dictionary<RecognitionTemplate, int> indicesTemplates, ClusterLinking linkMethod)
        {
            double[,] clusterDistances = new double[clusters.Count, clusters.Count];

            for (int i = 0; i < clusters.Count; i++)
                for (int j = i + 1; j < clusters.Count; j++)
                    clusterDistances[i, j] = GetClusterDistances(clusters[i], clusters[j], distances, indicesTemplates, linkMethod);

            return clusterDistances;
        }

        private static double GetClusterDistances(Cluster cluster1, Cluster cluster2, double[,] distances,
            Dictionary<RecognitionTemplate, int> indicesTemplates, ClusterLinking linkMethod)
        {
            switch (linkMethod)
            {
                case ClusterLinking.Single:
                    return GetMinDistance(cluster1, cluster2, distances, indicesTemplates);
                case ClusterLinking.Complete:
                    return GetMaxDistance(cluster1, cluster2, distances, indicesTemplates);
                case ClusterLinking.Average:
                    return GetAverageDistance(cluster1, cluster2, distances, indicesTemplates);
                default:
                    return GetMinDistance(cluster1, cluster2, distances, indicesTemplates);
            }
        }

        private static double GetAverageDistance(Cluster cluster1, Cluster cluster2, double[,] distances, Dictionary<RecognitionTemplate, int> indicesTemplates)
        {
            List<double> dists = new List<double>();
            foreach (RecognitionTemplate temp1 in cluster1.AllNodes)
            {
                foreach (RecognitionTemplate temp2 in cluster2.AllNodes)
                {
                    int i = indicesTemplates[temp1];
                    int j = indicesTemplates[temp2];
                    double dist = distances[i, j];
                    dists.Add(dist);
                }
            }

            return Utilities.Compute.Mean(dists.ToArray());
        }

        private static double GetMaxDistance(Cluster cluster1, Cluster cluster2, double[,] distances, Dictionary<RecognitionTemplate, int> indicesTemplates)
        {
            double max = double.MinValue;
            foreach (RecognitionTemplate temp1 in cluster1.AllNodes)
            {
                foreach (RecognitionTemplate temp2 in cluster2.AllNodes)
                {
                    int i = indicesTemplates[temp1];
                    int j = indicesTemplates[temp2];
                    double dist = distances[i, j];
                    max = Math.Max(max, dist);
                }
            }

            return max;
        }

        private static double GetMinDistance(Cluster cluster1, Cluster cluster2, double[,] distances, Dictionary<RecognitionTemplate, int> indicesTemplates)
        {
            double min = double.MaxValue;
            foreach (RecognitionTemplate temp1 in cluster1.AllNodes)
            {
                foreach (RecognitionTemplate temp2 in cluster2.AllNodes)
                {
                    int i = indicesTemplates[temp1];
                    int j = indicesTemplates[temp2];
                    double dist = distances[i, j];
                    min = Math.Min(min, dist);
                }
            }

            return min;
        }

        /// <summary>
        /// Helper function to grab the index of a value in the lookup table.
        /// Throws exception if the value is not found.
        /// </summary>
        /// <param name="indices">lookup table to find index of value</param>
        /// <param name="i">value to find index of</param>
        /// <returns>RecognitionTemplate with index 'i'</returns>
        /// <exception cref="Value Not Found">Throws exception if value 'i' is not found in the Dictionary 'indices'</exception>
        private RecognitionTemplate GetTemplate(Dictionary<RecognitionTemplate, int> indices, int i)
        {
            foreach (KeyValuePair<RecognitionTemplate, int> kvp in indices)
                if (kvp.Value == i)
                    return kvp.Key;

            throw new Exception("Value not found in Dictionary: " + i.ToString());
        }

        #endregion

        #region TREE BUILDING

        /// <summary>
        /// Builds a complete hierarchical tree and returns the root cluster.
        /// Finds the distances then calls the other BuildTree function.
        /// </summary>
        /// <param name="templates">All templates to cluster</param>
        /// <returns>Root Cluster of the tree</returns>
        public static Cluster BuildTree(List<RecognitionTemplate> templates)
        {
            Dictionary<RecognitionTemplate, int> indicesTemplates;
            double[,] distances = GetDistances(templates, out indicesTemplates);

            List<Cluster> clusters = new List<Cluster>(indicesTemplates.Count);
            foreach (RecognitionTemplate temp in indicesTemplates.Keys)
                clusters.Add(new Cluster(temp));

            return BuildTree(indicesTemplates, distances, clusters, m_LinkingMethod);
        }

        /// <summary>
        /// Builds a complete hierarchical tree and returns the root cluster.
        /// Finds the distances then calls the other BuildTree function.
        /// </summary>
        /// <param name="templates">All templates to cluster</param>
        /// <returns>Root Cluster of the tree</returns>
        public static Cluster BuildTree(List<RecognitionTemplate> templates, ClusterLinking linkingMethod)
        {
            Dictionary<RecognitionTemplate, int> indicesTemplates;
            double[,] distances = GetDistances(templates, out indicesTemplates);

            List<Cluster> clusters = new List<Cluster>(indicesTemplates.Count);
            foreach (RecognitionTemplate temp in indicesTemplates.Keys)
                clusters.Add(new Cluster(temp));

            return BuildTree(indicesTemplates, distances, clusters, linkingMethod);
        }


        /// <summary>
        /// Builds a complete hierarchical tree and returns the root cluster.
        /// </summary>
        /// <param name="indicesTemplates">List of all templates to cluster with their associated indices</param>
        /// <param name="distances">Complete distance matrix for templates</param>
        /// <returns>Root Cluster of the tree</returns>
        /// <exception cref="No Clusters">No clusters to build tree from</exception>
        /// <exception cref="Invalid Distances">No valid distances in the distance matrix to find which clusters to merge</exception>
        private static Cluster BuildTree(Dictionary<RecognitionTemplate, int> indicesTemplates, double[,] distances, List<Cluster> clusters, ClusterLinking linkMethod)
        {
            while (clusters.Count > 1)
            {
                double[,] clusterDistances = GetClusterDistances(clusters, distances, indicesTemplates, linkMethod);

                double min = double.MaxValue;
                Cluster remove1 = null;
                Cluster remove2 = null;

                for (int i = 0; i < clusters.Count; i++)
                {
                    for (int j = i + 1; j < clusters.Count; j++)
                    {
                        if (clusterDistances[i, j] < min)
                        {
                            min = distances[i, j];
                            remove1 = clusters[i];
                            remove2 = clusters[j];
                        }
                    }
                }

                if (remove1 == null || remove2 == null)
                    throw new Exception("Unable to build tree: no distances < double.MaxValue");

                Cluster merged = MergeClusters(remove1, remove2, indicesTemplates, distances);
                clusters.Add(merged);
                clusters.Remove(remove1);
                clusters.Remove(remove2);
            }

            if (clusters.Count > 0)
                return clusters[0];
            else
                throw new Exception("No clusters to build tree from");
        }

        /// <summary>
        /// Create a new cluster that is a merge of the two clusters inputted
        /// This new cluster is the parent of clusters 1 & 2.
        /// </summary>
        /// <param name="cluster_1">Cluster #1 to merge</param>
        /// <param name="cluster_2">Cluster #2 to merge</param>
        /// <param name="indicesTemplates">Templates and their indices in the distance matrix</param>
        /// <param name="distances">Distances matrix</param>
        /// <returns>New Cluster that is a parent of clusters 1 & 2</returns>
        private static Cluster MergeClusters(Cluster cluster_1, Cluster cluster_2, Dictionary<RecognitionTemplate, int> indicesTemplates, double[,] distances)
        {
            List<RecognitionTemplate> templates = new List<RecognitionTemplate>();
            foreach (RecognitionTemplate t in cluster_1.m_Nodes)
                templates.Add(t);
            foreach (RecognitionTemplate t in cluster_2.m_Nodes)
                templates.Add(t);

            List<Cluster> children = new List<Cluster>();
            children.Add(cluster_1);
            children.Add(cluster_2);

            return new Cluster(templates, children, indicesTemplates, distances);
        }

        /// <summary>
        /// Create a new cluster that is a merge of the two clusters inputted.
        /// This new cluster is the parent of clusters 1 & 2.
        /// This function creates the distance matrix and calls the other MergeClusters function.
        /// </summary>
        /// <param name="cluster_1">Cluster #1 to merge</param>
        /// <param name="cluster_2">Cluster #2 to merge</param>
        /// <returns>New Cluster that is a parent of clusters 1 & 2</returns>
        private static Cluster MergeClusters(Cluster cluster_1, Cluster cluster_2)
        {
            List<RecognitionTemplate> templates = new List<RecognitionTemplate>();
            foreach (RecognitionTemplate t in cluster_1.m_Nodes)
                templates.Add(t);
            foreach (RecognitionTemplate t in cluster_2.m_Nodes)
                templates.Add(t);

            Dictionary<RecognitionTemplate, int> indices;
            double[,] distances = GetDistances(templates, out indices);

            return MergeClusters(cluster_1, cluster_2, indices, distances);
        }

        #endregion

        #region GETTERS & SETTERS

        /// <summary>
        /// Is this Cluster a leaf - the bottom-most layer
        /// </summary>
        public bool IsLeaf
        {
            get { return (m_Children == null || m_Children.Count == 0); }
        }

        /// <summary>
        /// Is this Cluster a root - the top-most layer
        /// </summary>
        public bool IsRoot
        {
            get { return m_Parent == null; }
        }

        /// <summary>
        /// Gets the cluster's parent. 
        /// If this is the root node it will return null.
        /// </summary>
        public Cluster Parent
        {
            get { return m_Parent; }
        }

        /// <summary>
        /// Gets the immediate children of this Cluster
        /// Returns null if this is a leaf node.
        /// </summary>
        public List<Cluster> Children
        {
            get { return m_Children; }
        }

        /// <summary>
        /// Returns the most representative RecognitionTemplate in this Cluster.
        /// </summary>
        public RecognitionTemplate Center
        {
            get { return m_Center; }
        }

        /// <summary>
        /// Gets the maximum distance between the Center and all other nodes.
        /// </summary>
        public double Radius
        {
            get { return m_Radius; }
        }

        /// <summary>
        /// Gets all RecognitionTemplates in this Cluster.
        /// </summary>
        public List<RecognitionTemplate> AllNodes
        {
            get { return m_Nodes; }
        }

        /// <summary>
        /// Gets the depth of the current cluster.
        /// </summary>
        public int DepthOfCluster
        {
            get
            {
                int depth = 0;
                Cluster current = this;
                while (!current.IsRoot)
                {
                    current = current.Parent;
                    depth++;
                }

                return depth;
            }
        }

        #endregion

        private void PrintToConsole(double score)
        {
            Console.WriteLine("Name: " + m_Center.Name +
                        " --> Score = " + score.ToString("#0.000") +
                        ", Depth = " + DepthOfCluster.ToString());
        }
    }
}
