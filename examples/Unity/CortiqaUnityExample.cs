#if UNITY_2020_1_OR_NEWER
using System.Threading.Tasks;
using UnityEngine;
using Cortiqa.Sdk;
using Cortiqa.Sdk.Models;

/// <summary>
/// Attach this script to any GameObject in your Unity scene to integrate Cortiqa Falin AI.
/// Compatible with Unity 2021+, .NET Standard 2.0 / .NET 4.x profile.
/// </summary>
public class CortiqaNpcBrain : MonoBehaviour
{
    [Header("Cortiqa Configuration")]
    [SerializeField] private string apiKey = "sk-cortiqa-your-api-key";
    [SerializeField] private string characterPrompt = "You are an ancient blacksmith in an RPG village. Speak with wisdom and warmth.";

    private CortiqaClient client;

    private void Start()
    {
        client = new CortiqaClient(apiKey);
    }

    /// <summary>
    /// Ask NPC a question asynchronously from a player dialog UI or trigger.
    /// </summary>
    public async Task<string> TalkToNpcAsync(string playerQuestion)
    {
        var request = ChatCompletionRequest.Builder()
            .Model("falin-01")
            .AddMessage(ChatMessage.System(characterPrompt))
            .AddMessage(ChatMessage.User(playerQuestion))
            .Temperature(0.8)
            .MaxTokens(150)
            .Build();

        try
        {
            var response = await client.Messages.CreateAsync(request);
            return response.Content;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Cortiqa AI Error]: {ex.Message}");
            return "Forgive me traveler, the forge fire flickers and I cannot speak right now.";
        }
    }

    private void OnDestroy()
    {
        client?.Dispose();
    }
}
#endif
