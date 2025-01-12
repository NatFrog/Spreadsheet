// <copyright file="Formula_PS2.cs" company="UofU-CS3500">
// Copyright (c) 2024 UofU-CS3500. All rights reserved.
// </copyright>
// <author>
//     Natalie Hicks
// </author>
// <version>
//     September 20th, 2024
// </version>
// <summary>
//   <para>
//     This code is provides to start your assignment.  It was written
//     by Profs Joe, Danny, and Jim.  You should keep this attribution
//     at the top of your code where you have your header comment, along
//     with the other required information.
//   </para>
//   <para>
//     You should remove/add/adjust comments in your file as appropriate
//     to represent your work and any changes you make.
//   </para>
// </summary>


namespace CS3500.Formula;


using System.Text.RegularExpressions;

/// <summary>
///   <para>
///     This class represents formulas written in standard infix notation using standard precedence
///     rules.  The allowed symbols are non-negative numbers written using double-precision
///     floating-point syntax; variables that consist of one ore more letters followed by
///     one or more numbers; parentheses; and the four operator symbols +, -, *, and /. This class
///     contains the Formula constructor, and the GetVariables and ToString methods for Formulas.
/// </summary>
public class Formula
{
    /// <summary>
    ///   All variables are letters followed by numbers.  This pattern
    ///   represents valid variable name strings.
    /// </summary>
    private const string VariableRegExPattern = @"[a-zA-Z]+\d+";

    /// <summary>
    ///     List that holds all the tokens in this Formula.
    /// </summary>
    private List<string> tokens;

    /// <summary>
    ///     String that holds the string version of this Formula.
    /// </summary>
    private string formulaString;

    /// <summary>
    ///     Stack to be used by the evaluate method that holds all the values in this Formula from left to right
    /// </summary>
    private Stack<double> values;

    /// <summary>
    ///     Stack to be used by the evaluate method that holds all the operators in this Formula from left to right
    /// </summary>
    private Stack<string> operators;

    /// <summary>
    ///   Initializes a new instance of the <see cref="Formula"/> class.
    ///   <para>
    ///     Creates a Formula from a string that consists of an infix expression written as
    ///     described in the class comment.  If the expression is syntactically incorrect,
    ///     throws a FormulaFormatException with an explanatory Message. 
    ///   </para>
    /// </summary>
    /// <param name="formula"> The string representation of the formula to be created.</param>
    public Formula(string formula)
    {
        //Variables to keep track of how many open and end parentheses have occurred.
        int OpenParens = 0;
        int EndParens = 0;

        tokens = GetTokens(formula);
        formulaString = string.Empty;
        values = new Stack<double>();
        operators = new Stack<string>();    

        //Throws FormulaFormatException if there are no tokens in the Formula.
        if (tokens.Count is 0)
            throw new FormulaFormatException("Formula is Invalid");

        string previous = tokens.ElementAt(0);

        //If there's only 1 token, check if it is a valid token and return.
        if (tokens.Count is 1)
        {
            if (IsVar(previous) || double.TryParse(previous, out _))
            {
                if (IsVar(previous))
                    formulaString += previous.ToUpper();
                else
                {
                    double d = double.Parse(previous);
                    formulaString += d.ToString();
                }
                return;
            }
            else
                throw new FormulaFormatException("Formula is invalid");
        }

        //Loops through tokens to check if every token is valid and adds tokens to formulaString.
        for (int i = 0; i < tokens.Count; i++)
        {

            string token = tokens.ElementAt(i);
            //If this is the first token in the formula, checks that it's valid.
            if (i == 0)
            {
                if (token.Equals(")") || token.Equals("+") || token.Equals("-") || token.Equals("/") || token.Equals("*"))
                    throw new FormulaFormatException("Formula is Invalid");
                else if (!IsVar(token) && !double.TryParse(token, out _) && !token.Equals("("))
                    throw new FormulaFormatException("Formula is Invalid");
            }

            //Adds the normalized version of token to formulaString.
            if (IsVar(token))
                formulaString += token.ToUpper();
            else if (double.TryParse(token, out _) == true)
            {
                double d = double.Parse(token);
                formulaString += d.ToString();
            }
            else
                formulaString += token;

            //Increments parentheses counts if the token is a parentheses, and throws FormulaFormatException if there are ever more end parentheses than opening parentheses.
            if (token.Equals("("))
                OpenParens++;
            else if (token.Equals(")"))
                EndParens++;
            if (EndParens > OpenParens)
                throw new FormulaFormatException("Formula is Invalid");

            if (i == 0)
                continue;

            //Uses helper method to check if token is valid coming after token; throws exception if not.
            if (ValidFollowing(previous, token) == false)
            {
                throw new FormulaFormatException("Formula is Invalid");
            }
            previous = token;
        }
        //FormulaFormatException is thrown if a parentheses is missing its pair.
        if (OpenParens != EndParens)
            throw new FormulaFormatException("Formula is Invalid");

        //Makes sure that the last token in the Formula is valid to be last.
        if (previous.Equals("(") || previous.Equals("+") || previous.Equals("-") || previous.Equals("/") || previous.Equals("*"))
            throw new FormulaFormatException("Formula is Invalid");

    }

