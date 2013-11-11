using System;
using System.Collections.Generic;
using System.Text;
using Sketch;

namespace Grouper
{
    /// <summary>
    /// Goal is to find the correct group.  It is likely that grouping is incorrect,
    /// thus it is our job to search through the group space to find a more likely
    /// match. This is accomplished, among other things, through the use of a 
    /// recognizer.
    /// </summary>
    class CircuitSearch : BaseSearch
    {
        #region INTERNALS

        #region CONSTANTS (EXPERIMENTAL)
      
        /// <summary>
        /// To even consider recognition results, the measure must be greater than this
        /// </summary>
        private const double MEASURE_MINIMUM = 0.75;

        /// <summary>
        /// To disregard results above the top results, they must be this much lower than the top result
        /// </summary>
        private const double MEASURE_SPREAD = 0.03;
        
        /// <summary>
        /// Consider two strokes close if they are within this measure
        /// </summary>
        private const float CLOSE_STROKES = 100.0f;

        #endregion

        /// <summary>
        /// Results for all of the shapes is the sketch
        /// </summary>
        private List<OldRecognizers.Results> recognizedGroups;

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sketch">The Sketch must have already passed through the grouper</param>
        public CircuitSearch(Sketch.Sketch sketch) : base(sketch)
        {
            recognizer = new OldRecognizers.GateRecognizer();
            recognizedGroups = new List<OldRecognizers.Results>(sketch.Shapes.Length);
        }

        #endregion

        #region SEARCH

        /// <summary>
        /// Run search
        /// </summary>
        /// <param name="verbose"></param>
        public override void search(bool verbose)
        {
            int i, len = sketch.Shapes.Length;
            for (i = 0; i < len; ++i)
            {
                if (!sketch.Shapes[i].XmlAttrs.Type.Equals("Wire") && !sketch.Shapes[i].XmlAttrs.Type.Equals("Label") && !sketch.Shapes[i].XmlAttrs.Type.Equals("Mesh"))
                {
                    CircuitSearch.bestResults = new OldRecognizers.Results();
                    this.ProcessAllSubsets(sketch.Shapes[i].Substrokes);
                    sketch.Shapes[i].XmlAttrs.Type = CircuitSearch.bestResults.BestLabel;
                    sketch.Shapes[i].XmlAttrs.Probability = (float)CircuitSearch.bestResults.BestMeasure;
                }
            }
        }

        /// <summary>
        /// Run recognition on all of the groups that we think are gates
        /// </summary>
        private void recognizeGroups()
        {
            recognizedGroups.Clear();
            Shape[] shapes = sketch.Shapes;
            int i, len = shapes.Length;
            for (i = 0; i < len; ++i)
            {
                //Only if it is a gate
                if (!isGate(shapes[i].XmlAttrs.Type))
                    continue;

                OldRecognizers.Results r = recognizer.Recognize(shapes[i].Substrokes);
                //r.shape = shapes[i]; //IMPORTANT?
                recognizedGroups.Add(r);

                //if (r.bestMeasure() < 0.6)
                //{
                //    shapes[i].XmlAttrs.Type = "NONGATE";
                //    shapes[i].XmlAttrs.Probability = 2.0f;
                //}
                //else
                {
                    shapes[i].XmlAttrs.Type = r.BestLabel;
                    shapes[i].XmlAttrs.Probability = (float)r.BestMeasure;
                }
            }
        }

        private Shape bestMatch(List<Substroke> strokes)
        {
            Double score = Double.NegativeInfinity;
            Shape best = null;
            List<List<Substroke>> combos = new List<List<Substroke>>();
            combos.AddRange(createGroups(strokes));

            foreach (List<Substroke> shapeStrokes in combos)
            {
                if (shapeStrokes.Count != 0)
                {
                    OldRecognizers.Results r = recognizer.Recognize(shapeStrokes.ToArray());
                    Boolean test = r.BestLabel.Equals("UNKNOWN");
                    if (r.BestMeasure > score && !r.BestLabel.Equals("UNKNOWN"))
                    {
                        score = r.BestMeasure;
                        best = new Shape();
                        best.AddSubstrokes(shapeStrokes);
                        best.XmlAttrs.Type = r.BestLabel;
                    }
                }
            }

            return best;

        }

