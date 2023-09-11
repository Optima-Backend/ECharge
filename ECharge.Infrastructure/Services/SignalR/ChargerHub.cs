//using System.Collections.Concurrent;
//using ECharge.Domain.Enums;
//using ECharge.Infrastructure.Services.DatabaseContext;
//using Microsoft.AspNetCore.SignalR;
//using Microsoft.EntityFrameworkCore;

//namespace ECharge.Infrastructure.Services.SignalR;

//public class ChargerHub : Hub
//{
//    private readonly static ConcurrentDictionary<string, string> ConnectedClients = new();
//    private readonly DataContext _dataContext;

//    public ChargerHub(DataContext dataContext)
//    {
//        _dataContext = dataContext;
//    }

//    public override async Task OnConnectedAsync()
//    {
//        var httpContext = Context.GetHttpContext();
//        string orderId = httpContext.Request.Query["orderId"];

//        if (string.IsNullOrEmpty(orderId))
//        {
//            Context.Abort();
//            return;
//        };

//        if (!await _dataContext.Sessions.AsNoTracking().Include(x => x.Order).AnyAsync(x => x.Order.OrderId == orderId && x.Status == SessionStatus.Charging))
//        {
//            Context.Abort();
//            return;
//        }

//        ConnectedClients[Context.ConnectionId] = orderId;

//        await base.OnConnectedAsync();

//        return;
//    }

//    public override Task OnDisconnectedAsync(Exception exception)
//    {
//        ConnectedClients.TryRemove(Context.ConnectionId, out _);

//        return base.OnDisconnectedAsync(exception);
//    }

//    public static string GetConnectionIdByOrderId(string orderId)
//    {
//        return ConnectedClients.FirstOrDefault(x => x.Value == orderId).Key;
//    }
//}

