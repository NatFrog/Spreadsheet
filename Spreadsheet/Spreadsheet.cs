// <copyright file="Spreadsheet.cs" company="UofU-CS3500">
// Copyright (c) 2024 UofU-CS3500. All rights reserved.
// </copyright>
// <author
// Natalie Hicks
// </author>
// <version>
// October 18th, 2024
// </version>

// Written by Joe Zachary for CS 3500, September 2013
// Update by Profs Kopta and de St. Germain, Fall 2021, Fall 2024
//     - Updated return types
//     - Updated documentation
namespace CS3500.Spreadsheet;

using CS3500.Formula;
using CS3500.DependencyGraph;
using System.Text.RegularExpressions;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text.Encodings.Web;


    /// <summary>
    ///   <para>
    ///     Thrown to indicate that a change to a cell will cause a circular dependency.
    ///   </para>
    /// </summary>
    public class CircularException : Exception
    {
    }

    /// <summary>
    ///   <para>
    ///     Thrown to indicate that a name parameter was invalid.
    ///   </para>
    /// </summary>
    public class InvalidNameException : Exception
    {
        public InvalidNameException(string? message) : base(message)
        {
        }
    }

    /// <summary>
    /// <para>
    /// Thrown to indicate that a read or write attempt has failed with
    /// an expected error message informing the user of what went wrong.
    /// </para>
    /// </summary>
    public class SpreadsheetReadWriteException : Exception
    {
        /// <summary>
        /// <para>
        /// Creates the exception with a message defining what went wrong.
        /// </para>
        /// </summary>
        /// <param name="msg"> An informative message to the user. </param>
        public SpreadsheetReadWriteException(string msg)
        : base(msg)
        {
        }
    }

    /// <summary>
    ///   <para>
    ///     A Spreadsheet object represents the state of a simple spreadsheet. A
    ///     spreadsheet represents an infinite number of named cells. The spreadsheet
    ///     has a Dictionary of strings mapped to cells to hold the cells and has a 
    ///     dependency graph to map the dependencies of the cells to other cells.
    /// </summary>
    public class Spreadsheet
{

    /// <summary>
    ///     Holds all the normalized names of non-empty cells in this spreadsheet
    /// </summary>
    [JsonInclude]
    private Dictionary<string, Cell> Cells;

    /// <summary>
    ///     Holds all the relationships between cells in this Spreadsheet
    /// </summary>
    [JsonIgnore]
    private DependencyGraph Graph;

    /// <summary>
    /// True if this spreadsheet has been changed since it was
    /// created or saved (whichever happened most recently),
    /// False otherwise.
    /// </summary>
    [JsonIgnore]
    public bool Changed { get; private set; }

    /// <summary>
    ///   All cell names are letters followed by numbers. This pattern
    ///   represents valid cell name strings. Code from Formula code,
    ///   used to check is Cell name is valid.
    /// </summary>
    [JsonIgnore]
    private const string VariableRegExPattern = @"[a-zA-Z]+\d+";


    
    /// <summary>
    /// This nested class represents the Cell object which holds content, value, and a string form.
    /// </summary>
    /// <remarks>
    ///  This constructor creates a cell that has contents
    /// </remarks>
    /// <param name="content"></param>
    private class Cell
    {   
        /// <summary>
        ///     Get/Set for the Content of this object.
        ///     Represents the content in a cell.
        /// </summary>
        [JsonIgnore]
        public object Content { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonIgnore]
        public object Value { get; set; }

        /// <summary>
        ///     The String Form of a cell depends on the type of content in a cell.
        /// </summary>
        [JsonInclude]
        public string StringForm { get; set; }

        /// <summary>
        ///     Default Constructor
        ///     Cell constructor initializes Content, Value and
        ///     StringForm. 
        /// </summary>
        /// <param name="content"></param>
        public Cell()
        {
            this.Content = String.Empty;
            this.Value = String.Empty;
            this.StringForm = String.Empty;
        }

        /// <summary>
        ///     String input Constructor
        ///     Cell constructor sets Content, Value and
        ///     StringForm. 
        /// </summary>
        /// <param name="content"></param>
        public Cell(string content)
        {
            this.Content = content;
            this.Value = content;
            this.StringForm = content;
        }

