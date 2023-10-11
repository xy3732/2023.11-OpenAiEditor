using UnityEngine;
using System.Reflection;            // BindingFlags, MethodInfo
using System.IO;                    // File
using System.Collections.Generic;   // List

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace OpenAICommand
{
    class ReferenceWindow : EditorWindow
    {
        static int MAX = 10;
        public static MonoScript[] scripts = new MonoScript[MAX];

        public static string reference = "";
        static bool isOpen = false;

        static ReferenceWindow referwindow;
        Vector2 windowSize = new Vector2(360, 230);
        public static void init() => referwindow = GetWindow<ReferenceWindow>("Reference Scripts");
        public static void exit()
        {
            if(isOpen) referwindow.Close();
            scripts = new MonoScript[MAX];
        }
        void reset()
        {
            for (int i = 0; i < MAX; i++)
            {
                scripts = null;
            }
        }

        public string _reference()
        {
            reference = "";
            for (int i = 0; i < scripts.Length; i++)
            {
                if (scripts[i] == null) continue;

                reference += " - can use component in this script - \n" + scripts[i] + ".\n\n";
            }

            return reference;
        }
        private void OnGUI()
        {
            try
            {
                isOpen = true;

                referwindow.minSize = referwindow.maxSize = windowSize;

                for (int i = 0; i < MAX; i++)
                {
                    scripts[i] = EditorGUILayout.ObjectField("Script " + (i+1), scripts[i], typeof(MonoScript), true, GUILayout.Width(350)) as MonoScript;
                }
                if (GUILayout.Button("Reset",  GUILayout.Height(20f))) reset();

                _reference();
            }
            catch (System.Exception)
            {

            }
        }

        private void OnDisable()
        {
            isOpen = false;
        }
    }


    public class AIScriptWindow : EditorWindow
    {
        #region script operation
        // 파일 위치 주소
        string TempFilPath = "";

        // 스크립트 생성
        /*
        https://github.com/Unity-Technologies/UnityCsReference/blob/master/Editor/Mono/ProjectWindow/ProjectWindowUtil.cs

        CreateScriptAssetWithContent(string pathName, string templateContent)
          
        <summary>
        Create a new script asset.
        UnityEditor.ProjectWindowUtil.CreateScriptAssetWithContent (2019.1)
         
          internal static Object CreateScriptAssetWithContent(string pathName, string templateContent)
        {
            AssetModificationProcessorInternal.OnWillCreateAsset(pathName);

            templateContent = SetLineEndings(templateContent, EditorSettings.lineEndingsForNewScripts);

            string fullPath = Path.GetFullPath(pathName);
            File.WriteAllText(fullPath, templateContent);

            // Import the asset
            AssetDatabase.ImportAsset(pathName);

            return AssetDatabase.LoadAssetAtPath(pathName, typeof(Object));
        }

        */

        void CreateScriptAsset(string code)
        {
            BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
            //CreateAssetWithContent(string filename, string content, Texture2D icon = null);
            MethodInfo method = typeof(ProjectWindowUtil).GetMethod("CreateScriptAssetWithContent", flags);
            method.Invoke(null,new object[] {TempFilPath, code});
        }

        void ChangeScriptAsset(string code)
        {
            AssetDatabase.DeleteAsset(TempFilPath);
            CreateScriptAsset(code);
        }

        // 스크립트 미리보기
        void ReadTextAsset(TextAsset txt)
        {
            nowCommandNum = 0;

            TempFilPath = GetScriptAssetNamePath(_Script.name);
            tempCommandList = new List<string>();

            // 만약 txt 값이 넑값이라면 초기화
            if (txt == null)
            {
                _Head = "";
                _AiCommand = "";
                tempCommandList = new List<string>();

                _Script = null;
                return;
            }

            tempCommandList.Add(txt.text);
            nowCommandNum++;

            _AiCommand = txt.text;
            txtAsset = txt;
            _Head = _Script.name;
        }

        // 현재 에셋 위치 찾기.
        static string GetScriptAssetNamePath(string name)
        {
            string[] path = AssetDatabase.FindAssets($"t:Script {name}");

            if (path.Length <= 0)
            {
                Debug.LogError("현재 이름의 스크립트가 없습니다. - " + name);
                return null;
            }

            return AssetDatabase.GUIDToAssetPath(path[0]);
        }

        static string GetSelectedPath()
        {
            string path = "Assets";

            // 현재 프로젝트 폴더 안에서 선택된 파일을 찾는다 
            foreach (Object obj in Selection.GetFiltered(typeof(Object), SelectionMode.Assets))
            {
                // 선택된 폴더를 찾았으면 그 위치를 path에 저장
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }
            return path;
        }

        #endregion

        #region OpenAI Command

        // OpenAI 제안사항 
        static string InvokeCommand(string input)
            =>
            " - Its Unity C# script.\n" +
            " - Its must use using UnityEngine;. \n" +
            " - the script name is " + _Head.ToString() + ".\n" +

            " - Dont add '```csharp ```' \n" +
            " - member names cannot be the same as their enclosing type.\n" +
            " - There is no selected object. Find game objects manually.\n" +
            " - Don’t use GameObject.FindGameObjectsWithTag.\n" +
            " - I dont need any description. \n" +

            " - I only need the script body. Don't add any explanation.\n" + 
            "The task is described as follows:\n" + input;

        static string InvokeCommand(string input, string reference)
            =>
            " - Its Unity C# script.\n" +
            " - Its must use using UnityEngine;. \n" +
            " - "+ reference + "\n" +
            " - the script name is " + _Head.ToString() + ".\n" +

            " - Dont add '```csharp ```' \n" +
            " - member names cannot be the same as their enclosing type.\n" +
            " - There is no selected object. Find game objects manually.\n" +
            " - Don’t use GameObject.FindGameObjectsWithTag.\n" +
            " - I dont need any description. \n" +
            " - also dont add ```csharp \n" +

            " - I only need the script body. Don't add any explanation.\n" +
            "The task is described as follows:\n" + input;

        static string ChangeCommand(string input, string temp)
            =>
            " - Modify this script - \n" + temp + ".\n" +
            " - the script name must be " + _Head.ToString() + ".\n" +

            " - Dont add '```csharp ```' \n" +
            " - Existing variables should not be renamed. \n" +
            " - member names cannot be the same as their enclosing type.\n" +
            " - There is no selected object. Find game objects manually.\n" +
            " - Don’t use GameObject.FindGameObjectsWithTag.\n" +
            " - I dont need any description. \n" +

            " - Do not add anything other than script. \n" +
            " - I only need the script body. Don't add any explanation.\n" +
           "The task is described as follows:\n" + input;

        static string ChangeCommand(string input, string reference, string temp)
            =>
            " - Modify this script - \n" + temp + ".\n" +
            " - " + reference + "\n" +
            " - the script name must be " + _Head.ToString() + ".\n" +

            " - Dont add '```csharp ```' \n" +
            " - Existing variables should not be renamed. \n" +
            " - member names cannot be the same as their enclosing type.\n" +
            " - There is no selected object. Find game objects manually.\n" +
            " - Don’t use GameObject.FindGameObjectsWithTag.\n" +
            " - I dont need any description. \n" +

            " - Do not add anything other than script. \n" +
            " - I only need the script body. Don't add any explanation.\n" +
            "The task is described as follows:\n" + input;

        void Create()
        {
            string code;
            if (ReferenceWindow.scripts[0] != null)
            {
                Debug.Log("reference create");
                _ReferCommand = ReferenceWindow.reference;
                //_ReferCommand = _ReferenceScript.text;
                code = OpenAIUtil.InvokeChat(InvokeCommand(_Body, _ReferCommand));
            }
            else
            {
                code = OpenAIUtil.InvokeChat(InvokeCommand(_Body));
            }

            _AiCommand = code;
            tempCommandList.Add(code);
            nowCommandNum++;
        }

        void update()
        {
            var temp = (nowCommandNum-1 <= 0) ? 0 : nowCommandNum - 1;
            string code;

            if (ReferenceWindow.scripts[0] != null)
            {
                Debug.Log("reference update");
                _ReferCommand = ReferenceWindow.reference;
                //_ReferCommand = _ReferenceScript.text;
                code = OpenAIUtil.InvokeChat(ChangeCommand(_Body,_ReferCommand,tempCommandList[temp]));
            }
            else
            {
                code = OpenAIUtil.InvokeChat(ChangeCommand(_Body, tempCommandList[temp]));
            }

            if (nowCommandNum != tempCommandList.Count)
            {                   
                for (int i = temp+1; i <= tempCommandList.Count -1; i++)
                {
                    tempCommandList.RemoveAt(i);
                }
            }

            nowCommandNum++;
            _AiCommand = code;
            tempCommandList.Add(code);
        }

        void Change()
        {
            // 스크립트 가 없으면 리턴
            if (_Script == null) return;

            ChangeScriptAsset(_AiCommand);
            Close();
        }

        void next()
        {
            if (nowCommandNum >= tempCommandList.Count) return;
            nowCommandNum++;
            _AiCommand = tempCommandList[nowCommandNum -1];
        }

        void previous()
        {
            if (nowCommandNum <= 1) return;
            nowCommandNum--;
            _AiCommand = tempCommandList[nowCommandNum -1];
        }

        void CreateScript()
        {
            // 생성할 파일 위치 
            if (string.IsNullOrEmpty(TempFilPath)) TempFilPath = "Assets/" + _Head + ".cs";
            else TempFilPath = TempFilPath + "/" + _Head + ".cs";

            // 스크립트 생성
            CreateScriptAsset(tempCommandList[nowCommandNum - 1]);
            tempCommandList = new List<string>();

            // 생성후 에디터 창 닫기
            Close();
        }
        #endregion

        #region Edit GUI
        // 설정

        // 오류 텍스트
        string ApiKeyErrorText =
         "API키를 찾지 못했습니다. \n" +
         "(Edit > Project Settings > AI Command > API Key) 경로에서 OpenAI API키를 입력해주세요.";

        // 스크립트 이름
        static string _Head = "DefaultName";

        // 스크립트 생성명령어
        string _Body = "how can i make W,A,S,D move in 3D?";

        // 스크립트 ex
        //Vector2 scroll;
        TextAsset txtAsset;
        TextAsset newTxtAsset;
        MonoScript _Script;
        MonoScript _ReferenceScript;

        List<string> tempCommandList = new List<string>();
        string _ReferCommand = "";
        string _AiCommand = "";
        int nowCommandNum = 0;

        // API키 확인
        bool IsApiKey => !string.IsNullOrEmpty(AICommandSettings.instance.apikey);

        // 에디터 창 설정
        static AIScriptWindow window;
        Vector2 windowSize = new Vector2(640,340);

        // 위치
        [MenuItem("OpenAI/AI Script")]
        static void Init() => window = GetWindow<AIScriptWindow>("OpenAI Script");

        void OnGUI()
        {
            // 스크립트 업데이트 확인
            // 스크립트가 업데이트 되면 오류가 생긴다.
            try
            {
                if (IsApiKey)
                {
                 
                    // 에디터 윈도우 사이즈 고정
                    window.maxSize = window.minSize = windowSize;

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Script name", GUILayout.Width(147f));
                    GUI.enabled = (_Script == null) ? true : false;
                    _Head = EditorGUILayout.TextField(_Head, GUILayout.Width(140f));
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();

                    // 경로 
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Path",GUILayout.Width(147f));
                    GUI.enabled = (_Script == null) ? true : false;
                    EditorGUILayout.SelectableLabel(TempFilPath, EditorStyles.textArea, GUILayout.Width(200f),GUILayout.Height(20f));
                    if (GUILayout.Button("Path", GUILayout.Width(100f))) TempFilPath = GetSelectedPath();
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();

                    // 레퍼런스 스크립트 
                    _Script = EditorGUILayout.ObjectField("Script", _Script, typeof(MonoScript), true, GUILayout.Width(300f)) as MonoScript;
                    EditorGUILayout.BeginHorizontal();
                    //_ReferenceScript = EditorGUILayout.ObjectField("Reference Script", _ReferenceScript, typeof(MonoScript), true, GUILayout.Width(300f)) as MonoScript;
                    GUILayout.Label("Reference Script",GUILayout.Width(147f));
                    if (GUILayout.Button("Reference", GUILayout.Width(100f), GUILayout.Height(20f))) ReferenceWindow.init();
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    // 커맨드 박스
                    EditorGUILayout.BeginVertical();
                    GUILayout.Label("Command");
                    _Body = EditorGUILayout.TextArea(_Body, GUILayout.Height(200f), GUILayout.Width(300f));
                    EditorGUILayout.EndVertical();

                    // Open AI 스크립트 생성 박스
                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Script");
                    GUILayout.Space(255);
                    GUILayout.Label(nowCommandNum + " / " + tempCommandList.Count.ToString());
                    EditorGUILayout.EndHorizontal();
                    newTxtAsset = _Script;
                    if (txtAsset != newTxtAsset) ReadTextAsset(newTxtAsset);
                    EditorGUILayout.BeginHorizontal();
                    GUI.enabled = (nowCommandNum <= 1) ? false : true;
                    if (GUILayout.Button("<",GUILayout.Width(20f),GUILayout.Height(200f))) previous();
                    GUI.enabled = true;
                    EditorGUILayout.SelectableLabel(_AiCommand, EditorStyles.textArea, GUILayout.Height(200f), GUILayout.Width(290f));
                    GUI.enabled = (tempCommandList.Count == nowCommandNum) ? false : true;
                    if (GUILayout.Button(">", GUILayout.Width(20f), GUILayout.Height(200f))) next();
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.EndHorizontal();
                    // OpenAI 실행 버튼
                    EditorGUILayout.BeginHorizontal();
                    GUI.enabled = (_Script == null && nowCommandNum == 0) ? true : false;
                    if (GUILayout.Button("Invoke", GUILayout.Width(100f), GUILayout.Height(25f))) Create();
                    GUI.enabled = true;

                    GUI.enabled = (nowCommandNum!=0) ? true : false;
                    if (GUILayout.Button("Update", GUILayout.Width(100f), GUILayout.Height(25f))) update();
                    GUI.enabled = true;

                    GUILayout.Space(170);

                    GUI.enabled = (_Script == null) ? false : true;
                    if (GUILayout.Button("Change Script", GUILayout.Width(125f), GUILayout.Height(25f))) Change();
                    GUI.enabled = true;

                    GUI.enabled = (!string.IsNullOrEmpty(_Head) && !string.IsNullOrEmpty(TempFilPath) && _Script == null) ? true : false;
                    if(GUILayout.Button("Create Script.",GUILayout.Width(125f),GUILayout.Height(25f))) CreateScript();
                    GUI.enabled = true;
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    EditorGUILayout.HelpBox(ApiKeyErrorText, MessageType.Error);
                    Debug.LogError(ApiKeyErrorText);
                }
            }
            catch (System.Exception) 
            {
                // 에러 발생시 창 닫기
                //Close();
            }
        }
        #endregion

        private void OnEnable() => tempCommandList = new List<string>();

        private void OnDisable() => disable();
        void disable()
        {
            _ReferCommand = "";
            tempCommandList = new List<string>();
            ReferenceWindow.exit();
        }
    }
}
