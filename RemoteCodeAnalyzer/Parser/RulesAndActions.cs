///////////////////////////////////////////////////////////////////////
// RulesAndActions.cs - Parser rules specific to an application      //
// ver 2.3                                                           //
// Language:    C#, 2008, .Net Framework 4.0                         //
// Platform:    Dell Precision T7400, Win7, SP1                      //
// Application: Demonstration for CSE681, Project #2, Fall 2011      //
// Author:      Jim Fawcett, CST 4-187, Syracuse University          //
//              (315) 443-3948, jfawcett@twcny.rr.com                //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * RulesAndActions package contains all of the Application specific
 * code required for most analysis tools.
 *
 * It defines the following Four rules which each have a
 * grammar construct detector and also a collection of IActions:
 *   - DetectNameSpace rule
 *   - DetectClass rule
 *   - DetectFunction rule
 *   - DetectScopeChange
 *   - DetectAssociations
 *   
 *   Three actions - some are specific to a parent rule:
 *   - Print
 *   - PrintFunction
 *   - PrintScope
 *   - UpdateCoupling
 * 
 * The package also defines a Repository class for passing data between
 * actions and uses the services of a ScopeStack, defined in a package
 * of that name.
 *
 * Note:
 * This package does not have a test stub since it cannot execute
 * without requests from Parser.
 *  
 */
