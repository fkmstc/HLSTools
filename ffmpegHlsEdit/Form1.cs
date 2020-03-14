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
        // カレントディレクトリの取得
        public string currentDir = Directory.GetCurrentDirectory();

        public Form1() {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 0;
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
            if (listBox1.SelectedIndex != -1) {

                //tsファイル名の設定
                string outputFileName = Path.GetFileName((string)listBox1.SelectedItem);
                textBox7.Text = outputFileName.Substring(
                    0,
                    outputFileName.IndexOf("."))
                    + "%3d.ts"
                    ;

                //ffprobeを実行
                processStart(@"\ffprobe.exe", $"{(string)listBox1.SelectedItem} 2>&1 -hide_banner");

            }
        }



        //選択消去
        private void Button2_Click(object sender, EventArgs e) {
            if (listBox1.SelectedIndex != -1) {
                listBox1.Items.RemoveAt(listBox1.SelectedIndex);
                textBox9.Text = "";
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
                    textBox9.Text = "";
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


        //変換ボタン
        private void button4_Click(object sender, EventArgs e) {

            button4.Text = "変換中！！";

            string output = "";
            string g3output = "";

            //groupBoxからそれぞれのオブジェクトを取得，キャストし出力コマンド(output)を作成
            foreach (object obj in groupBox2.Controls) {
                CheckBox cb = obj as CheckBox;
                if (cb != null && cb.Checked) {
                    output += cb.Text + " ";

                    switch (cb.Text) {
                        case "-hls_time":
                            foreach (object g3obj in groupBox3.Controls) {
                                CheckBox g3cb = g3obj as CheckBox;
                                if (g3cb != null && g3cb.Checked) {
                                    g3output += g3cb.Text + " ";
                                    continue;
                                }
                                TextBox g3tb = g3obj as TextBox;
                                if (g3tb != null) {
                                    g3output += g3tb.Text + " ";
                                    continue;
                                }
                            }
                            break;
                    }
                    continue;
                }
                TextBox tb = obj as TextBox;
                if (tb != null && tb.Enabled) {
                    if (tb.Name == "textBox7") {
                        output += "-hls_segment_filename ";
                    }
                    output += tb.Text + " ";
                    switch (tb.Name) {
                        case "textBox2":
                            if (checkBox2.Checked) {
                                output += g3output;
                            }
                            break;
                    }

                    continue;
                }
                ComboBox comb = obj as ComboBox;
                if (comb != null && comb.Enabled) {
                    output += comb.Text + " ";
                    //aacを使う場合　追加でコマンドが必要
                    if (comboBox2.Text == "aac" && comboBox2.Enabled) {
                        output += "-strict -2 ";
                    }
                    continue;

                }
            }
            //出力コマンド
            Console.WriteLine(output);
            processStart(@"\ffmpeg.exe", $"{(string)listBox1.SelectedItem} 2>&1 " + output);
        }


        //-hls_time　詳細
        private void checkBox2_CheckedChanged(object sender, EventArgs e) {
            //有効化
            if (checkBox2.Checked) {
                textBox2.Enabled = true;
                foreach (object obj in groupBox3.Controls) {
                    CheckBox cb = obj as CheckBox;
                    if (cb != null) {
                        cb.Enabled = true;
                        continue;
                    }

                    TextBox tb = obj as TextBox;
                    if (tb != null) {
                        tb.Enabled = true;
                        continue;
                    }
                }
                //無効化
            } else {
                foreach (object obj in groupBox3.Controls) {
                    textBox2.Enabled = false;
                    CheckBox cb = obj as CheckBox;
                    if (cb != null) {
                        cb.Enabled = false;
                        continue;
                    }

                    TextBox tb = obj as TextBox;
                    if (tb != null) {
                        tb.Enabled = false;
                        continue;
                    }
                }
            }
        }

        //-start_number
        private void checkBox1_CheckedChanged(object sender, EventArgs e) {
            if (checkBox1.Checked) {
                textBox1.Enabled = true;
            } else {
                textBox1.Enabled = false;
            }
        }

        //-hls_list_size
        private void checkBox3_CheckedChanged(object sender, EventArgs e) {
            if (checkBox3.Checked) {
                textBox6.Enabled = true;
            } else {
                textBox6.Enabled = false;
            }
        }

        //-c:v
        private void checkBox4_CheckedChanged(object sender, EventArgs e) {
            if (checkBox4.Checked) {
                comboBox1.Enabled = true;
            } else {
                comboBox1.Enabled = false;
            }
        }

        //-c:a
        private void checkBox8_CheckedChanged(object sender, EventArgs e) {
            if (checkBox8.Checked) {
                comboBox2.Enabled = true;
            } else {
                comboBox2.Enabled = false;
            }
        }

        private void processStart(string exeDir, string argument) {
            data = "";
            string ffPath = currentDir + exeDir;
            Process p = new Process();
            p.StartInfo.FileName = "cmd.exe";

            //何故かffmpeg系は標準出力ではなく，
            //エラー出力なので"2>&1"を追加することで標準出力へ切り替える．
            p.StartInfo.Arguments = $"/c {ffPath} -i " + argument;

            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            p.OutputDataReceived += dataReceived;

            p.Start();
            p.BeginOutputReadLine();    //非同期で標準出力
            p.WaitForExit();
            p.Close();
            button4.Text = "変換";
        }

        //標準出力を受け取るイベントハンドラ
        private void dataReceived(object sender, DataReceivedEventArgs e) {
            //data += e.Data + "\r\n";
            Console.WriteLine(e.Data);
            Action act = () => {
                if (string.IsNullOrEmpty(e.Data) == false) {
                    textBox9.AppendText(e.Data);
                }
                textBox9.AppendText(Environment.NewLine);
            };

            BeginInvoke(act);
        }


    }
}