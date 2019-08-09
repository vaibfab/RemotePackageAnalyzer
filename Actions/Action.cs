///////////////////////////////////////////////////////////////////////
// Actions.cs - Parser Actions specific to an application            //
// ver 2.5                                                           //
// Language:    C# 7.0, 2018, .Net Framework 4.6.1                   //
// Platform:    Dell Inspiron 4736, Win10                            //
// Application: Provides actions required for Type Analysis in code  //
//              analysis                                             //
// Author:      Jim Fawcett, CST 4-187, Syracuse University          //
//              (315) 443-3948, jfawcett@twcny.rr.com                //
// Modified By: Vaibhav Kumar, Syracuse University, vkumar05@syr.edu //
// Modified on: 31st October 2018                                    //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * Actions package contains all of the Application specific Action
 * code required for most analysis tools.
 *   
 *   It provides following actions - some are specific to a parent rule:
 *   - Print    
 *   - PrintScope
 *   - PushStack
 *   - PopStack
 * Note:
 * This package does not have a test stub since it cannot execute
 * without requests from Parser.
 *  
 */
/* Required Files:
 *   IRuleAndAction.cs, Parser.cs, ITokenCollection.cs, Element.cs
 *   Semi.cs, Toker.cs, Display.cs, TTable.cs, ScopeStack.cs
 *   
 *  Public Interfaces:
 *      AAction.displaySemi=false           //Setting default value for displaySemi
 *      AAction.displayStack=false           //Setting default value for displayStack
 *      PushStack push=new PushStack(Repository repo)
 *      PopStack pop=new PopStack(Repository repo)
 *      AAction.DoAction(semi);
 *   
 * Maintenance History:
 * --------------------
 * ver 2.5 : 31st Oct 2018
 * - In order to follow single responsibility principle, the action part of the 
 *   earlier existing file RulesAndActions.cs have been segreggated into a seperate 
 *   package(this package)
 * ver 2.4 : 09 Oct 2018
 * - modified comments
 * - removed unnecessary definition from repository class
 * - moved local semi definition inside display test in PopStack action
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
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeAnalysis;
using TypeTable;
using Lexer;
namespace Actions
{
    public abstract class AAction : IAction
    {
        static bool displaySemi_ = false;   // default
        static bool displayStack_ = false;  // default

        protected Repository repo_;

        //static public Action<string> actionDelegate;

        public abstract void doAction(Lexer.ITokenCollection semi);

        public static bool displaySemi
        {
            get { return displaySemi_; }
            set { displaySemi_ = value; }
        }
        public static bool displayStack
        {
            get { return displayStack_; }
            set { displayStack_ = value; }
        }

        public virtual void display(Lexer.ITokenCollection semi)
        {
            if (displaySemi)
                for (int i = 0; i < semi.size(); ++i)
                    Console.Write("{0} ", semi[i]);
        }
    }

    ///////////////////////////////////////////////////////////////////
    // pushes scope info into stack
    public class PushStack : AAction
    {
        public PushStack(Repository repo)
        {
            repo_ = repo;
        }

        public override void doAction(ITokenCollection semi)
        {
            //Display.displayActions(actionDelegate, "action PushStack");
            ++repo_.scopeCount;
            Elem elem = new Elem();
            elem.type = semi[0];     // expects type, i.e., namespace, class, struct, ..
            elem.name = semi[1];     // expects name

            repo_.stack.push(elem);

            // display processing details if requested

            if (AAction.displayStack)
                repo_.stack.display();
            if (AAction.displaySemi)
            {
                Console.Write("\n  line# {0,-5}", repo_.semi.lineCount() - 1);
                Console.Write("entering ");
                string indent = new string(' ', 2 * repo_.stack.count);
                Console.Write("{0}", indent);
                this.display(semi); // defined in abstract action
            }

            // add starting location if namespace, type, or function

            if (elem.type == "control" || elem.name == "anonymous")
                return;
            repo_.locations.Add(elem);
        }
    }

    ///////////////////////////////////////////////////////////////////
    // pops scope info from stack when leaving scope    
    public class PopStack : AAction
    {
        public PopStack(Repository repo)
        {
            repo_ = repo;
        }
        public override void doAction(ITokenCollection semi)
        {
            Elem elem;
            try
            {
                // if stack is empty (shouldn't be) pop() will throw exception

                elem = repo_.stack.pop();

                // record ending line count and scope level

                for (int i = 0; i < repo_.locations.Count; ++i)
                {
                    Elem temp = repo_.locations[i];
                    if (elem.type == temp.type)
                    {
                        if (elem.name == temp.name)
                        {

                        }
                    }
                }
            }
            catch
            {
                return;
            }

            if (AAction.displaySemi)
            {
                Lexer.ITokenCollection local = Factory.create();
                local.add(elem.type).add(elem.name);
                if (local[0] == "control")
                    return;
                Console.Write("\n  line# {0,-5}", repo_.semi.lineCount());
                Console.Write("leaving  ");
                string indent = new string(' ', 2 * (repo_.stack.count + 1));
                Console.Write("{0}", indent);
                this.display(local); // defined in abstract action
            }
        }
    }




}