        private List<List<Sketch.Substroke>> createGroups(List<Sketch.Substroke> strokes)
        {
            List<List<Sketch.Substroke>> groups = new List<List<Sketch.Substroke>>();
            List<Sketch.Substroke> includesFirst = new List<Sketch.Substroke>();
            List<Sketch.Substroke> skipsFirst = new List<Sketch.Substroke>();
            List<List<Sketch.Substroke>> intermediate = new List<List<Substroke>>();
            Sketch.Substroke temp;

            if (strokes.Count == 0)
            {
                List<Substroke> nullList = new List<Substroke>();
                groups.Add(nullList);
                return groups;
            }

            temp = strokes[0];

            strokes.RemoveAt(0);

            groups.AddRange(createGroups(strokes));
            intermediate.AddRange(createGroups(strokes));

            strokes.Insert(0, temp);

            foreach (List<Sketch.Substroke> strokeGroup in intermediate)
            {
                strokeGroup.Add(temp);
            }

            groups.AddRange(intermediate);

            return groups;
        }

        #endregion

        #region REGROUP METHODS
        
        #region CHANGE SHAPES

        /// <summary>
        /// Add all of the closest substrokes into the shapes
        /// </summary>
        /// <param name="shapes"></param>
        private void updateShapes(List<Shape> shapes)
        {
            int i, len = shapes.Count;
            for (i = 0; i < len; ++i)
            {
                if(shapes[i].Substrokes.Length > 0)
                    addCloseSubstrokes(shapes[i], CLOSE_STROKES);
            }

            //Remove empty shapes
            Shape[] shps = sketch.Shapes;
            len = shps.Length;
            for (i = 0; i < len; ++i)
                if (shps[i].Substrokes.Length == 0)
                    sketch.RemoveShape(shps[i]);
        }

        /// <summary>
        /// Add substrokes close to shape into shape if it improves recognition.
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="DIST"></param>
        private void addCloseSubstrokes(Shape shape, float DIST)
        {
            List<Substroke> subs = shape.SubstrokesL;
            List<Substroke> close = closeSubstrokes(subs, DIST);

            //Console.WriteLine("Found {0} close strokes, type = {1}", close.Count, shape.XmlAttrs.Type);

            //Console.WriteLine("Before: {0}", recognizer.Recognize(shape.Substrokes));

            List<Substroke> subsC = new List<Substroke>(subs);
            List<Substroke> closeC = new List<Substroke>(close);

            addBestSubstrokes(ref subsC, ref closeC);

            bool changed = false;

            Substroke sub;
            int i, len = close.Count;
            for (i = 0; i < len; ++i)
            {
                sub = close[i];
                if (!closeC.Contains(sub)) //If not contained, it must have been moved
                {
                    sub.removeFromParentShapes();
                    shape.AddSubstroke(sub);
                    changed = true;
                }
            }

            if (changed)
                Console.WriteLine("After: {0}", recognizer.Recognize(shape.Substrokes));
        }

        /// <summary>
        /// Remove as many substrokes from groups as we can for the whole sketch
        /// </summary>
        private void runRemove()
        {
            Sketch.Shape[] shapes = this.sketch.Shapes;
            Sketch.Substroke sub;

            List<Sketch.Substroke> list;

            //Iterate through all of the shapes
            int len = shapes.Length;
            int i;
            for (i = 0; i < len; ++i)
            {
                list = new List<Sketch.Substroke>(shapes[i].Substrokes);
                removeWorstSubstrokes(list);

                //A place for the removed substrokes to go
                Sketch.Shape orphan = new Sketch.Shape();

                //I think we have to issue the following two commands...
                orphan.XmlAttrs.Id = System.Guid.NewGuid();
                orphan.XmlAttrs.Type = shapes[i].XmlAttrs.Type;
                orphan.XmlAttrs.Name = shapes[i].XmlAttrs.Name;
                orphan.XmlAttrs.Source = "CircuitSearch.runRemove";
                //time done when substroke is added

                //Iterate through all of the substrokes
                int len2 = shapes[i].Substrokes.Length;
                int j;
                for (j = 0; j < len2; ++j)
                {
                    //If the list does not contain the substroke,
                    //then it must have been removed
                    sub = shapes[i].Substrokes[j];
                    if (!list.Contains(sub))
                    {
                        --len2;
                        --j;
                        shapes[i].RemoveSubstroke(sub);
                        orphan.AddSubstroke(sub);
                    }
                }

                //If the orphan has substrokes
                if (orphan.Substrokes.Length > 0)
                    this.sketch.AddShape(orphan);
            }
        }

