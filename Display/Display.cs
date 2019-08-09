///////////////////////////////////////////////////////////////////////////
// Display.cs  -  Manage Display properties                              //
// ver 1.2                                                               //
// Language:    C# 7.0, Visual Studio 2017, .Net Framework 4.6.1         //
// Platform:    Dell Inspiron 4736 , Windows 10                          //
// Application: Handles all the display responsibilities for code        //
//              analyzer(CSE 681 SMA Project 3, Fall 2018                //
// Author:      Jim Fawcett, CST 2-187, Syracuse University              //
//              (315) 443-3948, jfawcett@twcny.rr.com                    //
// Modiefied By:Vaibhav Kumar, Syracuse University, vkumar05@syr.edu     // 
///////////////////////////////////////////////////////////////////////////
/*
 * Package Operations
 * ==================
 * Display manages static public properties used to control what is displayed and
 * provides static helper functions to send information to MainWindow and Console.
 * 
 * Public Interface
 * ================
 * Display.showConsole = false;                 // disables most writing to console
 * Display.showFooter = true;                   // enables status information display in footer
 * ...
 * Display.displayRules(act, ruleStr)           // sends ruleStr to console and/or footer
 * ...
 * Display.ShowTypeTable(tables)                //Displays Type Tables
 * Display.ShowDependencies(Dependencies)       //Displays file dependencies
 * Display.ShowSCC(StrongComponenets)           //Displays Strong Components dependencies
 */
