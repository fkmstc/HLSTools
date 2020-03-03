using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ffmpegHlsEdit {
    public partial class Form1 : Form {


        public string[] parameter = new string[10];

        public Form1() {
            InitializeComponent();
        }

        private void Button1_Click(object sender, EventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK) {
                textBox1.Text = ofd.FileName;
                parameter[0] = textBox1.Text;
                Console.WriteLine(parameter[0]);
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
            textBox1.Text = fileName[0];
            parameter[0] = textBox1.Text;
            Console.WriteLine(parameter[0]);

        }
    }
}