        /// <summary>
        /// Perform pairwise moveShapes for all shapes
        /// </summary>
        private void moveAllShapes()
        {
            Sketch.Shape[] shapes = sketch.Shapes;
            int len = shapes.Length;
            int i;
            for (i = 0; i < len - 1; ++i)
            {
                int j;
                for (j = i + 1; j < len; ++j)
                {
                    moveShapes(ref shapes[i], ref shapes[j]);
                }
            }
            //Is another double loop good here???

            //get rid of empty shapes (do we need this... is this automatic?)
            for (i = 0; i < len; ++i)
            {
                if (shapes[i].Substrokes.Length == 0)
                {
                    sketch.RemoveShape(shapes[i]);
                }
            }
        }

        /// <summary>
        /// Implements a double run of moveSubstrokes.  This function handles the updating of the shapes
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        private void moveShapes(ref Sketch.Shape A, ref Sketch.Shape B)
        {
            Sketch.Substroke[] listA = CircuitSearch.deepCopy(A.Substrokes);
            Sketch.Substroke[] listB = CircuitSearch.deepCopy(B.Substrokes);

            moveSubstrokes(ref listA, ref listB); //twice for both directions

            moveSubstrokes(ref listA, ref listB);

            Sketch.Substroke sub;

            //Update the shapes

            int len = B.Substrokes.Length;
            int i;
            for (i = 0; i < len; ++i)
            {
                sub = B.Substrokes[i];
                if (CircuitSearch.contains(listA, sub))
                {
                    //Console.WriteLine("From B to A");
                    B.RemoveSubstroke(sub);
                    A.AddSubstroke(sub);

                    --len;
                    --i;
                }
            }

            len = A.Substrokes.Length;

            for (i = 0; i < len; ++i)
            {
                sub = A.Substrokes[i];
                if (CircuitSearch.contains(listB, sub))
                {
                    //Console.WriteLine("From A to B");
                    A.RemoveSubstroke(sub);
                    B.AddSubstroke(sub);

                    --len;
                    --i;
                }
            }
        }

        /// <summary>
        /// Try adding substrokes from one to the other (bidirectional tested to see which direction is best)
        /// Consider running twice so substrokes can go both directions
        /// </summary>
        /// <param name="A"></param>
        /// <param name="B"></param>
        private void moveSubstrokes(ref Sketch.Substroke[] A, ref Sketch.Substroke[] B)
        {
            double aOrig = recognizer.Recognize(A).BestMeasure;
            double bOrig = recognizer.Recognize(B).BestMeasure;

            //should we enforce this?
            if (true)//aOrig < MEASURE_MINIMUM && bOrig < MEASURE_MINIMUM)
            {
                Sketch.Substroke[] A1 = CircuitSearch.deepCopy(A);
                Sketch.Substroke[] B1 = CircuitSearch.deepCopy(B);

                Sketch.Substroke[] A2 = CircuitSearch.deepCopy(A);
                Sketch.Substroke[] B2 = CircuitSearch.deepCopy(B);

                List<Substroke> LA1 = new List<Substroke>(A1);
                List<Substroke> LB1 = new List<Substroke>(B1);
                List<Substroke> LA2 = new List<Substroke>(A2);
                List<Substroke> LB2 = new List<Substroke>(B2);

                double aMeasure = addBestSubstrokes(ref LA1, ref LB1);
                double bMeasure = addBestSubstrokes(ref LB2, ref LA2);
                double max = aMeasure > bMeasure ? aMeasure : bMeasure;

                //Other methods may want to be tried here!!!
                //like, aMeasure > aOrig && bMeasure > bOrig
                // 
                //if(aMeasure > aOrig && bMeasure > bOrig)
                if (max > aOrig && max > bOrig)
                {
                    //Console.WriteLine("Moved! :)");
                    if (max == aMeasure)
                    {
                        A = LA1.ToArray();
                        B = LB1.ToArray();
                    }
                    else //max == bMeasure
                    {
                        A = LA2.ToArray();
                        B = LB2.ToArray();
                    }
                }
            }
        }

