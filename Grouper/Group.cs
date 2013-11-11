using System;
using System.Collections.Generic;
using System.Text;
using Sketch;

namespace Grouper
{
    /// <summary>
    /// Interface that all Groupers should implement
    /// </summary>
    public interface IGroup
    {
        void group();
        void group(bool verbose);
    }

    /// <summary>
    /// Abstract class is framework for derived children.
    /// This class serves as a general frame for grouping.
    /// </summary>
    public abstract class Group : IGroup
	{
		#region COMMON INTERNALS
		protected bool verbose;
		protected List<string> labels;

		protected List<LabelGroup> labelGroups;

		/// <summary>
		/// A shortcut to the "unlabeled" Label Group.
		/// </summary>
		protected LabelGroup unlabeledLabelGroup;

        /// <summary>
        /// Sketch
        /// </summary>
        protected Sketch.Sketch sketch;

		#endregion

		/// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sketch"></param>
        public Group(Sketch.Sketch sketch)
        {
            this.sketch = sketch;
        }

        /// <summary>
        /// Do the grouping
        /// </summary>
        public virtual void group()
        {
            group(false);
        }

        /// <summary>
        /// Do the grouping
        /// </summary>
        /// <param name="verbose">Debug if true</param>
        public abstract void group(bool verbose);

		#region COMMON UTILITY FUNCTIONS
		/// <summary>
		/// Separates the sketch's subgroups into smaller groups based on their shape's label
		/// (all the GATEs together, all the WIREs together, etc.)
		/// These are stored in labelGroups, a Collection of LabelGroups.
		/// </summary>
		protected void separateSubstrokes()
		{
			if (verbose)
			{
				foreach (Shape shape in sketch.ShapesL)
				{
					Console.WriteLine("Looking at an {0} with GUID {1}", shape.Type, shape.XmlAttrs.Id);
					Console.WriteLine("\tSubstrokes:");
					foreach (Substroke ss in shape.Substrokes)
					{
						Console.WriteLine("\t{0}", ss.XmlAttrs.Id);
					}
				}
			}

			// We go through shapes in the vain hope that some substrokes
			// might be un-shaped, and thus we might save some time. Maybe...
			// Plus, then the thing with the Find is O(number of shape)
			// instead of O(number of strokes), which may be significant
			foreach (Shape shape in sketch.Shapes)
			{
				string shapeLabel = shape.Type;
				// the following line is magic, because I like anonymous functions
				LabelGroup lg = labelGroups.Find(delegate(LabelGroup s) { return shapeLabel.ToLower().Equals(s.label.ToLower()); });
				// TODO: Set lg to unlabeledLabelGroup if probability is too low
				if (lg == null)
				{
					Console.WriteLine("Warning: Substroke with confusing non-standard label encountered. Changing to \"unlabeled\"");
					Console.WriteLine("Offending label was " + shapeLabel);
					lg = unlabeledLabelGroup;
				}
				foreach (Substroke sub in shape.SubstrokesL)
				{
					lg.addSubstroke(sub);
				}
			}
		}

		protected void destroyAllShapes()
		{
			List<Shape> shapes = new List<Shape>(sketch.Shapes);

			sketch.RemoveShapes(shapes);
		}

		protected void buildShapes(LabelGroup labelgroup)
		{
			// for every cluster, make a new shape
			foreach (Cluster cls in labelgroup.clusters)
			{
				sketch.AddShape(cls.convertToNewShape());
			}
		}

		#endregion
	}
}
