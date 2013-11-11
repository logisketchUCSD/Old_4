using System;
using System.Collections.Generic;
using System.Text;

namespace Grouper
{
    /// <summary>
    /// This class performs truth table specific grouping
    /// </summary>
    public class TTGroup : Group, IGroup
    {
        #region INTERNALS

        /// <summary>
        /// for determining how close dividers need to be so that they are grouped together
        /// </summary>
        private static float VERTICAL_DISTANCE_TOLERANCE = 30;

        /// <summary>
        /// for determining how close dividers need to be so that they are grouped together
        /// </summary>
        private static float HORIZONTAL_DISTANCE_TOLERANCE = 30;

        /// <summary>
        /// for determining how close dividers need to be so that they are grouped together
        /// </summary>
        private static float ABSOLUTE_DISTANCE_TOLERANCE = 30;

        /// <summary>
        /// for determining orientation thresholds for dividers
        /// </summary>
        private static float VERTICAL_ANGLE_THRESHOLD = 65; //45 + 20

        /// <summary>
        /// for determining orientation thresholds for dividers
        /// </summary>
        private static float HORIZONTAL_ANGLE_THRESHOLD = 15; //45 - 30

        /// <summary>
        /// for determining whether or not a potential divider segment is straight
        /// </summary>
        private static float AVERAGE_CURVATURE_THRESHOLD = 0.5f;

        /// <summary>
        /// the horizontal divider (shape)
        /// </summary>
        private Sketch.Shape horizontal = null;

        /// <summary>
        /// the vertical divider (shape)
        /// </summary>
        private Sketch.Shape vertical = null;

        /// <summary>
        /// turns output statements on and off
        /// </summary>
        private bool DEBUG = false;

        # endregion

        # region GETTERS/SETTERS

        /// <summary>
        /// get/set the vertical divider
        /// </summary>
        public Sketch.Shape horizontalDiv
        {
            get
            {
                return this.horizontal;
            }
            set
            {
                this.horizontal = value;
            }
        }

        /// <summary>
        /// get/set the horizontal divider
        /// </summary>
        public Sketch.Shape verticalDiv
        {
            get
            {
                return this.vertical;
            }
            set
            {
                this.vertical = value;
            }
        }

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// constructor that just takes in the sketch
        /// </summary>
        /// <param name="sketch"></param>
        public TTGroup(Sketch.Sketch sketch) : base(sketch)
        {
        }

        #endregion

        #region GROUPING

        /// <summary>
        /// group the truth table's strokes
        /// </summary>
        public override void group(bool verbose)
        {
            group();
        }

        /// <summary>
        /// group the truth table by first removing the dividers
        /// </summary>
        /// <param name="verbose">if true, prints out stats</param>
        public new Sketch.Sketch[] group()
        {
            // find the dividers
            horizontalDiv = horizontalDivider();
            verticalDiv = verticalDivider();

            // remove the dividers (so they aren't grouped)
            sketch.RemoveShapeAndSubstrokes(horizontalDiv);
            sketch.RemoveShapeAndSubstrokes(verticalDiv);

            // create the label sketch, which will be grouped separately
            Sketch.Sketch labelSketch = new Sketch.Sketch();
            labelSketch.XmlAttrs.Id = System.Guid.NewGuid();

            // create the temp sketch to store data
            Sketch.Sketch tempSketch = new Sketch.Sketch();
            tempSketch.XmlAttrs.Id = System.Guid.NewGuid();

            Sketch.Substroke[] subs = sketch.Substrokes;
            int len = subs.Length;

            for (int i = 0; i < len; i++)
            {   
                // remove small strokes
                if (subs[i].Points.Length < 9)
                {
                    sketch.RemoveSubstroke(subs[i]);
                }

                if (subs[i].XmlAttrs.Y.Value < horizontalDiv.XmlAttrs.Y.Value)
                {
                    // remove the labels
                    Sketch.Stroke st = new Sketch.Stroke(subs[i]);
                    st.XmlAttrs.Type = "shape";
                    st.XmlAttrs.Name = "shape";
                    st.XmlAttrs.Id = System.Guid.NewGuid();
                    labelSketch.AddStroke(st);
                }
                else
                {
                    Sketch.Stroke newStroke = new Sketch.Stroke(subs[i]);
                    newStroke.XmlAttrs.Type = "shape";
                    newStroke.XmlAttrs.Name = "shape";
                    newStroke.XmlAttrs.Id = System.Guid.NewGuid();
                    tempSketch.AddStroke(newStroke);
                }
            }

            // use naive grouping on the rest of the sketch
            NHGroup naive = new NHGroup(tempSketch);
            naive.group();

            if (DEBUG)
            {
                naive.printStatistics();
                ConverterXML.MakeXML newXml = new ConverterXML.MakeXML(tempSketch);
                newXml.WriteXML("000.xml");
            }

            // add the dividers back in
            sketch.AddShape(horizontalDiv);
            sketch.AddShape(verticalDiv);

            return new Sketch.Sketch[] { tempSketch, labelSketch };
        }

