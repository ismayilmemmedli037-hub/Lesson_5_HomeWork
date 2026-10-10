using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Lesson_5_HomeWork
{
    public partial class Form1 : Form
    {
        TextBox txtFile = new TextBox();
        TextBox txtPassword = new TextBox();

        Button btnFile = new Button();
        Button btnStart = new Button();
        Button btnCancel = new Button();

        RadioButton rbEncrypt = new RadioButton();
        RadioButton rbDecrypt = new RadioButton();

        private ProgressBar progressBar1 = new ProgressBar();
        Label lblStatus = new Label();

        public Form1()
        {
            InitializeComponent();

            this.Text = "XOR File Encryption";
            this.Size = new Size(600, 300);
            this.StartPosition = FormStartPosition.CenterScreen;

            Label fileLabel = new Label();
            fileLabel.Text = "File:";
            fileLabel.Location = new Point(20, 25);
            fileLabel.AutoSize = true;
            Controls.Add(fileLabel);

            txtFile.Location = new Point(100, 20);
            txtFile.Size = new Size(340, 25);
            Controls.Add(txtFile);

            btnStart.Click += btnStart_Click;
            btnCancel.Click += btnCancel_Click;

            btnFile.Text = "File";
            btnFile.Location = new Point(450, 18);
            btnFile.Size = new Size(90, 30);
            btnFile.Click += BtnFile_Click;
            Controls.Add(btnFile);

            Label passwordLabel = new Label();
            passwordLabel.Text = "Password:";
            passwordLabel.Location = new Point(20, 70);
            passwordLabel.AutoSize = true;
            Controls.Add(passwordLabel);

            txtPassword.Location = new Point(100, 65);
            txtPassword.Size = new Size(340, 25);
            txtPassword.UseSystemPasswordChar = true;
            Controls.Add(txtPassword);

            rbEncrypt.Text = "Encrypt";
            rbEncrypt.Location = new Point(100, 110);
            rbEncrypt.AutoSize = true;
            rbEncrypt.Checked = true;
            Controls.Add(rbEncrypt);

            rbDecrypt.Text = "Decrypt";
            rbDecrypt.Location = new Point(220, 110);
            rbDecrypt.AutoSize = true;
            Controls.Add(rbDecrypt);

            progressBar1.Location = new Point(20, 155);
            progressBar1.Size = new Size(520, 25);
            progressBar1.Minimum = 0;
            progressBar1.Maximum = 100;
            Controls.Add(progressBar1);

            btnStart.Text = "Start";
            btnStart.Location = new Point(330, 200);
            btnStart.Size = new Size(90, 30);
            Controls.Add(btnStart);

            btnCancel.Text = "Cancel";
            btnCancel.Location = new Point(440, 200);
            btnCancel.Size = new Size(100, 30);
            Controls.Add(btnCancel);

            lblStatus.Text = "Ready";
            lblStatus.Location = new Point(20, 205);
            lblStatus.AutoSize = true;
            Controls.Add(lblStatus);
        }

        private void BtnFile_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    txtFile.Text = dialog.FileName;
                }
            }
        }

        // __________________________________________________

        private CancellationTokenSource? cts;

        private async void btnStart_Click(object? sender, EventArgs e)
        {
            string path = txtFile.Text;
            string password = txtPassword.Text;

            if (!File.Exists(path) || password == "")
            {
                MessageBox.Show("Fayl secin ve parol daxil edin!");
                return;
            }

            byte[] key = Encoding.UTF8.GetBytes(password);
            string temp = path + ".tmp";

            cts = new CancellationTokenSource();
            btnStart.Enabled = false;
            btnCancel.Enabled = true;

            try
            {
                await Task.Run(() =>
                {
                    using FileStream input = new FileStream(path, FileMode.Open, FileAccess.Read);
                    using FileStream output = new FileStream(temp, FileMode.Create, FileAccess.Write);

                    byte[] buffer = new byte[8192];
                    long processed = 0;
                    long total = input.Length;
                    int count;

                    while ((count = input.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        cts.Token.ThrowIfCancellationRequested();

                        for (int i = 0; i < count; i++)
                        {
                            buffer[i] ^= key[(int)((processed + i) % key.Length)];
                        }
                        output.Write(buffer, 0, count);
                        processed += count;

                        int percent = total == 0 ? 100 : (int)(processed * 100 / total);

                        Invoke(() => progressBar1.Value = percent);
                    }
                }, cts.Token);

                File.Copy(temp, path, true);
                progressBar1.Value = 100;
                MessageBox.Show("Emeliyyat tamamlandi!");
            }
            catch (OperationCanceledException)
            {
                progressBar1.Value = 0;
                MessageBox.Show("Emeliyyat legv edildi!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (File.Exists(temp))
                {
                    File.Delete(temp);
                }
                btnStart.Enabled = true;
                btnCancel.Enabled = false;
                cts.Dispose();
                cts = null;
            }
        }

        private void btnCancel_Click(object? sender, EventArgs e)
        {
            cts?.Cancel();
        }
    }
}