/*
 * Build Process
 * =============
 * Required Files:
 *   Element.cs
 *   
 * 
 * Maintenance History
 * ===================
 * ver 1.2 : 31st Oct 2018
 * - Added ShowTypeTable(tables), ShowDependencies(Dependencies) and ShowSCC(StrongComponenets) methods
 * ver 1.1 : 09 Oct 2018
 * - removed non-essential items from display
 * ver 1.0 : 19 Oct 2014
 *   - first release
 * 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace CodeAnalysis
{
    ///////////////////////////////////////////////////////////////////
    // StringExt static class
    // - extension method to truncate strings

    public static class StringExt
    {
        public static string Truncate(this string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength);
        }
    }

    static public class Display
    {
        static Display()
        {
            showFiles = true;
            showDirectories = true;
            showActions = false;
            showRules = false;
            useFooter = false;
            useConsole = false;
            goSlow = false;
            width = 33;
        }
        static public bool showFiles { get; set; }
        static public bool showDirectories { get; set; }
        static public bool showActions { get; set; }
        static public bool showRules { get; set; }
        static public bool showSemi { get; set; }
        static public bool useFooter { get; set; }
        static public bool useConsole { get; set; }
        static public bool goSlow { get; set; }
        static public int width { get; set; }

        //----< display results of Code Analysis >-----------------------

        static public void ShowTypeTables(Dictionary<string, List<Elem>> dicTables)
        {
            foreach (KeyValuePair<string, List<Elem>> table in dicTables)
            {
                Console.Write("\n\n");
                Display.showMetricsTable(table.Key, table.Value);
            }
        }

        /// <summary>
        /// Displays all the files and their dependencies
        /// </summary>
        /// <param name="dicDependencies"></param>
        static public void ShowDependencies(Dictionary<string, HashSet<string>> dicDependencies)
        {
            Console.WriteLine("\n");
            foreach (KeyValuePair<string, HashSet<string>> depen in dicDependencies)
            {
                if (depen.Value != null)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (string d in depen.Value)
                    {
                        sb.Append(System.IO.Path.GetFileName(d));
                        sb.Append(" ");
                    }
                    Console.WriteLine(System.IO.Path.GetFileName(depen.Key) + " depends on " + sb.ToString());
                }
                else
                    Console.WriteLine(System.IO.Path.GetFileName(depen.Key) + " does not depend on any file");
            }
        }

        /// <summary>
        /// Displays all the strongly connected components, except the ones with single node.
        /// </summary>
        /// <param name="lstScc"></param>
        static public void ShowSCC(HashSet<List<string>> lstScc)
        {
            Console.WriteLine("NOTE: Intentionally not showing Strong components with just one node(file) in it\n");
            int count = 1;
            foreach (List<string> lst in lstScc)
            {
                if (lst.Count > 1)
                {
                    Console.Write("Strong Component " + count + " :");
                    foreach (string node in lst)
                    {
                        Console.Write(node + " ");
                    }

                    Console.WriteLine("\n");
                    count++;
                }
            }
        }

        /// <summary>
        /// Displays type table
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="table"></param>
        static public void showMetricsTable(string filename, List<Elem> table)
        {
            Console.WriteLine("Printing type table for file:" + System.IO.Path.GetFileName(filename));
            Console.Write(
                "\n  {0,10}  {1,25} ",
                "category", "name"
            );
            Console.Write(
                "\n  {0,10}  {1,25} ",
                "--------", "----"
            );
            foreach (Elem e in table)
            {
                /////////////////////////////////////////////////////////
                // Uncomment to leave a space before each defined type
                // if (e.type == "class" || e.type == "struct")
                //   Console.Write("\n");

                Console.Write(
                  "\n  {0,10}  {1,25}  ",
                  e.type, e.name
                );
            }
        }
        //----< display a semiexpression on Console >--------------------

        static public void displaySemiString(string semi)
        {
            if (showSemi && useConsole)
            {
                Console.Write("\n");
                System.Text.StringBuilder sb = new StringBuilder();
                for (int i = 0; i < semi.Length; ++i)
                    if (!semi[i].Equals('\n'))
                        sb.Append(semi[i]);
                Console.Write("\n  {0}", sb.ToString());
            }
        }
        //----< display, possibly truncated, string >--------------------

        static public void displayString(Action<string> act, string str)
        {
            if (goSlow) Thread.Sleep(200);  //  here only to support visualization
            if (act != null && useFooter)
                act.Invoke(str.Truncate(width));
            if (useConsole)
                Console.Write("\n  {0}", str);
        }
        //----< display string, possibly overriding client pref >--------

        static public void displayString(string str, bool force = false)
        {
            if (useConsole || force)
                Console.Write("\n  {0}", str);
        }
        //----< display rules messages >---------------------------------

        static public void displayRules(Action<string> act, string msg)
        {
            if (showRules)
            {
                displayString(act, msg);
            }
        }
        //----< display actions messages >-------------------------------

        static public void displayActions(Action<string> act, string msg)
        {
            if (showActions)
            {
                displayString(act, msg);
            }
        }
        //----< display filename >---------------------------------------

        static public void displayFiles(Action<string> act, string file)
        {
            if (showFiles)
            {
                displayString(act, file);
            }
        }
        //----< display directory >--------------------------------------

        static public void displayDirectory(Action<string> act, string file)
        {
            if (showDirectories)
            {
                displayString(act, file);
            }
        }

        /// <summary>
        /// Test Stub for Display package
        /// </summary>
        /// <param name="args"></param>
#if (TEST_DISPLAY)
        static void Main(string[] args)
        {
            Console.Write("\n  Testing Display Package");
            Console.Write("\n ===========================\n");
            Dictionary<string, HashSet<string>> dependencies = new Dictionary<string, HashSet<string>>();
            HashSet<string> lst = new HashSet<string> { "file2.cs", "file3.cs" };
            dependencies.Add("file1.cs", lst);
            Elem oElem = new Elem();
            oElem.type = "class";
            oElem.name = "testclass";
            Dictionary<string, List<Elem>> dicMetrics = new Dictionary<string, List<Elem>>();
            List<Elem> repoLoc = new List<Elem> { oElem };
            dicMetrics.Add("file1.cs", repoLoc);
            Console.Write("\n  showMetricsTable method\n");
            Display.showMetricsTable("file1.cs", repoLoc);
            Console.Write("\n  ShowTypeTables method\n");
            Display.ShowTypeTables(dicMetrics);
            Console.Write("\n  ShowDependencies method");
            Display.ShowDependencies(dependencies);
            HashSet<List<string>> lstScc = new HashSet<List<string>>();
            List<string> lstcc = new List<string> { "file1", "file2", "file3" };
            Console.Write("\n  ShowSCC method\n");
            lstScc.Add(lstcc);
            Display.ShowSCC(lstScc);
            Console.ReadLine();
        }
#endif
    }
}
