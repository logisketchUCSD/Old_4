using System;
using System.Collections.Generic;
using System.Text;

namespace Grouper
{
    internal class K_means : Group
    {
        /// <summary>
        /// must be grouped into K shapes
        /// </summary>
        private int K;

        /// <summary>
        /// constructor for K-means
        /// </summary>
        /// <param name="sketch">the sketch we want to analyze</param>
        /// <param name="K">number of shapes we want the substrokes grouped into</param>
        public K_means(Sketch.Sketch sketch, int K) : base(sketch)
		{
            this.K = K;
        }

        /// <summary>
        /// non-debugging grouping
        /// </summary>
        public override void group()	
		{
            group(false);
		}

        /// <summary>
        /// the overall loop that generates centroids and matches the strokes
        /// to the centroids until there is no movement of centroids
        /// </summary>
        /// <param name="verbose">true for debugging mode</param>
        public override void group(bool verbose)
        {
            Sketch.Point[] tempPoints = initializePoints();
            List<List<Sketch.Substroke>> assignments = assignToPoints(tempPoints);

            bool breaking = false;

            while (breaking == false)
            {
                Sketch.Point[] centroids = findCentroids(assignments);
                
                if (isEqual(centroids, tempPoints))
                {
                    breaking = true;
                }
                else
                {
                    assignments = assignToPoints(centroids);
                    tempPoints = centroids;
                }
            }

            for (int i = 0; i < this.K; i++)
            {
                Sketch.Shape sh = new Sketch.Shape();
                List<Sketch.Substroke> subs = assignments[i];

                for (int j = 0; j < subs.Count; j++)
                {
                    sh.AddSubstroke(subs[j]);
                }

                this.sketch.AddShape(sh);
//Console.WriteLine("k-mean numshapes: " + this.sketch.Shapes.Length);
            }
        }

        /// <summary>
        /// create a list of K random points
        /// </summary>
        /// <returns>the list of points</returns>
        public Sketch.Point[] initializePoints()
        {
            Sketch.Point[] centroids = new Sketch.Point[this.K];
            for (int i = 0; i < this.K; i++)
            {
                Sketch.Point p = new Sketch.Point();
                p.XmlAttrs.X = this.sketch.Substrokes[i].XmlAttrs.X.Value;
                p.XmlAttrs.Y = this.sketch.Substrokes[i].XmlAttrs.Y.Value;
                centroids[i] = p;
//Console.WriteLine("point[" + i + "] x,y: " + p.XmlAttrs.X + ", " + p.XmlAttrs.Y);
            }

            return centroids;
        }

        /// <summary>
        /// find the new centroids based on which strokes have been assigned to them
        /// </summary>
        /// <param name="assignments">the 2D list of current groupings</param>
        /// <returns>a list of points (centroids)</returns>
        public Sketch.Point[] findCentroids(List<List<Sketch.Substroke>> assignments)
        {
            Sketch.Point[] centroids = new Sketch.Point[this.K];

            for (int i = 0; i < this.K; i++)
            {
                List<Sketch.Substroke> shape = assignments[i];
                int len = shape.Count;
                double sumX = 0;
                double sumY = 0;

                for (int j = 0; j < len; j++)
                {
                    sumX += shape[j].XmlAttrs.X.Value;
                    sumY += shape[j].XmlAttrs.Y.Value;
                }

                Sketch.Point p = new Sketch.Point();
                p.XmlAttrs.X = (float)(sumX / shape.Count);
                p.XmlAttrs.Y = (float)(sumY / shape.Count);

                centroids[i] = p;
//Console.WriteLine("point[" + i + "] x,y: " + p.XmlAttrs.X + ", " + p.XmlAttrs.Y);
            }

            return centroids;
        }

        /// <summary>
        /// assigns the strokes in the sketch to the centroids (based on distance)
        /// </summary>
        /// <param name="points">the list of centroids</param>
        /// <returns>a 2D list of substoke assignments</returns>
        public List<List<Sketch.Substroke>> assignToPoints(Sketch.Point[] points)
        {
            List<List<Sketch.Substroke>> assignments = new List<List<Sketch.Substroke>>();

            // initialize the 2D list
            for (int m = 0; m < this.K; m++)
            {
                List<Sketch.Substroke> shape = new List<Sketch.Substroke>();
                assignments.Add(shape);
            }

            Sketch.Substroke[] subs = this.sketch.Substrokes;
            int len = subs.Length;

            for (int i = 0; i < len; i++)
            {
                int index = closest(subs[i], points);
                assignments[index].Add(subs[i]);
            }

            return assignments;
        }

        /// <summary>
        /// determines which centroid is closest to a given substroke
        /// </summary>
        /// <param name="sub">the substroke</param>
        /// <param name="points">the list of centroids</param>
        /// <returns>the index of the closest centroid</returns>
        public int closest(Sketch.Substroke sub, Sketch.Point[] points)
        {
            double small = double.PositiveInfinity;
            int index = 0;

            for (int i = 0; i < this.K; i++)
            {
                Sketch.Point subPoint = new Sketch.Point();
                subPoint.XmlAttrs.X = sub.XmlAttrs.X;    // This is the x value of a substroke... what does that mean?
                subPoint.XmlAttrs.Y = sub.XmlAttrs.Y;
                double dist = distance(points[i], subPoint);

                if (dist < small)
                {
                    small = dist;
                    index = i;
                }
            }

            return index;
        }

        /// <summary>
        /// returns the distance between two points
        /// </summary>
        /// <param name="p1">point 1</param>
        /// <param name="p2">point 2</param>
        /// <returns>distance between points</returns>
        public double distance(Sketch.Point p1, Sketch.Point p2)
        {
            double xDiff = p1.XmlAttrs.X.Value - p2.XmlAttrs.X.Value;
            double yDiff = p1.XmlAttrs.Y.Value - p2.XmlAttrs.Y.Value;

            return Math.Sqrt(xDiff * xDiff + yDiff * yDiff);
        }

        /// <summary>
        /// compares two arrays of points based on x,y coordinates of the points
        /// </summary>
        /// <param name="points1">the first array of points</param>
        /// <param name="points2">the second array of points</param>
        /// <returns>true if arrays are equal</returns>
        public bool isEqual(Sketch.Point[] points1, Sketch.Point[] points2)
        {
            if (points1.Length != points2.Length)
            {
                return false;
            }
            else
            {
                int len = points1.Length;

                for (int i = 0; i < len; i++)
                {
                    if ((int)points1[i].XmlAttrs.X.Value != (int)points2[i].XmlAttrs.X.Value || (int)points1[i].XmlAttrs.Y.Value != (int)points2[i].XmlAttrs.Y.Value)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }

    /// <summary>
    /// compares substrokes by their x-coordinate (sorts low to high)
    /// </summary>
    internal class SubstrokeComparerX : IComparer<Sketch.Substroke>
    {
        public int Compare(Sketch.Substroke s1, Sketch.Substroke s2)
        {
            return Math.Sign(s1.XmlAttrs.X.Value - s2.XmlAttrs.X.Value);
        }
    }

    /// <summary>
    /// compares shapes by their y-coordinate (sorts low to high)
    /// </summary>
    internal class SubstrokeComparerY : IComparer<Sketch.Substroke>
    {
        public int Compare(Sketch.Substroke s1, Sketch.Substroke s2)
        {
            return Math.Sign(s1.XmlAttrs.Y.Value - s2.XmlAttrs.Y.Value);
        }
    }
}
