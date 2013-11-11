using System;
using System.Collections.Generic;
using Sketch;

namespace Grouper
{
	/// <summary>
	/// Grouping method that uses a fairly naive approach to grouping: simply, those substrokes
	/// that are close to one another in time and space are grouped together in a growing 
	/// collection of groups.
	/// </summary>
	internal class PGroup : Group, IGroup
	{
		//private ArrayList clusters;

		public PGroup(Sketch.Sketch sketch) : base(sketch)
		{
			labels = sketch.LabelStrings;
			//clusters = new ArrayList();

			// Initialize substroke type-groups
			labelGroups = new List<LabelGroup>(labels.Count);
			foreach (string label in labels)
			{
				if (label == "unlabeled")
				{
					unlabeledLabelGroup = new LabelGroup(label);
					labelGroups.Add(unlabeledLabelGroup);
				}
				else
				{
					labelGroups.Add(new LabelGroup(label));
				}
			}
		}

		/// <summary>
		/// Groups the substrokes into logical shapes.
		/// </summary>
		public override void group()	
		{
            group(false);
		}

        /// <summary>
        /// Group
        /// </summary>
        /// <param name="verbose">Debug if true</param>
        public override void group(bool verbose)
        {
            if (verbose)
            {
            }

            separateSubstrokes();

            
            destroyAllShapes();			// expensive, but necessary


			int len = labelGroups.Count;
            for (int i = 0; i < len; ++i)
            {
                hierarchicalCluster(labelGroups[i]);
            }
            
        }

		/// <summary>
		/// Clusters an ArrayList of substrokes into small groups (hopefully into single gates/wires)
		/// NOTE: We are guaranteed that the substrokes are in ascending temporal order.
		/// NOTE: We should split this into a method for wires and a method for non-wires (gates), as the
		/// two structures are very different.
		/// </summary>
		/// <param name="substrokes">An ArrayList of the substrokes to be grouped.</param>
		public void hierarchicalCluster(LabelGroup labelgroup)	
		{
			cluster(labelgroup);
			buildShapes(labelgroup);
		}

		private void cluster(LabelGroup labelgroup)
		{
			labelgroup.analyze();	// gives us adjacency list

			labelgroup.clusters = breadthFirstSearch(labelgroup);
		}

		#region BFS
		private List<Cluster> breadthFirstSearch(LabelGroup labelgroup)	
		{
			Substroke[] substrokes = labelgroup.substrokes.ToArray();

            List<Cluster> clusters = new List<Cluster>(); // list of clusters we create

			// Keep track of which "nodes" we've already visited
			// -1: white (unvisited) 0: gray (queued) 1: black (visited)
    
        
			const int WHITE = -1;
			const int GRAY  = 0;
			const int BLACK = 1;
        
            int len = substrokes.Length;
			Dictionary<int, int> visited = new Dictionary<int, int>(len);
			for(int i = 0; i < len; ++i)
			{
				visited.Add(i, -1);
			}

			
			// BFS through the adjacency list, building a forest of clusters
			for (int i = 0; i < len ; ++i)	
			{ 
				if(visited[i] == WHITE)	
				{
					// Run a BFS with i as the root
					Cluster newCluster = new Cluster(this.sketch, labelgroup.label);

					// Stores the indices of the substrokes on the search stack
					Stack<int> stack = new Stack<int>(substrokes.Length);

					visited[i] = GRAY;
					stack.Push(i);

					while (stack.Count > 0)	
					{
						int node = (int)stack.Pop();

						visited[node] = BLACK;
						newCluster.addSubstroke(substrokes[node]);

						int[] neighbors = this.getUnvisitedNeighbors(visited, node, labelgroup);

						for(int j=0; j<neighbors.Length; j++)	
						{
							stack.Push(neighbors[j]);
							visited[neighbors[j]] = GRAY;
						}
					}
					clusters.Add(newCluster);
				}
			}

			return clusters;
		}

		private int[] getUnvisitedNeighbors(Dictionary<int, int> visited, int node, LabelGroup labelgroup)
		{
			const int WHITE = -1;

			List<int> neighbors = new List<int>();

            int len = labelgroup.substrokes.Count;
            for (int i = 0; i < len ; ++i)
            {
                if (labelgroup.adjacent[node, i] == 1.0 && visited[i] == WHITE)
                {
                    neighbors.Add(i);
                }
            }
			return neighbors.ToArray();
		}
		#endregion

		#region UTILITY

		public void printStatistics()	
		{
			foreach (LabelGroup group in this.labelGroups)	
			{
				if(group.substrokes.Count == 0)
                    continue;
				Console.WriteLine(group.label + ":");
				Console.WriteLine("    " + group.substrokes.Count + " substrokes");
				Console.WriteLine("    " + group.clusters.Count + "  clusters");
				if(group.substrokes.Count > 1)	
				{
					Console.WriteLine("    max delta: " + group.maxStrokeDelta);
					Console.WriteLine("    avg delta: " + (group.avgStrokeDelta/group.totalAdjacencies));
				}
			}
		}
		#endregion
	}
}
