using System;
using System.Collections.Generic;
using System.Text;

using ConverterXML;
using Sketch;
using Featurefy;

using Set;
using StrokeScanner;

namespace Grouper
{
    class Overview
    {

        #region Color List
        /// <summary>
        /// List of colors
        /// </summary>
        private static readonly string[] colorList = new string[]
			{	"Blue", "Crimson", "Green", "Purple", "Orange", "Pink", 
                "PowderBlue", "Salmon", "MediumAquamarine", 
				"Yellow", "Aqua", "Chartreuse", 
                "BlueViolet", "BurlyWood", 
                "CadetBlue", "Aquamarine", "Chocolate", "Coral", 
                "Cornflower", "Brown", "Cyan", "DarkBlue", 
                "DarkCyan", "DarkGoldenrod", "DarkGray", "DarkGreen",
                "DarkKhaki", "DarkMagenta", "DarkOliveGreen", "DarkOrange", 
                "DarkOrchid", "DarkRed", "DarkSalmon", "DarkSeaGreen", 
                "DarkSlateBlue", "DarkSlateGray", "DarkTurquoise", 
                "DarkViolet", "DeepPink", "DeepSkyBlue", "DimGray",
		        "DodgerBlue", "Firebrick", "FloralWhite", "ForestGreen",
			    "Fuchsia", "Gainsboro",	"GhostWhite", "Gold", "Goldenrod",
			    "Gray", "GreenYellow", "Honeydew",	"HotPink",
			    "IndianRed", "Indigo", "Ivory", "Khaki", "Lavender",
			    "LavenderBlush", "LawnGreen", "LemonChiffon", "LightBlue",
			    "LightCoral", "LightCyan", "LightGoldenrodYellow",
			    "LightGray", "LightGreen", "LightPink", "LightSalmon",
			    "LightSeaGreen", "LightSkyBlue", "LightSlateGray", 
                "LightSteelBlue", "LightYellow", "Lime", "LimeGreen", 
                "Linen", "Magenta", "Maroon", 
                "MediumBlue", "MediumOrchid", "MediumPurple", 
                "MediumSeaGreen", "MediumSlateBlue", "MediumSpringGreen", 
                "MediumTurquoise", "MediumVioletRed", "MidnightBlue", 
                "MintCream", "Moccasin", "NavajoWhite", "Navy", 
                "OldLace", "Olive", "OliveDrab", "OrangeRed", 
                "Orchid", "PaleGoldenrod", "PaleGreen", "PaleTurquoise", 
                "PaleVioletRed", "PapayaWhip", "PeachPuff", "Peru", 
                "Plum", "Red", "RosyBrown", 
                "RoyalBlue", "SaddleBrown", "SandyBrown", 
                "SeaGreen", "SeaShell", "Sienna", "Silver", "SkyBlue", 
                "SlateBlue", "SlateGray", "Snow", "SpringGreen", "SteelBlue", 
                "Tan", "Teal", "Thistle", "Tomato", "Transparent", 
                "Turquoise", "Violet", "WhiteSmoke", "YellowGreen"};
        #endregion

        [STAThread]
        public static void Main(string[] args)
        {
            writeDomainFile("overview.txt", 40);
            ReadXML rxml = new ReadXML("1234_3.5.1.labeled.xml");
            Sketch.Sketch s = rxml.Sketch;

            //new Overview().temporalAbsThreshold(s.Clone());
            //new Overview().temporalRelThreshold(s.Clone());
            //new Overview().CurvedThings(s.Clone());
            new Overview().TJunctions(s.Clone());
        }

        public void TJunctions(Sketch.Sketch sketch)
        {

            List<Substroke> sl = sketch.SubstrokesL;

            foreach (Substroke s1 in sl)
            {
                foreach (Substroke s2 in sl)
                {
                    if (s1 == s2) continue;

                    Scanner s = new Scanner(StrokeScanner.StrokeScanner.minRectBetweenStrokes(s1, s2));
                    s.testAndAdd(s1);
                    s.testAndAdd(s2);

                    if (s.ScanGroup.Count < 2) continue;

                    Substroke back = s.tJunction();
                    if (back != null)
                    {
                        //FeatureStroke fs = new FeatureStroke(back);
                        s.ClusterLabel = 1;
                        s.addToSketch(ref sketch);
                    }

                }
            }

            Console.WriteLine("tjuncts done");
            MakeXML mxml = new MakeXML(sketch);
            mxml.WriteXML("t_junctionss.xml");

        }

