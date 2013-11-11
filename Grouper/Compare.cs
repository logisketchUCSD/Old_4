using System;
using System.Collections;

namespace Grouper
{
    // Created by devin so stuff compiles.
    // Appears to be referenced in a different project
    // in this solution. If you can find where please document it here.
    public class Compare
    {
        public static int groupingDifferences(Sketch.Sketch a, Sketch.Sketch b)
        {
            return 0;
        }
    }
	/*
	/// <summary>
	/// Compares spatial distances only
	/// </summary>
	public class SimpleDistanceCompare : ICompare
	{
		int IComparer.Compare( Object x, Object y )  
		{
			return (int)(((Distance)x).s_dist - ((Distance)x).s_dist);
		}
	}

	/// <summary>
	/// Compares the differences in both time and space.
	/// Time deltas are normalized so as to have equal weight with space deltas.
	/// </summary>
	public class SimpleTimeDistanceCompare : ICompare	
	{
		int IComparer.Compare(Object x, Object y)	
		{
			return 0;
		}
	}*/
}
