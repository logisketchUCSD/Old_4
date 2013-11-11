using System;
using System.Collections.Generic;
using System.Text;
using Sketch;

namespace Grouper
{
    /// <summary>
    /// Interface that all Searchers should implement
    /// </summary>
    internal interface ISearch
    {
        void search();
        void search(bool verbose);
    }

    /// <summary>
    /// Abstract class is interface for derived children.
    /// This class serves as a general frame for searching
    /// through and recognizing groups.
    /// </summary>
    internal abstract class BaseSearch : ISearch
    {
        /// <summary>
        /// Sketch 
        /// </summary>
        protected Sketch.Sketch sketch;
        
        /// <summary>
        /// Recognizer
        /// </summary>
        protected OldRecognizers.IRecognizer recognizer;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sketch"></param>
        public BaseSearch(Sketch.Sketch sketch)
        {
            this.sketch = sketch;
        }

        /// <summary>
        /// Run the search
        /// </summary>
        public virtual void search()
        {
            search(false);
        }

         /// <summary>
         /// Run the search
         /// </summary>
         /// <param name="verbose"></param>
        public abstract void search(bool verbose);
    }
}
