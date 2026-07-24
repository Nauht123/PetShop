using Microsoft.AspNetCore.SignalR;

namespace PetShop.Hubs
{
    public class ChatHub : Hub
    {
        // Khách hàng join vào nhóm riêng của cuộc trò chuyện của họ
        public async Task JoinConversation(string conversationId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"conv_{conversationId}");
        }

        // Admin join vào nhóm chung để nhận thông báo có tin nhắn mới từ bất kỳ ai
        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
        }
    }
}