        /// <summary>
        ///     Double input Constructor
        ///     Cell constructor sets Content, Value and
        ///     StringForm. 
        /// </summary>
        /// <param name="content"></param>
        public Cell(double content)
        {
            this.Content = content;
            this.Value = content;    //possible error
            this.StringForm = content.ToString(); 
        }

        /// <summary>
        ///     Formula input Constructor
        ///     Cell constructor sets Content, initializes Value and
        ///     sets StringForm. 
        /// </summary>
        /// <param name="content"></param>
        public Cell(Formula content)
        {
            this.Content = content;
            //Value will be set in SetContentsOfCell
            this.Value = String.Empty;
            this.StringForm = "=" + content.ToString();
        }
    }

    /// <summary>
    ///     Default Constructor for a spreadsheet. No cells have been added
    ///     and nothing in the spreadsheet has been changed.
    /// </summary>
    public Spreadsheet()
    {
        Cells = [];
        Graph = new();
        Changed = false;
    }

    /// <summary>
    ///   Provides a copy of the normalized names of all of the cells in the spreadsheet
    ///   that contain information (i.e., non-empty cells).
    /// </summary>
    /// <returns>
    ///   A set of the names of all the non-empty cells in the spreadsheet.
    /// </returns>
    public ISet<string> GetNamesOfAllNonemptyCells()
    {
        HashSet<string> names = new HashSet<string>();
        return Cells.Keys.ToHashSet();
    }

    /// <summary>
    ///   Returns the contents (as opposed to the value) of the named cell.
    /// </summary>
    ///
    /// <exception cref="InvalidNameException">
    ///   Thrown if the name is invalid.
    /// </exception>
    ///
    /// <param name="name">The name of the spreadsheet cell to query. </param>
    /// <returns>
    ///   The contents as either a string, a double, or a Formula.
    ///   See the class header summary.
    /// </returns>
    public object GetCellContents(string name)
    {
        string normName = name.ToUpper();

        // IsCellName will throw InvalidNameException if invalid
        IsCellName(normName);

        if (!Cells.TryGetValue(normName, out Cell? value))
        {
            return string.Empty;
        }
        return Cells[normName].Content;
    }
    
    /// <summary>
    ///  Set the contents of the named cell to the given number.
    /// </summary>
    ///
    /// <exception cref="InvalidNameException">
    ///   If the name is invalid, throw an InvalidNameException.
    /// </exception>
    ///
    /// <param name="name"> The name of the cell. </param>
    /// <param name="number"> The new contents of the cell. </param>
    /// <returns>
    ///   <para>
    ///     This method returns an ordered list consisting of the passed in name
    ///     followed by the names of all other cells whose value depends, directly
    ///     or indirectly, on the named cell.
    ///   </para>
    ///   <para>
    ///     The order must correspond to a valid dependency ordering for recomputing
    ///     all of the cells, i.e., if you re-evaluate each cells in the order of the list,
    ///     the overall spreadsheet will be correctly updated.
    ///   </para>
    /// </returns>
    private IList<string> SetCellContents(string name, double number)
    {
        string upperName = name.ToUpper();

        // Checks that upperName is a valid cell name
        IsCellName(upperName);

        // Updates the Cell with name "name" in cells to have number as contents
        Cells[upperName] = new Cell(number);
        
        // List to hold "name" followed by all the cells that need to be re-evaluated afterwards
        List<string> cellNames = GetCellsToRecalculate(upperName).ToList();

        // Delete any dependees if name had dependees
        //if (Graph.HasDependees(upperName))   
        //{
            Graph.ReplaceDependees(upperName, []);
        //}
        return cellNames;
    }

