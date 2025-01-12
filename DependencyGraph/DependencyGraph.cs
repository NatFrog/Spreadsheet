// Skeleton implementation written by Joe Zachary for CS 3500, September 2013
// Version 1.1 - Joe Zachary
//   (Fixed error in comment for RemoveDependency)
// Version 1.2 - Daniel Kopta Fall 2018
//   (Clarified meaning of dependent and dependee)
//   (Clarified names in solution/project structure)
// Version 1.3 - H. James de St. Germain Fall 2024
// <version>
//      September 12th, 2024
// </version>
// <author
//      Natalie Hicks, u1469883
// </author>

namespace CS3500.DependencyGraph;

/// <summary>
///   <para>
///     This class represents a DependencyGraph where dependencies between two strings are represented. 
///     Each string is mapped to it's dependents and dependees via two Dictionaries. The class contains
///     methods that work on the DependencyGraph.
///   </para>
///   <para>
///     A DependencyGraph can be modeled as a set of ordered pairs of strings.
///     Two ordered pairs (s1,t1) and (s2,t2) are considered equal if and only
///     if s1 equals s2 and t1 equals t2.
///   </para>
/// </summary>
public class DependencyGraph
{
    // Dictionary that has a Key of type string that represnets a dependee. Each dependee/key has a Value
    // that's a HashSet which holds all of the dependents of the dependee/key.
    private Dictionary<string, HashSet<string>> dependents;

    // Dictionary that has a Key of type string that represnets a dependent. Each dependent/Key has a Value
    // that's a HashSet which holds all of the dependees of the dependent/Key.
    private Dictionary<string, HashSet<string>> dependees;

    // A int variable to reprsent the total number of dependencies (ordered pairs in this DependencyGraph.
    private int size;

    /// <summary>
    ///   Initializes a new instance of the <see cref="DependencyGraph"/> class.
    ///   The initial DependencyGraph is empty, and has a size of 0.
    /// </summary>
    public DependencyGraph()
    {
        dependents = new Dictionary<string, HashSet<string>>();
        dependees = new Dictionary<string, HashSet<string>>();
        size = 0;
    }

    /// <summary>
    /// The number of ordered pairs in the DependencyGraph.
    /// </summary>
    public int Size
    {
        // Loops through the dependents list and increments size for each unique ordered pair.
        get {
            this.size = 0;
            foreach (var node in dependents)
            {
                size += node.Value.Count;
            }
            return this.size;
        }
    }

    /// <summary>
    ///   Reports whether the given node has dependents (i.e., other nodes depend on it).
    /// </summary>
    /// <param name="nodeName"> The name of the node.</param>
    /// <returns> true if the node has dependents. </returns>
    public bool HasDependents(string nodeName)
    {
        // True if nodeName is in dependents and has more than zero dependents.
        return dependents.ContainsKey(nodeName) && dependents[nodeName].Count > 0;
    }

    /// <summary>
    ///   Reports whether the given node has dependees (i.e., depends on one or more other nodes).
    /// </summary>
    /// <returns> true if the node has dependees.</returns>
    /// <param name="nodeName">The name of the node.</param>
    public bool HasDependees(string nodeName)
    {
        // True if nodeName is in dependees and has more than zero dependees.
        return dependees.ContainsKey(nodeName) && dependees[nodeName].Count > 0;
    }

    /// <summary>
    ///   <para>
    ///     Returns the dependents of the node with the given name.
    ///   </para>
    /// </summary>
    /// <param name="nodeName"> The node we are looking at.</param>
    /// <returns> The dependents of nodeName. </returns>
    public IEnumerable<string> GetDependents(string nodeName)
    {
        // If nodeName isn't in dependents dictionary yet, return an empty HashSet.
        // Else, return the dependents HashSet of nodeName. 
        if (!dependents.ContainsKey(nodeName)) 
        { 
             return new HashSet<string>();
        }
        return dependents[nodeName]; 

        
    }