        #endregion
        
        #region ADD / REMOVE SUBSTROKES

        /// <summary>
        /// Keep removing substrokes until no improvements are made, or the threshold is surpassed
        /// </summary>
        /// <param name="substrokes"></param>
        /// <returns>Measure</returns>
        private double removeWorstSubstrokes(List<Sketch.Substroke> substrokes)
        {
            double measure = 0.0;

            while (removeWorstSubstroke(substrokes, out measure))// && measure < MEASURE_MINIMUM)
            {
                //Do nothing
            }
            return measure;
        }

        /// <summary>
        /// Removes the substroke that produces the highest measure score, if that measure score
        /// is greater than the original measure score. Return true if a substroke is removed, 
        /// otherwise false.
        /// </summary>
        /// <param name="substrokes"></param>
        /// <returns></returns>
        private bool removeWorstSubstroke(List<Sketch.Substroke> substrokes, out double measure)
        {
            //If the original is a gate that is better than 60% keep it
            OldRecognizers.Results original = recognizer.Recognize(substrokes.ToArray());
            if (original.BestMeasure > 0.6 && !original.BestLabel.Equals("NONGATE"))
            {
                measure = 0.0;
                return false;
            }

            //Only remove strokes if the symbol contains 3 or more strokes
            //Due to our domain, we know every symbol will have at least 2 strokes
            if (substrokes.Count < 3)
            {
                measure = 0.0;
                return false;
            }

            int len = substrokes.Count;
            
            Sketch.Substroke sub;
            OldRecognizers.Results res;

            int bestIndex = -1;
            //double best = 0.0;
            PairedList.Pair<double, string> bestPair = new PairedList.Pair<double,string>(0.0, "");
            PairedList.Pair<double, string> pair;
            OldRecognizers.Results bestResults = original;
            //List<OldRecognizers.Results> results = new List<OldRecognizers.Results>(len);

            int i;
            for (i = 0; i < len; ++i)
            {
                //Remove the substroke
                sub = substrokes[i];
                substrokes.RemoveAt(i);

                //Get the results after removing substroke
                res = recognizer.Recognize(substrokes.ToArray());
                //results.Add(res);
                
                //Add the substroke back in
                substrokes.Insert(i, sub);

                pair = res.BestPair;
                if (
                    //NON vs NON
                    (pair.ItemA < bestPair.ItemA && bestPair.ItemB.Equals("NONGATE") && pair.ItemB.Equals("NONGATE"))
                    || 
                    //NON vs GATE
                    (bestPair.ItemB.Equals("NONGATE") && !pair.ItemB.Equals("NONGATE"))
                    ||
                    //GATE vs GATE
                    (pair.ItemA > bestPair.ItemA && !bestPair.ItemB.Equals("NONGATE") && !pair.ItemB.Equals("NONGATE"))
                    )
                {
                    bestIndex = i;
                    bestPair = pair;
                    bestResults = res;
                }
            }

            bool useBest =
                (
                //NONGATE vs NONGATE
                (original.BestLabel.Equals("NONGATE") && bestResults.BestLabel.Equals("NONGATE") && bestResults.BestMeasure < original.BestMeasure)
                ||
                //NONGATE vs GATE
                (original.BestLabel.Equals("NONGATE") && !bestResults.BestLabel.Equals("NONGATE"))
                ||
                //GATE vs GATE
                (!original.BestLabel.Equals("NONGATE") && !bestResults.BestLabel.Equals("NONGATE") && bestResults.BestMeasure > original.BestMeasure)
                );
            
            if (useBest)
            {
                substrokes.RemoveAt(bestIndex);
                measure = bestPair.ItemA;
                return true;
            }
            else
            {
                measure = original.BestMeasure;
                return false;
            }
        }

