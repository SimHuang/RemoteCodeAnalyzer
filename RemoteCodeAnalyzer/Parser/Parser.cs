///////////////////////////////////////////////////////////////////////
// Parser.cs - Parser detects code constructs defined by rules       //
// ver 1.5                                                           //
// Language:    C#, 2008, .Net Framework 4.0                         //
// Platform:    Dell Precision T7400, Win7, SP1                      //
// Application: Demonstration for CSE681, Project #2, Fall 2011      //
// Author:      Jim Fawcett, CST 4-187, Syracuse University          //
//              (315) 443-3948, jfawcett@twcny.rr.com                //
///////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ------------------
 * This module defines the following class:
 *   Parser  - a collection of IRules
 */
/* Required Files:
 *   IRulesAndActions.cs, RulesAndActions.cs, Parser.cs, Semi.cs, Toker.cs
 *   Display.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 1.5 : 14 Oct 2014
 * - added bug fix to tokenizer to avoid endless loop on
 *   multi-line strings
 * ver 1.4 : 30 Sep 2014
 * - modified test stub to display scope counts
 * ver 1.3 : 24 Sep 2011
 * - Added exception handling for exceptions thrown while parsing.
 *   This was done because Toker now throws if it encounters a
 *   string containing @".
 * - RulesAndActions were modified to fix bugs reported recently
 * ver 1.2 : 20 Sep 2011
 * - removed old stack, now replaced by ScopeStack
 * ver 1.1 : 11 Sep 2011
 * - added comments to parse function
 * ver 1.0 : 28 Aug 2011
 * - first release
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using DefinedType;
using System.Xml.Linq;

namespace CodeAnalysis
{
  /////////////////////////////////////////////////////////
  // rule-based parser used for code analysis

  public class Parser
  {
    private List<IRule> Rules;

    public Parser()
    {
      Rules = new List<IRule>();
    }
    public void add(IRule rule)
    {
      Rules.Add(rule);
    }
    public void parse(CSsemi.CSemiExp semi)
    {
      // Note: rule returns true to tell parser to stop
      //       processing the current semiExp
      
      Display.displaySemiString(semi.displayStr());

      foreach (IRule rule in Rules)
      {
        if (rule.test(semi))
          break;
      }
    }
  }

  class TestParser
  {
    //----< process commandline to get file references >-----------------

    static List<string> ProcessCommandline(string[] args)
    {
      List<string> files = new List<string>();
      if (args.Length == 0)
      {
        Console.Write("\n  Please enter file(s) to analyze\n\n");
        return files;
      }
      string path = args[0];
      path = Path.GetFullPath(path);
      for (int i = 1; i < args.Length; ++i)
      {
        string filename = Path.GetFileName(args[i]);
        files.AddRange(Directory.GetFiles(path, filename));
      }
      return files;
    }
    
    //function to calculate 
    static int getMaintainibilityIndex(Elem e) {
        int a = 1;
        int b = 2;
        int c = 3;
        int d = 4;
        return (a * (e.endLine - e.beginLine + 1)) + (b * (e.endScopeCount - e.beginScopeCount + 1)) + (c * e.cohesion) + (d * e.coupling);
    }

    static void ShowCommandLine(string[] args)
    {
      Console.Write("\n  Commandline args are:\n  ");
      foreach (string arg in args)
      {
        Console.Write("  {0}", arg);
      }
      Console.Write("\n  current directory: {0}", System.IO.Directory.GetCurrentDirectory());
      Console.Write("\n");
    }

    public static void calculateMaintainibilityIndex(string[] args, string xmlName)
    {
        Console.Write("\n  Demonstrating Parser");
        Console.Write("\n ======================\n");

        ShowCommandLine(args);

        //THIS RETURNS A LIST OF FILES THAT THE USER PASSED IN 
        List<string> files = TestParser.ProcessCommandline(args);
           
        for (int i = 0; i < files.Count; i++)
        {
            Console.Write(files[i] + "\n");
        }

        //sh - preprocess all user input files to get all user defined types
        UserType.parseUserDefinedTypes(files);
        HashSet<string> definedSet = UserType.getUserDefinedSet();
        Console.Write("\t Parser size of definedset" + definedSet.Count.ToString());

        foreach (string file in files)
        {
            Console.Write("\n  Processing file {0}\n", System.IO.Path.GetFileName(file));

            CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
            semi.displayNewLines = false;
            if (!semi.open(file as string))
            {
                Console.Write("\n  Can't open {0}\n\n", args[0]);
                return;
            }

            Console.Write("\n  Type and Function Analysis");
            Console.Write("\n ----------------------------");

            BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
            Parser parser = builder.build();

            try
            {
                while (semi.getSemi())
                    parser.parse(semi);
                Console.Write("\n  locations table contains:");
            }
            catch (Exception ex)
            {
                Console.Write("\n\n  {0}\n", ex.Message);
            }

            //all data is stored in table object
            Repository rep = Repository.getInstance();
            List<Elem> table = rep.locations;
            Console.Write(
                "\n  {0,10}, {1,25}, {2,5}, {3,5}, {4,5}, {5,5}, {6,5}, {7,5}, {8,10}, {9, 10}, {10,6}",
                "category", "name", "bLine", "eLine", "bScop", "eScop", "size", "cmplx", "coupling", "cohesion", "M-Index"
            );
            Console.Write(
                "\n  {0,10}, {1,25}, {2,5}, {3,5}, {4,5}, {5,5}, {6,5}, {7,5}, {8,10}, {9,10}, {10,6}",
                "--------", "----", "-----", "-----", "-----", "-----", "----", "-----", "--------", "--------", "-------"
            );

            //load xml document for writing
            XDocument doc = XDocument.Load("../../FileMaintainibility/" + xmlName + ".xml");
            XElement root = doc.Element("Properties");

            foreach (Elem e in table)
            {
                if (e.type == "class" || e.type == "struct")
                {
                    Console.Write("\n");

                    //get the maintainibility index
                    e.mIndex = getMaintainibilityIndex(e);

                    Console.Write(
                    "\n  {0,10}, {1,25}, {2,5}, {3,5}, {4,5}, {5,5}, {6,5}, {7,5}, {8,10}, {9,10}, {10,6}",
                            e.type, e.name, e.beginLine, e.endLine, e.beginScopeCount, e.endScopeCount + 1,
                                e.endLine - e.beginLine + 1, e.endScopeCount - e.beginScopeCount + 1, e.coupling, e.cohesion, e.mIndex
                        );

                        int endline = e.endLine - e.beginLine + 1;
                        int scope = e.endScopeCount - e.beginScopeCount + 1;

                        string property = e.type + "\t" + e.name + "\t\t\t\t\t" + e.beginLine + "\t" + e.endLine + "\t" + e.beginScopeCount + "\t" + e.endScopeCount + 1
                            + "\t" + endline + "\t" + scope + "\t" + e.coupling + "\t" + e.cohesion +  "\t" + e.mIndex;

                        root.Add(new XElement("Property", property));
                }
                else
                {
                    Console.Write(
                                "\n  {0,10}, {1,25}, {2,5}, {3,5}, {4,5}, {5,5}, {6,5}, {7,5}, {8,10}, {9,10}",
                            e.type, e.name, e.beginLine, e.endLine, e.beginScopeCount, e.endScopeCount + 1,
                                e.endLine - e.beginLine + 1, e.endScopeCount - e.beginScopeCount + 1, e.coupling, e.cohesion
                        );

                        int endline = e.endLine - e.beginLine + 1;
                        int scope = e.endScopeCount - e.beginScopeCount + 1;

                        string property = e.type + "\t" + e.name + "\t\t\t\t\t" + e.beginLine + "\t" + e.endLine + "\t" + e.beginScopeCount + "\t" + e.endScopeCount + 1
                            + "\t" + endline + "\t" + scope + "\t" + e.coupling + e.cohesion;

                        root.Add(new XElement("Property", property));

                    }
            }

                doc.Save("../../FileMaintainibility/" + xmlName + ".xml");

            Console.Write("\n\n");

            semi.close();
        }
        Console.Write("\n\n");
        
    }

    //----< Test Stub >--------------------------------------------------

#if (TEST_PARSER)

    static void Main(string[] args)
    {
      Console.Write("\n  Demonstrating Parser");
      Console.Write("\n ======================\n");

      ShowCommandLine(args);

      //THIS RETURNS A LIST OF FILES THAT THE USER PASSED IN 
      List<string> files = TestParser.ProcessCommandline(args);

      for(int i = 0; i < files.Count; i++)
            {
                Console.Write(files[i] + "\n");
            }

      //sh - preprocess all user input files to get all user defined types
      UserType.parseUserDefinedTypes(files);
      HashSet<string> definedSet = UserType.getUserDefinedSet();
      Console.Write("Parser size of definedset" + definedSet.Count.ToString());

      foreach (string file in files)
      {
        Console.Write("\n  Processing file {0}\n", System.IO.Path.GetFileName(file));

        CSsemi.CSemiExp semi = new CSsemi.CSemiExp();
        semi.displayNewLines = false;
        if (!semi.open(file as string))
        {
          Console.Write("\n  Can't open {0}\n\n", args[0]);
          return;
        }

        Console.Write("\n  Type and Function Analysis");
        Console.Write("\n ----------------------------");

        BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
        Parser parser = builder.build();

        try
        {
          while (semi.getSemi())
            parser.parse(semi);
          Console.Write("\n  locations table contains:");
        }
        catch (Exception ex)
        {
          Console.Write("\n\n  {0}\n", ex.Message);
        }

        //all data is stored in table object
        Repository rep = Repository.getInstance();
        List<Elem> table = rep.locations;
        Console.Write(
            "\n  {0,10}, {1,25}, {2,5}, {3,5}, {4,5}, {5,5}, {6,5}, {7,5}, {8,10}, {9, 10}, {10,6}", 
            "category", "name", "bLine", "eLine", "bScop", "eScop", "size", "cmplx", "coupling", "cohesion", "M-Index"
        );
        Console.Write(
            "\n  {0,10}, {1,25}, {2,5}, {3,5}, {4,5}, {5,5}, {6,5}, {7,5}, {8,10}, {9,10}, {10,6}",  
            "--------", "----", "-----", "-----", "-----", "-----", "----", "-----", "--------", "--------", "-------"
        );
        foreach (Elem e in table)
        {
            if (e.type == "class" || e.type == "struct") {
                Console.Write("\n");
                   
                //get the maintainibility index
                e.mIndex = getMaintainibilityIndex(e);

                Console.Write(
                "\n  {0,10}, {1,25}, {2,5}, {3,5}, {4,5}, {5,5}, {6,5}, {7,5}, {8,10}, {9,10}, {10,6}",
                      e.type, e.name, e.beginLine, e.endLine, e.beginScopeCount, e.endScopeCount + 1,
                          e.endLine - e.beginLine + 1, e.endScopeCount - e.beginScopeCount + 1, e.coupling, e.cohesion, e.mIndex
                    );
            }else {
                Console.Write(
                          "\n  {0,10}, {1,25}, {2,5}, {3,5}, {4,5}, {5,5}, {6,5}, {7,5}, {8,10}, {9,10}",
                      e.type, e.name, e.beginLine, e.endLine, e.beginScopeCount, e.endScopeCount + 1,
                          e.endLine - e.beginLine + 1, e.endScopeCount - e.beginScopeCount + 1, e.coupling, e.cohesion
                    );
                
            }
        }

        Console.Write("\n\n");

        semi.close();
      }
      Console.Write("\n\n");
    }
#endif
    }
}
