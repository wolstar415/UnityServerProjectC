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

            bool lastLineVisible = textBox.GetPositionFromCharIndex(textBox.TextLength).Y < textBox.Size.Height;        // 마지막 글자의 Y 위치가 Size보다 작은지 여부

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

    // 현재 폴더와 상위 폴더를 순회하면서 Git 저장소의 루트를 찾고 업데이트 수행
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

                await Task.Delay(5000); // 5초마다 업데이트 체크
            }
        }
    }
    public string GitPath { get; set; } = "git"; // Git이 시스템 PATH에 포함되어 있는 경우 "git"으로 설정할 수 있습니다.
    // Git 저장소의 루트 경로를 찾습니다.
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

        // Git 저장소를 찾지 못한 경우 null 반환
        return null;
    }

    // Git 저장소 업데이트를 수행합니다.
    private async Task UpdateFolder(string folderPath)
    {
        // Git pull 명령 실행
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

        // 에러 및 결과 출력
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