        private PairedList.Pair<double, string> bestGate(OldRecognizers.Results results)
        {
            List<PairedList.Pair<double, string>> p = results.ProbabilityList;
            if (p[p.Count - 1].ItemB.Equals("NONGATE"))
                return p[p.Count - 2];
            else
                return p[p.Count - 1];
        }

        /// <summary>
        /// Keep adding substrokes until no improvements are made
        /// </summary>
        /// <param name="group"></param>
        /// <param name="toAdd"></param>
        /// <returns></returns>
        private double addBestSubstrokes(ref List<Sketch.Substroke> group, ref List<Sketch.Substroke> toAdd)
        {
            double measure = 0.0;
            while (addBestSubstroke(ref group, ref toAdd, out measure))
            {
                //Do nothing
            }

            return measure;
        }

        /// <summary>
        /// Add a substroke from toAdd to group that increases the measure by the most
        /// </summary>
        /// <param name="group">Main group</param>
        /// <param name="toAdd">Group to add from</param>
        /// <param name="measure"></param>
        /// <returns>True if new group is better</returns>
        private bool addBestSubstroke(ref List<Sketch.Substroke> group, ref List<Sketch.Substroke> toAdd, out double measure)
        {
            OldRecognizers.Results original = recognizer.Recognize(group.ToArray());
            if (original.BestMeasure > 0.5)
            {
                measure = 0.0;
                return false;
            }

            int len = toAdd.Count;
            if (len < 1)
            {
                measure = 0.0;
                return false;
            }
            
            int gLen = group.Count;
            
            Sketch.Substroke sub;
            OldRecognizers.Results res;

            int bestIndex = -1;
            OldRecognizers.Results best = new OldRecognizers.Results();
            List<OldRecognizers.Results> results = new List<OldRecognizers.Results>(len);

            int i;
            for (i = 0; i < len; ++i)
            {
                //Add the substroke
                sub = toAdd[i];
                group.Add(sub);

                //Get the results after adding substroke
                res = recognizer.Recognize(group.ToArray());
                //Console.WriteLine(res);
                results.Add(res);

                //Remove the substroke 
                group.RemoveAt(gLen);
                

                //Keep the best measure
                if (res.BestMeasure > best.BestMeasure)
                {
                    bestIndex = i;
                    best = res;
                }
            }

            //Don't do anything if the original was better
            if (best.BestMeasure <= original.BestMeasure - 0.10 || best.BestLabel.Equals("NONGATE"))
            {
                measure = original.BestMeasure;
                return false;
            }
            //Otherwise, add the best substroke
            else
            {
                group.Add(toAdd[bestIndex]);
                toAdd.RemoveAt(bestIndex);

                measure = best.BestMeasure;
                return true;
            }
        }

        /// <summary>
        /// Given a group and toAdd, add two substrokes at a time from toAdd to group.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="toAdd"></param>
        /// <returns></returns>
        private double addBestTwoSubstrokes(ref List<Sketch.Substroke> group, ref List<Sketch.Substroke> toAdd)
        {
            double measure = 0.0;
            while (addBestTwoSubstroke(ref group, ref toAdd, out measure))
            {
                //Do nothing
            }

            //Do single substrokes now. (Maybe comment out, unnecessary).
            measure = addBestSubstrokes(ref group, ref toAdd);

            return measure;
        }