    /// <summary>
    ///   The contents of the named cell becomes the given text.
    /// </summary>
    ///
    /// <exception cref="InvalidNameException">
    ///   If the name is invalid, throw an InvalidNameException.
    /// </exception>
    /// <param name="name"> The name of the cell. </param>
    /// <param name="text"> The new contents of the cell. </param>
    /// <returns>
    ///   The same list as defined in <see cref="SetCellContents(string, double)"/>.
    /// </returns>
    private IList<string> SetCellContents(string name, string text)
    {
        string upperName = name.ToUpper();
      
        // Checks that upperName is a valid cell name
        IsCellName(upperName);

        // Update the Cell with name "name" in cells to have text as contents
        Cells[upperName] = new Cell(text);

        // List to hold "name" followed by all the cells that need to be re-evaluated afterwards
        List<string> cellNames = GetCellsToRecalculate(upperName).ToList();

        // If new content is an empty string then remove it from cells and values, and the 
        // cells value no longer needs to get recalculated, so remove it from cellNames
        if (text.Equals(string.Empty))                           
        {
            Cells.Remove(upperName);   ///
           cellNames.Remove(upperName);
        }

        // Delete any dependees if name has dependees
        //if (Graph.HasDependees(upperName))
        //{
            Graph.ReplaceDependees(upperName, []);
        //}
        return cellNames;
    }

    /// <summary>
    ///   Set the contents of the named cell to the given formula.
    /// </summary>
    /// <exception cref="InvalidNameException">
    ///   If the name is invalid, throw an InvalidNameException.
    /// </exception>
    /// <exception cref="CircularException">
    ///   <para>
    ///     If changing the contents of the named cell to be the formula would
    ///     cause a circular dependency, throw a CircularException, and no
    ///     change is made to the spreadsheet.
    ///   </para>
    /// </exception>
    /// <param name="name"> The name of the cell. </param>
    /// <param name="formula"> The new contents of the cell. </param>
    /// <returns>
    ///   The same list as defined in <see cref="SetCellContents(string, double)"/>.
    /// </returns>
    private IList<string> SetCellContents(string name, Formula formula)
    {
        string upperName = name.ToUpper();

        // Checks that upperName is a valid cell name, throws if not
        IsCellName(upperName);

        //The initial cell status
        Cell initialCell;

        // If cells doesn't have cell "name" in dictionary yet, then add it to cells
        if (!Cells.TryGetValue(upperName, out Cell? cell))
        {
            Cells.Add(upperName, new Cell(formula));
            initialCell = new Cell("");
        }
        else
        { // Update the Cell with name "name" in cells to have formula as contents
            initialCell = Cells[upperName];
            cell = new Cell(formula);   
            Cells[upperName] = cell;
        }

        //Store old dependees in case of circular exception
        List<string> oldDependees = Graph.GetDependees(upperName).ToList();

        // List to hold "name" followed by all the cells that need to be re-evaluated afterwards
        List<string> cellNames = GetCellsToRecalculate(upperName).ToList(); 

        //Updates the dependees of this cell to be all of the cells that are referenced in formula
        Graph.ReplaceDependees(upperName, formula.GetVariables());

        //Check for circular exception
        try
        {
            _ = GetCellsToRecalculate(upperName).ToList();
        }
        catch (CircularException)
        {
            if (initialCell.Content.Equals(""))
            {
                Cells.Remove(upperName);
            }
            //replace with old value so spreadsheet isn't changed if circular exception is thrown
            else
            {
                Cells[upperName] = initialCell;
            }
                //Replace dependees of name with original dependees 
                Graph.ReplaceDependees(upperName, oldDependees);

                throw new CircularException();
        }
        return cellNames;
    }

    /// <summary>
    ///      Reports whether string is a valid cell name.  It must be one or more letters
    ///   followed by one or more numbers. Code from Formula assignment. Throws InvalidNameException
    ///   if the name isn't a valid cell name.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    /// <exception cref="InvalidNameException"></exception>
    private static bool IsCellName(string name)
    {
        string standaloneVarPattern = $"^{VariableRegExPattern}$";
        bool result =  Regex.IsMatch(name, standaloneVarPattern);

        //Throw InvalidNameException when checking name to save code
        if (!result)
        {
            throw new InvalidNameException("This cell name is invalid");
        }
        return result;
    }

    /// <summary>
    ///   Returns an enumeration, without duplicates, of the names of all cells whose
    ///   values depend directly on the value of the named cell.
    /// </summary>
    /// <param name="name"> This <b>MUST</b> be a valid name.  </param>
    /// <returns>
    ///   <para>
    ///     Returns an enumeration, without duplicates, of the names of all cells
    ///     that contain formulas containing name.
    ///   </para>
    ///   <para>For example, suppose that: </para>
    ///   <list type="bullet">
    ///      <item>A1 contains 3</item>
    ///      <item>B1 contains the formula A1 * A1</item>
    ///      <item>C1 contains the formula B1 + A1</item>
    ///      <item>D1 contains the formula B1 - C1</item>
    ///   </list>
    ///   <para> The direct dependents of A1 are B1 and C1. </para>
    /// </returns>
    private IEnumerable<string> GetDirectDependents(string name)
    {
        return Graph.GetDependents(name);
    }

