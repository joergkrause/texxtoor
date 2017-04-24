using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using HTML2XML.Nodes;

namespace HTML2XML
{
    public partial class Form1 : Form
    {
        private string fileSrc;
        private  string fileDesc;
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                fileSrc = openFileDialog1.FileName;
                folderBrowserDialog1.SelectedPath = Path.GetDirectoryName(fileSrc);              
                txtHtml.Text = fileSrc;
               
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK) // Test result.
            {
                fileDesc=folderBrowserDialog1.SelectedPath;
                txtXML.Text = fileDesc;
            }
        }

        private void btnConvert_Click(object sender, EventArgs e)
        {
            if (fileSrc == null || fileSrc.Equals(""))
            {
                MessageBox.Show("Please Select a HTML file");

            }
            else{
                if (fileDesc == null || fileDesc.Equals(""))
                {
                    MessageBox.Show("Please Select Destination Folder");
                }
                else
                {
                    try{
                       

                            //string html = File.ReadAllText(fileSrc, Encoding.UTF8);

                           
                            string line;
                            StringBuilder builder = new StringBuilder();
                            byte[] ansiBytes = File.ReadAllBytes(fileSrc);
                            string html = Encoding.UTF8.GetString(ansiBytes);
                            /*using (StreamReader reader =  new StreamReader(fileSrc,true))
                            {
                                while ((line = reader.ReadLine()) != null)
                                {

                                    builder.Append(Encoding.Unicode.GetString(reader.CurrentEncoding.GetBytes(line)));
                                    builder.Append("\n");
                                }
                            }*/
                            HTML2XMLUtil.basePath = Path.GetDirectoryName(fileSrc);
                            HTML2XMLUtil.imageAsBase64 = chkInlineBase64.Checked;
                            HTML2XMLUtil.TreatSpecialElement += (o, args) => {
                              // handle custom elements
                            };
                            HTML2XMLUtil.TreatExternalData += (o, args) => {
                              FileStream fs = null;
                              try {
                                fs = File.OpenRead(args.FilePath);
                                var b = new byte[fs.Length];
                                fs.Read(b, 0, b.Length);
                                args.Data = b;
                              }
                              catch (Exception exx) {
                                var fallbackPath = Path.Combine(Path.GetDirectoryName(fileSrc), Path.GetFileNameWithoutExtension(fileSrc) + "-Dateien", args.FileName);
                                try {
                                  fs = File.OpenRead(fallbackPath);
                                  var b = new byte[fs.Length];
                                  fs.Read(b, 0, b.Length);
                                  args.Data = b;
                                }
                                catch (Exception exxx) {
                                  MessageBox.Show(exx.Message + "\n\n" + exxx.Message);  
                                }                                
                              }
                              finally {
                                if (fs != null) {
                                  fs.Dispose();
                                }
                              }
                            };
                            HTML2XMLUtil.callBackClasses.AddRange(new[] { "ListingText", "ListingUnterschrift" });
                            string xml = HTML2XMLUtil.parseHTML(html);//builder.ToString()
                            //xml = WebUtility.HtmlDecode(xml);
                            File.WriteAllText(fileDesc + "\\" + Path.GetFileNameWithoutExtension(fileSrc) + ".xml", "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n" + xml, Encoding.UTF8);
                            MessageBox.Show("Done");
                       

                    }catch(Exception ex){
                        MessageBox.Show(ex.Message);
                    }
                    
                }
            }
             
        }
    }
}
