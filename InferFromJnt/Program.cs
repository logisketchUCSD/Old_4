using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

using ConverterJnt;
using ConverterXML;
using Fragmenter;
using CRF;
using Sketch;


/**
 * Test parameters 
 */

// -crf 3label.tcrf 3-label-CRF.txt -d "C:\Documents and Settings\Da Vinci\My Documents\Visual Studio Projects\E85\0128\0128_Sketches"
// -crf 3label.tcrf 3-label-CRF.txt -2p 2p-Gates.tcrf 8-label-Gate.txt -test2p
// -crf wgTest.tcrf wire-gate-labels.txt -d "C:\Documents and Settings\Da Vinci\My Documents\Visual Studio Projects\branches\redesign\circuit data"


namespace InferFromJnt
{
	/// <summary>
	/// Summary description for Program.
	/// </summary>
	class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			ArrayList argArray = new ArrayList(args);	
			
			// FOR DEMO
			/*argArray.Clear();
			argArray.Add("-crf");
			argArray.Add("WireGate.tcrf");
			argArray.Add("WireGateLabels.txt");
			argArray.Add("-d");
			argArray.Add("InferFilesHere");*/

			
			int numArgs = argArray.Count;
			string[] files;

			
			// 2-label Defaults
			string inferCRF = "C:\\Documents and Settings\\Da Vinci\\My Documents\\Visual Studio Projects\\branches\\redesign\\InferFromJnt\\wgTest.tcrf";
			string labelFile = "C:\\Documents and Settings\\Da Vinci\\My Documents\\Visual Studio Projects\\branches\\redesign\\InferFromJnt\\wire-gate-labels.txt";

			// 3-label Defaults
			//string inferCRF  = "C:\\Documents and Settings\\Da Vinci\\My Documents\\Visual Studio Projects\\branches\\redesign\\InferFromJnt\\3-label.tcrf";
			//string labelFile = "C:\\Documents and Settings\\Da Vinci\\My Documents\\Visual Studio Projects\\branches\\redesign\\InferFromJnt\\3-label-CRF.txt";

			// 2-pass CRF Defaults
			string mp_inferCRF = "C:\\Documents and Settings\\Da Vinci\\My Documents\\Visual Studio Projects\\branches\\redesign\\InferFromJnt";
			string mp_labelFile = "C:\\Documents and Settings\\Da Vinci\\My Documents\\Visual Studio Projects\\branches\\redesign\\InferFromJnt\\8-label-Gate.txt";

			// Default output
			string output = "C:\\Documents and Settings\\Da Vinci\\My Documents\\Visual Studio Projects\\branches\\redesign\\InferFromJnt\\inferred.xml";

			// Test labeled xml
			string testDirXML = "C:\\Documents and Settings\\Da Vinci\\My Documents\\Visual Studio Projects\\branches\\redesign\\RunCRF\\execution directory - Max\\WireGateTraining";
			bool test2p = false;

			// Are we running a multi-pass CRF inferrence?
			bool multiPassCRF = false;
			CRF.CRF mp_crf = null;

			// Print usage if nothing is entered
			if (numArgs == 0)
			{
				Console.WriteLine("*****************************************************************");
				Console.WriteLine("*** InferFromJnt.exe");
				Console.WriteLine("*** by Aaron Wolin, Devin Smith, Jason Fennell, and Max Pflueger.");
				Console.WriteLine("*** Harvey Mudd College, Claremont, CA 91711.");
				Console.WriteLine("*** Sketchers 2006.");
				Console.WriteLine("***");
				Console.WriteLine("*** Usage: InferFromJnt.exe (-crf <CRF> <labels>) [-2p <CRF> <labels>] (-c | -d directory | -r)");
				Console.WriteLine("*** Usage: InferFromJnt.exe (-crf <CRF> <labels>) [-2p <CRF> <labels>] input1.jnt [input2.jnt ...]");
				Console.WriteLine("***");
				Console.WriteLine("*** -crf: CRF and Label file to infer from");
				Console.WriteLine("*** -2p:  Optional 2-pass CRF and Labels");
				Console.WriteLine("*** -c:   Convert all files in current directory");
				Console.WriteLine("*** -d:   Convert all files in the specified directory");
				Console.WriteLine("*** -r:   Recursively convert files from the current directory");
			
				return;
			}
			
			// Load a CRF file if it's specified
			if (argArray.Contains("-crf"))
			{
				inferCRF  = (string)argArray[argArray.IndexOf("-crf") + 1];
				labelFile = (string)argArray[argArray.IndexOf("-crf") + 2];
			}