        /*
        /// <summary>
        /// group the labels separately
        /// </summary>
        public void groupLables(Sketch.Sketch labelSketch, int numCols)
        {

//Console.WriteLine("number of label substrokes: " + labelSketch.Substrokes.Length);
            
            // create a list of substrokes
            List<Sketch.Substroke> labels = new List<Sketch.Substroke>();
            for (int i = 0; i < labelSketch.Substrokes.Length; i++)
            {
                labels.Add(labelSketch.Substrokes[i]);
            }

            // group the labels
            // adjust the distance threshold for the number of columns

            float numShapes = float.PositiveInfinity;
            int k = 1;

//Console.WriteLine("number of cols in grouper: " + numCols);

            while (numCols < numShapes)
            {
                labelSketch.RemoveShapes();

                LabelGroup newLabelGroup = new LabelGroup("Label", labels, INITIAL_DISTANCE_THRESHOLD*k);
                NHGroup nhGrouper = new NHGroup(labelSketch);
                nhGrouper.hierarchicalCluster(newLabelGroup);
                k++;
                numShapes = labelSketch.Shapes.Length;

//Console.WriteLine("number of label shapes: " + numShapes);

            }

            // add the labels back in
            for (int i = 0; i < labelSketch.Shapes.Length; i++)
            {
                this.sketch.AddShape(labelSketch.Shapes[i]);
            }
        }
        

        
        /// <summary>
        /// alternate label grouping method using k-means
        /// </summary>
        /// <param name="labelSketch">the label sketch</param>
        /// <param name="numCols">the number of columns</param>
        public void groupLables(Sketch.Sketch labelSketch, int numCols)
        {
            K_means km = new K_means(labelSketch, numCols);
Console.WriteLine("number of k-mean cols: " + numCols);
            km.group();

            // add the labels back in
            for (int i = 0; i < labelSketch.Shapes.Length; i++)
            {
                this.sketch.AddShape(labelSketch.Shapes[i]);
            }
        }
        */

        #endregion

        #region FIND DIVIDERS

        /// <summary>
        /// finds the index of longest vertical stroke
        /// </summary>
        /// <returns>the index of longest vertical stroke</returns>
        private int dividerY()
        {
            int index = -1;
            float maxHeight = float.NegativeInfinity;

            Sketch.Substroke[] subs = this.sketch.Substrokes;
            int len = this.sketch.Substrokes.Length;

            for (int i = 0; i < len; ++i)
            {
                float h = subs[i].XmlAttrs.Height.Value;
                if (h > maxHeight)
                {
                    maxHeight = h;
                    index = i;
                }
            }

            return index;
        }

