///////////////////////////////////////////////////////////////////////
// Scc.cs - Identifies strongly connected components                 //
// ver 1.0                                                           //
// Language:    C# 7.0, 2018, .Net Framework 4.6.1                   //
// Platform:    Dell Inspiron 4736, Windows 10                       //
// Application: Identify Strongly connected components in a directed //
//              graph of files and dependencies.                     //
// Author:      Vaibhav Kumar, Syracuse University, vkumar05@syr.edu //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * Strong component package provide facilities to build a graph from a set of file dependencies 
 * and identify strongly connected components in that graph.
 *
 */
/* Required Files:
 *   Scc.cs TarjanStack.cs
 *   
 * Public Interfaces:
 *   Scc objScc = new Scc();
 *   List<HashSet<string>> lstScc = objScc.GetSCC(Dependencies);
 *   
 * Maintenance History
 * --------------------
 * ver 1.0 : 31st Oct 2018
 * - first release
 *
 */

using CsGraph;
using System;
using System.Collections.Generic;

namespace StrongComponent
{
    public class Scc
    {

        /// <summary>
        /// Gets all te strongly connected components. It implements Tarjan's algorithm which
        /// takes a graph as the only input
        /// </summary>
        /// <param name="dependencies"></param>
        /// <returns>List of strongly connected components</returns>

        public HashSet<List<string>> GetSCC(Dictionary<string, HashSet<string>> dependencies)
        {
            HashSet<List<string>> lstSCC = new HashSet<List<string>>();
            Stack<CsNode<string, string>> stack = new Stack<CsNode<string, string>>();
            HashSet<CsNode<string, string>> lstNodes = new HashSet<CsNode<string, string>>();
            Dictionary<string, HashSet<CsNode<string, string>>> myGraph = new Dictionary<string, HashSet<CsNode<string, string>>>();
            foreach (KeyValuePair<string, HashSet<string>> node in dependencies)
            {
                lstNodes.Add(new CsNode<string, string>(System.IO.Path.GetFileName(node.Key)));
                HashSet<CsNode<string, string>> adj = new HashSet<CsNode<string, string>>();
                if (node.Value != null)
                    foreach (string val in node.Value)
                        adj.Add(new CsNode<string, string>(System.IO.Path.GetFileName(val)));
                myGraph.Add(System.IO.Path.GetFileName(node.Key), adj);
            }
            lstSCC = DoTarjan(myGraph, lstNodes);
            return lstSCC;
        }

        /// <summary>
        /// Implementation of Tarjan's algorithm
        /// </summary>
        /// <param name="node"></param>
        /// <param name="S"></param>
        /// <param name="index"></param>
        /// <param name="lstRes"></param>
        /// <returns></returns>

        public HashSet<List<string>> DoTarjan(Dictionary<string, HashSet<CsNode<string, string>>> myGraph, HashSet<CsNode<string, string>> lstNodes)
        {
            int index = 0; Stack<CsNode<string, string>> stack = new Stack<CsNode<string, string>>(); HashSet<List<string>> lstSCC = new HashSet<List<string>>();
            Action<CsNode<string, string>> StrongConnect = null;
            StrongConnect = (node) =>
            {
                node.Index = node.lowlink = index;index++;stack.Push(node);
                if (myGraph[node.name].Count < 1)
                {
                    List<string> lstSingleNodeSCC = new List<string>();
                    lstSingleNodeSCC.Add(node.name);
                    lstSCC.Add(lstSingleNodeSCC);
                }
                else
                {
                    foreach (var adjNode in myGraph[node.name])
                    {
                        int indTrue = IsPresent(stack, adjNode.name);
                        if (adjNode.Index < 0 && indTrue < 0)
                        {
                            StrongConnect(adjNode);
                            node.lowlink = Math.Min(node.lowlink, adjNode.lowlink);
                        }
                        else if (indTrue > -1)
                            node.lowlink = Math.Min(node.lowlink, indTrue);
                        if ((node.lowlink == node.Index) && myGraph[adjNode.name].Count > 0)
                        {
                            List<string> lstSingleSCC = new List<string>();
                            CsNode<string, string> poppedNode = new CsNode<string, string>("popped");
                            do
                            {
                                if (stack.Count > 0)
                                {
                                    poppedNode = stack.Pop();
                                    lstSingleSCC.Add(poppedNode.name);
                                    UpdateNodeIndex(node, poppedNode, lstNodes);
                                }
                                else
                                    break;
                            } while (poppedNode != node);
                            lstSCC.Add(lstSingleSCC);
                        }
                    }
                }
            };
            foreach (var node in lstNodes)
                if (node.Index < 0)
                    StrongConnect(node);
            return lstSCC;
        }

        /// <summary>
        /// Updates the index value of every node 
        /// </summary>
        /// <param name="parentNode"></param>
        /// <param name="childNode"></param>
        /// <param name="lstNodes"></param>
        public void UpdateNodeIndex(CsNode<string, string> parentNode, CsNode<string, string> childNode, HashSet<CsNode<string, string>> lstNodes)
        {
            foreach (CsNode<string, string> node in lstNodes)
            {
                if (node.name == childNode.name || node.name == parentNode.name)
                {
                    if (node.name == childNode.name)
                        node.Index = childNode.Index;
                    else
                        node.Index = parentNode.Index;
                    break;
                }
            }
        }

        /// <summary>
        /// Checks for the presence of a node in the stack and returns the index if present.
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public int IsPresent(Stack<CsNode<string, string>> stack, string name)
        {
            int ind = -1;
            foreach (CsNode<string, string> node in stack)
            {
                if (node.name == name)
                {
                    return node.Index;
                }
            }
            return ind;
        }
    }

    /// <summary>
    /// Test stub for strong component package
    /// </summary>
    class DemoSCC
    {
#if (TEST_SCC)
        static void Main(string[] args)
        {
            Console.Write("\n  Demonstrating Strong Components");
            Console.Write("\n ==================================\n");
            Dictionary<string, HashSet<string>> testDependencies = new Dictionary<string, HashSet<string>>();
            HashSet<string> testDepen = new HashSet<string> { "File2.cs" };
            testDependencies.Add("File1.cs", testDepen);
            testDepen = new HashSet<string> { "File3.cs" };
            testDependencies.Add("File2.cs", testDepen);
            testDepen = new HashSet<string> { "File1.cs" };
            testDependencies.Add("File3.cs", testDepen);
            Scc objSCC = new Scc();
            HashSet<List<string>> lstSCC = new HashSet<List<string>>();
            lstSCC = objSCC.GetSCC(testDependencies);
            int count = 1;
            foreach (List<string> lst in lstSCC)
            {
                if (lst.Count > 1)
                {
                    Console.Write("Strong Component " + count + " :");
                    foreach (string node in lst)
                        Console.Write(node + " ");
                    Console.WriteLine("\n");
                    count++;
                }
            }
            Console.ReadLine();
        }

#endif
    }
}