    /// <summary>
    ///   <para>
    ///     This method is implemented for you, but makes use of your GetDirectDependents.
    ///   </para>
    ///   <para>
    ///     Returns an enumeration of the names of all cells whose values must
    ///     be recalculated, assuming that the contents of the cell referred
    ///     to by name has changed.  The cell names are enumerated in an order
    ///     in which the calculations should be done.
    ///   </para>
    ///   <exception cref="CircularException">
    ///     If the cell referred to by name is involved in a circular dependency,
    ///     throws a CircularException.
    ///   </exception>
    ///   <para>
    ///     For example, suppose that:
    ///   </para>
    ///   <list type="number">
    ///     <item>
    ///       A1 contains 5
    ///     </item>
    ///     <item>
    ///       B1 contains the formula A1 + 2.
    ///     </item>
    ///     <item>
    ///       C1 contains the formula A1 + B1.
    ///     </item>
    ///     <item>
    ///       D1 contains the formula A1 * 7.
    ///     </item>
    ///     <item>
    ///       E1 contains 15
    ///     </item>
    ///   </list>
    ///   <para>
    ///     If A1 has changed, then A1, B1, C1, and D1 must be recalculated,
    ///     and they must be recalculated in an order which has A1 first, and B1 before C1
    ///     (there are multiple such valid orders).
    ///     The method will produce one of those enumerations.
    ///   </para>
    ///   <para>
    ///      PLEASE NOTE THAT THIS METHOD DEPENDS ON THE METHOD GetDirectDependents.
    ///      IT WON'T WORK UNTIL GetDirectDependents IS IMPLEMENTED CORRECTLY.
    ///   </para>
    /// </summary>
    /// <param name="name"> The name of the cell.  Requires that name be a valid cell name.</param>
    /// <returns>
    ///    Returns an enumeration of the names of all cells whose values must
    ///    be recalculated.
    /// </returns>
    private IEnumerable<string> GetCellsToRecalculate(string name)
    {
        LinkedList<string> changed = new();
        HashSet<string> visited = [];
        Visit(name, name, visited, changed);
        return changed;
    }

    /// <summary>
    ///   A helper for the GetCellsToRecalculate method.
    ///   Visits each dependent of start, and then the dependents of those in order to get a list of all the
    ///   direct and indirect dependents of "start"
    ///   as well as inline comments in the code.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="name"></param>
    /// <param name="visited"></param>
    /// <param name="changed"></param>
    /// <exception cref="CircularException"></exception>
    private void Visit(string start, string name, ISet<string> visited, LinkedList<string> changed)
    {
        // Adds name to visited
        visited.Add(name);
        // For each direct dependent of name, as long as n is not the same as name,
        // do the recursive call with name set to n.
        foreach (string n in GetDirectDependents(name))
        {
            if (n.Equals(start)) // If the current dependent is the same as the start cell -> circular dependency
            { 
                throw new CircularException();  // throws a CircularException if any Cell's contents contain start
            }
            else if (!visited.Contains(n))      // if all the nodes visited don't contain current dependee
            {
                Visit(start, n, visited, changed); // Recursive call, n is set as value of name
            }
        }
        // After the recursion is over changed is ordered, and name is added to the front.
        changed.AddFirst(name);
    }


    /// <summary>
    /// <para>
    /// Return the value of the named cell, as defined by
    /// <see cref="GetCellValue(string)"/>.
    /// </para>
    /// </summary>
    /// <param name="name"> The cell in question. </param>
    /// <returns>
    /// <see cref="GetCellValue(string)"/>
    /// </returns>
    /// <exception cref="InvalidNameException">
    /// If the provided name is invalid, throws an InvalidNameException.
    /// </exception>
    public object this[string name]
    {
        get
        {
            if (!Cells.ContainsKey(name))
            {
                throw new InvalidNameException("The spreadsheet doesn't contain the cell name entered");
            }
            return GetCellValue(name);
        }
    }