			// Load a 2-pass CRF file if it's specified
			if (argArray.Contains("-2p"))
			{
				mp_inferCRF  = (string)argArray[argArray.IndexOf("-2p") + 1];
				mp_labelFile = (string)argArray[argArray.IndexOf("-2p") + 2];

				multiPassCRF = true;
			}

			// Convert everything in this directory
			if (argArray.Contains("-c"))
			{
				files = Directory.GetFiles(Directory.GetCurrentDirectory());
			}
			
			// Convert everything in specified directory
			else if (argArray.Contains("-d"))
			{
				// Are we in range?
				if (argArray.IndexOf("-d") + 1  >= argArray.Count)
				{
					Console.Error.WriteLine("No directory specified.");
					return;
				}
				else if (!Directory.Exists((string)argArray[argArray.IndexOf("-d") + 1])) //Does dir exist?
				{
					Console.Error.WriteLine("Directory doesn't exist.");
					return;
				}
				else
				{
					files = Directory.GetFiles((string)argArray[argArray.IndexOf("-d") + 1]);
				}
			}

			// Recursive from current dir
			else if (argArray.Contains("-r"))
			{
				// Get recursive files
				ArrayList rFiles = new ArrayList();
				DirSearch(Directory.GetCurrentDirectory(), ref rFiles);
			
				// Get current dir files
				string [] currDir = Directory.GetFiles(Directory.GetCurrentDirectory());

				files = new string[rFiles.Count + currDir.Length];

				// Populate both recursive and current into files
				int current;
				for (current = 0; current < currDir.Length; ++current)
					files[current] = currDir[current];

				foreach (string s in rFiles)
				{
					files[current++] = s;
				}
			}

			// Test the 2-pass CRF on labeled files (tests the 2nd CRF's performance)
			else if (argArray.Contains("-test2p"))
			{
				// Does dir exist?
				if (!Directory.Exists(testDirXML))
				{
					Console.Error.WriteLine("Directory doesn't exist.");
					return;
				}
				else
				{
					files = Directory.GetFiles(testDirXML);
				}
				
				test2p = true;
			}

			// Convert only the specified files
			else
			{
				files = args;
			}

			

			// Create an inferred Jnt subdirectory
			string subDir = "InferredJnt";
			Directory.CreateDirectory(subDir);

			// Run the CRF to do labeling
			CRF.CRF crf = new CRF.CRF(inferCRF, false);

			// Read the label file						
			StreamReader labelReader = new StreamReader(labelFile);
			System.Collections.Hashtable stringToIntTable = new System.Collections.Hashtable(crf.numLabels);
			System.Collections.Hashtable intToStringTable = new System.Collections.Hashtable(crf.numLabels);
			for (int k = 0; k < crf.numLabels; k++)
			{
				string textLabel = labelReader.ReadLine();
				int intLabel = Convert.ToInt32(labelReader.ReadLine());

				stringToIntTable.Add(textLabel, intLabel);
				intToStringTable.Add(intLabel, textLabel);
			}

			// Get the multi-pass CRF labels
			System.Collections.Hashtable mp_stringToIntTable = null;
			System.Collections.Hashtable mp_intToStringTable = null;

			if (multiPassCRF)
			{
				// Run the 2-pass CRF to do labeling
				mp_crf = new CRF.CRF(mp_inferCRF, false);

				// Read the label file						
				StreamReader mp_labelReader = new StreamReader(mp_labelFile);
				mp_stringToIntTable = new System.Collections.Hashtable(mp_crf.numLabels);
				mp_intToStringTable = new System.Collections.Hashtable(mp_crf.numLabels);
				for (int k = 0; k < mp_crf.numLabels; k++)
				{
					string textLabel = mp_labelReader.ReadLine();
					int intLabel = Convert.ToInt32(mp_labelReader.ReadLine());

					mp_stringToIntTable.Add(textLabel, intLabel);
					mp_intToStringTable.Add(intLabel, textLabel);
				}
			}