        /// <summary>
        /// finds the vertical divider in a truth table and groups it with other strokes
        /// that are part of it. then adds this entire shape to the sketch
        /// </summary>
        /// <returns>the vertical divider shape</returns>
        public Sketch.Shape verticalDivider()
        {
            Sketch.XmlStructs.XmlShapeAttrs newXML = new Sketch.XmlStructs.XmlShapeAttrs(true);
            newXML.Name = "shape";
            newXML.Type = "Divider";

            // check to see if the vertical divider has already been created
            if (this.verticalDiv == null)
            {
                int index = dividerY();
                this.verticalDiv = new Sketch.Shape(new List<Sketch.Shape>(), new List<Sketch.Substroke>(), newXML);
                Sketch.Substroke[] subs = this.sketch.Substrokes;

                Sketch.Substroke vertical = subs[index];

//Console.WriteLine("VERTICAL INDEX: " + index);

                Sketch.Substroke sub;

                // go through the substrokes and see which ones should be
                // added to the vertical divider
                int len = subs.Length;
                for (int i = 0; i < len; ++i)
                {
                    sub = subs[i];

                    float dist = Featurefy.Distance.minXDistance(sub, vertical);
                    float absoluteDist = Featurefy.Distance.minimumDistance(sub, vertical);

//Console.WriteLine("substroke[" + i + "] min dist to vert div: " + dist + " vertical? " + isVertical(sub) + " x,y " + sub.XmlAttrs.X + ", " + sub.XmlAttrs.Y);
//Console.WriteLine("abso. dist: " + absoluteDist + "  height: " + sub.XmlAttrs.Height.Value + " points: " + sub.Points.Length);

                    if (((dist < TTGroup.VERTICAL_DISTANCE_TOLERANCE || absoluteDist < TTGroup.ABSOLUTE_DISTANCE_TOLERANCE) && isVertical(sub)) || sub.Equals(vertical))
                    { 
                        // add substroke to the vertical divider shape
                        sub.XmlAttrs.Type = "Divider";
                        verticalDiv.AddSubstroke(sub);
                    }
                }

                verticalDiv.XmlAttrs.Type = "Divider";
                this.sketch.AddLabel(new List<Sketch.Substroke>(verticalDiv.SubstrokesL), "Divider");
                this.sketch.AddShape(verticalDiv);
            }

            return verticalDiv;
        }

        /// <summary>
        /// finds the index of longest horizontal stroke
        /// </summary>
        /// <returns>index of the longest horizontal stoke</returns>
        private int dividerX()
        {
            int index = -1;
            float maxWidth = float.NegativeInfinity;

            Sketch.Substroke[] subs = this.sketch.Substrokes;

            int len = this.sketch.Substrokes.Length;
            for (int i = 0; i < len; ++i)
            {
                float w = subs[i].XmlAttrs.Width.Value;
                if (w > maxWidth)
                {
                    maxWidth = w;
                    index = i;
                }
            }

//Console.WriteLine("horizontal index: " + index);

            return index;
        }