    /// <summary>
    /// Constructs a spreadsheet using the saved data in the file referred to by
    /// the given filename.
    /// <see cref="Save(string)"/>
    /// </summary>
    /// <exception cref="SpreadsheetReadWriteException">
    /// Thrown if the file can not be loaded into a spreadsheet for any reason
    /// </exception>
    /// <param name="filename">The path to the file containing the spreadsheet to
    ///load</param>
    public Spreadsheet(string filename)
        {
        Cells = [];
        Graph = new DependencyGraph();

        try
        {
            string file = File.ReadAllText(filename);
            Spreadsheet? savedSpreadSheet = JsonSerializer.Deserialize<Spreadsheet>(file);

            if(savedSpreadSheet != null)
            {
                foreach(string s in savedSpreadSheet.Cells.Keys)
                {
                    SetContentsOfCell(s, savedSpreadSheet.Cells[s].StringForm);
                }
            }
        }
        catch (Exception)
        {
            throw new SpreadsheetReadWriteException("Error loading file into spreadsheet");
        }

    }

    /// <summary>
    /// <para>
    /// Writes the contents of this spreadsheet to the named file using a JSON
    /// format.
    /// If the file already exists, overwrite it.
    /// </para>
    /// <para>
    /// The output JSON should look like the following.
    /// </para>
    /// <para>
    /// For example, consider a spreadsheet that contains a cell "A1"
    /// with contents being the double 5.0, and a cell "B3" with contents
    /// being the Formula("A1+2"), and a cell "C4" with the contents "hello".
    /// </para>
    /// <para>
    /// This method would produce the following JSON string:
    /// </para>
    /// <code>
    /// {
    /// "Cells": {
    /// "A1": {
    /// "StringForm": "5"
    /// },
    /// "B3": {
    /// "StringForm": "=A1+2"
    /// },
    /// "C4": {
    /// "StringForm": "hello"
    /// }
    /// }
    /// }
    /// </code>
    /// <para>
    /// You can achieve this by making sure your data structure is a dictionary
    /// and that the contained objects (Cells) have property named "StringForm"
    /// (if this name does not match your existing code, use the
    /// JsonPropertyName
    /// attribute).
    /// </para>
    /// <para>
    /// There can be 0 cells in the dictionary, resulting in { "Cells" : {} }
    /// </para>
    /// <para>
    /// Further, when writing the value of each cell...
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// If the contents is a string, the value of StringForm is that string
    /// </item>
    /// <item>
    /// If the contents is a double d, the value of StringForm is
    /// d.ToString()
    /// </item>
    /// <item>
    /// If the contents is a Formula f, the value of StringForm is "=" +
    /// f.ToString()
    /// </item>
    /// </list>
    /// </summary>
    /// <param name="filename"> The name (with path) of the file to save
    /// to.</param>
    /// <exception cref="SpreadsheetReadWriteException">
    /// If there are any problems opening, writing, or closing the file,
    /// the method should throw a SpreadsheetReadWriteException with an
    /// explanatory message.
    /// </exception>
    public void Save(string filename)
    {
        try
        {
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            string text = JsonSerializer.Serialize(this, jsonOptions);
            Changed = false;
            File.WriteAllText(filename, text);
        } 
        catch (Exception) 
        {
            throw new SpreadsheetReadWriteException("The spreadsheet failed to be written, opened, or closed");
        }
    }

    /// <summary>
    /// <para>
    /// Return the value of the named cell.
    /// </para>
    /// </summary>
    /// <param name="name"> The cell in question. </param>
    /// <returns>
    /// Returns the value (as opposed to the contents) of the named cell. The
    /// return
    /// value should be either a string, a double, or a
    /// CS3500.Formula.FormulaError.
    /// </returns>
    /// <exception cref="InvalidNameException">
    /// If the provided name is invalid, throws an InvalidNameException.
    /// </exception>
    public object GetCellValue(string name)
    {
        string cellName = name.ToUpper();

        // IsCellName will throw InvalidNameException if name is invalid
        IsCellName(cellName);

        // Throw exception if cells does not contain a Cell with for name 
        if (!Cells.ContainsKey(cellName))   //simplify this? /need this?
        {
            return String.Empty;
        }
     
