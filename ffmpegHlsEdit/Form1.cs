using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace ffmpegHlsEdit {
    public partial class Form1 : Form {
        //ファイル情報
        public string data = "";

        public Form1() {
            InitializeComponent();
        }


        //ファイル参照
        private void Button1_Click(object sender, EventArgs e) {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = ConfigurationManager.AppSettings["OFDffmpegFilter"];
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == DialogResult.OK) {
                addListBoxName(ofd.FileNames);
            }
        }

        //ドラッグされたファイル　どれか1つでも非対応の拡張子だった場合許可しない
        //extCheck:true 許可   false:不許可
        // 当てはまった時点で次のファイル参照
        //1つでも当てはまらないものが来たら（falseのまま通り抜けたら）break

        //ドラッグされたとき
        private void Panel1_DragEnter(object sender, DragEventArgs e) {
            string[] extensions = ConfigurationManager.AppSettings["DragDropffmpegFilter"].Split(',');
            Boolean extCheck = false;
            foreach (string filename in (string[])e.Data.GetData(DataFormats.FileDrop)) {
                string ext = Path.GetExtension(filename);
                extCheck = false;

                for (int i = 0; i < extensions.Length; i++) {
                    if (ext == "." + extensions[i]) {
                        extCheck = true;
                        break;
                    }
                }
                if (extCheck) {
                    continue;
                } else {
                    break;
                };
            }

            if (extCheck) {
                e.Effect = DragDropEffects.Copy;
            } else {
                e.Effect = DragDropEffects.None;
            }
        }

        //ドロップされたとき
        private void Panel1_DragDrop(object sender, DragEventArgs e) {
            string[] fileName = (string[])e.Data.GetData(DataFormats.FileDrop, false);
            addListBoxName(fileName);
        }

        //ファイル情報取得
        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e) {
            string ffprobePath = @"C:\GitHub\fkmstc\HLSTools\ffprobe.exe";
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd.exe";

            //何故かffmpeg系は標準出力ではなく，
            //エラー出力なので"2>&1"を追加することで標準出力へ切り替える．
            psi.Arguments = $"/c {ffprobePath}  {(string)listBox1.SelectedItem} 2>&1";

            psi.CreateNoWindow = true;
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.StandardOutputEncoding = Encoding.UTF8;

            Process p = Process.Start(psi);
            p.OutputDataReceived += dataReceived;
            p.BeginOutputReadLine();	//非同期で標準出力
            p.WaitForExit();

            label1.Text = data;


        }

        //標準出力を受け取るイベントハンドラ
        private void dataReceived(object sender, DataReceivedEventArgs e) {
            data += e.Data + "\n";
        }

        //選択消去
        private void Button2_Click(object sender, EventArgs e) {
            if (listBox1.SelectedIndex != -1) {
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
            }
        }

        //全消去
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

        //ListBoxにデータを追加(重複削除)
        private void addListBoxName(string[] fileName) {
            for (int i = 0; i < fileName.Length; i++) {
                if (listBox1.Items.IndexOf(fileName[i]) == -1) {
                    listBox1.Items.Add(fileName[i]);
                }
            }
        }


    }
}
