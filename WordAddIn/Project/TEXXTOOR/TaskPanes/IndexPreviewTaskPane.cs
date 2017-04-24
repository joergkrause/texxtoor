using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using TEXXTOOR.Services;
using Microsoft.Office.Interop.Word;

namespace TEXXTOOR.TaskPanes
{
    public partial class IndexPreviewTaskPane : UserControl
    {

        public IndexPreviewTaskPane()
        {
            InitializeComponent();
        }

        public void PopulateIndex()
        {
            var document = ServicePool.Instance.GetService<DocumentService>().CurrentDocument;
            var indizes = document.Fields.OfType<Field>().Where(f => f.Type == WdFieldType.wdFieldIndexEntry).ToList();
            listBoxEntries.DisplayMember = "Text";
            listBoxEntries.ValueMember = "Value";
            listBoxEntries.Items.Clear();

            indizes.ForEach(x => listBoxEntries.Items.Add(new ListBoxEntry
            {
                // Could not find the origial text value of that field entry
                Text = x.Code.Text.Substring(x.Code.Text.IndexOf("\"") + 1, x.Code.Text.LastIndexOf("\"") - (x.Code.Text.IndexOf("\"")+1)),
                Value = x
            }));
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            var entry = listBoxEntries.SelectedItem as ListBoxEntry;
            if (entry != null)
            {
                entry.Value.Delete();
                listBoxEntries.Items.RemoveAt(listBoxEntries.SelectedIndex);
                IndexTextBox.Text = string.Empty;
            }
        }

        private void buttonChange_Click(object sender, EventArgs e)
        {
            var entry = listBoxEntries.SelectedItem as ListBoxEntry;
            if (entry != null)
            {
                entry.Value.Code.Text = entry.Value.Code.Text.Replace(entry.Text, IndexTextBox.Text);
                entry.Text = IndexTextBox.Text;
                var pos = listBoxEntries.Items.IndexOf(entry);
                listBoxEntries.Items.RemoveAt(pos);
                listBoxEntries.Items.Insert(pos, entry);
                IndexTextBox.Text = string.Empty;
            }
        }

        #region internal class

        internal class ListBoxEntry
        {
            public string Text { get; set; }
            public Field Value { get; set; }
        }

        #endregion

        private void listBoxEntries_SelectedIndexChanged(object sender, EventArgs e)
        {
            var entry = listBoxEntries.SelectedItem as ListBoxEntry;
            if (entry != null)
            {
                IndexTextBox.Text = entry.Text;
            }
        }

	}
}
