namespace PetShop.ViewModels
{
    public class ChatRequestVM
    {
        public string Message { get; set; } = "";
        public List<ChatMessageVM> History { get; set; } = new();
    }

    public class ChatMessageVM
    {
        public string Role { get; set; } = ""; // "user" | "bot"
        public string Text { get; set; } = "";
    }
}