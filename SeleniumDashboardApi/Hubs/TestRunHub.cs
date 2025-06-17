using Microsoft.AspNetCore.SignalR;

namespace SeleniumDashboardApi.Hubs
{
    public class TestRunHub : Hub
    {
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            Console.WriteLine($"Client {Context.ConnectionId} joined group {groupName}");
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            Console.WriteLine($"Client {Context.ConnectionId} left group {groupName}");
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            Console.WriteLine($"Client {Context.ConnectionId} disconnected");
            await base.OnDisconnectedAsync(exception);
        }
    }
}