///////////////////////////////////////////////////////////////////////
// Rule.cs - Parser rules specific to an application                 //
// ver 2.5                                                           //
// Language:    C# 7.0, 2018, .Net Framework 4.6.1                   //
// Platform:    Dell Inspiron 4736, Windows 10                       //
// Application: Demonstration for CSE681, Project #2, Fall 2011      //
// Author:      Jim Fawcett, CST 4-187, Syracuse University          //
//              (315) 443-3948, jfawcett@twcny.rr.com                //
// Modiefied By: Vaibhav Kumar, Syracuse University, vkumar05@syr.edu//
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * Rules package contains all of the Application specific 
 * rules required for most analysis tools.
 *
 * It defines the following rules which each have a
 * grammar construct detector and also a collection of IActions:
 *   - DetectNameSpace rule
 *   - DetectClass rule
 *   - DetectFunction rule
 *   - DetectScopeChange rule
 *   - DetectUserNamespace rule
 *   - DetectDelegate rule
 *   
 * Note:
 * This package does not have a test stub since it cannot execute
 * without requests from Parser.
 *  
 */
/* Required Files:
 *   IRuleAndAction.cs, Parser.cs, Display.cs,
 *   Semi.cs, Toker.cs
 *   
 * Public Interfaces:
 *   List<IRule> Rules = new List<IRule>();
 *   Rules.Add(rule);
 *   rule.test(semi);
 *   
 * Maintenance History
 * --------------------
 * ver 2.5 :31st Oct 2018
 * - Added rules to detect imported user defined libraries and delegates (DetectUserNamespace,DetectDelegate)
 * - In order to follow single responsibility principle, the rule part of the 
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

using CodeAnalysis;
using Lexer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rules
{
    public abstract class ARule : IRule
    {
        private List<IAction> actions;
        public ARule()
        {
            actions = new List<IAction>();
        }
        public void add(IAction action)
        {
            actions.Add(action);
        }
        abstract public bool test(Lexer.ITokenCollection semi);
        public void doActions(Lexer.ITokenCollection semi)
        {
            foreach (IAction action in actions)
                action.doAction(semi);
        }
        public int indexOfType(Lexer.ITokenCollection semi)
        {
            int indexCL;
            semi.find("class", out indexCL);
            int indexIF;
            semi.find("interface", out indexIF);
            int indexST;
            semi.find("struct", out indexST);
            int indexEN;
            semi.find("enum", out indexEN);
            int indexDE;
            semi.find("delegate", out indexDE);

            int index = Math.Max(indexCL, indexIF);
            index = Math.Max(index, indexST);
            index = Math.Max(index, indexEN);
            index = Math.Max(index, indexDE);
            return index;
        }
    }


    ///////////////////////////////////////////////////////////////////
    // rule to detect namespace declarations

    public class DetectNamespace : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            //Display.displayRules(actionDelegate, "rule   DetectNamespace");
            int index;
            semi.find("namespace", out index);
            if (index != -1 && semi.size() > index + 1)
            {
                ITokenCollection local = Factory.create();
                // create local semiExp with tokens for type and name
                local.add(semi[index]).add(semi[index + 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }

    ///////////////////////////////////////////////////////////////////
    // rule to detect user defined namespace declarations
    public class DetectUserNamespace : ARule
    {
        
        public override bool test(ITokenCollection semi)
        {
            int index;
            string type = String.Empty;
            semi.find("using", out index);
            if (index != -1 && semi.size() > index + 1 && (!(semi[index + 1].ToString().ToLower().Equals("system"))))
            {
                ITokenCollection local = Factory.create();
                if ((semi.contains("=") && semi.Count() > 3 ) || semi[index+1].Contains("("))
                {
                    ;
                }
                else
                {
                    type = "using";               
                    local.add(semi[index]).add(semi[index + 1]);
                    doActions(local);
                }
                return true;
            }
            return false;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // rule to dectect class definitions

    public class DetectClass : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            // Display.displayRules(actionDelegate, "rule   DetectClass");
            int indexCL;
            semi.find("class", out indexCL);
            int indexIF;
            semi.find("interface", out indexIF);
            int indexST;
            semi.find("struct", out indexST);
            int indexEnum;
            semi.find("enum", out indexEnum);
            int index = Math.Max(indexCL, indexIF);
            index = Math.Max(index, indexST);
            index = Math.Max(index, indexEnum);
            if (index != -1 && semi.size() > index + 1)
            {
                ITokenCollection local = Factory.create();
                // local semiExp with tokens for type and name
                local.add(semi[index]).add(semi[index + 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }

    ///////////////////////////////////////////////////////////////////
    // rule to detect delegate declarations
    public class DetectDelegate : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            int indexDel;
            semi.find("delegate", out indexDel);
           
            if (indexDel != -1 && semi.size() > indexDel + 1)
            {
                ITokenCollection local = Factory.create();
                local.add(semi[indexDel]).add(semi[indexDel + 2]);
                doActions(local);
                return true;
            }
            return false;
        }
    }

    ///////////////////////////////////////////////////////////////////
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
        public override bool test(ITokenCollection semi)
        {
            if (semi[semi.size() - 1] != "{")
                return false;

            int index;
            semi.find("(", out index);
            if (index > 0 && !isSpecialToken(semi[index - 1]))
            {
                ITokenCollection local = Factory.create();
                local.add("function").add(semi[index - 1]);
                doActions(local);
                return true;
            }
            return false;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // detect entering anonymous scope
    // - expects namespace, class, and function scopes
    //   already handled, so put this rule after those

    public class DetectAnonymousScope : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            //Display.displayRules(actionDelegate, "rule   DetectAnonymousScope");
            int index;
            semi.find("{", out index);
            if (index != -1)
            {
                ITokenCollection local = Factory.create();
                // create local semiExp with tokens for type and name
                local.add("control").add("anonymous");
                doActions(local);
                return true;
            }
            return false;
        }
    }
    ///////////////////////////////////////////////////////////////////
    // detect public declaration

    //public class DetectPublicDeclar : ARule
    //{
    //    public override bool test(ITokenCollection semi)
    //    {
    //        //Display.displayRules(actionDelegate, "rule   DetectPublicDeclar");
    //        int index;
    //        semi.find(";", out index);
    //        if (index != -1)
    //        {
    //            semi.find("public", out index);
    //            if (index == -1)
    //                return true;
    //            ITokenCollection local = Factory.create();
    //            // create local semiExp with tokens for type and name
    //            //local.displayNewLines = false;
    //            local.add("public " + semi[index + 1]).add(semi[index + 2]);

    //            semi.find("=", out index);
    //            if (index != -1)
    //            {
    //                doActions(local);
    //                return true;
    //            }
    //            semi.find("(", out index);
    //            if (index == -1)
    //            {
    //                doActions(local);
    //                return true;
    //            }
    //        }
    //        return false;
    //    }
    //}
    ///////////////////////////////////////////////////////////////////
    // detect leaving scope

    public class DetectLeavingScope : ARule
    {
        public override bool test(ITokenCollection semi)
        {
            // Display.displayRules(actionDelegate, "rule   DetectLeavingScope");
            int index;
            semi.find("}", out index);
            if (index != -1)
            {
                doActions(semi);
                return true;
            }
            return false;
        }
    }
}
