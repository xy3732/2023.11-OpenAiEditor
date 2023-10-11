namespace OpenAICommand.openAI
{
    // ªÁ¿Ã∆Æ - https://platform.openai.com/docs/api-reference/chat/create

    public static class Api
    {
        // OpenAI 
        public const string Url = "https://api.openai.com/v1/chat/completions";
    }

    /* Response
     {
        "id": "chatcmpl-123",                                                       - Response
        "choices": 
        [{
            "index": 0,                                                         - ResChoice
            "message": 
        
            {x`
            "role": "assistant",                                            - ResMessage
            "content": "\n\nHello there, how may I assist you today?",
            },
        }],
     */

    // model    - ID of the model to use. See the model endpoint compatibility table for details on which models work with the Chat API.
    // messages - A list of messages describing the conversation so far.
    // role     - The role of the author of this message. One of system, user, or assistant.
    // content  - The contents of the message.

    [System.Serializable]
    public struct ResMessage
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    public struct ResChoice
    {
        public int index;
        public ResMessage message;
    }

    [System.Serializable]
    public struct Response
    {
        public string id;
        public ResChoice[] choices;
    }

    /*  - Parameters -
        {                                                   - Request
            "model": "gpt-3.5-turbo",
            "messages": 
            [{                                          - RequestMessage
                "role": "user", 
                "content": "Hello!"
            }]         
        }
    */

    [System.Serializable]
    public struct RequestMessage
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    public struct Request
    {
        public string model;
        public RequestMessage[] messages;
    }
}
