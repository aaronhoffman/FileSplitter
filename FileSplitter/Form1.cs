using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;

namespace FileSplitter
{
    public partial class Form1 : Form
    {
        void updateButtons()
        {
            if (File.Exists(textBox1.Text) && Directory.Exists(textBox2.Text))
            {
                button1.Enabled = true;
            }
            else
            {
                button1.Enabled = false;
            }
        }

        public Form1()
        {
            InitializeComponent();
            updateButtons();
        }

        

        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
            {
                textBox1.Text = openFileDialog1.FileName;
                textBox2.Text = Path.GetDirectoryName(openFileDialog1.FileName);
                button1.Select();
            }
        }

        

        private void button3_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = textBox2.Text;
            if (folderBrowserDialog1.ShowDialog(this) == DialogResult.OK)
            {
                textBox2.Text = folderBrowserDialog1.SelectedPath;
                button1.Select();
            }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                SplittingForm f = new SplittingForm();

                int kbs = (int)numericUpDown1.Value * 1024;
                int chunkSize = (int)(numericUpDown1.Value) * 1024 * 1024;

                byte[] buffer = new byte[1024];



                f.progressBar1.Minimum = 0;


                f.progressBar2.Minimum = 0;
                f.progressBar2.Maximum = kbs;

                f.textBox1.Text = "Preparing";


                f.Show(this);

                Application.DoEvents();

                string cmdout = "copy/b ";

                f.textBox1.Text = "Opening input file";
                FileStream infile = File.OpenRead(textBox1.Text);

                f.progressBar1.Maximum = (int)(infile.Length / 1024);

                f.textBox1.Text = "Initializing";

                DateTime lasttime = DateTime.Now;
                for (long i = 0; i <= infile.Length / chunkSize; i++)
                {
                    f.progressBar2.Value = 0;
                    f.textBox1.Text = "Chunk i of " + (infile.Length / chunkSize) + " preparing";

                    Application.DoEvents();
                    string fname = Path.Combine(textBox2.Text, Path.Combine(textBox2.Text, Path.GetFileName(textBox1.Text)) + "." + chunkSize + "." + i.ToString().PadLeft(4, '0') + ".part");
                    string fname_x = Path.GetFileName(textBox1.Text) + "." + chunkSize + "." + i.ToString().PadLeft(4, '0') + ".part";
                    if (i == infile.Length / chunkSize)
                        cmdout += "\"" + fname_x + "\"";
                    else
                        cmdout += "\"" + fname_x + "\" + ";
                    FileStream outfile = File.Create(fname);


                    for (int kb = 0; kb <= kbs; kb++)
                    {
                        DateTime now = DateTime.Now;
                        if ((now - lasttime).Milliseconds > 40)
                        {
                            f.textBox1.Text = "Chunk " + i + " of " + (infile.Length / chunkSize) + ": " + kb + " / " + kbs;
                            f.progressBar2.Value = kb;
                            f.progressBar1.Value = Math.Min(f.progressBar1.Maximum, (int)(kb + i * kbs));

                            lasttime = now;
                        }

                        Application.DoEvents();

                        int len = infile.Read(buffer, 0, 1024);
                        outfile.Write(buffer, 0, len);
                    }
                    outfile.Close();
                }


                cmdout += " \"" + Path.GetFileName(textBox1.Text) + "\"";

                string combinerbatch = Path.Combine(textBox2.Text, Path.Combine(textBox2.Text, Path.GetFileName(textBox1.Text)) + "." + chunkSize + ".combine.bat");
                File.WriteAllText(combinerbatch, cmdout);

                f.Close();

                /*if (MessageBox.Show(this, "File successfully splitted.\nA combiner script has been created at \"" + combinerbatch + "\". Run it at the target machine you want to merge the parts at again.\n\nOpen output folder now ???", "Success !!!", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                {
                    Process.Start("explorer", "/select,\"" + combinerbatch + "\"");
                }*/

                SuccessForm sf = new SuccessForm();
                sf.textBox1.Text = combinerbatch;
                sf.ShowDialog(this);
                if (sf.checkBox1.Checked)
                {
                    Process.Start("explorer", "/select,\"" + combinerbatch + "\"");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            updateButtons();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            updateButtons();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://weltweitimnetz.de");
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