        /// <summary>
        /// pure temporal grouping based on local pause time maxima.  uses absolute threshold to
        /// eliminate noise.
        /// </summary>
        /// <param name="sketch"></param>
        public void temporalAbsThreshold(Sketch.Sketch sketch)
        {

            for (int thresh = 50; thresh < 150; thresh += 25)
            {

                Sketch.Sketch temp = sketch.Clone();
                temp.RemoveLabelsAndGroups();

                List<Substroke> subs = temp.SubstrokesL;
                List<Substroke> group = new List<Substroke>();
                group.Add(subs[0]);
                int gcntr = 0;

                for (int i = 1; i < subs.Count - 1; i++)
                {
                    if (timeGap(subs[i - 1], subs[i]) > timeGap(subs[i], subs[i + 1]) + thresh)
                    {
                        addGroup(group, gcntr++, ref temp);
                        group = new List<Substroke>();
                    }
                    else if (minDistBetweenSubs(subs[i - 1], subs[i]) > 100)
                    {
                        addGroup(group, gcntr++, ref temp); 
                        group = new List<Substroke>();
                    }
                    group.Add(subs[i]);
                }

                if (subs.Count > 3 &&
                    timeGap(subs[subs.Count - 2], subs[subs.Count - 1]) >
                    timeGap(subs[subs.Count - 3], subs[subs.Count - 2]) + thresh)
                {
                    addGroup(group, gcntr++, ref temp);
                    group = new List<Substroke>();
                }
                group.Add(subs[subs.Count - 1]);

                addGroup(group, gcntr++, ref temp);

                Console.WriteLine("absolute temporal {0}: {1} groups found", thresh, gcntr);
                MakeXML mxml = new MakeXML(temp);
                mxml.WriteXML("overview_abs_thresh_" + thresh + ".xml");
            }

        }

        /// <summary>
        /// pure temporal grouping based on local pause time maxima.  uses a relative threshold to
        /// eliminate noise.
        /// </summary>
        /// <param name="sketch"></param>
        public void temporalRelThreshold(Sketch.Sketch sketch)
        {

            double[] percents = new double[] {1.05, 1.1, 1.2, 1.25, 1.33, 1.5};

            foreach (double percent in percents)
            {

                Sketch.Sketch temp = sketch.Clone();
                temp.RemoveLabelsAndGroups();

                List<Substroke> subs = temp.SubstrokesL;
                List<Substroke> group = new List<Substroke>();
                group.Add(subs[0]);
                int gcntr = 0;

                for (int i = 1; i < subs.Count - 1; i++)
                {
                    if (timeGap(subs[i - 1], subs[i]) > timeGap(subs[i], subs[i + 1]) * percent)
                    {
                        addGroup(group, gcntr++, ref temp);
                        group = new List<Substroke>();
                    }
                    else if (minDistBetweenSubs(subs[i - 1], subs[i]) > 100)
                    {
                        addGroup(group, gcntr++, ref temp);
                        group = new List<Substroke>();
                    }
                    group.Add(subs[i]);
                }

                if (subs.Count > 3 &&
                    timeGap(subs[subs.Count - 2], subs[subs.Count - 1]) >
                    timeGap(subs[subs.Count - 3], subs[subs.Count - 2]) * percent)
                {
                    addGroup(group, gcntr++, ref temp);
                    group = new List<Substroke>();
                }
                group.Add(subs[subs.Count - 1]);

                addGroup(group, gcntr++, ref temp);
                Console.WriteLine("relative temporal {0}: {1} groups found", percent, gcntr);
                MakeXML mxml = new MakeXML(temp);
                mxml.WriteXML("overview_rel_thresh_" + percent + ".xml");
            }

        }

        private double minDistBetweenSubs(Substroke a, Substroke b)
        {
            double mind = double.PositiveInfinity;

            foreach (Point p in a.PointsL)
            {
                foreach (Point p2 in b.PointsL)
                {
                    double test = p.distance(p2);
                    if (test < mind) mind = test;
                }
            }

            return mind;
        }

