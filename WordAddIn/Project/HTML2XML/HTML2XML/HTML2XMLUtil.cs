using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using HTML2XML.Select;
using HTML2XML.Nodes;
using System.Text.RegularExpressions;
using System.Collections;
using System.IO;

namespace HTML2XML
{

    public class ExternalDataEventArgs : EventArgs {
      public string FilePath { get; set; }
      public string FileName { get; set; }
      public byte[] Data { get; set; }
    }

    public class HTML2XMLUtil
    {
        public static string basePath = "";
        public static bool imageAsBase64;
        public static List<string> callBackClasses = new List<string>();

        public static EventHandler TreatSpecialElement;
        public static EventHandler<ExternalDataEventArgs> TreatExternalData;
        private static Document docDes;
        private static void OnTreatSpecialElement(EventArgs e) {
          if (TreatSpecialElement != null) {
            TreatSpecialElement(docDes, e); 
          }
        }

        private static void OnTreatExternalData(ExternalDataEventArgs e) {
          if (TreatExternalData != null) {
            TreatExternalData(docDes, e);
          }
        }

        public static string parseHTML(string html)
        {
            

            Document docSrc=NSoupClient.Parse(html);
            Elements elements = docSrc.GetElementsByTag("body");
            Element body=elements.First;
            Elements section1s = docSrc.GetElementsByClass("Section1");
          
            docDes = NSoupClient.Parse("");
            Element content = docDes.CreateElement("Content");
           
            process(docDes, body, content);

            return content.ToString();
        }

        

