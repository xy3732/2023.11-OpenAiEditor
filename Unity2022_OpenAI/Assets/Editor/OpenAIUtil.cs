using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;


namespace OpenAICommand
{
    static class OpenAIUtil
    {
        static string CreateChatRequestBody(string prompt)
        {
            var msg = new openAI.RequestMessage();
            msg.role = "user";                      // system, user, assistant �߿� �ϳ�
            msg.content = prompt;                   // ���빰

            var req = new openAI.Request();
            req.model = "gpt-3.5-turbo-16k";       // �� Ÿ��
            req.messages = new[] { msg };           

            return JsonUtility.ToJson(req);
        }

        public static string InvokeChat(string prompt)
        {
            AICommandSettings settings = AICommandSettings.instance;

            // ����
            using var post = UnityWebRequest.Post(openAI.Api.Url, CreateChatRequestBody(prompt), "application/json"); // Content-Type

            /*
            curl https://api.openai.com/v1/chat/completions 

            -H "Content-Type: application/json" \
            -H "Authorization: Bearer $OPENAI_API_KEY" \
            -d '{
            "model": "gpt-3.5-turbo",
            "messages": [{"role": "user", "content": "Hello!"}]
            }'
             */

            // API key Ȯ��
            post.SetRequestHeader("Authorization", "Bearer " + settings.apikey); // Authorization

            // openAI�� ����.
            var req = post.SendWebRequest();

            // �׽�ũ ���μ��� �� (��¥)
            EditorUtility.DisplayProgressBar("OepnAI Generate", "Process . . . ", 99);
            for (var progress = 0.0f; !req.isDone; progress += 0.01f)
            {
                System.Threading.Thread.Sleep(100);
            }
            EditorUtility.ClearProgressBar();


            // OpenAI�� �� �ޱ�.
            var json = post.downloadHandler.text;
            var data = JsonUtility.FromJson<openAI.Response>(json);
            return data.choices[0].message.content;
        }
    }

} // namespace AICommand