    /// <summary>
    ///     This helper method checks if it's acceptable for token2 to follow after token1. Used by
    ///     the Formula constructor to ensure that all tokens in the Formula adhere to the given
    ///     rules for how a Formula should function.
    /// </summary>
    /// <param name="token1">
    ///     The first token out of the two.
    /// </param>
    /// <param name="token2">
    ///     The second token, comes after token1.
    /// </param>
    /// <returns>
    ///     True if the token2 following token1 is valid.
    /// </returns>
    private static Boolean ValidFollowing(string token1, string token2)
    {
        //If the first token is an operator or opening parentheses.
        if (token1.Equals("(") || token1.Equals("+") || token1.Equals("-") || token1.Equals("/") || token1.Equals("*"))
        {
            if (IsVar(token2) || double.TryParse(token2, out _) || token2.Equals("("))
                return true;
        }
        //If the first token is a variable, number, or end parentheses.
        else if (IsVar(token1) || double.TryParse(token1, out _) || token1.Equals(")"))
        {
            if (token2.Equals("+") || token2.Equals("-") || token2.Equals("/") || token2.Equals("*") || token2.Equals(")"))
                return true;
        }
        return false;
    }

    /// <summary>
    ///   <para>
    ///     Returns a set of all the variables in the formula.
    ///   </para>
    /// </summary>
    /// <returns> the set of variables (string names) representing the variables referenced by the formula. </returns>
    public ISet<string> GetVariables()
    {
        HashSet<string> variables = new HashSet<string>();

        foreach (string token in tokens)
        {
            //Converts token to uppercase because all variables are normalized.
            string normToken = token.ToUpper();

            if (IsVar(normToken) && !variables.Contains(normToken))
            {
                variables.Add(normToken);
            }
        }
        return variables;
    }

    /// <summary>
    ///   <para>
    ///     Returns a string representation of a canonical form of the formula. The string gets rid 
    ///     of spaces. All variables in the string will be normalized, and numbers should be normalized.
    ///     Should execute in O(1) time.
    ///   </para>
    /// <returns>
    ///   A canonical version (string) of the formula. All "equal" formulas
    ///   should have the same value here.
    /// </returns>
    public override string ToString()
    {
        return formulaString;
    }

    /// <summary>
    ///   Reports whether "token" is a variable.  It must be one or more letters
    ///   followed by one or more numbers.
    /// </summary>
    /// <param name="token"> A token that may be a variable. </param>
    /// <returns> true if the string matches the requirements, e.g., A1 or a1. </returns>
    private static bool IsVar(string token)
    {
        // notice the use of ^ and $ to denote that the entire string being matched is just the variable
        string standaloneVarPattern = $"^{VariableRegExPattern}$";
        return Regex.IsMatch(token, standaloneVarPattern);
    }

