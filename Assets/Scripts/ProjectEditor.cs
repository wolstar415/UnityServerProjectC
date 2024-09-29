#if UNITY_EDITOR
using System.Diagnostics;
using System.IO;
using UnityEditor;

static public class ProjectEditor
{
    public const string Menu = "★Server Project★/";

    [MenuItem(Menu + "비주얼 스튜디오 열기")]
    static void OpenVisualStudio()
    {
        EditorApplication.ExecuteMenuItem("Assets/Open C# Project");
    }

    [MenuItem(Menu + "서버/서버 실행")]
    static void OpenServer()
    {
        // 실행할 파일의 상대 경로
        string relativePath = "Server/Output/GameServer.exe";

        // 상대 경로를 절대 경로로 변환
        string absolutePath = Path.GetFullPath(relativePath);

        // 외부 응용 프로그램 실행
        Process.Start(absolutePath);
    }
}
#endif