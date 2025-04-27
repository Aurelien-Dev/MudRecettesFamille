using Microsoft.Extensions.AI;

namespace RecettesFamille.Components.AiChat.ChatMessageTypes;

public class UserChatMessageModel(string content) : ChatMessage(ChatRole.User, content)
{


}
