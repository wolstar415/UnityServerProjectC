using System.Diagnostics;

public partial class Form : System.Windows.Forms.Form
{
    public static Form Inst;

    public string DataHash;
    public bool DataOn(string hash)
    {
        return DataHash == hash;
    }
    public Form()
    {
        Inst = this;
        InitializeComponent();
    }

    public void AddLog(string msg)
    {
        Action action = (() =>
        {
            msg = $"[{DateTime.Now:s}] " + msg;
            RichTextBox textBox = richTextBox1;

            bool lastLineVisible = textBox.GetPositionFromCharIndex(textBox.TextLength).Y < textBox.Size.Height;        // ������ ������ Y ��ġ�� Size���� ������ ����

            textBox.Select(textBox.TextLength, 0);

            textBox.SelectedText = msg + "\n";

            if (lastLineVisible)
                textBox.ScrollToCaret();

        });
        if (richTextBox1.InvokeRequired)
        {
            richTextBox1.BeginInvoke(action);
        }
        else
        {
            action.Invoke();
        }

    }

    private void Form_Load(object sender, EventArgs e)
    {
        Task.Run(async () =>
            {
                Server.Init();

#if DEBUG
                Server.Start();
#endif
            });

        Task.Run(() => UpdateGitRepositories());
    }

    // ���� ������ ���� ������ ��ȸ�ϸ鼭 Git ������� ��Ʈ�� ã�� ������Ʈ ����
    private async void UpdateGitRepositories()
    {
        string? currentPath = Directory.GetCurrentDirectory();
        string? gitRoot = "";
        while (!string.IsNullOrEmpty(currentPath))
        {
            gitRoot = FindGitRoot(currentPath);

            if (gitRoot != null)
            {
                break;

            }

            currentPath = Directory.GetParent(currentPath)?.FullName;
        }
        if (gitRoot != null && gitRoot.IsValid())
        {
            while (true)
            {
                await UpdateFolder(gitRoot);

                await Task.Delay(5000); // 5�ʸ��� ������Ʈ üũ
            }
        }
    }
    public string GitPath { get; set; } = "git"; // Git�� �ý��� PATH�� ���ԵǾ� �ִ� ��� "git"���� ������ �� �ֽ��ϴ�.
    // Git ������� ��Ʈ ��θ� ã���ϴ�.
    private string? FindGitRoot(string startPath)
    {
        string? currentPath = startPath;

        while (!string.IsNullOrEmpty(currentPath))
        {
            if (Directory.Exists(Path.Combine(currentPath, ".git")))
            {
                return currentPath;
            }

            currentPath = Directory.GetParent(currentPath)?.FullName;
        }

        // Git ����Ҹ� ã�� ���� ��� null ��ȯ
        return null;
    }

    // Git ����� ������Ʈ�� �����մϴ�.
    private async Task UpdateFolder(string folderPath)
    {
        // Git pull ��� ����
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = GitPath,
            Arguments = $"-C \"{folderPath}\" pull",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        Process process = new Process
        {
            StartInfo = startInfo
        };

        process.Start();
        await process.WaitForExitAsync();

        // ���� �� ��� ���
        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();
    }

    private void button1_Click(object sender, EventArgs e)
    {
        Server.ReStart();
    }

    private void richTextBox1_TextChanged(object sender, EventArgs e)
    {

    }

    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {

    }

    private void button2_Click(object sender, EventArgs e)
    {
        Server.Stop();
    }
}

public static class StringExtensions
{
    public static bool IsValid(this string input)
    {
        // Check if the string is not null, not empty, and not only whitespace
        return !string.IsNullOrWhiteSpace(input);
    }
}
