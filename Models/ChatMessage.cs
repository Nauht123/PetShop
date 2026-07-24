namespace PetShop.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public int ConversationId { get; set; }
        public bool LaAdmin { get; set; }
        public string NoiDung { get; set; } = "";
        public DateTime NgayGui { get; set; } = DateTime.Now;
        public bool DaDoc { get; set; } = false;

        public ChatConversation? Conversation { get; set; }
    }
}