/* Required Files:
 *   IRuleAndAction.cs, RulesAndActions.cs, Parser.cs, ScopeStack.cs,
 *   Semi.cs, Toker.cs
 *   
 * Build command:
 *   csc /D:TEST_PARSER Parser.cs IRuleAndAction.cs RulesAndActions.cs \
 *                      ScopeStack.cs Semi.cs Toker.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 2.3 : 30 Sep 2014
 * - added scope-based complexity analysis
 *   Note: doesn't detect braceless scopes yet
 * ver 2.2 : 24 Sep 2011
 * - modified Semi package to extract compile directives (statements with #)
 *   as semiExpressions
 * - strengthened and simplified DetectFunction
 * - the previous changes fixed a bug, reported by Yu-Chi Jen, resulting in
 * - failure to properly handle a couple of special cases in DetectFunction
 * - fixed bug in PopStack, reported by Weimin Huang, that resulted in
 *   overloaded functions all being reported as ending on the same line
 * - fixed bug in isSpecialToken, in the DetectFunction class, found and
 *   solved by Zuowei Yuan, by adding "using" to the special tokens list.
 * - There is a remaining bug in Toker caused by using the @ just before
 *   quotes to allow using \ as characters so they are not interpreted as
 *   escape sequences.  You will have to avoid using this construct, e.g.,
 *   use "\\xyz" instead of @"\xyz".  Too many changes and subsequent testing
 *   are required to fix this immediately.
 * ver 2.1 : 13 Sep 2011
 * - made BuildCodeAnalyzer a public class
 * ver 2.0 : 05 Sep 2011
 * - removed old stack and added scope stack
 * - added Repository class that allows actions to save and 
 *   retrieve application specific data
 * - added rules and actions specific to Project #2, Fall 2010
 * ver 1.1 : 05 Sep 11
 * - added Repository and references to ScopeStack
 * - revised actions
 * - thought about added folding rules
 * ver 1.0 : 28 Aug 2011
 * - first release
 *
 * Planned Modifications (not needed for Project #2):
 * --------------------------------------------------
 * - add folding rules:
 *   - CSemiExp returns for(int i=0; i<len; ++i) { as three semi-expressions, e.g.:
 *       for(int i=0;
 *       i<len;
 *       ++i) {
 *     The first folding rule folds these three semi-expression into one,
 *     passed to parser. 
 *   - CToker returns operator[]( as four distinct tokens, e.g.: operator, [, ], (.
 *     The second folding rule coalesces the first three into one token so we get:
 *     operator[], ( 
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CSsemi;
using DefinedType;

namespace CodeAnalysis
{
  public class Elem  // holds scope information
  {
    public string type { get; set; }
    public string name { get; set; }
    public int beginLine { get; set; }
    public int endLine { get; set; }
    public int beginScopeCount { get; set; }
    public int endScopeCount { get; set; }
    public int coupling { get; set; }
    public int cohesion { get; set;  }
    public int mIndex { get; set; }

    public override string ToString()
    {
      StringBuilder temp = new StringBuilder();
      temp.Append("{");
      temp.Append(String.Format("{0,-10}", type)).Append(" : ");
      temp.Append(String.Format("{0,-10}", name)).Append(" : ");
      temp.Append(String.Format("{0,-5}", beginLine.ToString()));  // line of scope start
      temp.Append(String.Format("{0,-5}", endLine.ToString()));    // line of scope end
      temp.Append("}");
      return temp.ToString();
    }
  }

  public class Repository
  {
    ScopeStack<Elem> stack_ = new ScopeStack<Elem>();
    List<Elem> locations_ = new List<Elem>();
    Dictionary<string, List<Elem>> locationsTable_ = new Dictionary<string, List<Elem>>();
    Dictionary<string, List<Elem>> functionData = new Dictionary<string, List<Elem>>(); //keep track of all the function members for cohesion
                      
    static Repository instance;

    public Repository()
    {
      instance = this;
    }

    //----< provides all code access to Repository >-------------------
    public static Repository getInstance()
    {
      return instance;
    }

    //------< keep track of the current class scope >-----------
    //the type of the scope will always be class
    public String currentClassScope {
        get;
        set;
    }

    //------< keep track of the current function scope > ---------
    public String currentFunctionScope {
        get;
        set;
    }

    //----< provides all actions access to current semiExp >-----------

    public CSsemi.CSemiExp semi
    {
      get;
      set;
    }

    //-----< keeps track of the number of functions in a class >-----
    public int functionCount {
        get;
        set;
    }

    // semi gets line count from toker who counts lines
    // while reading from its source

    public int lineCount  // saved by newline rule's action
    {
      get { return semi.lineCount; }
    }
    public int prevLineCount  // not used in this demo
    {
      get;
      set;
    }

    //----< enables recursively tracking entry and exit from scopes >--

    public int scopeCount
    {
      get;
      set;
    }

    public ScopeStack<Elem> stack  // pushed and popped by scope rule's action
    {
      get { return stack_; } 
    }
 
    // the locations table is the result returned by parser's actions
    // in this demo

    public List<Elem> locations
    {
      get { return locations_; }
      set { locations_ = value; }
    }

    public Dictionary<string, List<Elem>> LocationsTable 
    {
      get { return locationsTable_; }
      set { locationsTable_ = value; } 
    }
    
    //setters and getters for function table
    public Dictionary<string,List<Elem>> FunctionTable {
        get { return functionData;  }
        set { functionData = value; }
    }

  }
  /////////////////////////////////////////////////////////
  // pushes scope info on stack when entering new scope

  public class PushStack : AAction
  {
    public PushStack(Repository repo)
    {
      repo_ = repo;
    }
    public override void doAction(CSsemi.CSemiExp semi)
    {
      Display.displayActions(actionDelegate, "action PushStack");
      ++repo_.scopeCount;
      Elem elem = new Elem();
      elem.type = semi[0];  // expects type
      elem.name = semi[1];  // expects name
      elem.beginLine = repo_.semi.lineCount - 1;
      elem.endLine = 0;
      elem.beginScopeCount = repo_.scopeCount;
      elem.endScopeCount = 0;
      elem.cohesion = 0;
      elem.coupling = 0;

      //keep track of the current class and function scope
      if(elem.name != "anonymous") {
        if(elem.type.Equals("class") || elem.type.Equals("struct") || elem.type.Equals("interface")) {
            repo_.currentClassScope = elem.name; //set the current class name
        }else if(elem.type.Equals("function")) {
            repo_.currentFunctionScope = elem.name;    
        }
      }

      repo_.stack.push(elem);

      if (AAction.displayStack)
        repo_.stack.display();
      if (AAction.displaySemi)
      {
        Console.Write("\n  line# {0,-5}", repo_.semi.lineCount - 1);
        Console.Write("entering ");
        string indent = new string(' ', 2 * repo_.stack.count);
        Console.Write("{0}", indent);
        this.display(semi); // defined in abstract action
      }
      if (elem.type == "control" || elem.name == "anonymous")
        return;
      repo_.locations.Add(elem);
      
      //update coupling base on class inhertiance on use
      if(semi.Contains("class") != -1) {
          inheritCoupling(semi);

      }else {
          userCoupling(semi);   
      }
     
    }

    //helper method to update the coupling parameter 
    //base on inheritance if  conditions are met
    public void inheritCoupling(CSsemi.CSemiExp semi) {
        ///update class coupling if inheritance found
        if (semi.userInheritance)
        {
          if (UserType.userDefinedSet.Contains(semi[2]))
          {
              string classScope = repo_.currentClassScope;
              if (classScope != null)
              {
                  for (int i = 0; i < repo_.locations.Count; i++)
                  {
                      Elem tempElem = repo_.locations[i];
                      if (tempElem.name.Equals(classScope))
                      {
                          repo_.locations[i].coupling += 1;
                          break;
                      }
                  }
              }
          }
        }
    }

    //helper method to update coupling base on use
    public void userCoupling(CSsemi.CSemiExp semi) {
        for (int i = 0; i < semi.count; i++) {
            if(UserType.userDefinedSet.Contains(semi[i])) {
                string classScope = repo_.currentClassScope;
                string functionScope = repo_.currentFunctionScope;
                if(classScope != null) {
                    for (int j = 0; j < repo_.locations.Count; j++)
                    {
                        Elem tempElem = repo_.locations[j];
                        if (tempElem.name.Equals(classScope) || tempElem.name.Equals(functionScope)) 
                        {
                            repo_.locations[j].coupling += 1;
                            //break;
                        }
                    }
                }
            } 
        }
    }

  }

  /////////////////////////////////////////////////////////
  // pops scope info from stack when leaving scope

  public class PopStack : AAction
  {
    public PopStack(Repository repo)
    {
      repo_ = repo;
    }
    public override void doAction(CSsemi.CSemiExp semi)
    {
      Display.displayActions(actionDelegate, "action SaveDeclar");
      Elem elem;
      try
      {
        elem = repo_.stack.pop();
        
        //clear the function scope if we are exiting from it
        if(elem.name.Equals(repo_.currentFunctionScope)) {
            repo_.currentFunctionScope = "";
        }

        for (int i = 0; i < repo_.locations.Count; ++i )
        {
          Elem temp = repo_.locations[i];
          if (elem.type == temp.type)
          {
            if (elem.name == temp.name)
            {
              if ((repo_.locations[i]).endLine == 0)
              {
                (repo_.locations[i]).endLine = repo_.semi.lineCount;
                (repo_.locations[i]).endScopeCount = repo_.scopeCount;
                break;
              }
            }
          }
        }
      }
      catch
      {
        return;
      }
      CSsemi.CSemiExp local = new CSsemi.CSemiExp();
      local.Add(elem.type).Add(elem.name);
      if(local[0] == "control")
        return;

      if (AAction.displaySemi)
      {
        Console.Write("\n  line# {0,-5}", repo_.semi.lineCount);
        Console.Write("leaving  ");
        string indent = new string(' ', 2 * (repo_.stack.count + 1));
        Console.Write("{0}", indent);
        this.display(local); // defined in abstract action
      }
    }
  }
  ///////////////////////////////////////////////////////////
  // action to print function signatures - not used in demo

  public class PrintFunction : AAction
  {
    public PrintFunction(Repository repo)
    {
      repo_ = repo;
    }
    public override void display(CSsemi.CSemiExp semi)
    {
      Console.Write("\n    line# {0}", repo_.semi.lineCount - 1);
      Console.Write("\n    ");
      for (int i = 0; i < semi.count; ++i)
        if (semi[i] != "\n" && !semi.isComment(semi[i]))
          Console.Write("{0} ", semi[i]);
    }
    public override void doAction(CSsemi.CSemiExp semi)
    {
      this.display(semi);
    }
  }
  /////////////////////////////////////////////////////////
  // concrete printing action, useful for debugging

  public class Print : AAction
  {
    public Print(Repository repo)
    {
      repo_ = repo;
    }
    public override void doAction(CSsemi.CSemiExp semi)
    {
      Console.Write("\n  line# {0}", repo_.semi.lineCount - 1);
      this.display(semi);
    }
  }
    /////////////////////////////////////////////////////////
    // display public declaration

    public class SaveDeclar : AAction
    {
        public SaveDeclar(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(CSsemi.CSemiExp semi)
        {
            Display.displayActions(actionDelegate, "action SaveDeclar");
            Elem elem = new Elem();
            elem.type = semi[0];  // expects type
            elem.name = semi[1];  // expects name
            elem.beginLine = repo_.semi.lineCount;
            elem.endLine = elem.beginLine;
            elem.beginScopeCount = repo_.scopeCount;
            elem.endScopeCount = elem.beginScopeCount;
            if (AAction.displaySemi)
            {
                Console.Write("\n  line# {0,-5}", repo_.semi.lineCount - 1);
                Console.Write("entering ");
                string indent = new string(' ', 2 * repo_.stack.count);
                Console.Write("{0}", indent);
                this.display(semi); // defined in abstract action
            }
            repo_.locations.Add(elem);
        }
    }

    //////////////////////////////////////////////////////////
    ///action to update the coupling value for a class elem
    /// for now inner class and anonymous classes are ignored 
                    /// deprecated
    public class UpdateCoupling : AAction
    {
        public UpdateCoupling(Repository repo) {
            repo_ = repo;
        }

        public override void doAction(CSemiExp semi)
        {
            string classScope = repo_.currentClassScope;
            string functionScope = repo_.currentFunctionScope;
            if(classScope != null) {
                for (int i = 0; i < repo_.locations.Count; i++) {
                    Elem tempElem = repo_.locations[i];

                    //update class variable
                    if(tempElem.name.Equals(classScope) || tempElem.name.Equals(functionScope)) {
                        repo_.locations[i].coupling += 1;

                        //add one more if there is instantiation 
                        if(semi.Contains("new") != -1) {
                            repo_.locations[i].coupling += 1;
                        }
                        //break;

                    }
                }
            }
        }
    }

    //////////////////////////////////////////////////////////
    //action to save user defined type

    public class SaveUserDefinedType : AAction
    {

        private HashSet<string> definedSet;

        public SaveUserDefinedType(HashSet<string> definedSet)
        {
            this.definedSet = definedSet;
        }

        //extract the type out of the expression
        public override void doAction(CSemiExp semi)
        {
            int classIndex = semi.Contains("class");
            int structIndex = semi.Contains("struct");

            if (classIndex != -1)
            {
                UserType.getUserDefinedSet().Add(semi[classIndex + 1]);
                string key = semi[classIndex + 1];
            }
            else if (structIndex != -1) {
                string key = semi[structIndex + 1];
                UserType.getUserDefinedSet().Add(semi[structIndex + 1]);
            }
        }
    }

    //sh-rule to detect association which includes both aggregation and composition
    //assumption is made that all 
    //this also handles the logic for cohesion
    public class DetectAssociation : ARule
    {
        public override bool test(CSemiExp semi)
        {
            HashSet<string> definedSet = UserType.userDefinedSet;
            string[] specialTokens = { "get", "set" };
            Display.displayFiles(actionDelegate, "rule Detect Association");
            int publicIndex = semi.Contains("public");
            int staticIndex = semi.Contains("static");
            int privateIndex = semi.Contains("private");
            int protectedIndex = semi.Contains("protected");

            //ignoreing special words as aggregation
            for (int i = 0; i < specialTokens.Length; i++) {
                if(semi.Contains(specialTokens[i]) != -1) {
                    return false;
                }
            }
            
            //edge case for a single semi colon
            if (semi.count == 1 && semi[0].Equals(";")) {
                return false;
            }
            
            //these characteristics is probably a function call rather than a declaration
            if (semi.Contains(".") != -1 && semi.Contains("(") != -1) {
                return false; 
            }
 
            if(semi.Contains("class") == -1 && semi.Contains("interface") == -1) {  //not class            
                //make sure it is not a function
                if(semi.Contains("(") != -1 && semi.Contains(";") == -1) {
                    return false;
                }
                
                //parse for the index of the declaration type
                int typeIndex = 0;  //the default type index is 0 if no special variables
                //the coupling might datastructure related
                if (semi.Contains("<") != -1)
                {
                    typeIndex = semi.Contains("<") + 1;

                }
                else if (staticIndex != -1)
                {
                    typeIndex = staticIndex + 1;

                }
                else if (publicIndex != -1 || privateIndex != -1 || protectedIndex != -1)
                {
                    int privacyIndex = Math.Max(Math.Max(publicIndex, privateIndex), protectedIndex);
                    typeIndex = privacyIndex + 1;

                }else {
                    for (int i = 0; i < semi.count; i++) {
                        if(UserType.userDefinedSet.Contains(semi[i])) {
                            typeIndex = i;
                            break;
                        } 
                    }     
                }
                
                //keep track of the declaration type
                string declarationType = semi[typeIndex];

                //store all data type and data member in repo for cohesion use
                Repository repository = Repository.getInstance();
                if(repository.currentClassScope != null && repository.currentClassScope != "") 
                {
                    CSsemi.CSemiExp local = new CSemiExp();
                    local.Add(declarationType);
                    local.Add(semi[typeIndex + 1]);

                    if (repository.currentFunctionScope == null || repository.currentFunctionScope.Equals(""))
                    {
                        //since we are not in a function scope we can just store the class data members 
                        storeClassDataMember(repository, local);

                    }
                    else
                    {
                        //we are in a function scope so process the varibales in the function for cohesion
                        processFunctionCohesion(repository, local);
                    }
                }
                
                //only do action if this is user defined type for coupling
                if (UserType.userDefinedSet.Contains(declarationType))
                {
                    CSsemi.CSemiExp local = new CSemiExp();
                    local.Add(declarationType);
                    if(semi.Contains("new") != -1) {
                        local.Add("new");
                    }
                    doActions(local);
                    return true;
                }
            }
            
            return false;
        }

        ////helper method to store all class data memebers in repo
        public void storeClassDataMember(Repository repository, CSsemi.CSemiExp local) 
        {
            if(repository != null) {
                if(repository.currentClassScope != null && (repository.currentFunctionScope == "" || repository.currentFunctionScope == null)) {
                    Dictionary<string, List<Elem>> locationsTable = repository.LocationsTable;
                    Elem elem = new Elem();
                    elem.type = local[0];
                    elem.name = local[1];

                    if(locationsTable.ContainsKey(repository.currentClassScope)) {
                        List<Elem> elements = locationsTable[repository.currentClassScope];
                        elements.Add(elem);
                        locationsTable[repository.currentClassScope] = elements;

                     }else {
                        List<Elem> elements = new List<Elem>();
                        elements.Add(elem);
                        locationsTable.Add(repository.currentClassScope, elements);
                     }
                }
            }    
        }
        
        ////helper method to calculation cohesion of a function 
        public void processFunctionCohesion(Repository repository, CSsemi.CSemiExp local) 
        {
            if(repository != null) {
                Elem elem = new Elem();
                elem.type = local[0];
                elem.name = local[1];

                string currentClassScope = repository.currentClassScope;
                Dictionary<string, List<Elem>> classDataMembers = repository.LocationsTable;

                if(classDataMembers.ContainsKey(currentClassScope)) {
                    List<Elem> classScopeVariables = classDataMembers[currentClassScope];

                    string keyname = elem.type + elem.name;
                    Dictionary<string, List<Elem>> functions = repository.FunctionTable;

                    //if the dictionary already store the existing function
                    if(functions.ContainsKey(keyname)) {
                        //first find all variables that the current function has
                        for (int i = 0; i < classScopeVariables.Count; i++)
                        {
                            Elem tempElem = classScopeVariables[i];
                            if (elem.name.Equals(tempElem.name) && elem.type.Equals(tempElem.type))
                            {
                                List<Elem> functionVariables = functions[keyname];
                                functionVariables.Add(elem);
                                functions[keyname] = functionVariables;

                                //increment the cohesion for that function elem
                                for (int j = 0; j < repository.locations.Count; j++)
                                {
                                    Elem currentElem = repository.locations[i];

                                    //update class variable
                                    if (currentElem.name.Equals(elem.name))
                                    {
                                        repository.locations[j].cohesion += 1;
                                        break;
                                    }
                                }
                            }
                        }    

                    }else {
                        for (int i = 0; i < classScopeVariables.Count; i++)
                        {
                            Elem tempElem = classScopeVariables[i];
                            if (elem.name.Equals(tempElem.name) && elem.type.Equals(tempElem.type))
                            {
                                List<Elem> functionVariables = new List<Elem>();
                                functionVariables.Add(elem);
                                functions[keyname] = functionVariables;

                                //increment the cohesion for that function elem
                                for (int j = 0; j < repository.locations.Count; j++)
                                {
                                    Elem currentElem = repository.locations[i];

                                    //update class variable
                                    if (currentElem.name.Equals(elem.name))
                                    {
                                        repository.locations[j].cohesion += 1;
                                        break;
                                    }
                                }
                            }
                        }     
                    }

                }else {
                    return;
                }
            }
        }
    }

    ////////////////////////////////////////////////////////
    //sh-rule to detect inheritance
    //deprecated
    public class DetectInheritance : ARule
    {
        public override bool test(CSsemi.CSemiExp semi)
        {
            HashSet<string> definedSet = UserType.getUserDefinedSet();
            Display.displayRules(actionDelegate, "rule Detect Inheritance");
            int classIndex = semi.Contains("class");
            if (classIndex != -1)
            {
                //check for inheritance
                int inheritanceIndex = semi.Contains(":");
                if (inheritanceIndex != -1)
                {
                    string inheritedClass = semi[inheritanceIndex + 1];
                    if(definedSet.Contains(inheritedClass))
                    {
                        //do some action
                        Console.Write("inheritance found: " + inheritedClass + "\n");
                        doActions(semi);
                        return true;
                    }
                }
            }
            return false;
        }
    }

    /////////////////////////////////////////////////////////
    // rule to detect namespace declarations

    public class DetectNamespace : ARule
  {
    public override bool test(CSsemi.CSemiExp semi)
    {
      Display.displayRules(actionDelegate, "rule   DetectNamespace");
      int index = semi.Contains("namespace");
      if (index != -1)
      {
        CSsemi.CSemiExp local = new CSsemi.CSemiExp();
        // create local semiExp with tokens for type and name
        local.displayNewLines = false;
        local.Add(semi[index]).Add(semi[index + 1]);
        doActions(local);
        return true;
      }
      return false;
    }
  }

  /////////////////////////////////////////////////////////
  // rule to dectect class definitions
  // this rule also detects inheritance in that class
  public class DetectClass : ARule
  {
    public override bool test(CSsemi.CSemiExp semi)
    {
      Display.displayRules(actionDelegate, "rule   DetectClass");
      int indexCL = semi.Contains("class");
      int indexIF = semi.Contains("interface");
      int indexST = semi.Contains("struct");

      int index = Math.Max(indexCL, indexIF);
      index = Math.Max(index, indexST);
      if (index != -1)
      {
        CSsemi.CSemiExp local = new CSsemi.CSemiExp();
        // local semiExp with tokens for type and name
        local.displayNewLines = false;

        //check for inheritance
        if(semi.Contains(":") != -1) {
            local.userInheritance = true;
            local.Add(semi[index]).Add(semi[index + 1]).Add(semi[semi.Contains(":") + 1]);

        }else {
            local.Add(semi[index]).Add(semi[index + 1]);
        }

        doActions(local);
        return true;
      }
      return false;
    }
  }

  /////////////////////////////////////////////////////////
  // rule to dectect function definitions

  public class DetectFunction : ARule
  {
    public static bool isSpecialToken(string token)
    {
      string[] SpecialToken = { "if", "for", "foreach", "while", "catch", "using" };
      foreach (string stoken in SpecialToken)
        if (stoken == token)
          return true;
      return false;
    }
    public override bool test(CSsemi.CSemiExp semi)
    {
      Display.displayRules(actionDelegate, "rule   DetectFunction");
      if (semi[semi.count - 1] != "{")
        return false;

      int index = semi.FindFirst("(");
      if (index > 0 && !isSpecialToken(semi[index - 1]))
      {
        CSsemi.CSemiExp local = new CSsemi.CSemiExp();
        local.Add("function").Add(semi[index - 1]);

        //search for all arguments in the function and add to local
        for (int i = index; i < semi.count; i++) {
            if(UserType.userDefinedSet.Contains(semi[i])) {
                local.Add(semi[i]);
            }
        }        

        doActions(local);
        return true;
      }
      return false;
    }
  }

  /////////////////////////////////////////////////////////
  // detect entering anonymous scope
  // - expects namespace, class, and function scopes
  //   already handled, so put this rule after those
  public class DetectAnonymousScope : ARule
  {
    public override bool test(CSsemi.CSemiExp semi)
    {
      Display.displayRules(actionDelegate, "rule   DetectAnonymousScope");
      int index = semi.Contains("{");
      if (index != -1)
      {
        CSsemi.CSemiExp local = new CSsemi.CSemiExp();
        // create local semiExp with tokens for type and name
        local.displayNewLines = false;
        local.Add("control").Add("anonymous");
        doActions(local);
        return true;
      }
      return false;
    }
  }

  /////////////////////////////////////////////////////////
/// 

  /////////////////////////////////////////////////////////
  // detect public declaration

  public class DetectPublicDeclar : ARule
  {
    public override bool test(CSsemi.CSemiExp semi)
    {
      Display.displayRules(actionDelegate, "rule   DetectPublicDeclar");
      int index = semi.Contains(";");
      if (index != -1)
      {
        index = semi.Contains("public");
        if (index == -1)
          return true;
        CSsemi.CSemiExp local = new CSsemi.CSemiExp();
        // create local semiExp with tokens for type and name
        local.displayNewLines = false;
        local.Add("public "+semi[index+1]).Add(semi[index+2]);

        index = semi.Contains("=");
        if (index != -1)
        {
          doActions(local);
          return true;
        }
        index = semi.Contains("(");
        if(index == -1)
        {
          doActions(local);
          return true;
        }
      }
      return false;
    }
  }

  /////////////////////////////////////////////////////////
  // detect leaving scope

  public class DetectLeavingScope : ARule
  {
    public override bool test(CSsemi.CSemiExp semi)
    {
      Display.displayRules(actionDelegate, "rule   DetectLeavingScope");
      int index = semi.Contains("}");
      if (index != -1)
      {
        doActions(semi);
        return true;
      }
      return false;
    }
  }

  public class BuildCodeAnalyzer
  {
    Repository repo = new Repository();

    public BuildCodeAnalyzer(CSsemi.CSemiExp semi)
    {
      repo.semi = semi;
    }
    public virtual Parser build()
    {
      Parser parser = new Parser();
      //UserType userType = new UserType();

      // decide what to show
      AAction.displaySemi = false;
      AAction.displayStack = false;  // false is default

      // action used for namespaces, classes, and functions
      PushStack push = new PushStack(repo);

      // capture namespace info
      DetectNamespace detectNS = new DetectNamespace();
      detectNS.add(push);
      parser.add(detectNS);

      // capture class info
      DetectClass detectCl = new DetectClass();
      //SaveUserDefinedType save = new SaveUserDefinedType(UserType.getUserDefinedSet());
      detectCl.add(push);
      //detectCl.add(save);
      parser.add(detectCl);

      //// handle inheritance detection
      //DetectInheritance detectInheritance = new DetectInheritance();
      //UpdateCoupling update = new UpdateCoupling(repo);
      //detectInheritance.add(update);
      //parser.add(detectInheritance);

      // capture function info
      DetectFunction detectFN = new DetectFunction();
      detectFN.add(push);
      parser.add(detectFN);

      // handle entering anonymous scopes, e.g., if, while, etc.
      DetectAnonymousScope anon = new DetectAnonymousScope();
      anon.add(push);
      parser.add(anon);

      // show public declarations
      //DetectPublicDeclar pubDec = new DetectPublicDeclar();
      //SaveDeclar print = new SaveDeclar(repo);
      //pubDec.add(print);
      //parser.add(pubDec);

      // handle leaving scopes
      DetectLeavingScope leave = new DetectLeavingScope();
      PopStack pop = new PopStack(repo);
      leave.add(pop);
      parser.add(leave);

      //this rule detects variable declaration and instantiation for coupling
      DetectAssociation detectAssociation = new DetectAssociation();
      UpdateCoupling update = new UpdateCoupling(repo);
      detectAssociation.add(update);
      parser.add(detectAssociation); 

      // parser configured
      return parser;
    }
  }
}

