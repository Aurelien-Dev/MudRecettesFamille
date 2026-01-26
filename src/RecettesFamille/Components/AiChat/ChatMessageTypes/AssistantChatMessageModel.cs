using Microsoft.Extensions.AI;
using MudBlazor.Extensions;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace RecettesFamille.Components.AiChat.ChatMessageTypes;

public class AssistantChatMessageModel(IList<AIContent> content) : ChatMessage(ChatRole.Assistant, content)
{

    public IEnumerable<RecipeAi> GetExtraInfo()
    {
        string json = ExtraireJson();

        if (string.IsNullOrEmpty(json))
            return Enumerable.Empty<RecipeAi>();

        var aiRecipe = JsonSerializer.Deserialize<RecipeResultAi>(json);
        return aiRecipe?.result ?? Enumerable.Empty<RecipeAi>();
    }

    public string RetirerJson()
    {
        string sansJson = string.Empty;

        foreach (var content in Contents)
        {
            if (content is not TextContent { Text: { Length: > 0 } text })
            {
                return string.Empty;
            }

            // On enlève tout ce qui commence par ```json jusqu'à la fin ou jusqu'à ```
            sansJson += Regex.Replace(text, @"```json[\s\S]*?(```|$)", "", RegexOptions.IgnoreCase).Trim();
        }

        return sansJson;
    }

    public string ExtraireJson()
    {
        if (Contents.Count == 0 || Contents[0] is not TextContent { Text: { Length: > 0 } text })
        {
            return string.Empty;
        }

        // Essayer d'extraire un JSON complet entre ```json et ```
        var match = Regex.Match(text, @"```json\s*(\{[\s\S]*?)```", RegexOptions.IgnoreCase);

        if (match.Success)
        {
            return match.Groups[1].Value.Trim();
        }

        // Aucun JSON trouvé
        return string.Empty;
    }


    public class RecipeResultAi
    {
        public List<RecipeAi> result { get; set; } = new();
    }

    public class RecipeAi
    {
        public string RecipeName { get; set; } = string.Empty;
        public string RecipeId { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();

        [JsonIgnore]
        public string Id => RecipeId.Split("_")[0];
    }
}