        /// <summary>
        /// finds the horizontal divider in a truth table and groups it with other strokes
        /// that are part of it. then adds this entire shape to the sketch
        /// </summary>
        /// <returns>the horizontal divider shape</returns>
        public Sketch.Shape horizontalDivider()
        {
            Sketch.XmlStructs.XmlShapeAttrs newXML = new Sketch.XmlStructs.XmlShapeAttrs(true);
            newXML.Name = "shape";
            newXML.Type = "Divider";

            // check to see if the horizontal divider has already been created
            if (this.horizontalDiv == null)
            {
                int index = dividerX();
                this.horizontalDiv = new Sketch.Shape(new List<Sketch.Shape>(), new List<Sketch.Substroke>(), newXML);
                Sketch.Substroke[] subs = this.sketch.Substrokes;

                Sketch.Substroke horizontal = subs[index];

//Console.WriteLine("HORIZONTAL INDEX: " + index);

                Sketch.Substroke sub;

                int len = subs.Length;
                for (int i = 0; i < len; ++i)
                {
                    sub = subs[i];
                    float dist = Featurefy.Distance.minYDistance(sub, horizontal);
                    float absoluteDist = Featurefy.Distance.minimumDistance(sub, horizontal);

//Console.WriteLine("substroke[" + i + "] min dist to horizontal div: " + dist + " horizontal? " + isHorizontal(sub) + " x,y " + sub.XmlAttrs.X + ", " + sub.XmlAttrs.Y);
//Console.WriteLine("angle: " + Math.Abs(angle(sub)));
//Console.WriteLine("width: " + sub.XmlAttrs.Width);
//Console.WriteLine("num points: " + sub.Points.Length);

                    if ((dist < TTGroup.HORIZONTAL_DISTANCE_TOLERANCE || absoluteDist < TTGroup.ABSOLUTE_DISTANCE_TOLERANCE) && isHorizontal(sub))
                    {
                        // make sure the stroke is relatively horizontal
                        if (sub.XmlAttrs.Width.Value / sub.XmlAttrs.Height.Value > 1)
                        {
                            sub.XmlAttrs.Type = "Divider";
                            horizontalDiv.AddSubstroke(sub);
                        }
                    }
                }

                horizontalDiv.XmlAttrs.Type = "Divider";
                this.sketch.AddLabel(new List<Sketch.Substroke>(horizontalDiv.Substrokes), "Divider");
                this.sketch.AddShape(horizontalDiv);
            }

            return this.horizontalDiv;
        }

        /// <summary>
        /// checks to see whether a substroke is vertical (and thus part of a divider)
        /// </summary>
        /// <param name="sub">substroke to be analyzed</param>
        /// <returns>true if stroke is vertical</returns>
        private bool isVertical(Sketch.Substroke sub)
        {
            if (Math.Abs(angle(sub)) > TTGroup.VERTICAL_ANGLE_THRESHOLD)
            {
                Featurefy.ArcLength arc = new Featurefy.ArcLength(sub.Points);
                Featurefy.Slope slope = new Featurefy.Slope(sub.Points);
                Featurefy.Curvature curvature = new Featurefy.Curvature(sub.Points, arc.Profile, slope.TanProfile);
                return curvature.AverageCurvature < TTGroup.AVERAGE_CURVATURE_THRESHOLD;
            }
            return false;
        }

        /// <summary>
        /// checks to see whether a substroke is horizontal (and thus part of a divider)
        /// </summary>
        /// <param name="sub">substroke to be analyzed</param>
        /// <returns>true if stroke is horizontal</returns>
        private bool isHorizontal(Sketch.Substroke sub)
        {
            if (Math.Abs(angle(sub)) < TTGroup.HORIZONTAL_ANGLE_THRESHOLD)
            {
                Featurefy.ArcLength arc = new Featurefy.ArcLength(sub.Points);
                Featurefy.Slope slope = new Featurefy.Slope(sub.Points);
                Featurefy.Curvature curvature = new Featurefy.Curvature(sub.Points, arc.Profile, slope.TanProfile);
                return curvature.AverageCurvature < TTGroup.AVERAGE_CURVATURE_THRESHOLD;
            }
            return false;
        }

        /// <summary>
        /// Returns the angle in degrees
        /// </summary>
        /// <param name="sub">substroke to be analyzed</param>
        /// <returns>angle of substroke</returns>
        private double angle(Sketch.Substroke sub)
        {
            int len = sub.Length;

            Sketch.Point p1 = sub.Points[0];
            Sketch.Point p2 = sub.Points[len - 1];

            int x = Convert.ToInt32(p2.X) - Convert.ToInt32(p1.X);
            //Use a -Y since we are not in cartiesian
            int y = Convert.ToInt32(p1.Y) - Convert.ToInt32(p2.Y);

            //Argument takes y then x
            if (x == 0)
                return 90.0;
            else
                return Math.Atan((double)y / (double)x) * 180 / Math.PI;
        }

        #endregion
    }
}
