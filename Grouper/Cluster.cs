using System;
using System.Collections.Generic;
using Sketch;

namespace Grouper
{
	/// <summary>
	/// Represents a logical cluster of substrokes
	/// </summary>
	public class Cluster
	{
		private Sketch.Sketch sketch;
		private string label;
		private List<Substroke> substrokes;

		public Cluster(Sketch.Sketch sketch, string label)
		{
			this.sketch = sketch;
			this.label = label;
			substrokes = new List<Substroke>();
		}

		public void addSubstroke(Substroke substroke)	
		{
			// Substrokes is a set, but since we don't have a set,
			// we just need to be careful not to put duplicates in.
			if (!substrokes.Contains(substroke))
				substrokes.Add(substroke);
		}

		#region SHAPE GENERATION/MANIPULATION

		/// <summary>
		/// Creates a new shape containing all of the cluster's strokes. Does not preserve any
		/// information from the original parent shapes. Use consolidateShapes() if you want
		/// to keep this information.
		/// </summary>
		/// <returns></returns>
		public Shape convertToNewShape()	
		{
			Shape newShape = new Shape();

			newShape.AddSubstrokes(this.substrokes);

            if (this.substrokes.Count != newShape.Substrokes.Length)
                throw new Exception("Substrokes count should equal new shapes count");

			newShape.XmlAttrs.Name   = "shape";
			newShape.XmlAttrs.Type   = label;
			newShape.XmlAttrs.Id	  = System.Guid.NewGuid();
			newShape.XmlAttrs.Source = "Cluster.convertToNewShape";

			return newShape;
		}

		/// <summary>
		/// Merges all of the substrokes to be children of one shape. Note that this will
		/// only really work well if the shapes have never been merged before.
		/// </summary>
		/// <returns>A pointer to the resulting shape (which is already in the sketch)</returns>
		public Shape consolidateShapes()	
		{
			// Trying to avoid messy casts.
			Substroke[] finalStrokes = substrokes.ToArray();
            int len = finalStrokes.Length;
			for(int i = 1; i < len; ++i)	
			{
				// We've guaranteed that each substroke has only one parent
				mergeShapes(finalStrokes[0].ParentShapes[0], finalStrokes[i].ParentShapes[0]);
			}

			return finalStrokes[0].ParentShapes[0];
		}

		/// <summary>
		/// Merges one shape into another. The second shape is removed in the process.
		/// </summary>
		/// <param name="s1">The first shape (will remain)</param>
		/// <param name="s2">The second shape (will be deleted)</param>
		private void mergeShapes(Shape s1, Shape s2)	
		{
			// check for same shape
			if(s1.Equals(s2))	
			{
				Console.WriteLine("WARNING: Trying to merge the same shape!");
				return;
			}
			// We're assuming that the shapes have no child shapes

			Substroke[] s2Substrokes = s2.Substrokes;
			sketch.RemoveShape(s2);

			for (int i=0; i<s2Substrokes.Length; i++)	
			{
				s1.AddSubstroke(s2Substrokes[i]);
			}
			Substroke[] s1Substrokes = s1.Substrokes;
			s1.XmlAttrs.End = s1Substrokes[s1Substrokes.Length-1].XmlAttrs.Id;
		}
		#endregion
	}
}
