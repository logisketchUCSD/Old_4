using System;
using System.Collections.Generic;
using System.Threading;
using System.Drawing;
using Sketch;

namespace Grouper
{
    //      _.:::.                  
    //     /o''   '''./)
    //    >\.))...''' \)

    /// <summary>
    /// Different Grouping and Search algorithms
    /// </summary>
    public enum Algorithm { OLD = 0, NAIVE_H = 1, TRUTH_TABLE = 2, PROXIMITY = 3, TEMPORAL = 4, DEBUG = -1, NONE = -2 };

    /// <summary>
    /// Different Color Modes
    /// </summary>
    public enum ColorMode
    {
        /// <summary>
        /// Generate random colors from the 24-bit color spectrum
        /// </summary>
        RANDOM = 0,
        /// <summary>
        /// Use a pre-defined list of colors. (finite size)
        /// </summary>
        LIST = 1,
        /// <summary>
        /// Do not color strokes
        /// </summary>
        NONE = -1
    };

    /// <summary>
    /// Wrapper class for grouping labeled, fragmented strokes into meaningful groups.
    /// The CRF will tag each substroke with a specific label (e.g. "wire" or "gate").
    /// This class then examines the labeled strokes and tries to group appropriate
    /// substrokes into shapes. For example, an AND gate is probably made up of two
    /// substrokes, S1 and S2. Starting out, S1 is contained inside some shape
    /// H1 (and, moreover, is the only substroke in that shape), while S2 is in a different
    /// shape, H2. The grouper will (hopefully) consolidate them into a single shape, Hc
    /// which is itself tagged with the appropriate label ("AND" or "GATE" depending).
    /// 
    /// </summary>
    public class Grouper : IGroup, ISearch
    {
        #region CONSTANTS
        /// <summary>
        /// This will be our default grouping algorithm
        /// </summary>
        private const Algorithm DEFAULT_GROUP_ALGORITHM = Algorithm.NAIVE_H;

        /// <summary>
        /// Default searching algorithm
        /// </summary>
        private const Algorithm DEFAULT_SEARCH_ALGORITHM = Algorithm.NAIVE_H;

        /// <summary>
        /// Default coloring mode
        /// </summary>
        private const ColorMode DEFAULT_COLOR_MODE = ColorMode.RANDOM;