        return Cells[cellName].Value;

    } 

    /// <summary>
    /// Helper method that looks up the value of a cell given its name. 
    /// Passed as parameter for Evaluate when it's used.
    /// 
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    private double LookupValue(string s)
    {
        object value = GetCellValue(s);

        if (value is not Double)
        {
            throw new ArgumentException();
        }
        else
        {
            return (double)value;
        }
    }

    /// <summary>
    /// <para>
    /// Set the contents of the named cell to be the provided string
    /// which will either represent (1) a string, (2) a number, or
    /// (3) a formula (based on the prepended '=' character).
    /// </para>
    /// <para>
    /// Rules of parsing the input string:
    /// </para>
    /// <list type="bullet">
    /// <item>
    /// <para>
    /// If 'content' parses as a double, the contents of the named
    /// cell becomes that double.
    /// </para>
    /// </item>
    /// <item>
    /// If the string does not begin with an '=', the contents of the
    /// named cell becomes 'content'.
    /// </item>
    /// <item>
    /// <para>
    /// If 'content' begins with the character '=', an attempt is made
    /// to parse the remainder of content into a Formula f using the
    /// Formula
    /// constructor. There are then three possibilities:
    /// </para>
    /// <list type="number">
    /// <item>
    /// If the remainder of content cannot be parsed into a Formula, a
    /// CS3500.Formula.FormulaFormatException is thrown.
    /// </item>
    /// <item>
    /// Otherwise, if changing the contents of the named cell to be f
    /// would cause a circular dependency, a CircularException is thrown,
    /// and no change is made to the spreadsheet.
    /// </item>
    /// <item>
    /// Otherwise, the contents of the named cell becomes f.
    /// </item>
    /// </list>
    /// </item>
    /// </list>
    /// </summary>
    /// <returns>
    /// <para>
    /// The method returns a list consisting of the name plus the names
    /// of all other cells whose value depends, directly or indirectly,
    /// on the named cell. The order of the list should be any order
    /// such that if cells are re-evaluated in that order, their dependencies
    /// are satisfied by the time they are evaluated.
    /// </para>
    /// <example>
    /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1,
    /// the
    /// list {A1, B1, C1} is returned.
    /// </example>
    /// </returns>
    /// <exception cref="InvalidNameException">
    /// If name is invalid, throws an InvalidNameException.
    /// </exception>
    /// <exception cref="CircularException">
    /// If a formula would result in a circular dependency, throws
    /// CircularException.
    /// </exception>
    public IList<string> SetContentsOfCell(string name, string content)
    {
        string cellName = name.ToUpper();

        // Throw exception if cells does not contain a Cell with normalized "name"
        IsCellName(cellName);

        List<string> orderedList;

        // If the string is a double
        if (Double.TryParse(content, out double _))
        {
            orderedList = (List<string>)SetCellContents(cellName, double.Parse(content));
        }
        // Check if the string starts with "=", if so, it's a formula.
        else if (content.StartsWith('='))
        {
            string formulaText = content.Substring(1);

            Formula formula = new Formula(formulaText);
            orderedList = (List<string>)SetCellContents(cellName, formula);
        }
        // Else, content of cell should be a string.
        else
        {
            orderedList = (List<string>)SetCellContents(cellName, content);
        }

        // It's true that spreadsheet has been changed at this point.
        if (Changed == false)
        {
            Changed = true;
        }

        //  Recalculate values of cells that need to be recalculated 
        RecalculateCellValues(orderedList);

        return orderedList;
    }

    /// <summary>
    /// This private helper method is used to recalculate cells that need to have their values updated if they are dependents 
    /// of another cell that was changed
    /// </summary>
    /// <param name="list"> 
    ///     List of cells in order to recalculate
    /// </param>
    public void RecalculateCellValues(List<string> list)
    {
        // Calculate the cells' new values in the order they should be calculated in based on dependencies
        foreach (string cellName in list)
        {
            object content = Cells[cellName].Content;
            
            // If it is a formula then evaluate it to get the value
            if (content is Formula)
            {
                Formula f = (Formula)content;
                Cells[cellName].Value = f.Evaluate(LookupValue);
            }
        }
    }
}
