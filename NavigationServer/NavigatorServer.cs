////////////////////////////////////////////////////////////////////////////
// NavigatorServer.cs - File Server for WPF NavigatorClient Application   //
// ver 2.0                                                                //
// Jim Fawcett, CSE681 - Software Modeling and Analysis, Fall 2017        //
////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * This package defines a single NavigatorServer class that returns file
 * and directory information about its rootDirectory subtree.  It uses
 * a message dispatcher that handles processing of all incoming and outgoing
 * messages.
 * 
 * Maintanence History:
 * --------------------
 * ver 2.0 - 24 Oct 2017
 * - added message dispatcher which works very well - see below
 * - added these comments
 * ver 1.0 - 22 Oct 2017
 * - first release
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Navigator;
using Environ;
using System.Threading.Tasks;
using ResultManager;
using MessagePassingComm;
using ServerProcess;
using CodeAnalysis;
namespace Server
{
    public class NavigatorServer
    {
        IFileMgr localFileMgr { get; set; } = null;
        Comm comm { get; set; } = null;

        Dictionary<string, Func<CommMessage, CommMessage>> messageDispatcher =
          new Dictionary<string, Func<CommMessage, CommMessage>>();

        /*----< initialize server processing >-------------------------*/

        public NavigatorServer()
        {
            initializeEnvironment();
            Console.Title = "Navigator Server";
            localFileMgr = FileMgrFactory.create(FileMgrType.Local);
        }
        /*----< set Environment properties needed by server >----------*/

        void initializeEnvironment()
        {
            Environ.Environment.root = ServerEnvironment.root;
            Environ.Environment.address = ServerEnvironment.address;
            Environ.Environment.port = ServerEnvironment.port;
            Environ.Environment.endPoint = ServerEnvironment.endPoint;
        }
        /*----< define how each message will be processed >------------*/

        void initializeDispatcher()
        {
            InitializeConnect();
            InitializeTopFile();
            Func<CommMessage, CommMessage> analyze = (CommMessage msg) =>
            {
                ProcessMain oProcess = new ProcessMain();
                localFileMgr.currentPath = "";
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "analyze";
                oProcess.SpawnProc(msg.arguments);
                reply.arguments.Add(ReadTypeResults());
                reply.arguments.Add(ReadDepResults());
                reply.arguments.Add(ReadSCCResults());
                return reply;
            };
            messageDispatcher["analyze"] = analyze;
            Func<CommMessage, CommMessage> moveIntoFolderFiles = (CommMessage msg) =>
            {
                //if (msg.arguments.Count() == 1)
                //  localFileMgr.currentPath = msg.arguments[0];
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "moveIntoFolderFiles";
                reply.arguments = localFileMgr.getFiles().ToList<string>();
                reply.parentFolder = msg.parentFolder;
                return reply;
            };
            messageDispatcher["moveIntoFolderFiles"] = moveIntoFolderFiles;
            Func<CommMessage, CommMessage> moveIntoFolderDirs = (CommMessage msg) =>
            {
                //if (msg.arguments.Count() == 1)
                //localFileMgr.currentPath = msg.arguments[0];
                CommMessage reply = new CommMessage(CommMessage.MessageType.reply);
                reply.to = msg.from;
                reply.from = msg.to;
                reply.command = "moveIntoFolderDirs";
                reply.arguments = localFileMgr.getDirs().ToList<string>();
                reply.parentFolder = msg.parentFolder;
                return reply;
            };
            messageDispatcher["moveIntoFolderDirs"] = moveIntoFolderDirs;
        }

        void InitializeConnect()
        {
            Func<CommMessage, CommMessage> connectCS = (CommMessage msg) => { localFileMgr.currentPath = ""; CommMessage reply = new CommMessage(CommMessage.MessageType.reply); reply.to = msg.from; reply.from = msg.to; reply.command = "connectCS"; reply.arguments.Add(ServerEnvironment.root); return reply; };
            messageDispatcher["connectCS"] = connectCS;
        }

        void InitializeTopFile()
        {
            Func<CommMessage, CommMessage> getTopFiles = (CommMessage msg) => { localFileMgr.currentPath = ""; CommMessage reply = new CommMessage(CommMessage.MessageType.reply); reply.to = msg.from; reply.from = msg.to; reply.command = "getTopFiles"; reply.arguments = localFileMgr.getFiles().ToList<string>(); return reply; }; messageDispatcher["getTopFiles"] = getTopFiles;
        }

        public string ReadDepResults()
        {
            ResultSet oResults = new ResultSet();
            Dictionary<string, HashSet<string>> depResult = oResults.ReadDepResult();
            StringBuilder strDep = new StringBuilder();
            foreach (KeyValuePair<string, HashSet<string>> depRes in depResult)
            {
                strDep.Append(depRes.Key).Append(':');
                foreach (string depFile in depRes.Value)
                {
                    strDep.Append(depFile).Append(',');
                }
                strDep.Append('%');
            }
            return strDep.ToString();
        }

        public string ReadTypeResults()
        {
            ResultSet oResults = new ResultSet();
            Dictionary<string, List<Elem>> depResult = oResults.ReadTypeResult();
            StringBuilder strDep = new StringBuilder();
            foreach (KeyValuePair<string, List<Elem>> depRes in depResult)
            {
                strDep.Append(depRes.Key).Append(':');
                foreach (Elem item in depRes.Value)
                {
                    strDep.Append(item.type).Append('@').Append(item.name).Append('|');
                }
                strDep.Append('%');
            }
            return strDep.ToString();
        }

        public string ReadSCCResults()
        {
            ResultSet oResults = new ResultSet();
            HashSet<List<string>> depResult = oResults.ReadSCCResult();
            StringBuilder strDep = new StringBuilder();
            foreach (List<string> depRes in depResult)
            {
                foreach (string item in depRes)
                {
                    strDep.Append(item).Append(',');
                }
                strDep.Append('%');

            }
            return strDep.ToString();
        }
        /*----< Server processing >------------------------------------*/
        /*
         * - all server processing is implemented with the simple loop, below,
         *   and the message dispatcher lambdas defined above.
         */
        static void Main(string[] args)
        {
            TestUtilities.title("Starting Navigation Server", '=');
            try
            {
                NavigatorServer server = new NavigatorServer();
                server.initializeDispatcher();
                server.comm = new MessagePassingComm.Comm(ServerEnvironment.address, ServerEnvironment.port);
                while (true)
                {
                    CommMessage msg = server.comm.getMessage();
                    if (msg.type == CommMessage.MessageType.closeReceiver)
                        break;
                    msg.show();
                    if (msg.command == null)
                        continue;
                    CommMessage reply = server.messageDispatcher[msg.command](msg);
                    reply.show();
                    server.comm.postMessage(reply);
                }
            }
            catch (Exception ex)
            {
                Console.Write("\n  exception thrown:\n{0}\n\n", ex.Message);
            }
        }
    }
}
