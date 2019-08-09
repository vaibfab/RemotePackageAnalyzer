///////////////////////////////////////////////////////////////////////
// TTable.cs- Provides a container to store result of Type analysis  //
// ver 1.0                                                           //
// Language:    C# 7.0, 2018, .Net Framework 4.6.1                   //
// Platform:    Dell Inspiron 4736, Windows 10                       //
// Application: Provides container to store type analysis result     //
// Author:      Vaibhav Kumar, Syracuse University, vkumar05@syr.edu //
///////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * TypeTable package generates a container everytime a file is provided to TypeAnalysis.
 *
 */
/* Required Files:
 * ---------------
 *   TTable.cs ScopeStack.cs Element.cs
 *   
 * Public Interfaces:
 * ------------------
 *   Repository repo = new Repository();
 *   repo.semi = semi;
 *   
 * Maintenance History
 * --------------------
 * ver 1.0 : 31st Oct 2018
 * - first release
 *
 */

using CodeAnalysis;
using Lexer;
using System;
using System.Collections.Generic;
namespace TypeTable
{
    public class Repository
    {

        ScopeStack<Elem> stack_ = new ScopeStack<Elem>();
        List<Elem> locations_ = new List<Elem>();

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

        //----< provides all actions access to current semiExp >-----------

        public ITokenCollection semi
        {
            get;
            set;
        }

        // semi gets line count from toker who counts lines
        // while reading from its source

        public int lineCount  // saved by newline rule's action
        {
            get { return semi.lineCount(); }
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
    }

    /// <summary>
    /// Test Stub for Type Table package 
    /// </summary>
    class TestTypeTable
    {
        #if (TEST_TTABLE)
        static void Main(string[] args)
        {
            Console.Write("\n  Testing Type Table package");
            Console.Write("\n ===========================\n");
            Repository objRepo = new Repository();
            Elem oElem = new Elem();
            oElem.type = "class";
            oElem.name = "testclass";
            List<Elem> repoLoc = new List<Elem> { oElem };
            objRepo.locations = repoLoc;
            Console.WriteLine("Printing type table structure\n");
            Console.Write(
                "\n  {0,10}  {1,25} ",
                "category", "name"
            );
            Console.Write(
                "\n  {0,10}  {1,25} ",
                "--------", "----"
            );
            foreach (Elem element in objRepo.locations)
            {
                Console.Write(
                  "\n  {0,10}  {1,25}  ",
                  element.type, element.name);
            }
            Console.ReadLine();
        }
        #endif
    }
}
