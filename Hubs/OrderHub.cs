using Microsoft.AspNetCore.SignalR;

namespace RestaurantBackend.Hubs
{
    /// <summary>
    /// SignalR Hub để broadcast đơn hàng realtime
    /// </summary>
    public class OrderHub : Hub
    {
        // Khi client connect
        public override async Task OnConnectedAsync()
        {
            var connectionId = Context.ConnectionId;
            Console.WriteLine($"✅ Client connected to OrderHub: {connectionId}");
            await base.OnConnectedAsync();
        }

        // Khi client disconnect
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var connectionId = Context.ConnectionId;
            Console.WriteLine($"❌ Client disconnected from OrderHub: {connectionId}");
            
            if (exception != null)
            {
                Console.WriteLine($"⚠️ Disconnect reason: {exception.Message}");
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        // Method để client join vào group theo số bàn (optional - dùng cho filter)
        public async Task JoinTableGroup(string tableNumber)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Table_{tableNumber}");
            Console.WriteLine($"📍 Client {Context.ConnectionId} joined Table_{tableNumber}");
        }

        // Method để client leave group
        public async Task LeaveTableGroup(string tableNumber)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Table_{tableNumber}");
            Console.WriteLine($"📍 Client {Context.ConnectionId} left Table_{tableNumber}");
        }
    }
}