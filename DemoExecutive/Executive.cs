/////////////////////////////////////////////////////////////////////////// 
// Executive.cs -  Entry point of the LexicalScanner applcation          //
// Version  :    1.0                                                     //
// Language :    C#, Visual Studio 2017,.Net Framework 4.6.1             //
// Platform :    Dell Inspiron  , Windows 10                             //
// Application:  Entry point of Code Analyzer                            //                                             //
// Author:      Vaibhav Kumar, Syracuse University                       //
///////////////////////////////////////////////////////////////////////////
/*
 * Module Operations
 * =================
 * Executive package is the entry point of the LexicalScanner application. It has ClientExecutive.cs 
 * class file which contains methods to call SemiExpression package's methods via interface ITokCollection
 * and displays the result for demonstrating requirements.
 * 
 * Public Interface
 * ================
 * 
 * NOTE: No public interface required for this package as this is just the entry point
 *       of the application and doesn't contain any public class or public methods.
 * 
 */
//
/*
 * Build Process
 * =============
 * Required Files:
 *   TAnal.cs TTable.cs ScopeStack.cs Parser.cs Action.cs Rule.cs IRuleAndAction.cs CsGraph.cs
 *   ITokCollection.cs Semi.cs Toker.cs Dependencies.cs Display.cs Element.cs FileMgr.cs Scc.cs 
 *   TarjanStack.cs 
 *   
 * Maintenance History
 * ===================
 * Version 1.0: 31st October 2018
 * - first release 
 * 
 */
//
using DepAnalysis;
using StrongComponent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ResultManager;
namespace CodeAnalysis
{
    using System.Text.RegularExpressions;

    class Executive
    {
        /// <summary>
        /// Gets all the cSharp(*.cs) files from the path rooted at specified directory. It also 
        /// fetches .cs files from subdirectories, if any.
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        static List<string> GetFileFromDir(string[] args)
        {
            List<string> lstFiles = new List<string>();
            string path;
            if (args.Length > 0)
                path = args[0];
            else
                path = Directory.GetCurrentDirectory();
            //Navigate nav = new Navigate();

            int i = 0;
            for (i = 1; i < args.Length; ++i)
            {
              //  nav.Add(args[i]);
            }
            //nav.go(path,ref lstFiles);
            return lstFiles;
        }

        /// <summary>
        /// Demonstration of requirement 1 and 2
        /// </summary>
        static void ReqOneTwo()
        {
            Console.WriteLine("\n\n\t********************REQUIREMENT 1********************");
            Console.WriteLine("\tThis console project is developed in C# using Visual Studio 2017");
            Console.WriteLine("\n\n\t********************REQUIREMENT 2********************");
            Console.WriteLine("\tThis project uses .NET System.IO and System.IO.Text");
        }

        /// <summary>
        /// Demonstration of requirement 3
        /// </summary>
        static void ReqThree()
        {
            Console.WriteLine("\n\n\t********************REQUIREMENT 3********************");
            Console.WriteLine("\n\tPrinting all the packages in current solution to demonstrate the packages present.");
            var slnPath = Path.GetFullPath(@"../../../RemoteCodeAnalyzer.sln");
            var Content = File.ReadAllText(slnPath);
            Regex projReg = new Regex(
                "Project\\(\"\\{[\\w-]*\\}\"\\) = \"([\\w _]*.*)\", \"(.*\\.(cs|vcx|vb)proj)\""
                , RegexOptions.Compiled);
            var matches = projReg.Matches(Content).Cast<Match>();
            var Projects = matches.Select(x => x.Groups[2].Value).ToList();
            Console.WriteLine("\n");
            for (int i = 0; i < Projects.Count; ++i)
            {
                if (!Path.IsPathRooted(Projects[i]))
                    Projects[i] = Path.Combine(Path.GetDirectoryName(slnPath),
                        Projects[i]);
                Projects[i] = Path.GetFullPath(Projects[i]);
                Projects[i] = Path.GetFileName(Projects[i]);
                Console.WriteLine(Projects[i]);
            }
        }

        public static List<string> GetFilesFromArg(string args)
        {
            List<string> lst = new List<string>();
            string[] filenames=args.Split(',');
            return filenames.ToList<string>();
        }

        /// <summary>
        /// Main method (Entry point)
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Console.Write("\n\t\t\t  Demonstrating Parser");
            Console.Write("\n\t =========================================================\n");
            ReqOneTwo();
            ReqThree();
            Console.WriteLine("Following files are being evaluated:\t");
            Dictionary<string, List<Elem>> dicTables = new Dictionary<string, List<Elem>>();
            ResultSet oResult = new ResultSet(); 
            Console.Write("\n\n\t\t  Displaying Type Tables ");
            Console.Write("\n\t\t ==========================\n");
            TAnal objTAnalysis = new TAnal();
            dicTables = objTAnalysis.DoTypeAnalysis(GetFilesFromArg(args[0]));
            oResult.WriteTypeAnalysisResult(dicTables);
            Display.ShowTypeTables(dicTables);
            Console.Write("\n\t\t  Displaying Dependencies ");
            Console.Write("\n\t\t =========================\n");
            Dependencies objDependencies = new Dependencies();
            Dictionary<string, HashSet<string>> dicDependencies = new Dictionary<string, HashSet<string>>();
            dicDependencies = objDependencies.DoAnalysis(dicTables);            
            oResult.WriteDepAnalysisResult(dicDependencies);
            Display.ShowDependencies(dicDependencies);
            Console.Write("\n\t\t  Displaying Strong Components ");
            Console.Write("\n\t\t ===============================\n");
            Scc objScc = new Scc();
            HashSet<List<string>> lstScc = objScc.GetSCC(dicDependencies);
            oResult.WriteSCCResult(lstScc);
            Display.ShowSCC(lstScc);
            Console.Write("\n\n");
            //Console.ReadLine();
        }
    }
}