        /// <summary>
        /// Adds the two best substrokes from toAdd to group if the new group has a higher measure than the old one.
        /// </summary>
        /// <param name="group">Main group</param>
        /// <param name="toAdd">Substrokes to add to group</param>
        /// <param name="measure"></param>
        /// <returns>True if new group is better</returns>
        private bool addBestTwoSubstroke(ref List<Sketch.Substroke> group, ref List<Sketch.Substroke> toAdd, out double measure)
        {
            int len = toAdd.Count;
            if (len < 2)
            {
                measure = 0.0;
                return false;
            }

            int gLen = group.Count;

            Sketch.Substroke sub1, sub2;
            OldRecognizers.Results res;

            int bestIndex1 = -1;
            int bestIndex2 = -1;
            double best = 0.0;
            List<OldRecognizers.Results> results = new List<OldRecognizers.Results>(len);

            int i;
            for (i = 0; i < len - 1; ++i)
            {
                int j;
                for (j = i + 1; j < len; ++j)
                {
                    //Add the 2 substrokes
                    sub1 = toAdd[i];
                    sub2 = toAdd[j];
                    group.Add(sub1);
                    group.Add(sub2);

                    //Get the results after removing substroke
                    res = recognizer.Recognize(group.ToArray());
                    results.Add(res);

                    //Remove the 2 substrokes 
                    group.RemoveAt(gLen);
                    group.RemoveAt(gLen);


                    //Keep the best measure
                    if (res.BestMeasure > best)
                    {
                        bestIndex1 = i;
                        bestIndex2 = j;
                        best = res.BestMeasure;
                    }
                }
            }

            //Don't do anything if the original was better
            OldRecognizers.Results original = recognizer.Recognize(group.ToArray());
            if (best <= original.BestMeasure)
            {
                measure = original.BestMeasure;
                return false;
            }
            //Otherwise, add the best substroke
            else
            {
                group.Add(toAdd[bestIndex1]);
                group.Add(toAdd[bestIndex2]);

                toAdd.RemoveAt(bestIndex1);
                if (bestIndex2 > bestIndex1)
                    toAdd.RemoveAt(bestIndex2 - 1);
                else
                    toAdd.RemoveAt(bestIndex2);

                measure = best;
                return true;
            }
        }

        #endregion
        
        #endregion

        #region MISC

        /// <summary>
        /// Find the substrokes whose minimum distance is at most DIST away from a substroke is subs.
        /// </summary>
        /// <param name="subs"></param>
        /// <param name="DIST"></param>
        /// <returns>The list of close substrokes</returns>
        private List<Substroke> closeSubstrokes(List<Substroke> subs, float DIST)
        {
            List<Substroke> close = new List<Substroke>();
            Substroke[] allSubs = this.sketch.Substrokes;
            Substroke sub;
            int i, len = allSubs.Length;
            for (i = 0; i < len; ++i)
            {
                sub = allSubs[i];
                if (!subs.Contains(sub) && Featurefy.Distance.minimumDistance(sub, subs) < DIST)
                    close.Add(sub);
            }
            return close;
        }


        /// <summary>
        /// For testing purposes.
        /// 
        /// Treat the whole sketch as a symbol and try to recognize it.
        /// </summary>
        private void saySketch()
        {
            Substroke[] subs = sketch.Substrokes;

            OldRecognizers.Results results = recognizer.Recognize(subs);
            Console.WriteLine("Substrokes: {0}", subs.Length);
            Console.WriteLine("Recognition:");
            Console.WriteLine(results);
            Console.WriteLine();

        }

        /// <summary>
        /// Randomize the substrokes
        /// </summary>
        /// <param name="subs"></param>
        private void randomize(Substroke[] subs)
        {
            Random r = new Random();
            int i, len = subs.Length;
            for (i = len - 1; i >= 0; --i)
            {
                int swapWith = r.Next(i + 1);
                if (swapWith != i)
                {
                    Substroke temp = subs[i];
                    subs[i] = subs[swapWith];
                    subs[swapWith] = temp;
                }
            }
        }

        /// <summary>
        /// For testing: 
        /// Groups up all substrokes into one shape incrementally.
        /// See if recognition improves with each additional stroke.
        /// </summary>
        private void groupUpRandom()
        {
            Substroke[] subs = sketch.Substrokes;
            randomize(subs);

            Random r = new Random();
            List<Substroke> test = new List<Substroke>();


            int i, len = subs.Length;
            for (i = 0; i < len; ++i)
            {
                test.Add(subs[i]);
                //sayResults(recognizer.Recognize(test.ToArray()));
            }
        }

        /// <summary>
        /// Is a Gate
        /// </summary>
        /// <param name="str"></param>
        /// <returns>True iff str is gate</returns>
        private bool isGate(string str)
        {

            return (str.Equals("AND")
                    || str.Equals("NAND")
                    || str.Equals("NOR")
                    || str.Equals("NOT")
                    || str.Equals("OR")
                    || str.Equals("XOR")
                    || str.Equals("XNOR")
                    || str.Equals("Gate"));
        }

