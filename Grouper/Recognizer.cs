using System;
using System.Collections.Generic;
using Sketch;

namespace Grouper
{
    /// <summary>
    /// Inherit from this class to implement a Recognizer
    /// </summary>
    public abstract class Recognizer
    {
        /// <summary>
        /// Recognize a list of substrokes
        /// </summary>
        /// <param name="substrokes">Substrokes to recognize</param>
        /// <returns>The results of recognition</returns>
        public abstract Results Recognize(Substroke[] substrokes);

        #region RESULTS

        /// <summary>
        /// Class that stores the results after recognition has been performed
        /// </summary>
        public class Results
        {
            #region INTERNALS

            /// <summary>
            /// Stores the doubley sorted list
            /// </summary>
            private PairedList.PairedList<string, double> m_list;

            /// <summary>
            /// Indicates whether the PairedList has been sorted
            /// </summary>
            private bool sorted;

            #endregion

            #region CONSTRUCTORS

            /// <summary>
            /// Default Constructor
            /// </summary>
            public Results()
                : this(5) { }

            /// <summary>
            /// Constructor
            /// </summary>
            /// <param name="capacity">Initial capacity</param>
            public Results(int capacity)
            {
                m_list = new PairedList.PairedList<string, double>(capacity);
                sorted = false;
            }

            #endregion

            #region MODIFIERS

            /// <summary>
            /// Add the following pair to the PairedList
            /// </summary>
            /// <param name="label">Label to add</param>
            /// <param name="probability">Corresponding probability</param>
            public void Add(string label, double probability)
            {
                m_list.Add(label, probability);
                sorted = false;
            }

            /// <summary>
            /// Sort the PairedList (bidirectional)
            /// </summary>
            public void Sort()
            {
                m_list.Sort();
                sorted = true;
            }

            #endregion

            #region GETTERS

            /// <summary>
            /// Get the probability associated with a specific label
            /// </summary>
            /// <param name="label">Label to find</param>
            /// <returns>The probability of label</returns>
            public double this[string label]
            {
                get
                {
                    List<PairedList.Pair<string, double>> labelList = LabelList;
                    int i, len = labelList.Count;
                    for (i = 0; i < len; ++i)
                        if (labelList[i].ItemA.Equals(label))
                            return labelList[i].ItemB;
                    
                    return 0.0;  
                }
            }

            /// <summary>
            /// Return the label sorted list
            /// </summary>
            public List<PairedList.Pair<string, double>> LabelList
            {
                get
                {
                    if (!sorted)
                        Sort();

                    return m_list.ListA;
                }
            }

            /// <summary>
            /// Return the probability sorted list
            /// </summary>
            public List<PairedList.Pair<double, string>> ProbabilityList
            {
                get
                {
                    if (!sorted)
                        Sort();

                    return m_list.ListB;
                }
            }

            #endregion            

            #region MISC

            /// <summary>
            /// Returns the highest probability
            /// </summary>
            /// <returns></returns>
            public double bestMeasure()
            {
                if (m_list.ListA.Count == 0)
                    return 0.0;
                else
                {
                    List<PairedList.Pair<double, string>> probabilityList = ProbabilityList;
                    return probabilityList[probabilityList.Count - 1].ItemA;
                }
            }

            #endregion
        }

        #endregion
    }
}
