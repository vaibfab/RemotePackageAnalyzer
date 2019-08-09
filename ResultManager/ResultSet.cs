using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using CodeAnalysis;
namespace ResultManager
{
    public class ResultSet
    {
        public bool WriteTypeAnalysisResult(Dictionary<string, List<Elem>> tAnalResult)
        {
            XmlDocument xmlDoc = new XmlDocument();
            XmlNode rootNode = xmlDoc.CreateElement("root");
            xmlDoc.AppendChild(rootNode);
            XmlNode TypeAnal = xmlDoc.CreateElement("TypeAnalysis");
            foreach (KeyValuePair<string, List<Elem>> typeData in tAnalResult)
            {
                XmlNode TypeTable = xmlDoc.CreateElement("TypeTable");
                XmlAttribute attribute = xmlDoc.CreateAttribute("filename");
                attribute.Value = typeData.Key;
                TypeTable.Attributes.Append(attribute);
                foreach (Elem result in typeData.Value)
                {
                    XmlNode Category = xmlDoc.CreateElement("Category");
                    XmlNode Name = xmlDoc.CreateElement("Name");
                    XmlAttribute ctaAttr = xmlDoc.CreateAttribute("type");
                    ctaAttr.Value = result.type;
                    Category.Attributes.Append(ctaAttr);
                    Name.InnerText = result.name;
                    Category.AppendChild(Name);
                    TypeTable.AppendChild(Category);
                }
                TypeAnal.AppendChild(TypeTable);
            }

            rootNode.AppendChild(TypeAnal);
            xmlDoc.Save("test-result.xml");
            return true;
        }

        public bool WriteDepAnalysisResult(Dictionary<string, HashSet<string>> dicDepAnal)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(@"test-result.xml");
            XmlNode rootnode = doc.ChildNodes[0];
            XmlNode depNode = doc.CreateElement("DepAnalysis");
            foreach (KeyValuePair<string, HashSet<string>> result in dicDepAnal)
            {
                XmlNode filenode = doc.CreateElement("Filename");
                XmlAttribute fileAttr = doc.CreateAttribute("fName");
                fileAttr.Value = result.Key;
                filenode.Attributes.Append(fileAttr);
                if (result.Value != null)
                {
                    foreach (string dep in result.Value)
                    {
                        XmlNode depen = doc.CreateElement("Dependency");
                        depen.InnerText = dep;
                        filenode.AppendChild(depen);
                    }
                    depNode.AppendChild(filenode);
                }
            }
            rootnode.AppendChild(depNode);
            doc.Save("test-result.xml");
            return true;
        }

        public bool WriteSCCResult(HashSet<List<string>> resSCC)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(@"test-result.xml");
            XmlNode rootnode = doc.ChildNodes[0];
            XmlNode SccNode = doc.CreateElement("SCC");
            foreach(List<string> SccSet in resSCC)
            {
                XmlNode strComp = doc.CreateElement("StrongComponent");
                
                    foreach(string file in SccSet)
                    {
                        XmlNode filename = doc.CreateElement("filename");
                        filename.InnerText = file;
                        strComp.AppendChild(filename);
                    }
                    SccNode.AppendChild(strComp);
                
            }
            rootnode.AppendChild(SccNode);
            doc.Save("test-result.xml");
            return true;
        }

        public Dictionary<string, List<Elem>> ReadTypeResult()
        {
            Dictionary<string, List<Elem>> dicTypeAnal = new Dictionary<string, List<Elem>>();
            XmlDocument xDocument = new XmlDocument();
            xDocument.Load(@"test-result.xml");
            XmlNode rootnode = xDocument.ChildNodes[0];
            foreach(XmlNode typeNode in rootnode.ChildNodes[0])
            {
                string key = typeNode.Attributes[0].Value;
                List<Elem> values = new List<Elem>();
                foreach(XmlNode catNode in typeNode.ChildNodes)
                {
                    Elem item = new Elem();
                    item.type = catNode.Attributes[0].Value;
                    item.name = catNode.ChildNodes[0].InnerText.ToString();
                    values.Add(item);
                }
                dicTypeAnal.Add(key, values);
            }
            return dicTypeAnal;
        }

        public Dictionary<string, HashSet<string>> ReadDepResult()
        {
            Dictionary<string, HashSet<string>> dicDepAnal = new Dictionary<string,HashSet<string>>();
            XmlDocument xDocument = new XmlDocument();
            xDocument.Load(@"test-result.xml");
            XmlNode rootnode = xDocument.ChildNodes[0];
            foreach (XmlNode typeNode in rootnode.ChildNodes[1])
            {
                string key = typeNode.Attributes[0].Value;
                HashSet<string> values = new HashSet<string>();
                foreach (XmlNode depNode in typeNode.ChildNodes)
                {
                    values.Add(depNode.InnerText);
                }
                dicDepAnal.Add(key, values);
            }
            return dicDepAnal;
        }

        public HashSet<List<string>> ReadSCCResult()
        {
            HashSet<List<string>> resSCC = new HashSet<List<string>>();
            XmlDocument xDocument = new XmlDocument();
            xDocument.Load(@"test-result.xml");
            XmlNode rootnode = xDocument.ChildNodes[0];
            foreach (XmlNode typeNode in rootnode.ChildNodes[2])
            {
                List<string> scc = new List<string>();
                foreach(XmlNode compScc in typeNode.ChildNodes)
                {
                    scc.Add(compScc.InnerText);
                }
                resSCC.Add(scc);
            }
            return resSCC;
        }
    }
}