        private static void process(Document docDes, Node current,Node parent)           
        {
           
            Stack<Node> stack = new Stack<Node>();
            Node[] nodes = current.ChildNodesAsArray();
            Node elem = null;
            for (int i = 0; i < nodes.Length; i++){
                Node n = nodes[i];
                string name = n.NodeName;
                if (name.IndexOf("div") >= 0){
                    Console.WriteLine("j");
                }
                var match = Regex.Match(name, @"[hH]\d");
                if (match.Success){
                    elem = docDes.CreateElement("Element");
                    elem.Attr("Type", "Section");
                    elem.Attr("Level", name.Substring(1));
                    while (stack.Count > 0){
                        Node top = stack.Peek();
                        if (top != null){
                            string topname = top.Attr("Level");
                            int toplevel = Int32.Parse(topname);
                            int currentlevel = Int32.Parse(name.Substring(1));
                            if (toplevel >= currentlevel){
                                stack.Pop();
                            }else{
                                top.AddChildren(elem);
                                break;
                            }
                        }

                    }
                    process(docDes, n, elem);
                    if (stack.Count == 0){
                        parent.AddChildren(elem);
                    }
                    stack.Push(elem);
                    
                }
                else{

                    if (n is TextNode){

                        TextNode text = (TextNode)n;
                        if (!text.IsBlank){
                            string t = text.Text();
                            if (t != null && !t.Equals("")){
                                elem = new TextNode(t, "");                                
                            }                           
                        }
                    }else{
                        string nodename = n.NodeName;
                        if (nodename.Equals("div", StringComparison.OrdinalIgnoreCase)){
                            elem = docDes.CreateElement("Element");
                            elem.Attr("Type", "Section");
                            // parent.AddChildren(elem);
                            //parent = elem;
                        }else if (nodename.Equals("p", StringComparison.OrdinalIgnoreCase)){
                            if (callBackClasses.Contains(n.Attr("class"))) {
                              OnTreatSpecialElement(new EventArgs());
                            }
                            elem = docDes.CreateElement("p");
                            if (parent.NodeName.Equals("td", StringComparison.OrdinalIgnoreCase)){

                            }else if (parent.NodeName.Equals("Element", StringComparison.OrdinalIgnoreCase) && parent.ChildNodes.Count > 0 && parent.Attr("Type").Equals("Text"))
                            {

                            }else if (parent.NodeName.Equals("Element", StringComparison.OrdinalIgnoreCase) && parent.ChildNodes.Count > 0 && parent.ChildNodes.Last().Attr("Type").Equals("Text"))
                            {
                                //elem = docDes.CreateElement("p");
                                parent = parent.ChildNodes.Last();
                            }else{
                                Node elemP = docDes.CreateElement("Element");
                                elemP.Attr("Type", "Text");
                                addToParent(new Stack<Node>(), elem, docDes, elemP, n);
                                elem = elemP;
                                n = null;
                                //elem = docDes.CreateElement("p");
                            }


                            //parent = elem;
                        }else if (nodename.Equals("table", StringComparison.OrdinalIgnoreCase)){

                            Node tableElement = docDes.CreateElement("Element");
                            tableElement.Attr("Type", "Table");
                            elem = docDes.CreateElement("table");
                            //tableElement.AddChildren(elem);
                            addToParent(new Stack<Node>(), elem, docDes, tableElement, n);
                            elem = tableElement;
                            n = null;
                            //elem = tableElement;

                        }
                        else if (nodename.Equals("b", StringComparison.OrdinalIgnoreCase))
                        {
                            elem = docDes.CreateElement("b");

                        }
                        else if (nodename.Equals("a", StringComparison.OrdinalIgnoreCase))
                        {
                            if (!n.Attr("href").Equals("") && n.Attr("href").IndexOf("_Toc")<0)
                            {
                                elem = docDes.CreateElement("a");
                                elem.Attr("Href", n.Attr("href"));

                            } 
                            else
                            {
                                if (stack.Count > 0)
                                {
                                    elem = stack.Peek();
                                }
                                else
                                {
                                    elem = parent;
                                }
                            }
                            
                        }
                        else if (nodename.Equals("span", StringComparison.OrdinalIgnoreCase))
                        {
                            if (stack.Count > 0)
                            {
                                elem = stack.Peek();
                            }
                            else
                            {
                                elem = parent;
                            }
                        }
                        else if (nodename.Equals("img", StringComparison.OrdinalIgnoreCase)) {
                            elem = docDes.CreateElement("Element");
                            elem.Attr("Type", "Image");
                            TextNode src = null;
                            if (imageAsBase64) {
                              elem.Attr("Src", n.Attr("src"));
                              elem.Attr("Method", "Base64");
                              elem.Attr("Format", Path.GetExtension(n.Attr("src")).Substring(1));
                              elem.Attr("Name", Path.GetFileNameWithoutExtension(n.Attr("src")));
                              var extPath = Path.Combine(basePath, HttpUtility.UrlDecode(n.Attr("src")));
                              var lessPath = Path.GetFileName(n.Attr("src"));
                              var args = new ExternalDataEventArgs {
                                FilePath = extPath,
                                FileName = lessPath
                              };
                              OnTreatExternalData(args);
                              src = new TextNode(args.Data == null ? "" : Convert.ToBase64String(args.Data), "");
                            }
                            else {
                              elem.Attr("Method", "RefPath");
                              src = new TextNode(HttpUtility.UrlDecode(n.Attr("src")), "");
                            }                                          
                            elem.AddChildren(src);
                            //elem.Attr("SRC", n.Attr("src"));
                            if (n.Attr("caption") != null && !n.Attr("caption").Equals(""))
                            {
                                elem.Attr("Name", n.Attr("name"));
                            }
                            else if (n.Attr("name") != null && !n.Attr("name").Equals(""))
                            {
                                elem.Attr("Name", n.Attr("caption"));
                            }
                            

                        }
                        else
                        {
                            elem = docDes.CreateElement(nodename);
                        }
                      
                       // 

                    }
                    
                    addToParent(stack, elem, docDes, parent,n);
                   
                }

            }

        }

