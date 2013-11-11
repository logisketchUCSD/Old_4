/*
 * File: LabelGroup.cs
 * 
 * Authors: Originally by unknown,
 *			Modified by James Brown
 * Harvey Mudd College, Claremont, CA 91711.
 * Sketchers 2008.
 * 
 * Use at your own risk.  This code is not maintained and not guaranteed to work.
 * We take no responsibility for any harm this code may cause.
 */

using System;
using System.Collections.Generic;
using Sketch;
using MathNet.Numerics.LinearAlgebra;

namespace Grouper
{
	/// <summary>
	/// This is basically just my version of Component from Segment, chopped down into just the stuff
	/// I need/want.
	/// </summary>
	public class LabelGroup
	{
		#region CONSTANTS
		/// <summary>
		/// When examining strokes, look at every point (1), every other point (2), every third
		/// point (3), etc.
		/// </summary>
		private const int POINT_COARSENESS = 1;

		/// <summary>
		/// Empirical distance threshold for connectedness
		/// </summary>
		private const double GATE_DISTANCE_THRESHOLD = 80;
		private const double WIRE_DISTANCE_THRESHOLD = 80;
        private const double DATA_DISTANCE_THRESHOLD = 15;

		#endregion

		#region INTERNALS
		/// <summary>
		/// The substrokes (must all have the same label)
		/// </summary>
		public List<Substroke> substrokes;

		/// <summary>
		/// The clusters of strokes that we've come up with
		/// </summary>
		public List<Cluster> clusters;

		/// <summary>
		/// All the substrokes in this group are part of Shapes with this label
		/// </summary>
		public string label;

		/// <summary>
		/// True if analyze has been called on the current substrokes. Is set to false if
		/// any substrokes are added/removed.
		/// </summary>
		public bool analyzed;

		// Statistics
		/// <summary>
		/// A measure of the largest number of substrokes between any two clustered substrokes as measured
		/// by their index number. A very coarse representation of how serial the author was in the way
		/// they drew the diagram (finishing one wire/gate before starting another).
		/// </summary>
		public int maxStrokeDelta = int.MinValue;

		/// <summary>
		/// The sum of all the distances in the substrokes list (based on index number) between pairs of
		/// strokes considered adjacent to one another. Divide by totalAdjacencies for the average.
		/// </summary>
		public double avgStrokeDelta = 0;

		/// <summary>
		/// The number of pairs of strokes considered adjacent to one another.
		/// </summary>
		public int totalAdjacencies = 0;

		/// <summary>
		/// The minimum distance between any two substrokes. Since most strokes in a gate should share
		/// a common endpoint (for a reasonable threshold), we'll use this measure instead of average
		/// distance.
		/// </summary>
		public Matrix minDistances;

		/// <summary>
		/// Taking into account time, etc.
		/// </summary>
		public Matrix adjustedDistances;

		/// <summary>
		/// 1 if two substrokes are adjacent to one another, 0 otherwise
		/// </summary>
		public Matrix adjacent;

        /// <summary>
        /// distance threshold that can be passed into the grouper
        /// </summary>
        public double THRESHOLD;

		#endregion

		#region CONSTRUCTORS

		public LabelGroup(string label)
		{
			this.substrokes = new List<Substroke>();
			this.label = label;

			this.analyzed = false;
			this.minDistances = null;
			this.adjustedDistances = null;
		}

        public LabelGroup(string label, List<Substroke> substrokes, int threshold)
        {
            this.substrokes = substrokes;
            this.label = label;

            this.analyzed = false;
            this.minDistances = null;
            this.adjustedDistances = null;
            this.THRESHOLD = threshold;
        }

		#endregion

		public void addSubstroke(Sketch.Substroke substroke)
		{
			substrokes.Add(substroke);
			analyzed = false;
		}

		/// <summary>
		/// Calculates distance and other data for the substrokes in the group.
		/// </summary>
		public void analyze()
		{
			if(!analyzed)	
			{
				calculateDistances();
				calculateAdjacency();
				analyzed = true;
			} 
			else	
			{
				Console.WriteLine("ANALYZE: labelgroup " + this.label + " is already up-to-date.");
			}
		}

		#region DISTANCE

		public void calculateDistances()	
		{
			int length = substrokes.Count;

			// minDistances
			minDistances = new Matrix(length, length);
			for (int i = 0; i < length - 1; i++)	
			{
				minDistances[i, i] = 0.0;
				for (int j = i + 1; j < length; j++)	
				{
					minDistances[i,j] = minDistance((Sketch.Substroke)substrokes[i], (Sketch.Substroke)substrokes[j]);
					minDistances[j,i] = minDistances[i,j];
				}
			}

			// adjusted
			adjustedDistances = new Matrix(length, length);
			for (int i=0; i<length - 1; i++)
			{
				adjustedDistances[i,i] = 0.0;
				for (int j=i + 1; j<length; j++)
				{
					adjustedDistances[i,j] = getAdjustedValue(i, j);
					adjustedDistances[j,i] = adjustedDistances[i,j];
				}
			}
		}

