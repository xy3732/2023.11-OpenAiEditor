#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class MonoBehaviourEditorExtension
{
    public static string GetScriptContent(this MonoBehaviour instance)
    {
        var script = MonoScript.FromMonoBehaviour(instance);
        if (script != null) return script.text;
        return "";
    }

    public static string GetScriptContent(this ScriptableObject instance)
    {
        var script = MonoScript.FromScriptableObject(instance);
        if (script != null) return script.text;
        return "";
    }
}
#endif