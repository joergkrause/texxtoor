using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Texxtoor.BaseLibrary.Core.HtmlAgility.ToXml.Nodes;

namespace Texxtoor.BaseLibrary.Core.HtmlAgility.ToXml.Select
{
    /// <summary>
    /// Collects a list of elements that match the supplied criteria.
    /// </summary>
    /// <!--
    /// Original Author: Jonathan Hedley
    /// Ported to .NET by: Amir Grozki
    /// -->
    internal class Collector
    {
        private Collector() { }

        /// <summary>
        /// Build a list of elements, by visiting root and every descendant of root, and testing it against the evaluator.
        /// </summary>
        /// <param name="eval">Evaluator to test elements against</param>
        /// <param name="root">root of tree to descend</param>
        /// <returns>list of matches; empty if none</returns>
        public static Elements Collect(Evaluator eval, Element root)
        {
            Elements elements = new Elements();
            new NodeTraversor(new Accumulator(root, elements, eval)).Traverse(root);
            return elements;
        }

        private class Accumulator : NodeVisitor
        {
            private readonly Element root;
            private readonly Elements elements;
            private readonly Evaluator eval;

            public Accumulator(Element root, Elements elements, Evaluator eval)
            {
                this.root = root;
                this.elements = elements;
                this.eval = eval;
            }

            public void Head(Node node, int depth)
            {
                if (node is Element)
                {
                    Element el = (Element)node;
                    if (eval.Matches(root, el))
                    {
                        elements.Add(el);
                    }
                }
            }

            public void Tail(Node node, int depth)
            {
                // void
            }
        }
    }
}