		private double minDistance(Sketch.Substroke sub1, Sketch.Substroke sub2)	
		{
			if (this.label == "Wire")	
			{
				return minWireDistance(sub1, sub2);
			}
			else
			{
				return minGateDistance(sub1, sub2);
			}
		}

		/// <summary>
		/// Calculates the min distance between two wires. Makes the assumption that one of the
		/// the lines terminates at the other. There are certain issues with the user "overshooting"
		/// the line and thus creating a mini-x. We'll have to test to see if this is a real problem.
		/// </summary>
		/// <param name="s1"></param>
		/// <param name="s2"></param>
		/// <returns></returns>
		private double minWireDistance(Substroke s1, Substroke s2)	
		{
			// We only need to examine the endpoints of the wires
			// NOTE: This uses the assumption that the first and last points are the endpoints.
			// I believe the labeler uses this same assumption and seems to do okay, so here we go
			Point[] s1points = s1.Points;
			Point[] s2points = s2.Points;
			Point s1A = s1points[0];
			Point s1B = s1points[s1points.Length-1];
			Point s2A = s2points[0];
			Point s2B = s2points[s2points.Length-1];

			return Math.Min( minWireDistance(s1, s2A, s2B), minWireDistance(s2, s1A, s1B) );
		}

		private double minWireDistance(Substroke sub, Point pt1, Point pt2)	
		{
			return Math.Min( minWireDistance(sub, pt1), minWireDistance(sub, pt2) );
		}

		private double minWireDistance(Substroke sub, Point endpt)	
		{
			Point[] points = sub.Points;

			double min = Double.MaxValue;
			double dist;
            int len = points.Length;
			for(int i = 0; i < len; i += POINT_COARSENESS)	
			{
				dist = euclideanDistance(endpt.X, endpt.Y, points[i].X, points[i].Y);
				if (dist < min)	
					min = dist;
			}
			return min;
		}

		private double minGateDistance(Substroke s1, Substroke s2)	
		{
			Sketch.Point[] p1 = s1.Points;
			Sketch.Point[] p2 = s2.Points;

			double min = Double.MaxValue;
			double dist;
            int len = p1.Length;
			for(int i = 0; i < len; i += LabelGroup.POINT_COARSENESS)
			{
                int len2 = p2.Length;
				for(int j = 0; j < len2; j += LabelGroup.POINT_COARSENESS)
				{
					dist = LabelGroup.euclideanDistance(p1[i].X, p1[i].Y, p2[j].X, p2[j].Y);
					if(dist < min)
						min = dist;
				}
			}

			return min;
		}

		private double getAdjustedValue(int i, int j)	
		{
			return minDistances[i,j];
		}

		#endregion

		#region ADJACENCY

		private void calculateAdjacency()
		{
			int length = substrokes.Count;
			adjacent = new Matrix(length,length);

			for (int i=0; i < length-1; i++)	
			{
				adjacent[i,i] = 0;				// we're actually not adjacent to ourselves
				for (int j= i + 1; j < length; j++)	
				{
					if (isAdjacent(i,j))	
					{
						/*Console.WriteLine(i + " " + j + "  " + minDistances[i,j]);
						if(minDistances[i,j] > 35)	
						{
							((Substroke)substrokes[i]).XmlAttrs.Color 
								= ((Substroke)substrokes[j]).XmlAttrs.Color 
								= Grouper.generateColor(-1).ToArgb();
						}
						*/
						int delta = Math.Abs(i-j);
						if(delta > maxStrokeDelta)
							maxStrokeDelta = delta;
						if(delta > 0)	
						{
							avgStrokeDelta += delta;
							totalAdjacencies++;
						}

						adjacent[i,j] = 1;
						adjacent[j,i] = 1;
					}
					else
					{
						adjacent[i,j] = 0;
						adjacent[j,i] = 0;
					}
				}
			}
		}

		private bool isAdjacent(int i, int j)	
		{
			if (this.label.Equals("Wire") || (this.label.Equals("Mesh")))	
            {
				return adjustedDistances[i,j] < WIRE_DISTANCE_THRESHOLD;
			}
            if(this.label.Equals("Gate") 
                || this.label.Equals("AND") 
                || this.label.Equals("OR") 
                || this.label.Equals("NOT") 
                || this.label.Equals("NAND") 
                || this.label.Equals("NOR") 
                || this.label.Equals("XNOR")
				|| this.label.Equals("XOR")) 
            {
                return adjustedDistances[i, j] < GATE_DISTANCE_THRESHOLD;
            }
            if (this.label.Equals("Label"))
            {
                return adjustedDistances[i, j] < THRESHOLD;
            }
            return adjustedDistances[i, j] < DATA_DISTANCE_THRESHOLD;
		}

		#endregion

		#region MATH

		internal static double euclideanDistance(double x1, double y1, double x2, double y2)
		{
			return Math.Sqrt((x1 - x2)*(x1 - x2) + (y1 - y2)*(y1 - y2));
		}
		#endregion
	}
}
