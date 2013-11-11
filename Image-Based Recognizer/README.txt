# -------------------------------------------------------------------------- #
######################### Image-Based Recognizer #############################
# -------------------------------------------------------------------------- #

Overview
The image-based recognizer works by comparing a newly created BitmapSymbol to
a list of templates, which are themselves simply BitmapSymbols. 

Getting Results
The first step is to load in (or train) a list of template BitmapSymbols.
The second step is to create a new BitmapSymbol (which we'll call UnknownSymbol 
this example) from the substrokes in the shape. Once the UnknownSymbol has been
created it needs to be processed using UnknownSymbol.Process(). Then you can 
find the most similar symbols using 
ResultNames = UnknownSymbol.FindSimilarity_and_Rank(Definitions, out SymbolRanks)
where the ResultNames is a ranked list of the names of the most similar symbols
(e.g. AND, OR, etc.), Definitions is the list of templates that were loaded, and 
the SymbolRanks are an uninstantiated Dictionary<string, List<SymbolRank>> which 
are essentially the full results (contain the individual scores for each metric).

Example Code:
List<BitmapSymbol> ImageDefinitions = LoadImageDefinitions(CurrentUsersDefinitionFilename);
Sketch.Sketch sketch = LoadSketchFromFile(filename);
List<Substrokes> strokes = sketch.Shapes[0].SubstrokesL;
BitmapSymbol UnknownSymbol = new BitmapSymbol(strokes);
UnknownSymbol.Process();
Dictionary<string, List<SymbolRank>> SRs;
List<string> results = UnknownSymbol.FindSimilarity_and_Rank(ImageDefinitions, out SRs);

Creating Templates
In order to train the image-based recognizer you must create templates by first 
creating a new BitmapSymbol, then call the Process() command, then finally call 
the Output(filename) command. The output function creates 2 files, one in the 
_Polar directory and the other in the _Screen directory. These text files are 
templates. 

Creating User Definition Files
User Definition files simply contain the names of BitmapSymbol templates that 
you want to include in the list of image definitions (e.g. AND_01_T_RPT_001).
Each symbol gets its own line.

Loading Image Definitions
Read the definition file for the user, for each line (symbol) create a new 
BitmapSymbol() - DefnSymbol, then call DefnSymbol.Input(templateFilename, 
BaseDirectory), then Process() the DefnSymbol, and add the DefnSymbol to the
list of Image Definitions.