    /// <summary>
    ///   <para>
    ///     Given an expression, enumerates the tokens that compose it.
    ///   </para>
    ///   <para>
    ///     Tokens returned are:
    ///   </para>
    ///   <list type="bullet">
    ///     <item>left paren</item>
    ///     <item>right paren</item>
    ///     <item>one of the four operator symbols</item>
    ///     <item>a string consisting of one or more letters followed by one or more numbers</item>
    ///     <item>a double literal</item>
    ///     <item>and anything that doesn't match one of the above patterns</item>
    ///   </list>
    ///   <para>
    ///     There are no empty tokens; white space is ignored (except to separate other tokens).
    ///   </para>
    /// </summary>
    /// <param name="formula"> A string representing an infix formula such as 1*B1/3.0. </param>
    /// <returns> The ordered list of tokens in the formula. </returns>
    private static List<string> GetTokens(string formula)
    {
        List<string> results = [];

        string lpPattern = @"\(";
        string rpPattern = @"\)";
        string opPattern = @"[\+\-*/]";
        string doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
        string spacePattern = @"\s+";

        // Overall pattern
        string pattern = string.Format(
                                        "({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                        lpPattern,
                                        rpPattern,
                                        opPattern,
                                        VariableRegExPattern,
                                        doublePattern,
                                        spacePattern);

        // Enumerate matching tokens that don't consist solely of white space.
        foreach (string s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
        {
            if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
            {
                results.Add(s);
            }
        }

        return results;
    }

    /// <summary>
    /// <para>
    /// Reports whether f1 == f2, using the notion of equality from the <see
    //cref="Equals"/> method.
    /// </para>
    /// </summary>
    /// <param name="f1"> The first of two formula objects. </param>
    /// <param name="f2"> The second of two formula objects. </param>
    /// <returns> true if the two formulas are the same.</returns>
    public static bool operator ==(Formula f1, Formula f2)
    {
         return f1.Equals(f2);
    }

    /// <summary>
    /// <para>
    /// Reports whether f1 != f2, using the notion of equality from the <see
    /// cref="Equals"/> method.
    /// </para>
    /// </summary>
    /// <param name="f1"> The first of two formula objects. </param>
    /// <param name="f2"> The second of two formula objects. </param>
    /// <returns> true if the two formulas are not equal to each other.</returns>
    public static bool operator !=(Formula f1, Formula f2)
    {
        return !(f1 == f2);
    }

    /// <summary>
    /// <para>
    /// Determines if two formula objects represent the same formula.
    /// </para>
    /// <para>
    /// By definition, if the parameter is null or does not reference
    /// a Formula Object then return false.
    /// </para>
    /// <para>
    /// Two Formulas are considered equal if their canonical string representations
    /// (as defined by ToString) are equal.
    /// </para>
    /// </summary>
    /// <param name="obj"> The other object.</param>
    /// <returns>
    /// True if the two objects represent the same formula.
    // </returns>
    public override bool Equals(object? obj)
    {
        // If the parameter is not null and is a Formula object, compares Formulas by their ToString representations
        Formula? f = obj as Formula;

        if (f is not null && obj is Formula)
        {
            return f.ToString().Equals(this.ToString());
        }
        return false; 
    }
    /// <summary>
    /// <para>
    /// Evaluates this Formula, using the lookup delegate to determine the values
    ///of
    /// variables.
    /// </para>
    /// <remarks>
    /// When the lookup method is called, it will always be passed a normalized
    /// (capitalized)
    /// variable name. The lookup method will throw an ArgumentException if there
    /// is
    /// not a definition for that variable token.
    /// </remarks>
    /// <para>
    /// If no undefined variables or divisions by zero are encountered when
    //evaluating
    /// this Formula, the numeric value of the formula is returned. Otherwise, a
    /// FormulaError is returned (with a meaningful explanation as the Reason
    ///property).
    /// </para>
    /// <para>
    /// This method should never throw an exception.
    /// </para>
    /// </summary>
    /// <param name="lookup">
    /// <para>
    /// Given a variable symbol as its parameter, lookup returns the variable's
    ///value
    /// (if it has one) or throws an ArgumentException (otherwise). This method
    ///will expect
    /// variable names to be normalized.
    /// </para>
    /// </param>
    /// <returns> Either a double or a FormulaError, based on evaluating the formula.</
    //returns>
    public object Evaluate(Lookup lookup)
        {
        object result = ProcessFormula(lookup);

        return result;
        }

    /// <summary>
    ///     Helper method to calculate the value of this formula
    /// </summary>
    /// <returns></returns>
    private object ProcessFormula(Lookup lookup)
    {
        // int to keep track of the value of this Formula
        double sum;
        // List<String> tokenStrings = [.. this.ToString().Split("")];

        // Goes through every token, t, in this Formula
        foreach (string t in tokens)   //before this was a foreach of tokens
        {
            // If t is a number
            if (double.TryParse(t, out _)) {

                // Holds the double value of this token
                double tValue = double.Parse(t);

                if (IsOnTop("*", operators))
                {
                    operators.Pop();
                    sum = values.Pop() * tValue;
                    values.Push(sum);
                }
                else if (IsOnTop("/", operators))
                {
                    if (tValue.Equals(0))
                        return new FormulaError("Cannot divide by zero");

                    operators.Pop();
                    sum = values.Pop() / tValue;
                    values.Push(sum);
                }
                else
                    values.Push(tValue);
            }
            // If t is a variable
            else if (IsVar(t)) 
            {
                // Holds the double value of this token
                double tValue = 0;

                //If the lookup of t has no value then returns FormulaError
                try
                {
                    tValue = lookup(t.ToUpper());
                }
                catch (ArgumentException)
                {
                    return new FormulaError("Variable has no value");

                }    
                if (IsOnTop("*", operators)) 
                {
                    operators.Pop();
                    sum = values.Pop() * tValue;
                    values.Push(sum);
                }
                else if (IsOnTop("/", operators))
                {
                    if (tValue.Equals(0))
                        return new FormulaError("Cannot divide by zero");

                    operators.Pop();
                    sum = values.Pop() / tValue;
                    values.Push(sum);
                }
                else
                    values.Push(tValue);
            }

            // If t is + or -
            else if (t.Equals("+") || t.Equals("-"))
            {
                if (IsOnTop("+", operators)) 
                {
                    operators.Pop();
                    sum = values.Pop() + values.Pop();
                    values.Push(sum);
                    operators.Push(t);
                }
                else if (IsOnTop("-", operators)) 
                {
                    operators.Pop();
                    double secondNum = values.Pop();
                    sum = values.Pop() - secondNum;
                    values.Push(sum);
                    operators.Push(t);
                }                    
                else
                    operators.Push(t);
            }

            // If t is * or /
            else if (t.Equals("*") || t.Equals("/"))
            {
                operators.Push(t);
            }

            // If t is "("
            else if (t.Equals("("))
            {
                operators.Push(t);
            }

            // If t is ")"
            else if (t.Equals(")"))
            {
                // If the top of operators is + or -, Pop 2 values and apply the operator, push back into stack
                if (IsOnTop("+", operators)) 
                {
                    operators.Pop();
                    sum = values.Pop() + values.Pop();
                    values.Push(sum);
                }
                else if (operators.Peek().Equals("-"))
                {
                    operators.Pop();
                    double secondNum = values.Pop();
                    sum = values.Pop() - secondNum;
                    values.Push(sum);
                }
                // The operator here will be a (, Pop it
                operators.Pop();

                // If the operator is a * or /, Pop 2 values and apply the operator, push back into stack
                if (IsOnTop("*", operators)) 
                {
                    operators.Pop();
                    sum = values.Pop() * values.Pop();
                    values.Push(sum);
                }
                else if (IsOnTop("/", operators))
                {

                    operators.Pop();
                    double secondVal = values.Pop();
                    double firstVal = values.Pop();

                    if (secondVal.Equals(0))
                        return new FormulaError("Cannot divide by zero");

                    sum = firstVal / secondVal;
                    values.Push(sum);
                }
            }
        }

        // If operator stack is empty, values has 1 number left
        if (operators.Count.Equals(0))
        {
            return values.Pop();
        }
        //If Operator stack isn't empty, values has 2 numbers left
        else if (IsOnTop("+", operators)) 
        {
            operators.Pop();
            sum = values.Pop() + values.Pop();
        }
        else
        {
            // Need to subtract the most recently number from the old sum
            operators.Pop();
            double secondNum = values.Pop();
            sum = values.Pop() - secondNum;
        }
     
        return sum;
    }

    /// <summary>
    /// This private helper method is used to check if a stack isn't empty, and if it isn't 
    /// it checks whether or not param string is on top of the stack.
    /// </summary>
    /// <param name="s"> - string you are checking if it's on top </param> 
    /// <param name="stack"> - the stack you are checking the top of </param>
    /// <returns></returns>
    private Boolean IsOnTop(string s, Stack<string> stack)
    {
        if (stack.Count != 0 && operators.Peek().Equals(s))
            return true;
        return false;
    }

    /// <summary>
    /// <para>
    /// Returns a hash code for this Formula. If f1.Equals(f2), then it must be
    ///the
    /// case that f1.GetHashCode() == f2.GetHashCode(). Ideally, the probability
    ///that two
    /// randomly-generated unequal Formulas have the same hash code should be
    ///extremely small.
    /// </para>
    /// </summary>
    /// <returns> The hashcode for the object. </returns>
    public override int GetHashCode()
    {
        //Use GetHashCode on the string equivalent of this formula, where GetHashCode of same strings have same the hash code.
        return this.ToString().GetHashCode();
    }
}


