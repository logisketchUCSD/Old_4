# -------------------------------------------------------------------------- #
################################ GROUPER #####################################
# -------------------------------------------------------------------------- #

This folder organizes the different clustering algorithms. Some of these
also implement searching. 

# -------------------------------------------------------------------------- #

Group is an abstract class that all groupers will probably want to inherit
from to get some common functionality. However, radically different classes
(functionality-wise) that merely want to be useable in our workflow must
simply implement the IGroup interface (both of which can be found in
Group.cs).

GroupException is a common way to send messages back up the chain in an
attempt to use error handling for feedback. It currently isn't used much.


--- Grouping Algorithms ---

 * NHGroup: A naive spatial grouper. Actually works pretty well. Yay naive!
 * TemportalGroup: A very naive temporal grouper. Does not work well.
 * KMeans: uses centroids and K-Means to group
 * TTGroup/TTSearch: Truth table stuff
 * Overview: some "in-progress" algorithms from Marty. Some of these work
   very well, but need to be fleshed out before they'll be usable.
