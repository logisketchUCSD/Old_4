using System;
using System.Collections.Generic;
using System.Text;
using SymbolRec;
using SymbolRec.Image;

namespace Grouper
{
    /// <summary>
    /// Recognize Gates ( AND, NAND, NOR, NOT, OR )
    /// </summary>
    public class GateRecognizer : Recognizer
    {
        /// <summary>
        /// SVM Classifier used to classify substrokes
        /// </summary>
        private Svm.ClassifyGate classify;        

        /// <summary>
        /// Used to ensure that we only create one instance of the class (otherwise we are adding multiple definition images)
        /// </summary>
        private static int numInstances = 0;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public GateRecognizer() 
        {
            //Make sure that this is the first instance
            if (numInstances == 0)
            {
                //We can only add these DefinitionImages once!
                SymbolRec.Image.DefinitionImage.AddMatch(new SymbolRec.Image.DefinitionImage(32, 32, "data/and.amat"));
                SymbolRec.Image.DefinitionImage.AddMatch(new SymbolRec.Image.DefinitionImage(32, 32, "data/nand.amat"));
                SymbolRec.Image.DefinitionImage.AddMatch(new SymbolRec.Image.DefinitionImage(32, 32, "data/nor.amat"));
                SymbolRec.Image.DefinitionImage.AddMatch(new SymbolRec.Image.DefinitionImage(32, 32, "data/not.amat"));
                SymbolRec.Image.DefinitionImage.AddMatch(new SymbolRec.Image.DefinitionImage(32, 32, "data/or.amat"));
            }
            else
            {
                throw new Exception("This class should only be instantiated once!");
            }

            classify = new Svm.ClassifyGate("data/gates.model");
        }

        /// <summary>
        /// Recognize a list of substrokes
        /// </summary>
        /// <param name="substrokes">Substrokes to recognize</param>
        /// <returns>Sorted recognition results</returns>
        public override Recognizer.Results Recognize(Sketch.Substroke[] substrokes)
        {
            //Get the predictions and probabilities
            Substrokes subs = new Substrokes(substrokes);
            DefinitionImage di = new DefinitionImage(32, 32, subs);

            double[] probs;
            string[] labels;

            classify.predict(di.toNodes(), out probs, out labels);


            //Add it to the results
            int i, len = probs.Length;
            Results r = new Results(len);
            for (i = 0; i < len; ++i)
                r.Add(labels[i], probs[i]);

            //Sort :)
            r.Sort();

            return r;
        }
    }
}