        private static void addToParent(Stack<Node> stack, Node elem, Document docDes, Node parent, Node n)
        {
            Node top = null;
            if (stack.Count > 0)
            {
                top = stack.Peek();
            }
            //Node[] childs = n.ChildNodesAsArray();
            //foreach (Node child in n.ChildNodes)
            //{
            if (elem != null && n!=null)
            {
                process(docDes, n, elem);
            }
            if (elem!=null && elem.Attr("Type").Equals("Table"))
            {
                Console.Write("");
            }
            if (elem != null && elem.Attr("Type").Equals("Text"))
            {
                Console.Write("");
            }
            //}
            if (elem != null && (elem.ChildNodes.Count > 0 || elem.Attr("Type").Equals("Image") || elem.Attr("Type").Equals("Table") || elem is TextNode))
            {
                if (top != null )
                {
                    if ( elem != top)
                    {
                        if (elem is TextNode)
                        {
                            TextNode text = (TextNode)elem;
                            top.Attr("NAME", text.Text());
                        }
                        else if (elem.NodeName.Equals("p", StringComparison.OrdinalIgnoreCase) || elem.Attr("Type").Equals("Text"))
                        {
                            ArrayList list = new ArrayList();
                            foreach (Node childP in elem.ChildNodes)
                            {
                                if (childP.Attr("Type").Equals("Image") || childP.Attr("Type").Equals("Table"))
                                {
                                    list.Add(childP);

                                }
                            }
                            foreach (Node childP in list)
                            {
                                elem.RemoveChild(childP);
                                top.AddChildren(childP);
                            }
                            if (top.ChildNodes.Count > 0)
                            {

                                Node[] childs = top.ChildNodesAsArray();
                                Node last = childs[childs.Length - 1];
                                if (elem.Attr("Type").Equals("Text") && last.Attr("Type").Equals("Text"))
                            {
                                if (!top.ChildNodes.Last().Equals(elem))
                                {
                                    top.ChildNodes.Last().AddChildren(elem.ChildNodesAsArray());
                                }
                                elem = null;
                               
                            }
                            }
                            

                        }

                        if (elem != null && (elem.ChildNodes.Count > 0 || elem.Attr("Type").Equals("Image") || elem is TextNode))
                        {
                            top.AddChildren(elem);
                        }
                       
                    }

                }
                else
                {
                    if (elem != parent)//|| elem.Attr("Type").Equals("Image")
                    {
                        if (elem is TextNode && parent.Attr("Type").Equals("Section"))
                        {
                            TextNode text = (TextNode)elem;
                            parent.Attr("Name",parent.Attr("Name")+ text.Text());
                        }
                        else if (elem.NodeName.Equals("p", StringComparison.OrdinalIgnoreCase) || elem.Attr("Type").Equals("Text"))
                        {
                            ArrayList list = new ArrayList();
                            foreach (Node childP in elem.ChildNodes)
                            {
                                if (childP.Attr("Type").Equals("Image") || childP.Attr("Type").Equals("Table"))
                                {
                                    list.Add(childP);
                                   
                                }
                            }
                            foreach (Node childP in list)
                            {
                                elem.RemoveChild(childP);
                                //parent.AddChildren(childP);
                                addToParent(childP, parent);
                                
                            }

                        }
                        if (elem.ChildNodes.Count > 0 || elem.Attr("Type").Equals("Image") || elem is TextNode)
                        {
                            //parent.AddChildren(elem);
                            addToParent(elem, parent);
                           
                        }
                    }
                }
            }
        }

        private static void addToParent(Node elem,Node parent)
        {
            if ((elem.Attr("Type").Equals("Image") || elem.Attr("Type").Equals("Table") || elem.Attr("Type").Equals("Text")) && (parent.Attr("Type").Equals("Image") || parent.Attr("Type").Equals("Table") || parent.Attr("Type").Equals("Text")) && parent.ParentNode!=null)
            {
                addToParent(elem,parent.ParentNode);
            }
            else
            {
                parent.AddChildren(elem);
            }
        }
       

        
    }
}
