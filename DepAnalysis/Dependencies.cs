///////////////////////////////////////////////////////////////////////
// Dependencies.cs- Performs dependency analysis                     //
// ver 1.0                                                           //
// Language:    C# 7.0, 2018, .Net Framework 4.6.1                   //
// Platform:    Dell Inspiron 4736, Windows 10                       //
// Application: Identify dependencies among provided set of input    // 
//              files.                                               //
// Author:      Vaibhav Kumar, Syracuse University, vkumar05@syr.edu //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * DepAnalysis package provide facilities to identify dependencies among files.
 *
 */
/* Required Files:
 * ---------------
 *   Dependencies.cs Element.cs Semi.cs Toker.cs
 *   
 * Public Interfaces:
 * ------------------
 *   Dictionary<string, HashSet<string>> dicDependencies = new Dictionary<string, HashSet<string>>();
 *   dicDependencies = objDependencies.DoAnalysis(dicTables);
 *   
 * Maintenance History
 * --------------------
 * ver 1.0 : 31st Oct 2018
 * - first release
 *
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAnalysis;
using Lexer;
namespace DepAnalysis
{
    public class Dependencies
    {
        /// <summary>
        /// Performs dependency analysis by making use of tpe analysis results
        /// </summary>
        /// <param name="DicTables"></param>
        /// <returns></returns>
        public Dictionary<string, HashSet<string>> DoAnalysis(Dictionary<string, List<Elem>> DicTables)
        {
            List<string> uTypes = new List<string> { "class", "struct", "enum", "delegate", "interface" };
            Dictionary<string, HashSet<string>> resDepen = new Dictionary<string, HashSet<string>>();
            foreach (KeyValuePair<string, List<Elem>> table in DicTables)
            {
                foreach (Elem item in table.Value)
                {
                    if (uTypes.Contains(item.type.ToString().ToLower()))
                    {
                        DoComparison(item.name, table.Key, DicTables, ref resDepen);
                    }
                    else if (item.type.ToString().ToLower() == "using")
                    {
                        CheckUsing(DicTables,uTypes,item, resDepen);
                    }
                }
            }
            foreach (KeyValuePair<string, List<Elem>> keyItem in DicTables)
            {
                if (!resDepen.ContainsKey(keyItem.Key))
                {
                    resDepen.Add(keyItem.Key, null);
                }

            }
            return resDepen;
        }

        /// <summary>
        /// Checks for the file dependency based upon namespace referece along with usage of 
        /// types declared under the fetched namespace
        /// </summary>
        /// <param name="DicTables"></param>
        /// <param name="uTypes"></param>
        /// <param name="item"></param>
        /// <param name="resDepen"></param>
        public void CheckUsing(Dictionary<string, List<Elem>> DicTables, List<string> uTypes,Elem item, Dictionary<string, HashSet<string>> resDepen)
        {
            foreach (KeyValuePair<string, List<Elem>> tTable in DicTables)
            {
                for (int i = 0; i < tTable.Value.Count; ++i)
                {
                    if (tTable.Value[i].type.Contains("namespace") && tTable.Value[i].name.Equals(item.name))
                    {
                        foreach (Elem elem in tTable.Value)
                        {
                            if (uTypes.Contains(elem.type.ToString().ToLower()))
                            {
                                DoComparison(elem.name, tTable.Key, DicTables, ref resDepen);
                            }
                        }
                    }
                }

            }
        }

        /// <summary>
        /// Performs comparisons to identify the existence of dependencies among files
        /// </summary>
        /// <param name="typeName"></param>
        /// <param name="filename"></param>
        /// <param name="tables"></param>
        /// <param name="dicRes"></param>
        public void DoComparison(string typeName, string filename, Dictionary<string, List<Elem>> tables, ref Dictionary<string, HashSet<string>> dicRes)
        {
            ITokenCollection semi = Factory.create();
            foreach (KeyValuePair<string, List<Elem>> table in tables)
            {
                if (table.Key != filename)
                {
                    semi.open(table.Key as string);
                    while (semi.get().Count > 0)
                    {
                        if (semi.Contains(typeName))
                        {
                            if (!dicRes.ContainsKey(table.Key))
                            {
                                HashSet<string> valDic = new HashSet<string>();
                                dicRes.Add(table.Key, valDic);
                                dicRes[table.Key].Add(filename);
                                break;
                            }
                            else
                            {
                                dicRes[table.Key].Add(filename);
                                break;
                            }
                        }
                    }
                }
            }
        }

    }
}
