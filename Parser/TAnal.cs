///////////////////////////////////////////////////////////////////////
// TAnal.cs- Performs Type analysis                                  //
// ver 1.0                                                           //
// Language:    C# 7.0, 2018, .Net Framework 4.6.1                   //
// Platform:    Dell Inspiron 4736, Windows 10                       //
// Application: Identify user defined types, namespaces in a file    // 
//              files(Type analysis)                                               //
// Author:      Vaibhav Kumar, Syracuse University, vkumar05@syr.edu //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * TypeAnalysis package provide facilities to identify all the user defined types in the provided file.
 *
 */
/* Required Files:
 * ---------------
 *   TAnal.cs Element.cs Semi.cs Toker.cs Display.cs Parser.cs IRulesAndActions.cs Rule.cs Action.cs
 *   TTable.cs ScopeStack.cs
 *   
 * Public Interfaces:
 * ------------------
 *   TAnal objTAnalysis = new TAnal();
 *   dicTables = objTAnalysis.DoTypeAnalysis(files);
 *   
 * Maintenance History
 * --------------------
 * ver 1.0 : 31st Oct 2018
 * - first release
 *
 */

using Lexer;
using System;
using System.Collections.Generic;
using TypeTable;
namespace CodeAnalysis
{
    public class TAnal
    {
        /// <summary>
        /// Performs type analysis and generates type table for every file.
        /// </summary>
        /// <param name="files"></param>
        /// <returns>type tabeles for every file</returns>
        public Dictionary<string, List<Elem>> DoTypeAnalysis(List<string> files)
        {
            Dictionary<string, List<Elem>> dicTables = new Dictionary<string, List<Elem>>();
            foreach (string file in files)
            {
                ITokenCollection semi = Factory.create();
                if (!semi.open(file as string))
                {
                    Console.Write("\n  Can't open {0}\n\n", file);
                    //return;
                }
                BuildCodeAnalyzer builder = new BuildCodeAnalyzer(semi);
                Parser parser = builder.build();
                try
                {
                    while (semi.get().Count > 0)
                        parser.parse(semi);
                }
                catch (Exception ex)
                {
                    Console.Write("\n\n  {0}\n", ex.Message);
                }
                Repository rep = Repository.getInstance();
                List<Elem> table = rep.locations;
                dicTables.Add(file, table);
                semi.close();
            }
            return dicTables;
        }
    }

    /// <summary>
    /// Test stub for type analysis package
    /// </summary>
    class TestTypeAnalysis
    {
#if (TEST_TANALYSIS)
        static void Main(string[] args)
        {
            Console.Write("\n  Testing Type Analysis package");
            Console.Write("\n ===========================\n");
            TAnal objAnalysis = new TAnal();
            Dictionary<string, List<Elem>> dicResult = new Dictionary<string, List<Elem>>();
            List<string> lstFiles = new List<string> { System.IO.Path.GetFullPath("../../Parser.cs"), System.IO.Path.GetFullPath("../../TAnal.cs") };
            dicResult = objAnalysis.DoTypeAnalysis(lstFiles);
            Console.Write("\n  Printing type tables\n");
            foreach (KeyValuePair<string, List<Elem>> table in dicResult)
            {
                Console.Write("\n\n");
                Display.showMetricsTable(table.Key, table.Value);
            }
            Console.ReadLine();
        }
#endif
    }
}