        /// <summary>
        /// Create a deep copy of Substroke[]
        /// </summary>
        /// <param name="A">The Substroke[]</param>
        /// <returns>deep copy</returns>
        private static Sketch.Substroke[] deepCopy(Sketch.Substroke[] A)
        {
            int len = A.Length;
            Sketch.Substroke[] B = new Sketch.Substroke[len];

            int i;
            for (i = 0; i < len; ++i)
                B[i] = A[i].Clone();

            return B;
        }

        /// <summary>
        /// Compute whether subs contains sub.
        /// </summary>
        /// <param name="subs">The Substroke[]</param>
        /// <param name="sub">Substroke</param>
        /// <returns>True iff subs contains sub</returns>
        private static bool contains(Sketch.Substroke[] subs, Sketch.Substroke sub)
        {
            int len = subs.Length;
            int i;
            for (i = 0; i < len; ++i)
                if (subs[i].Equals(sub))
                    return true;
            return false;
        }
        
        #endregion

        private void showSubset(Substroke[] subs, int len)
        {
            int i;
            for (i = 0; i < len; ++i)
            {
                Console.Write(subs[i].XmlAttrs.Id.ToString().Substring(0, 4) + " ");
            }
            Console.WriteLine();
        }

        #region ALL SUBSETS PROCESSING

        private void ProcessAllSubsets(Substroke[] subs)
        {
            if (true) // Single-threaded mode
            {
                int numSubsets = GenerateSubsets.GenerateSubsets.AllSubsets<Substroke>(subs, ProcessSubsetSinglethread);
            }
			/*Multithreading is a little non-completed right now
            else
            {
                int numSubsets = GenerateSubsets.GenerateSubsets.AllSubsets<Substroke>(subs, ProcessSubsetMultithread);
                while (currentSubset < numSubsets - 1) System.Threading.Thread.Sleep(100);
            }
			 */
        }
        
        private delegate OldRecognizers.Results ResultsDelegate(Substroke[] subs);

        private void ProcessSubsetMultithread(Substroke[] subs, int len)
        {
            //Copy the references into a new array            
            Substroke[] nSubs = new Substroke[len];
            int i;
            for (i = 0; i < len; ++i)
                nSubs[i] = subs[i];     

            //Pass the new array into this function, thus we can process more subsets
            ResultsDelegate rd = new ResultsDelegate(recognizer.Recognize);

            //Asynchronous
            rd.BeginInvoke(nSubs, new AsyncCallback(ResultsCallback), null);
        }

        private void ProcessSubsetSinglethread(Substroke[] subs, int len)
        {
            //Copy the references into a new array            
            Substroke[] nSubs = new Substroke[len];
            int i;
            for (i = 0; i < len; ++i)
                nSubs[i] = subs[i];

            //Pass the new array into this function, thus we can process more subsets
            ResultsDelegate rd = new ResultsDelegate(recognizer.Recognize);

            //Synchronous
            OldRecognizers.Results results = rd.Invoke(nSubs);
            updateBestResults(results);
            ++currentSubset;
        }

        private static OldRecognizers.Results bestResults = new OldRecognizers.Results();

        private const string NON = "UNKNOWN";

        private static int currentSubset = 0;

        private void updateBestResults(OldRecognizers.Results results)
        {
            bool bestIsGate = !CircuitSearch.bestResults.BestLabel.Equals(NON);
            bool resultsIsGate = !results.BestLabel.Equals(NON);

            if (!bestIsGate && resultsIsGate
                || (bestIsGate && resultsIsGate && results.BestMeasure > CircuitSearch.bestResults.BestMeasure))
            {
                CircuitSearch.bestResults = results;
                //Console.WriteLine("Found new best {0}", results);
            }
        }

        private void ResultsCallback(IAsyncResult iar)
        {
            System.Runtime.Remoting.Messaging.AsyncResult ar = 
                (System.Runtime.Remoting.Messaging.AsyncResult)iar;
            ResultsDelegate rd = (ResultsDelegate)ar.AsyncDelegate;

            OldRecognizers.Results results = rd.EndInvoke(iar);
            
            //update bestResults, make sure nobody else can modify it while we are doing this
            lock (CircuitSearch.bestResults)
            {
                updateBestResults(results);
                ++currentSubset;  
            }

        }

        #endregion

    }
}

