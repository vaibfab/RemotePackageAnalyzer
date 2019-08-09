///////////////////////////////////////////////////////////////////////////
// Element.cs - Data Structure for holding Parser analysis results       //
// ver 1.1                                                               //
// Language:    C# 7.0, .Net Framework 4.6.1                             //
// Platform:    Dell XPS 8900, Win10                                     //
// Application: Demonstration for CSE681, Project #2, Fall 2018          //
// Author:      Jim Fawcett, CST 4-187, Syracuse University              //
//              (315) 443-3948, jfawcett@twcny.rr.com                    //
// Modified By: Vaibhav Kumar, Syracuse University, vkumar05@syr.edu     //
///////////////////////////////////////////////////////////////////////////
/*
 * Module Operations:
 * ------------------
 * This module defines the Elem class, which holds:
 *   - type: class, struct, enum
 *   - name
 *  
 */
/* Required Files:
 *   Element.cs
 *   
 * Maintenance History:
 * --------------------
 * ver 1.1 : 31st Oct 2018
 * - Removed beginLine, endLine, size and complexity fields
 * ver 1.0 : 04 Oct 2018
 * - first release
 *
 * Note:
 * This package does not have a test stub as it contains only a data structure.
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalysis
{
    public class Elem  // holds scope information
    {
        public string type { get; set; }
        public string name { get; set; }


        public override string ToString()
        {
            StringBuilder temp = new StringBuilder();
            temp.Append("{");
            temp.Append(String.Format("{0,-10}", type)).Append(" : ");
            temp.Append(String.Format("{0,-10}", name)).Append(" : ");
            temp.Append("}");
            return temp.ToString();
        }
    }
}