    /// <summary>
    ///   Used to report syntax errors in the argument to the Formula constructor.
    /// </summary>
public class FormulaFormatException : Exception
{
    /// <summary>
    ///   Initializes a new instance of the <see cref="FormulaFormatException"/> class.
    ///   <para>
    ///      Constructs a FormulaFormatException containing the explanatory message.
    ///   </para>
    /// </summary>
    /// <param name="message"> A developer defined message describing why the exception occured.</param>
    public FormulaFormatException(string message)
            : base(message)
    {
            // All this does is call the base constructor. No extra code needed.
    }
}

/// <summary>
/// Used as a possible return value of the Formula.Evaluate method.
/// </summary>
public class FormulaError
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="FormulaError"/> class.
    /// <para>
    /// Constructs a FormulaError containing the explanatory reason.
    /// </para>
    /// </summary>
    /// <param name="message"> Contains a message for why the error
    ///occurred.</param>
    public FormulaError(string message)
    {
            Reason = message;
    }

    /// <summary>
    /// Gets the reason why this FormulaError was created.
    /// </summary>
    public string Reason { get; private set; }
    }

    /// <summary>
    /// Any method meeting this type signature can be used for
    /// looking up the value of a variable.
    /// </summary>
    /// <exception cref="ArgumentException">
    /// If a variable name is provided that is not recognized by the implementing
    /// method,
    /// then the method should throw an ArgumentException.
    /// </exception>
    /// <param name="variableName">
    /// The name of the variable (e.g., "A1") to lookup.
    /// </param>
    /// <returns> The value of the given variable (if one exists). </returns>
    public delegate double Lookup(string variableName);