        /// <summary>
        /// groups out of curved lines and the things attached to endpoints
        /// </summary>
        /// <param name="sketch"></param>
        public void CurvedThings(Sketch.Sketch sketch)
        {
            List<Substroke> ls = new List<Substroke>(sketch.SubstrokesL);

            bool found = true;
            int gcntr = 0;

            double thresh = Math.PI/4d;

            while (found)
            {
                found = false;

                for (int i = 0; i < ls.Count; i++)
                {
                    FeatureStroke fs = new FeatureStroke(ls[i]);
                    //List<Substroke> group = new List<Substroke>();
                    //group.Add(ls[i]);
                    //addGroup(group, fs.Curvature.TotalCurvature, ref sketch);
                    if (Math.Abs(fs.Curvature.TotalAngle) > thresh)
                    {
                        Console.WriteLine("curvey lines thresh exceeded");

                        Substroke oneEnd = closest(ls[i].PointsL[0], ls, ls[i]);
                        Substroke otherEnd = closest(ls[i].PointsL[ls[i].PointsL.Count - 1], ls, ls[i]);

                        List<Substroke> group = new List<Substroke>();
                        group.Add(ls[i]);
                        group.Add(oneEnd);
                        if (oneEnd != otherEnd) group.Add(otherEnd);

                        addGroup(group, gcntr++, ref sketch);

                        ls.Remove(ls[i]);
                        ls.Remove(oneEnd);
                        ls.Remove(otherEnd);
                        found = true;
                        break;
                    }
                }

            }



            Console.WriteLine("curved lines {0}: {1} groups found", thresh, gcntr);
            MakeXML mxml = new MakeXML(sketch);
            mxml.WriteXML("curved_lines.xml");
        }

        #region utilities
        /// <summary>
        /// Writes out a special domain file specific to this sketch. Also modifies the sketch so that
        /// all shape names are unique (this will break other code modules, but will allow you to view
        /// groupings in the new labeler).        
        /// </summary>
        /// <param name="filename"></param>
        private static void writeDomainFile(string filename, int numGroups)
        {
            System.IO.StreamWriter domain_writer = new System.IO.StreamWriter(filename, false);
            domain_writer.WriteLine("Clustering Research");
            domain_writer.WriteLine("Special debug domain file for " + filename);

            domain_writer.WriteLine(String.Format("Background {0} {1}", 0, colorList[0]));

            for (int i = 0; i < numGroups; ++i)
            {

                domain_writer.WriteLine(String.Format("Group{0} {1} {2}", i, i + 1,
                    colorList[(i + 1) % colorList.Length]));
            }
            domain_writer.Close();
        }

        /// <summary>
        /// Add a group to the sketch
        /// </summary>
        /// <param name="group"></param>
        /// <param name="gnum"></param>
        /// <param name="sketch"></param>
        private static void addGroup(List<Substroke> group, int gnum, ref Sketch.Sketch sketch)
        {
            // ignore empty groups
            if (group.Count == 0) return;
            
            Shape g = new Shape(group, new XmlStructs.XmlShapeAttrs(true));
            g.XmlAttrs.Type = String.Format("Group{0}", gnum);
            g.XmlAttrs.Name = "shape";
            g.XmlAttrs.Time = (ulong)DateTime.Now.Ticks;

            sketch.AddShape(g);
        }

        /// <summary>
        /// Time gap between a and b.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static double timeGap(Substroke a, Substroke b)
        {
            return b.PointsL[b.PointsL.Count - 1].Time - a.PointsL[0].Time;
        }

        /// <summary>
        /// guassian function from sezgin scale space paper
        /// </summary>
        /// <param name="i"></param>
        /// <param name="s"></param>
        /// <returns></returns>
        private static double g(double i, double s)
        {
            return (1d / (s * Math.Sqrt(2 * Math.PI))) * Math.Exp(-((i * i) / (s * s * 2.0)));
        }

        private static Substroke closest(Point x, List<Substroke> ls, Substroke exclude)
        {
            Substroke res = null;
            double mindist = double.MaxValue;

            foreach (Substroke s in ls)
            {
                if (s == exclude) continue;

                foreach (Point p in s.PointsL)
                {
                    double d = Math.Sqrt(Math.Pow(p.X - x.X, 2) + Math.Pow(p.Y - x.Y, 2));
                    if (d < mindist)
                    {
                        res = s;
                        mindist = d;
                    }
                }
            }

            return res;
        }
        #endregion

    }
}
