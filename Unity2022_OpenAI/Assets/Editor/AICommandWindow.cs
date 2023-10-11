using UnityEngine;
using System.Reflection;            // BindingFlags, MethodInfo

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OpenAICommand
{

    public sealed class AICommandWindow : EditorWindow
    {
        #region script operation
        // OpenAI 커맨드 실행 스크립트 주소
        const string TempFilePath = "Assets/OpenAICommand.cs"; //AICommandTemp

        // 해당 주소내에 스크립트가 존재하는지 확인
        bool TempFileExists => System.IO.File.Exists(TempFilePath);

        // 스크립트 생성
        /*
        https://github.com/jeffvella/UnityNavMeshAreas/blob/master/NavMeshAreas.cs

        CreateScriptAssetWithContent(string pathName, string templateContent)
          
        <summary>
        Create a new script asset.
        UnityEditor.ProjectWindowUtil.CreateScriptAssetWithContent (2019.1)
         
        private static UnityEngine.Object CreateScriptAssetWithContent(string pathName, string templateContent)
        {
            templateContent = SetLineEndings(templateContent, EditorSettings.lineEndingsForNewScripts);
            string fullPath = Path.GetFullPath(pathName);
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding(true);
            File.WriteAllText(fullPath, templateContent, encoding);
            AssetDatabase.ImportAsset(pathName);
            return AssetDatabase.LoadAssetAtPath(pathName, typeof(UnityEngine.Object));
        } 
        */
        void CreateScriptAsset(string code)
        {
            BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
            // OpenAI가 생성한 스크립트 실행
            MethodInfo method = typeof(ProjectWindowUtil).GetMethod("CreateScriptAssetWithContent", flags);
            method.Invoke(null, new object[] { TempFilePath, code });
        }
        #endregion

        #region OpenAI Command
        // OpenAI 제안사항 
        static string Command(string input)
          =>
            "Write a Unity Editor script.\n" +
            " - It provides its functionality as a menu item placed \"Edit\" > \"Do Task\".\n" +
            " - The type or namespace name 'MenuItem' .\n" +
            " - It must use using UnityEditor;. \n " +
            " - It not a static void DoTask(), Make sure public static void DoTask().\n" +
            " - member names cannot be the same as their enclosing type.\n" +
            " - It doesn’t provide any editor window. It immediately does the task when the menu item is invoked.\n" +
            " - Don’t use GameObject.FindGameObjectsWithTag.\n" +
            " - It not FindObjectsOfType, Make sure GameObject.FindObjectsOfType. \n" +
            " - There is no selected object. Find game objects manually.\n" +
            " - I only need the script body. Don't add any explanation.\n" +
            "The task is described as follows:\n" + input;

        void Run()
        {
            // OpenAI가 작성한 스크립트를 불러오기
            var code = OpenAIUtil.InvokeChat(Command(_Body));
            // 해당 스크립트를 생성
            CreateScriptAsset(code);
        }
        #endregion

        #region Editor GUI
        // 기본 설정
        string _Body = "Create 10 Capsule object at random position, random Scale.";
        string ApiKeyErrorText =
          "API키를 찾지 못했습니다. \n" +
          "(Edit > Project Settings > AI Command > API Key) 경로에서 OpenAI API키를 입력해주세요.";

        // editableApiKey에 API키가 있는지 없는지 홖인.
        bool IsApiKeyOk => !string.IsNullOrEmpty(AICommandSettings.instance.apikey);

        // 에디터 윈도우 설정
        static AICommandWindow window;
        Vector2 windowSize = new Vector2(500, 275);

        [MenuItem("OpenAI/AI Command")]
        static void Init() => window = GetWindow<AICommandWindow>("OpenAI Command");
        void OnGUI()    
        {
            try
            {
                if (IsApiKeyOk)
                {
                    window.maxSize = window.minSize = windowSize;
                    _Body = EditorGUILayout.TextArea(_Body, GUILayout.Height(250));
                    if (GUILayout.Button("Task Run", GUILayout.Height(25))) Run();
                }
                else
                {
                    EditorGUILayout.HelpBox(ApiKeyErrorText, MessageType.Error);
                    Debug.LogError(ApiKeyErrorText);
                }
            }
            catch (System.Exception)
            {
            }
        }
        #endregion

        #region Action
        void OnEnable() => AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;

        void OnDisable() => AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;

        // 행동 스크립트 삭제
        void OnAfterAssemblyReload()
        {
            if (!TempFileExists) return;

            EditorApplication.ExecuteMenuItem("Edit/Do Task");
            AssetDatabase.DeleteAsset(TempFilePath);
            Close();
        }
        #endregion
    }
}
