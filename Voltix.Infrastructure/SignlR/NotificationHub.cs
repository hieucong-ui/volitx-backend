using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Voltix.Domain.Constants;
using System.Security.Claims;
using System.Text.RegularExpressions;

[Authorize]
public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        var dealerId = Context.User?.FindFirst("DealerId")?.Value;
        var roles = Context.User?.FindAll(ClaimTypes.Role)
                           .Select(r => r.Value)
                           .ToList()
                           ?? new List<string>();

        Console.WriteLine($"[Hub] Connected user: {userId}, dealer: {dealerId}, roles: {string.Join(",", roles)}");

        if (!string.IsNullOrEmpty(dealerId) &&
            (roles.Contains(StaticUserRole.Admin) ||
             roles.Contains(StaticUserRole.EVMStaff) ||
             roles.Contains(StaticUserRole.DealerManager) ||
             roles.Contains(StaticUserRole.DealerStaff)))
        {
            var groupName = GetDealerGroupName(dealerId);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            Console.WriteLine($"[Hub] Added connection {Context.ConnectionId} to group {groupName}");
        }

        await base.OnConnectedAsync();
    }

    private static string GetDealerGroupName(string dealerId)
        => $"dealer:{dealerId}:all";
}
