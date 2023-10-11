using UnityEngine;
using UnityEditor;

namespace OpenAICommand
{
    [FilePath("UserSettings/AICommandSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    // 싱글톤 스크립터블 오브젝트 선언
    // 런타임 최적화를 위해서 sealed 키워드 선언
    public sealed class AICommandSettings : ScriptableSingleton<AICommandSettings>
    {
        public string apikey = null;
        public void Save() => Save(true);
        void OnDisable() => Save();
    }

    sealed class AICommandSettingsProvider : SettingsProvider
    {
        // Edit - ProjectSettings.. 에 OpenAI Settings / AI Command 생성할 주소값 지정
        public AICommandSettingsProvider() : base("OpenAI Settings/AI Command", SettingsScope.Project) { }

        public override void OnGUI(string search)
        {
            // settings 인스턴스 불러오기
            var settings = AICommandSettings.instance;

            GUI.Label(new Rect(0, 10, 500, 20), "if you dont have any key, make a new one!");
            GUILayout.Space(30);
            if (EditorGUILayout.LinkButton("https://platform.openai.com/account/api-keys")) Application.OpenURL("https://platform.openai.com/account/api-keys");
            GUILayout.Space(30);
            // 현재 입력한 정보를 저장할 변수
            var editKey = settings.apikey;

            // OpenAI Settings/AI Command 창에 있는 정보가 바꼈는지 확인
            EditorGUI.BeginChangeCheck();
            editKey = EditorGUILayout.TextField("Edit API Key", editKey);

            // 바뀐값 정보를 저장.
            if (EditorGUI.EndChangeCheck())
            {
                settings.apikey = editKey;
                settings.Save();
            }
        }

        // Edit - ProjectSettings.. 에 OpenAI Settings / AI Command를 생성
        [SettingsProvider]
        public static SettingsProvider CreateCustomSettingsProvider() => new AICommandSettingsProvider();
    }
}