    /// <summary>
    ///   <para>
    ///     Returns the dependees of the node with the given name.
    ///   </para>
    /// </summary>
    /// <param name="nodeName"> The node we are looking at.</param>
    /// <returns> The dependees of nodeName. </returns>
    public IEnumerable<string> GetDependees(string nodeName)
    {
        // If nodeName isn't in deependees dictionary yet, return an empty HashSet,
        // Else, return the dependee HashSet of nodeName.
        if (!dependees.ContainsKey(nodeName))
        {
            return new HashSet<string>();
        }
        return dependees[nodeName];
    }

    /// <summary>
    /// <para>Adds the ordered pair (dependee, dependent), if it doesn't exist.</para>
    ///
    /// <para>
    ///   This can be thought of as: dependee must be evaluated before dependent
    /// </para>
    /// </summary>
    /// <param name="dependee"> the name of the node that must be evaluated first</param>
    /// <param name="dependent"> the name of the node that cannot be evaluated until after dependee</param>
    public void AddDependency(string dependee, string dependent)
    {
        // If dependent hasn't been added to the dependees Dictionary yet, add it and initialize a HashSet for it's dependees,
        // add dependee to the dependent's dependee set.
        if (!dependees.ContainsKey(dependent))
        {
            dependees.Add(dependent, new HashSet<string>());
        }
        dependees[dependent].Add(dependee);


        // If the dependee hasn't been added to the dependents Dictionary yet, add it and initialize a HashSet for it's dependents,
        // add dependent to the dependee's dependent set.
        if (!dependents.ContainsKey(dependee))
        {
            dependents.Add(dependee, new HashSet<string>());
        }
        dependents[dependee].Add(dependent);
    }

    /// <summary>
    ///   <para>
    ///     Removes the ordered pair (dependee, dependent), if it exists.
    ///   </para>
    /// </summary>
    /// <param name="dependee"> The name of the node that must be evaluated first</param>
    /// <param name="dependent"> The name of the node that cannot be evaluated until after dependee</param>
    public void RemoveDependency(string dependee, string dependent)
    {
        // If dependees contains dependent - remove it deprendee from its dependee set.
        if (dependees.ContainsKey(dependent))
        {
            dependees[dependent].Remove(dependee);
        }
        // If dependents contains dependee - remove dependent from its dependent set.
        if (dependents.ContainsKey(dependee))
        {
            dependents[dependee].Remove(dependent);
        }

    }

    /// <summary>
    ///   Removes all existing ordered pairs of the form (nodeName, *).  Then, for each
    ///   t in newDependents, adds the ordered pair (nodeName, t).
    /// </summary>
    /// <param name="nodeName"> The name of the node who's dependents are being replaced </param>
    /// <param name="newDependents"> The new dependents for nodeName</param>
    public void ReplaceDependents(string nodeName, IEnumerable<string> newDependents)
    {
        // If nodeName is in dependents, then remove all of its dependencies.
        if (dependents.ContainsKey(nodeName))
        {
            HashSet<string> dependentsOfNodeName = dependents[nodeName];
            foreach (var dependent in dependentsOfNodeName)
            {
                RemoveDependency(nodeName, dependent);
            }
        }
        // Add new dependencies from nodeName to the newDependents.
        foreach (var dependent in newDependents)
        {
            AddDependency(nodeName, dependent);
        }

    }

    /// <summary>
    ///   <para>
    ///     Removes all existing ordered pairs of the form (*, nodeName).  Then, for each
    ///     t in newDependees, adds the ordered pair (t, nodeName).
    ///   </para>
    /// </summary>
    /// <param name="nodeName"> The name of the node who's dependees are being replaced</param>
    /// <param name="newDependees"> The new dependees for nodeName</param>
    public void ReplaceDependees(string nodeName, IEnumerable<string> newDependees)
    {
        // If nodeName is in dependees, then remove all of its dependencies.
        if (dependees.ContainsKey(nodeName))
        {
            HashSet<string> dependeesOfNodeName = dependees[nodeName];
            foreach (var dependee in dependeesOfNodeName)
            {
                RemoveDependency(dependee, nodeName);
            }
        }
        // Add new dependencies from the newDependees to NodeName.
        foreach (var dependee in newDependees)
        {
            AddDependency(dependee, nodeName);
        }

    }
}
