using System.Diagnostics;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Audio_And_Video_Transcript_And_Summary
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        string tscriptPath;




        private string selectedFilePath = ""; // global olarak dosya yolunu tutmak için
        private void btnSelect_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select a video or voice file";
                openFileDialog.Filter = "Video or Audio Files|*.mp4;*.mp3;*.wav;*.m4a;*.avi;*.mov;*.mkv";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    selectedFilePath = openFileDialog.FileName;
                    MessageBox.Show("Selected File: " + selectedFilePath, "File Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    lblSelectedFile.Text = "Selected File : " + Path.GetFileName(selectedFilePath);
                }
            }
        }

        private async void btnConvert_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(selectedFilePath))
            {
                MessageBox.Show("Please select a file first.");
                return;
            }
            pgbConvertTime.Style = ProgressBarStyle.Marquee;
            pgbConvertTime.Visible = true;


            string pythonScriptPath = GetPythonScriptBasedOnSize(selectedFilePath);

            var output = await Task.Run(() =>
            {
                try
                {
                    Process process = new Process();
                    process.StartInfo.FileName = "python";
                    process.StartInfo.Arguments = $"\"{pythonScriptPath}\" \"{selectedFilePath}\"";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true; // hata kontrolü için
                    process.StartInfo.CreateNoWindow = true;

                    process.Start();

                    string stdOutput = process.StandardOutput.ReadToEnd();
                    string stdError = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    return string.IsNullOrEmpty(stdOutput) ? stdError : stdOutput;
                }
                catch (Exception ex)
                {
                    return "An error occurred during processing: " + ex.Message;
                }
            });
            pgbConvertTime.Visible = false;
            txtConvertedText.Text = output;
        }


        private string GetPythonScriptBasedOnSize(string filePath)
        {
            long fileSizeBytes = new FileInfo(filePath).Length;
            long limitSize = 25 * 1024 * 1024; // 25 MB

            if (fileSizeBytes > limitSize)
            {
                tscriptPath = Path.Combine(basePath, "transcribe_split.py");
            }
            else
            {
                tscriptPath = Path.Combine(basePath, "transcribe.py");
            }
            return tscriptPath;
        }








        private async void btnSummarize_Click(object sender, EventArgs e)
        {
            // Transkripsiyon metni boþsa iþlem yapma
            string transcriptText = txtConvertedText.Text;
            if (string.IsNullOrWhiteSpace(transcriptText))
            {
                MessageBox.Show("First, transcribe the audio.");
                return;
            }
            // Geçici bir .txt dosyasýna transkript metnini yaz
            string tempTxtPath = Path.Combine(Path.GetTempPath(), "transcript.txt");
            File.WriteAllText(tempTxtPath, transcriptText);
            // Python summarize.py dosyasýnýn tam yolu
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string summarizeScriptPath = Path.Combine(basePath, "summarize.py");

            pgvSummarizeTime.Style = ProgressBarStyle.Marquee;
            pgvSummarizeTime.Visible = true;
            var output = await Task.Run(() =>
            {
                try
                {
                    Process process = new Process();
                    process.StartInfo.FileName = "python";
                    process.StartInfo.Arguments = $"\"{summarizeScriptPath}\" \"{tempTxtPath}\"";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.CreateNoWindow = true;

                    process.Start();

                    string stdOutput = process.StandardOutput.ReadToEnd();
                    string stdError = process.StandardError.ReadToEnd();

                    process.WaitForExit();

                    if (!string.IsNullOrEmpty(stdError))
                        return "Error: " + stdError;

                    return stdOutput;
                }
                catch (Exception ex)
                {
                    return "An error occurred during processing: " + ex.Message;
                }
            });

            pgvSummarizeTime.Visible = false;

            // Özetlenen metni txtSummarize TextBox'ýna yaz
            txtSummarize.Text = output;


        }

        private void MainForm_Load(object sender, EventArgs e)
        {

        }
    }
}
