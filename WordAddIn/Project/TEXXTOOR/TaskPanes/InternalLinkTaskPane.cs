using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TEXXTOOR.Properties;
using TEXXTOOR.TexxtoorAddInService;
using TEXXTOOR.Services;
using Microsoft.Office.Interop.Word;
using System.Text.RegularExpressions;
using System.Drawing;

namespace TEXXTOOR {
	public partial class InternalLinkTaskPane : UserControl {
		
		private ServerService _service;
		private DocumentService _ds;
		private readonly string CaptionPattern = Resources.InternalLinkTaskPane_CaptionPattern_;

		public InternalLinkTaskPane() {
			InitializeComponent();
			_ds = ServicePool.Instance.GetService<DocumentService>();
		}

		public void BindChildren() {
			try {
				// TODO: Create Hierarchy of the document with all but text
			} catch (Exception exe) {
			}
		}
		private void treeViewChapter_AfterSelect(object sender, TreeViewEventArgs e) {
			var strNodeText = e.Node.Text;
			var para = e.Node.Tag as Paragraph;
			if (para != null) {
				txtCaption.Text = strNodeText;
			}
		}

		private void btnSave_Click(object sender, EventArgs e) {
			var range = _ds.CurrentSelection.Range;
			var para = treeViewChapter.SelectedNode.Tag as Paragraph;
			if (para == null) {
				MessageBox.Show(Resources.InternalLinkTaskPane_btnSave_Click_The_selected_paragraph_is_not_valid, Resources.InternalLinkTaskPane_btnSave_Click_Error_Adding_Link, MessageBoxButtons.OK);
				return;
			}
			object id = AssureBookmark(para.Range);
			object o = Type.Missing;
			string text = txtCaption.Text;
			// for captions we need to extract the invariable part
			string n1 = para.get_Style().NameLocal.ToString();			
			switch (n1) {
				case "TableCaption":
				case "FigureCaption":
				case "ListingCaption":
					var match = Regex.Match(text, CaptionPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
					text = match.Success ? match.Groups["text"].Value : text;
					break;
				default:
					//
					break;
			}
			_ds.CurrentSelection.Hyperlinks.Add(range, text, ref id, ref o, ref o, ref o);
		}

		private object AssureBookmark(Range range) {
			Bookmark b = null;
			if (range.BookmarkID == 0) {
				b = range.Bookmarks.Add("bm" + range.GetHashCode());
				return b.Range.BookmarkID;
			} else {
				return range.BookmarkID;
			}
		}

		internal void PopulateLinks() {
			var paras = _ds.CurrentDocument.Paragraphs.Cast<Paragraph>();
			var tree = paras
				.Where(p => _ds.CheckLocalizedStyleName(p.get_Style(), WdBuiltinStyle.wdStyleHeading1))
				.Select(p => new TreeNode {
					Text = p.Range.Text == null ? "Err" : p.Range.Text.Trim(),
					ImageIndex = 4,
					Tag = p
				}).ToArray();
			tree.ToList().ForEach(n => {
				n.Nodes.AddRange(FindStylesBelow(WdBuiltinStyle.wdStyleHeading1, WdBuiltinStyle.wdStyleHeading2, n.Tag as Paragraph));
			});
			treeViewChapter.Nodes.Clear();
			treeViewChapter.Nodes.AddRange(tree);
			treeViewChapter.ExpandAll();
			GatherHyperLinks();
		}

		private void GatherHyperLinks() {
			var hl = _ds.CurrentDocument.Hyperlinks.Cast<Hyperlink>();
			lbHyperLinks.Items.Clear();
			foreach (var hyperlink in hl) {
				lbHyperLinks.Items.Add(hyperlink.TextToDisplay, false);
			}
		}

		private TreeNode[] FindStylesBelow(WdBuiltinStyle wdCurrent, WdBuiltinStyle wdSearch, Paragraph currentPara) {
			var children = new List<TreeNode>();			
			var para = currentPara.Next(1);
			do {
				if (_ds.CheckLocalizedStyleName(para.get_Style(), wdCurrent)) break;
				TreeNode[] grandChildren = null;
				// Add sections as subnodes
				if (_ds.CheckLocalizedStyleName(para.get_Style(), wdSearch)) {
					var t = new TreeNode {
						Text = para.Range.Text.Trim(),
						ImageIndex = 4,
						Tag = para
					};
					grandChildren = FindStylesBelow(wdSearch, GetNextHeadingStyle(wdSearch), para);
					t.Nodes.AddRange(grandChildren);
					children.Add(t);
				}
				var hasHeading = grandChildren != null && grandChildren.Any(t => 
					_ds.CheckLocalizedStyleName(((Paragraph) t.Tag).get_Style(), WdBuiltinStyle.wdStyleHeading1)
					|| _ds.CheckLocalizedStyleName(((Paragraph)t.Tag).get_Style(), WdBuiltinStyle.wdStyleHeading2)
					|| _ds.CheckLocalizedStyleName(((Paragraph)t.Tag).get_Style(), WdBuiltinStyle.wdStyleHeading3)
					|| _ds.CheckLocalizedStyleName(((Paragraph)t.Tag).get_Style(), WdBuiltinStyle.wdStyleHeading4)
					|| _ds.CheckLocalizedStyleName(((Paragraph) t.Tag).get_Style(), WdBuiltinStyle.wdStyleHeading5));
				if (!hasHeading) {
					// Leaf Level
					// add regular elements as nodes
					string n1 = para.get_Style().NameLocal.ToString();
					TreeNode snippetNode = null;
					switch (n1) {
						case "FigureCaption":
							// images only if NOT in table
							if (para.Range.Tables.Count == 0) {
								snippetNode = new TreeNode {
									Text = para.Range.Text.Trim(), // caption after
									ImageIndex = 2,
									Tag = para,
									ForeColor = Color.Blue
								};
							}
							break;
						case "ListingCaption":
							snippetNode = new TreeNode {
								Text = para.Range.Text.Trim(), // caption before
								ImageIndex = 1,
								Tag = para,
								ForeColor = Color.Blue
							};
							break;
						case "TableCaption":
							snippetNode = new TreeNode {
								Text = para.Range.Text.Trim(), // caption before
								ImageIndex = 3,
								Tag = para,
								ForeColor = Color.Blue
							};
							break;
						default:
							if (_ds.CheckLocalizedStyleName(para.get_Style(), WdBuiltinStyle.wdStyleNormal)) {
								// text only if NOT in table
								if (para.Range.Tables.Count == 0) {
									if (para.Range.Text.Trim().Length > 1) {
										snippetNode = new TreeNode {
											Text = para.Range.Text.Trim().Substring(0, Math.Min(para.Range.Text.Trim().Length - 1, 20)), // caption
											ImageIndex = 0,
											Tag = para,
											ForeColor = Color.Black
										};
										snippetNode.NodeFont = new System.Drawing.Font(treeViewChapter.Font.FontFamily, treeViewChapter.Font.Size * 0.9f);
									}
								}
							}
							break;

					}
					if (snippetNode != null) {
						children.Add(snippetNode);
					}
				}
				// proceed
				para = para.Next(1);
			} while (para != null);
			return children.ToArray();
		}

		private WdBuiltinStyle GetNextHeadingStyle(WdBuiltinStyle currentStyle) {
			if (currentStyle == WdBuiltinStyle.wdStyleHeading1) return WdBuiltinStyle.wdStyleHeading2;
			if (currentStyle == WdBuiltinStyle.wdStyleHeading2) return WdBuiltinStyle.wdStyleHeading3;
			if (currentStyle == WdBuiltinStyle.wdStyleHeading3) return WdBuiltinStyle.wdStyleHeading4;
			if (currentStyle == WdBuiltinStyle.wdStyleHeading4) return WdBuiltinStyle.wdStyleHeading5;
			if (currentStyle == WdBuiltinStyle.wdStyleHeading5) return WdBuiltinStyle.wdStyleHeading6;
			return WdBuiltinStyle.wdStyleHeading7;
		}

		private void txtSearch_TextChanged(object sender, EventArgs e) {
			var searchFor = txtSearch.Text;
			if (String.IsNullOrEmpty(searchFor)) {
				PopulateLinks();
				return;
			}
			// make a flat list for search
			var allNodes = new List<TreeNode>();
			Action<TreeNode[]> getNodes = null;
			getNodes = t => {
				if (t.Any()) {
					allNodes.AddRange(t);
					t.ToList().ForEach(c => getNodes(c.Nodes.Cast<TreeNode>().ToArray()));
				}
			};
			getNodes(treeViewChapter.Nodes.Cast<TreeNode>().ToArray());
			// clear and add filtered list
			treeViewChapter.Nodes.Clear();
			treeViewChapter.Nodes.AddRange(allNodes.Where(node => node.Text.Contains(searchFor)).ToArray());
		}

		private void btnClear_Click(object sender, EventArgs e) {
			if (String.IsNullOrEmpty(txtSearch.Text)) return;
			txtSearch.Text = String.Empty;
			PopulateLinks();
		}

		private void lbHyperLinks_ItemCheck(object sender, ItemCheckEventArgs e) {
			btnToText.Enabled = lbHyperLinks.CheckedItems.Count > 0;
		}

		private void btnToText_Click(object sender, EventArgs e) {
			// TODO: Convert To Text
		}

		private void InternalLinkTaskPane_Load(object sender, EventArgs e) {

		}

	}
}