			// Go through all of the input files and infer them
			foreach (string input in files)
			{
				// The Microsoft Journal file must end with .jnt or .jtp
				if (!test2p && !input.ToLower().EndsWith(".jnt") && !input.ToLower().EndsWith(".jtp"))
				{
					Console.Error.WriteLine("Unknown extension in " + input + ", must be .jnt or .jtp");
					continue;
				}				
				
				try
				{
					Console.WriteLine("Trying " + input);
				
					int numPages = 1;
					if (!test2p)
						numPages = ReadJnt.NumberOfPages(input);
					
					for (int i = 1; i <= numPages; ++i)
					{
						Sketch.Sketch inputSketch = null;

						if (!test2p)
						{	
							ReadJnt ink = new ConverterJnt.ReadJnt(input, i);
							inputSketch = ink.Sketch;
						}
						else
						{
							// Test case for 2-pass CRFs
							// We needed a reliably labeled file...
							inputSketch = new ReadXML(input).Sketch;
						}
						
						// Fragment the Sketch
						Fragment.fragmentSketch(inputSketch);
						Featurefy.FeatureSketch fs = new Featurefy.FeatureSketch(ref inputSketch);
						Substroke[] inputSubstrokes = inputSketch.Substrokes;
				
						// Initialize the graph
						Console.WriteLine("Initializing graph");
						crf.initGraph(ref fs);
						
						// Calculate the CRF features
						Console.WriteLine("Calculating features");
						crf.calculateFeatures();
						
						// Do the inferrence
						Console.WriteLine("Doing inference");
						crf.infer();
						
						// Find the labels
						Console.WriteLine("Finding labels");
						int[] outIntLabels;
						double[] outProbLabels;
						crf.findLabels(out outIntLabels, out outProbLabels);

						// Add the labels to the xml file
						if (!multiPassCRF)
						{
							for (int k = 0 ; k < outIntLabels.Length; k++)
							{
								inputSketch.AddLabel(inputSubstrokes[k], 
									(string)intToStringTable[outIntLabels[k]], 
									outProbLabels[k]);
							}
						}

						/**
						 * 2-pass CRF code!
						 */

						if (multiPassCRF)
						{
							// Initialize a 2-pass graph
							Console.WriteLine("Initializing a 2-pass graph");

							List<Substroke> gates = new List<Substroke>();
							List<Shape> gateShapes = new List<Shape>();
							foreach(Substroke s in fs.Sketch.SubstrokesL)
							{
								if (s.FirstLabel == "Gate")
									gates.Add(s);
							}
							foreach (Shape s in fs.Sketch.ShapesL)
							{
								if (s.Type == "Gate")
									gateShapes.Add(s);
							}
							Sketch.Sketch multiSketch = new Sketch.Sketch(gateShapes.ToArray(), fs.Sketch.XmlAttrs);
							Featurefy.FeatureSketch multifs = new Featurefy.FeatureSketch(ref multiSketch);

							mp_crf.initGraph( ref multifs );
					
							// Calculate the CRF features
							Console.WriteLine("Calculating features");
							mp_crf.calculateFeatures();
					
							// Do the inferrence
							Console.WriteLine("Doing inference");
							mp_crf.infer();
					
							// Find the labels
							Console.WriteLine("Finding gate labels");
							mp_crf.findLabels(out outIntLabels, out outProbLabels);

							// Add the labels to the xml file
							for (int k = 0 ; k < outIntLabels.Length; k++)
							{
								int intLabel = outIntLabels[k];
								inputSketch.AddLabel(gates[k], 
									(string)mp_intToStringTable[intLabel], 
									outProbLabels[k]);
							}
						}

						// Output path for the inferred xml
						output = subDir + "\\" + Path.GetFileNameWithoutExtension(input) + "."
						+ "autolabeled.xml";

						// Write the new XML file
						MakeXML xml = new ConverterXML.MakeXML(inputSketch);
						xml.WriteXML(output);
					}

					Console.WriteLine();
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					Console.WriteLine(e.InnerException);
					Console.WriteLine(e.StackTrace);
					Console.ReadLine();
					continue;
				}				
			}
		}


		/// <summary>
		/// Perform a recursive directory search. http://support.microsoft.com/default.aspx?scid=kb;en-us;303974
		/// </summary>
		/// <param name="sDir">Directory to search recursively</param>
		/// <param name="rFiles">Array to add the files to</param>
		static void DirSearch(string sDir, ref ArrayList rFiles) 
		{
			try	
			{
				foreach (string d in Directory.GetDirectories(sDir)) 
				{
					foreach (string f in Directory.GetFiles(d, "*.*")) 
					{
						rFiles.Add(f);
					}
					DirSearch(d, ref rFiles);
				}
			}
			catch (System.Exception excpt) 
			{
				Console.WriteLine(excpt.Message);
			}
		}
	}
}