        /// <summary>
        /// List of colors
        /// </summary>
        private static readonly string[] colorList = new string[]		// can't be const b/c C# is silly
            {	"Blue", "Brown", "Green", "Purple", "Orange", "Pink", 
                "PowderBlue", "MistyRose", "Salmon", "MediumAquamarine", 
                "Yellow", "Wheat", "Aqua", "Aquamarine", 
                "BlueViolet", "BurlyWood", 
                "CadetBlue", "Chartreuse", "Chocolate", "Coral", 
                "Cornflower", "Crimson", "Cyan", "DarkBlue", 
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

        #region INTERNALS

        /// <summary>
        /// The sketch that we want to analyze.
        /// </summary>
        private Sketch.Sketch sketch;

        /// <summary>
        /// Which particular grouping algorithm to use
        /// </summary>
        private Algorithm groupAlgorithm = DEFAULT_GROUP_ALGORITHM;

        private Algorithm searchAlgorithm = DEFAULT_SEARCH_ALGORITHM;

        private bool domain_colored = false;

        private static Random rand = new Random();

        private int colorListCounter = 0;

        private bool grouped = false;

        #endregion

        #region CONSTRUCTORS

        public Grouper(Sketch.Sketch sketch)
        {
            this.sketch = sketch;

            Thread.Sleep(1);
            rand = new Random();
        }

        public Grouper(Sketch.Sketch sketch, Algorithm gAlgorithm, Algorithm sAlgorithm)
            : this(sketch)
        {
            this.groupAlgorithm = gAlgorithm;
            this.searchAlgorithm = sAlgorithm;
        }

        #endregion

        #region GET

        public Sketch.Sketch Sketch
        {
            get
            {
                return this.sketch;
            }
        }

        #endregion

        #region GROUPING

        /// <summary>
        /// Start grouping
        /// </summary>
        public void group()
        {
            group(false);
        }

        /// <summary>
        /// Start grouping
        /// </summary>
        /// <param name="verbose">Debug mode if verbose is true</param>
        public void group(bool verbose)
        {
            if (verbose)
                Console.WriteLine("Using {0} grouping algorithm...", methodToString(groupAlgorithm));

            switch (groupAlgorithm)
            {
                case Algorithm.OLD:
                    // Make sure that we start out with 1 substroke per shape
                    explodeShapes();

                    Segment.Segment testSegment = new Segment.Segment(sketch);
                    testSegment.randomColorConnectedComponents();
                    break;

                // The naive algorithm appears to cluster each label group (set of strokes in
                // the sketch that all have the same label) by repeatedly running a BFS search
                // over a graph induced on the label group by distance between strokes.  As much
                // as possible is added to each cluster, then the algorithm moves on.
                //
                // I wonder how well it works...?
                case Algorithm.NAIVE_H:
                    // Make sure that we start out with 1 substroke per shape
                    explodeShapes();

                    NHGroup naive = new NHGroup(sketch);
                    naive.group(verbose);
                    //colorSubstrokes();

                    if (verbose)
                    {
                        naive.printStatistics();
                    }
                    break;

                case Algorithm.TRUTH_TABLE:
                    // Make sure that we start out with 1 substroke per shape
                    explodeShapes();

                    TTGroup truth = new TTGroup(sketch);
                    truth.group(verbose);

                    break;

                //Stealing code from other cases, should be the same
                case Algorithm.PROXIMITY:
                    // Make sure that we start out with 1 substroke per shape
                    explodeShapes();

                    PGroup proximity = new PGroup(sketch);
                    proximity.group(verbose);

                    break;

                case Algorithm.TEMPORAL:
                    // Make sure that we start out with 1 substroke per shape
                    explodeShapes();

                    TemporalGroup temporal = new TemporalGroup(sketch);
                    temporal.group(verbose);
                    //colorSubstrokes();

                    if (verbose)
                    {
                        temporal.printStatistics();
                    }
                    break;

                case Algorithm.DEBUG:
                    // Make sure that we start out with 1 substroke per shape
                    explodeShapes();

                    // Do whatever we feel like here
                    Substroke[] substrokes = sketch.Substrokes;
                    for (int i = 0; i < substrokes.Length; i++)
                    {
                        substrokes[i].XmlAttrs.Color = System.Drawing.Color.Black.ToArgb();
                    }
                    group();
                    break;

                case Algorithm.NONE:
                    break;

                default:
                    Console.WriteLine("ERROR: Unrecognized grouping method requested.");
                    break;
            }
            this.grouped = true;
        }

        #endregion


        /// <summary>
        /// Writes out a special domain file specific to this sketch. Also modifies the sketch so that
        /// all shape names are unique (this will break other code modules, but will allow you to view
        /// groupings in the new labeler).
        /// </summary>
        /// <param name="filename"></param>
        public void writeDomainFile(string filename)
        {
            if (!this.domain_colored)
                colorShapes();

            // Get rid of xml extension if it's there
            if (filename.EndsWith(".xml"))
            {
                filename = filename.Substring(0, filename.Length - 4);
                filename += ".domain.txt";
            }

            System.IO.StreamWriter domain_writer = new System.IO.StreamWriter(filename, false);
            domain_writer.WriteLine("HMC Research 07");
            domain_writer.WriteLine("Special debug domain file for " + filename + ".xml.");

            for (int i = 0; i < Math.Min(colorList.Length, sketch.Shapes.Length); i++)
            {
                colorListCounter = (colorListCounter + 1) % colorList.Length;
                domain_writer.WriteLine("DebugGroup" + i + " " + i + " " + colorList[colorListCounter]);
            }
            domain_writer.Close();
        }


        #region SEARCHING

        /// <summary>
        /// Start searching
        /// </summary>
        public void search()
        {
            search(false);
        }

        /// <summary>
        /// Start searching
        /// </summary>
        /// <param name="verbose">Debug if verbose is true</param>
        public void search(bool verbose)
        {

            if (!this.grouped)
            {
                Console.Error.WriteLine("Must group before you can search...");
                return;
            }


            if (verbose)
                Console.WriteLine("Using {0} search algorithm...", methodToString(searchAlgorithm));

            ISearch search;

            switch (searchAlgorithm)
            {
                case Algorithm.OLD:
                    break;

                case Algorithm.NAIVE_H:
                    search = new CircuitSearch(sketch);
                    search.search(verbose);
                    break;

                case Algorithm.TRUTH_TABLE:
                    search = new TTSearch(sketch);
                    search.search(verbose);
                    break;

                //This should be identical to other cases, aside from the actual search functionality itself
                case Algorithm.PROXIMITY:
                    search = new ProximitySearch(sketch);
                    search.search(verbose);
                    break;

                case Algorithm.DEBUG:
                    break;

                case Algorithm.NONE:
                    break;

                //Temporal algorithm doesn't currently support Searching
                case Algorithm.TEMPORAL:
                    break;

                default:
                    Console.Error.WriteLine("ERROR: Unrecognized search method requested.");
                    break;
            }
        }

        #endregion

        #region COLOR

        /// <summary>
        /// FOR DEBUG PURPOSES ONLY. XML files generated from data constructed with this
        /// code are nonstandard and will not be understood by other pieces of the project.
        /// 
        /// Gives every shape a unique name (this field is normally "Wire", "Gate", etc.)
        /// Also generates an accompanying domain file that you can load into the Labeler
        /// and so have each group be a different color. Happy!
        /// </summary>
        public void colorShapes(ColorMode color_method)
        {
            Shape[] shapes = sketch.Shapes;
            int len = shapes.Length;
            for (int i = 0; i < len; i++)
            {
                //shapes[i].XmlAttrs.Name = "DebugGroup" + i;
                shapes[i].XmlAttrs.Type = "DebugGroup" + i;
                shapes[i].XmlAttrs.Color = generateColor(color_method).ToArgb();

            }
            domain_colored = true;
        }

        public void colorShapes()
        {
            colorShapes(DEFAULT_COLOR_MODE);
        }

        /// <summary>
        /// Sets the individual "color" xml attribute of each to reflect what group it
        /// belogs to. The old labeler will display this information automatically. If
        /// using the new labeler, generate a domain file.
        /// </summary>
        public void colorSubstrokes(ColorMode method)
        {
            if (method == ColorMode.NONE)
                return;

            Sketch.Shape[] shapes = sketch.Shapes;
            int len = shapes.Length;
            for (int i = 0; i < len; i++)
            {
                System.Drawing.Color color = generateColor(method);

                Sketch.Substroke[] substrokes = shapes[i].Substrokes;
                int len2 = substrokes.Length;
                for (int j = 0; j < len2; j++)
                    substrokes[j].XmlAttrs.Color = color.ToArgb();

                /*
                foreach (Sketch.Substroke substroke in substrokes)	
                {
                    substroke.XmlAttrs.Color = color.ToArgb();
                }
                 */
            }
        }

        public void colorSubstrokes()
        {
            colorSubstrokes(DEFAULT_COLOR_MODE);
        }

        /// <summary>
        /// Generates a color based on the method requested.
        /// </summary>
        /// <param name="method">The ColorMode to use</param>
        /// <example>generateColor(ColorMode.RANDOM)</example>
        public Color generateColor(ColorMode method)
        {
            switch (method)
            {
                case ColorMode.RANDOM:
                    return randomColor();

                case ColorMode.LIST:
                    return nextListColor();

                case ColorMode.NONE:
                    return Color.Black;

                default:
                    Console.WriteLine("WARNING: Nonstandard input passed to colorizeSubstrokes");
                    return randomColor();
            }
        }

        private Color randomColor()
        {
            Thread.Sleep(1);						// let timer advance some
            return Color.FromArgb(Grouper.rand.Next(255), Grouper.rand.Next(255), Grouper.rand.Next(255));
        }

        private Color nextListColor()
        {
            colorListCounter = (colorListCounter + 1) % colorList.Length;
            return Color.FromName(colorList[colorListCounter]);
        }

        #endregion

        #region STRUCTURE REARRANGEMENT
        /// <summary>
        /// Makes sure that each shape contains exactly one substroke. Technically, the output from the CRF
        /// is supposed to be in this form, but since I haven't been able to find any .xml files that actually
        /// match that description, we'll just be on the safe side...
        /// </summary>
        private void explodeShapes()
        {
            Sketch.Shape[] shapes = sketch.Shapes;
            int len = shapes.Length;

            List<Shape> newShapes = new List<Shape>(len);
            for (int i = 0; i < len; ++i)
            {
                // Disown any child shapes (we aren't allowing them in this representation)
                if (shapes[i].Shapes.Length > 0)
                {
                    shapes[i].RemoveShapes(new List<Shape>(shapes[i].Shapes));
                }

                // Explode multiple substrokes into separate shapes
                if (shapes[i].Substrokes.Length > 1)
                {
                    List<Substroke> extraSubstrokes = new List<Substroke>();

                    Sketch.Substroke[] shapestrokes = shapes[i].Substrokes;
                    for (int j = 1; j < shapestrokes.Length; j++)
                    {
                        Sketch.Substroke substroke = shapestrokes[j];
                        extraSubstrokes.Add(substroke);

                        // Create a new shape for the substroke
                        Sketch.Shape shape = new Shape();
                        shape.XmlAttrs = shapes[i].XmlAttrs.Clone();
                        shape.XmlAttrs.Id = System.Guid.NewGuid();
                        shape.XmlAttrs.Time = substroke.XmlAttrs.Time;
                        shape.XmlAttrs.Start = shape.XmlAttrs.End = substroke.XmlAttrs.Id;
                        shape.XmlAttrs.Source = "Grouper.explodeShape";
                        shape.AddSubstroke(substroke);

                        newShapes.Add(shape);
                    }
                    shapes[i].RemoveSubstrokes(extraSubstrokes);
                    //shapes[i].XmlAttrs.Time = shapestrokes[0].XmlAttrs.Time; this will unfortunately mess with the ordering off the sketch list
                    shapes[i].XmlAttrs.End = shapestrokes[0].XmlAttrs.Id;
                }
            }
            sketch.AddShapes(newShapes);
            if (sketch.Substrokes.Length != sketch.Shapes.Length)
            {
                createMissingShapes();
                if (sketch.Substrokes.Length != sketch.Shapes.Length)
                {
                    Console.WriteLine("WARNING: Sketch still has too many/few shapes!");
                    Console.WriteLine("    Substrokes: " + sketch.Substrokes.Length);
                    Console.WriteLine("    Shapes:     " + sketch.Shapes.Length);
                }
            }
        }

        /// <summary>
        /// Finds any substrokes that aren't in shapes and creates shapes for them.
        /// </summary>
        private void createMissingShapes()
        {
            List<Shape> newShapes = new List<Shape>();

            Substroke[] substrokes = sketch.Substrokes;
            int len = substrokes.Length;
            for (int i = 0; i < len; ++i)
            {
                if (substrokes[i].ParentShapes == null || substrokes[i].ParentShapes.Count == 0)
                {
                    // Create a shape for this stroke

                    Substroke sub = substrokes[i];			//because I'm lazy

                    Sketch.Shape shape = new Shape();

                    // Ahh XML asplode
                    shape.XmlAttrs = new XmlStructs.XmlShapeAttrs(true);
                    shape.XmlAttrs.Type = "unlabeled";
                    shape.XmlAttrs.Name = "shape";
                    shape.XmlAttrs.PenTip = sub.XmlAttrs.PenTip;
                    shape.XmlAttrs.Raster = sub.XmlAttrs.Raster;
                    shape.XmlAttrs.Time = sub.XmlAttrs.Time;
                    shape.XmlAttrs.Start = shape.XmlAttrs.End = sub.XmlAttrs.Id;
                    shape.XmlAttrs.Source = "Grouper.createMissingShapes";

                    shape.AddSubstroke(sub);
                    newShapes.Add(shape);
                }
            }
            sketch.AddShapes(newShapes);
        }

        /// <summary>
        /// Will reconstruct actual group objects from the color xml attributes.
        /// This is useful for some hand-labeled files which lack the former but
        /// have the latter.
        /// </summary>
        public void createGroupsFromColoring()
        {
            // get rid of any existing shapes - we don't want conflicts between old and new shapes
            Shape[] shapes = Sketch.Shapes;
            foreach (Shape shape in shapes)
            {
                sketch.RemoveShape(shape);
            }

            // Find all the color groups
            Substroke[] substrokes = sketch.Substrokes;
            Dictionary<int, List<Substroke>> colorGroups = new Dictionary<int, List<Substroke>>();

            foreach (Substroke substroke in substrokes)
            {
                int color = substroke.XmlAttrs.Color.Value;

                if (!colorGroups.ContainsKey(color))
                {
                    colorGroups.Add(color, new List<Substroke>());
                }

                colorGroups[color].Add(substroke);
            }

            // Create the actual group objects
            foreach (List<Substroke> groupStrokes in colorGroups.Values)
            {
                Sketch.Shape shape = new Shape();

                shape.AddSubstrokes(groupStrokes);

                // Ahh XML asplode
                Substroke sub = groupStrokes[0];
                shape.XmlAttrs = new XmlStructs.XmlShapeAttrs(true);
                shape.XmlAttrs.Type = "shape";
                shape.XmlAttrs.Name = "unlabeled";
                shape.XmlAttrs.PenTip = sub.XmlAttrs.PenTip;
                shape.XmlAttrs.Raster = sub.XmlAttrs.Raster;
                shape.XmlAttrs.Source = "Grouper.createGroupsFromColoring";
                shape.XmlAttrs.Time = sub.XmlAttrs.Time;
                shape.XmlAttrs.Start = sub.XmlAttrs.Id;
                shape.XmlAttrs.End = ((Substroke)groupStrokes[groupStrokes.Count - 1]).XmlAttrs.Id;
                shape.XmlAttrs.Color = randomColor().ToArgb();

                sketch.AddShape(shape);
            }
        }

        #endregion

        #region MISC

        private string methodToString(Algorithm algorithm)
        {
            switch (algorithm)
            {
                case Algorithm.OLD:
                    return "old segmentation";
                case Algorithm.NAIVE_H:
                    return "naive hierarchical";
                case Algorithm.DEBUG:
                    return "DEBUG";
                case Algorithm.TRUTH_TABLE:
                    return "truth table";
                default:
                    return "unrecognized";
            }
        }
        #endregion
    }
}
