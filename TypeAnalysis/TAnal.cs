using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TypeTable;
using CodeAnalysis;
namespace TypeAnalysis
{
    public class TAnal
    {
        public class BuildCodeAnalyzer
        {
            Repository repo = new Repository();

            public BuildCodeAnalyzer(Lexer.ITokenCollection semi)
            {
                repo.semi = semi;
            }
            public virtual Parser build()
            {
                Parser parser = new Parser();

                // decide what to show
                AAction.displaySemi = false;
                AAction.displayStack = false;  // false is default

                // action used for namespaces, classes, and functions
                PushStack push = new PushStack(repo);

                // capture namespace info
                DetectNamespace detectNS = new DetectNamespace();
                detectNS.add(push);
                parser.add(detectNS);

                // capture class info
                DetectClass detectCl = new DetectClass();
                detectCl.add(push);
                parser.add(detectCl);

                // capture function info
                DetectFunction detectFN = new DetectFunction();
                detectFN.add(push);
                parser.add(detectFN);

                // handle entering anonymous scopes, e.g., if, while, etc.
                DetectAnonymousScope anon = new DetectAnonymousScope();
                anon.add(push);
                parser.add(anon);

                // show public declarations
                DetectPublicDeclar pubDec = new DetectPublicDeclar();
                SaveDeclar print = new SaveDeclar(repo);
                pubDec.add(print);
                parser.add(pubDec);

                // handle leaving scopes
                DetectLeavingScope leave = new DetectLeavingScope();
                PopStack pop = new PopStack(repo);
                leave.add(pop);
                parser.add(leave);

                // parser configured
                return parser;
            }
        }
    }
}
