using UnityEngine;
using UnityEditor;

namespace OpenAICommand
{
    [FilePath("UserSettings/AICommandSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    // �̱��� ��ũ���ͺ� ������Ʈ ����
    // ��Ÿ�� ����ȭ�� ���ؼ� sealed Ű���� ����
    public sealed class AICommandSettings : ScriptableSingleton<AICommandSettings>
    {
        public string apikey = null;
        public void Save() => Save(true);
        void OnDisable() => Save();
    }

    sealed class AICommandSettingsProvider : SettingsProvider
    {
        // Edit - ProjectSettings.. �� OpenAI Settings / AI Command ������ �ּҰ� ����
        public AICommandSettingsProvider() : base("OpenAI Settings/AI Command", SettingsScope.Project) { }

        public override void OnGUI(string search)
        {
            // settings �ν��Ͻ� �ҷ�����
            var settings = AICommandSettings.instance;

            GUI.Label(new Rect(0, 10, 500, 20), "if you dont have any key, make a new one!");
            GUILayout.Space(30);
            if (EditorGUILayout.LinkButton("https://platform.openai.com/account/api-keys")) Application.OpenURL("https://platform.openai.com/account/api-keys");
            GUILayout.Space(30);
            // ���� �Է��� ������ ������ ����
            var editKey = settings.apikey;

            // OpenAI Settings/AI Command â�� �ִ� ������ �ٲ����� Ȯ��
            EditorGUI.BeginChangeCheck();
            editKey = EditorGUILayout.TextField("Edit API Key", editKey);

            // �ٲﰪ ������ ����.
            if (EditorGUI.EndChangeCheck())
            {
                settings.apikey = editKey;
                settings.Save();
            }
        }

        // Edit - ProjectSettings.. �� OpenAI Settings / AI Command�� ����
        [SettingsProvider]
        public static SettingsProvider CreateCustomSettingsProvider() => new AICommandSettingsProvider();
    }
}

