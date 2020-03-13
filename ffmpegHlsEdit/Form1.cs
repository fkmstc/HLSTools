﻿using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace ffmpegHlsEdit {
    public partial class Form1 : Form {

        public Form1() {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == DialogResult.OK) {
                addListBoxName(ofd.FileNames);
            }
        }
        private void Panel1_DragEnter(object sender, DragEventArgs e) {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) {
                e.Effect = DragDropEffects.Copy;
            } else {
                e.Effect = DragDropEffects.None;
            }
        }
        private void Panel1_DragDrop(object sender, DragEventArgs e) {
            string[] fileName = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            addListBoxName(fileName);
        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e) {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "ffprobe.exe";
            psi.Arguments = (string)listBox1.SelectedItem;
            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;

            psi.RedirectStandardOutput = true;
            Process p = Process.Start(psi);
            p.OutputDataReceived += p_OutputDataReceived;
            p.BeginOutputReadLine();
            p.WaitForExit();
            p.Close();
        }
        private void p_OutputDataReceived(object sender, DataReceivedEventArgs e) {
            //出力された文字列を表示する
            Console.WriteLine("ok:" + e.Data);
            //label1.Text = e.Data;
            //label1.Text += "aa";
        }

        private void Button2_Click(object sender, EventArgs e) {
            if (listBox1.SelectedIndex != -1)
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
        }

        private void Button3_Click(object sender, EventArgs e) {
            if (listBox1.Items.Count != 0) {
                DialogResult result = MessageBox.Show("全て消去しますか?",
                "全て消去",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Warning,
                MessageBoxDefaultButton.Button2);

                if (result == DialogResult.OK) {
                    listBox1.Items.Clear();
                }
            }
        }

        private void addListBoxName(string[] fileName) {
            for (int i = 0; i < fileName.Length; i++) {
                if (listBox1.Items.IndexOf(fileName[i]) == -1) {
                    listBox1.Items.Add(fileName[i]);
                }
            }
        }


    }
}
