using System;
using System.Collections.Generic;
using System.Text;
using Sketch;


namespace Grouper
{
    class TemporalGroup : Group
    {
		private struct StrokeTime
		{
			/// <summary>
			/// The pause after this stroke
			/// </summary>
			public ulong postt;
			/// <summary>
			/// The substroke's id in the stroke array
			/// </summary>
			public int strokeIdx;
		}

        // Jason's addition
        private const ulong THRESHOLD = 900;
		private const double WAIT_TIME = 1.33D;
        private int numShapes = 0;
		
		NHGroup spatialGrouper;

		public TemporalGroup(Sketch.Sketch sketch) : base(sketch)
		{
			labels = sketch.LabelStrings;
			spatialGrouper = new NHGroup(sketch);

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
        /// Group the substrokes based on temporal data
        /// </summary>
        /// <param name="verbose">Debug if true.  No effect currently</param>
        public override void group(bool verbose)
        {
			// TODO: Decide to use labels depending on their Belief
            bool useLabels = true;


            separateSubstrokes(); // Populates the label groups
            destroyAllShapes();   // Get rid of all the inital shapes

            if (useLabels)
                for (int i = 0; i < labels.Count; ++i)
					temporalHistoryGroup(labelGroups[i].substrokes.ToArray(), labelGroups[i].label);
//                    temporalLocalMaxGroup(labelGroups[i].substrokes.ToArray()); //local maximum grouping
            else
                temporalLocalMaxGroup(sketch.Substrokes);
        }

        /// <summary>
        /// Group all of the substrokes in the passed in the list of substrokes.
        /// Note that the list of substrokes can be just the substrokes from a given
        /// label group, allowing us to separately group strokes from different label
        /// groups.
        /// </summary>
        /// <param name="substrokes">Array of substrokes *in temporal order*</param>
        private void temporalGroup(Substroke[] substrokes)
        {
            // Substrokes are supposed to be in temporal order
            int numSubstrokes = substrokes.Length;

            # region pathological cases
            if (numSubstrokes == 0) 
            {
                if (verbose) Console.WriteLine("No substrokes.");
                return;
            }

            if (numSubstrokes == 1)
            {
                if (verbose) Console.WriteLine("Only one substroke.");
                List<Substroke> quickShape = new List<Substroke>(substrokes);
                createNewShape(quickShape);
                return;
            }
            # endregion

            //int numShapes = 0;  Global variable, initialized with Grouper
            List<Substroke> shapeAccumulator = new List<Substroke>();
            List<ulong> timeDeltas = new List<ulong>();

            for (int substrokeIdx = 0; substrokeIdx < (numSubstrokes - 1); ++substrokeIdx)
            {
                shapeAccumulator.Add(substrokes[substrokeIdx]);
                ulong difference = timeDelta(substrokes[substrokeIdx], substrokes[substrokeIdx + 1]);
                timeDeltas.Add(difference);

                // If there is more than a THRESHOLD gap between the two substrokes
                // then it signals a border between shapes.  Dump all the substrokes
                // seen since the last shape was created into a new shape in the sketch,
                // reset the list of substrokes in the current shape, and then continue
                // through the sketch.
                //if ((firstNextPt.XmlAttrs.Time - lastCurrPt.XmlAttrs.Time) > THRESHOLD)
                if (difference > THRESHOLD)
                {
                    createNewShape(shapeAccumulator);
                    shapeAccumulator = new List<Substroke>();
                }
            }
            shapeAccumulator.Add(substrokes[numSubstrokes - 1]);
            // Clean up leftovers
            if (shapeAccumulator.Count != 0)
            {
                createNewShape(shapeAccumulator);
            }

            //Console.WriteLine("Summary statistics for this group of substrokes.");
            //Console.WriteLine("The label on the first substroke in this array is {0}", substrokes[0].XmlAttrs.Type);
            //double sum = 0.0;
            //foreach (ulong delta in timeDeltas)
            //{
            //    sum += (double)delta;
            //}
            //Console.WriteLine("The mean time difference between substrokes is {0}", sum / ((double)(timeDeltas.Count)));

            //timeDeltas.Sort();
            //double numDeltas = (double)(timeDeltas.Count);
            //int midpoint = (int)Math.Ceiling(numDeltas / 2.0);
            //Console.WriteLine("The median time difference between substrokes is {0}", timeDeltas[midpoint]);
        }


        /// <summary>
        /// Group all of the substrokes in the passed in the list of substrokes
        /// by splitting groups whenever there is a local maximum in the time difference
        /// between strokes
        /// </summary>
        /// <param name="substrokes">Array of substrokes *in temporal order*</param>
        private void temporalLocalMaxGroup(Substroke[] substrokes)
        {
            // Substrokes are supposed to be in temporal order
            int numSubstrokes = substrokes.Length;

            # region pathalogical cases
            if (numSubstrokes == 0)
            {
				if(verbose) Console.WriteLine("No substrokes.");
                return;
            }
            else if (numSubstrokes == 1)
            {
                if (verbose) Console.WriteLine("Only one substroke.  Not enough for single difference");
                List<Substroke> quickShape = new List<Substroke>(substrokes);
                createNewShape(quickShape);
                return;
            }
			else if (numSubstrokes <= 4)
			{
				NHGroup spatial = new NHGroup(sketch);
			}
			else if (numSubstrokes == 2)
			{
				if (verbose) Console.WriteLine("Only two substrokes.  Not enough for double difference.");
				// This is not necessarily right TODO: FIXME!
				List<Substroke> quickShape = new List<Substroke>(substrokes);
				createNewShape(quickShape);
				return;
				//if (timeDelta(substrokes[0], substrokes[1]) > THRESHOLD)
				//{
				//    // Split the two strokes into separate shapes
				//    List<Substroke> quickShape = new List<Substroke>(substrokes[0]);
				//    createNewShape(quickShape);
				//    List<Substroke> quickShape = new List<Substroke>(substrokes[1]);
				//    createNewShape(quickShape);
				//    return;
				//}
				//else
				//{
				//    // Keep the two strokes together as a single shape.
				//    List<Substroke> quickShape = new List<Substroke>(substrokes);
				//    createNewShape(quickShape);
				//    return;
				//}
			}
			else if (numSubstrokes == 3)
			{
				if (verbose) Console.WriteLine("Only three substrokes.  Not enough to look at successive double differnces.");
				// This is not necessarily right TODO: FIXME!
				List<Substroke> quickShape = new List<Substroke>(substrokes);
				createNewShape(quickShape);
				return;
			}
            # endregion

            List<ulong> timeDeltas = new List<ulong>();
            for (int substrokeIdx = 0; substrokeIdx < (numSubstrokes - 1); ++substrokeIdx)
            {
                timeDeltas.Add(timeDelta(substrokes[substrokeIdx], substrokes[substrokeIdx+1]));
            }

            // Detect local maximum by finding where the sign of the difference between successive
            // time differences changes from positive to negative.  Have to separately check boundry
            // cases with a threshold because there are not enough points to take a second difference
            List<Substroke> shapeAccumulator = new List<Substroke>();
            shapeAccumulator.Add(substrokes[0]);
            if (timeDeltas[0] > THRESHOLD)
            {
                createNewShape(shapeAccumulator);
                shapeAccumulator = new List<Substroke>();
            }
            for (int deltaIdx = 1; deltaIdx < (timeDeltas.Count - 1); ++deltaIdx)
            {
                shapeAccumulator.Add(substrokes[deltaIdx]);

                //if (((timeDeltas[deltaIdx] - timeDeltas[deltaIdx-1]) >= 0) &&
                //    ((timeDeltas[deltaIdx+1] - timeDeltas[deltaIdx]) < 0))

                if ((timeDeltas[deltaIdx] > timeDeltas[deltaIdx-1]) && (timeDeltas[deltaIdx] > timeDeltas[deltaIdx+1]))
                {
                    createNewShape(shapeAccumulator);
                    shapeAccumulator = new List<Substroke>();
                }
            }
            shapeAccumulator.Add(substrokes[timeDeltas.Count - 1]);
            if (timeDeltas[timeDeltas.Count - 1] > THRESHOLD)
            {
                createNewShape(shapeAccumulator);
                shapeAccumulator = new List<Substroke>();
            }
            shapeAccumulator.Add(substrokes[numSubstrokes - 1]);

            // Clean up leftovers
            if (shapeAccumulator.Count != 0)
            {
                createNewShape(shapeAccumulator);
            }

        }

		/// <summary>
		/// Groups using temporal history rather than a fixed local maxima
		/// or threshold
		/// </summary>
		/// <param name="strokes">The substrokes to group, in temporal order</param>
		/// <param name="label">The label for the group</param>
		private void temporalHistoryGroup(Substroke[] strokes, string label)
		{
			#region special cases
			int numStrokes = strokes.Length;
			if (numStrokes == 0)
				return;
			else if (numStrokes == 1)
			{
				createNewLabeledShape(new List<Substroke>(strokes), label);
				return;
			}
			else if (numStrokes == 2 || numStrokes == 3)
			{
				// Use a spatial grouper in this case
				spatialGrouper.hierarchicalCluster(new List<Substroke>(strokes), label);
				return;
			}
			#endregion
			List<StrokeTime> times = new List<StrokeTime>();
			ulong totalTime = 0;
			for (int i = 0; i < (strokes.Length - 1); ++i)
			{
				StrokeTime st = new StrokeTime();
				ulong td = timeDelta(strokes[i], strokes[i + 1]);
				totalTime += td;
				st.postt = td;
				st.strokeIdx = i;
				times.Add(st);
			}
			ulong averageTime = (totalTime / ((ulong)(strokes.Length - 1)));
			List<int> splitIndices = new List<int>();
			foreach(StrokeTime splitter in times.FindAll(delegate(StrokeTime st) { return (st.postt > averageTime*WAIT_TIME); }))
			{
				splitIndices.Add(splitter.strokeIdx);
			}
			List<Substroke> shapeAccumulator = new List<Substroke>();
			for(int i = 0; i < strokes.Length; ++i)
			{
				shapeAccumulator.Add(strokes[i]);
				if (splitIndices.Contains(i))
				{
					createNewLabeledShape(shapeAccumulator, label);
					shapeAccumulator.RemoveRange(0, shapeAccumulator.Count);
				}
			}
			if (shapeAccumulator.Count != 0)
				createNewLabeledShape(shapeAccumulator, label);
		}

		#region UTILITY

		/// <summary>
		/// Calculates the time difference between the end of the first substroke and the
		/// beginning of the next substroke.
		/// </summary>
		/// <param name="currSubstroke">The ending time of this substroke is used</param>
		/// <param name="nextSubstroke">The starting time of this substroke is used</param>
		/// <returns></returns>
		private ulong timeDelta(Substroke currSubstroke, Substroke nextSubstroke)
        {
            List<Point> currPoints = currSubstroke.PointsL;
            List<Point> nextPoints = nextSubstroke.PointsL;

            // Make sure the points are ordered by ascending time
            currPoints.Sort();
            nextPoints.Sort();

            // Assuming all strokes are non-empty
            Point lastCurrPt = currPoints[currPoints.Count - 1];
            Point firstNextPt = nextPoints[0];
            // Assuming with this cast that all the points have time values (originally were type ulong?)
            ulong difference = (ulong)(firstNextPt.XmlAttrs.Time - lastCurrPt.XmlAttrs.Time);

            return difference;
        }

        /// <summary>
        /// Given a list of substrokes in the sketch, turn it into a new shape
        /// </summary>
        /// <param name="shapeAccumulator">List of substrokes</param>
        public void createNewShape(List<Substroke> shapeAccumulator)
        {
			createNewLabeledShape(shapeAccumulator, "Unknown");
        }

		/// <summary>
		/// Given a list of substrokes and a label, create a new shape
		/// </summary>
		/// <param name="strokes">List of substrokes</param>
		/// <param name="label">The label for the new shape</param>
		public void createNewLabeledShape(List<Substroke> strokes, string label)
		{
			Shape newShape = new Shape();
			newShape.AddSubstrokes(strokes);
			if (strokes.Count != newShape.Substrokes.Length)
				throw new Exception("Substrokes count should equal new shapes count");
			newShape.XmlAttrs.Name = "shape";
			numShapes++;
			newShape.XmlAttrs.Type = label;
			newShape.XmlAttrs.Id = Guid.NewGuid();
			newShape.XmlAttrs.Source = "Time-based clustering";

			sketch.AddShape(newShape);
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

		public void printStatistics()	
		{
			foreach (LabelGroup group in this.labelGroups)	
			{
				if(group.substrokes.Count == 0)
                    continue;
				Console.WriteLine(group.label + ":");
				Console.WriteLine("    " + group.substrokes.Count + " substrokes");
                //Console.WriteLine("    " + group.clusters.Count + "  clusters");
                //if(group.substrokes.Count > 1)	
                //{
                //    Console.WriteLine("    max delta: " + group.maxStrokeDelta);
                //    Console.WriteLine("    avg delta: " + (group.avgStrokeDelta/group.totalAdjacencies));
                //}
			}
		}
		#endregion
    }
}
