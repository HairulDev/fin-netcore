using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace PortfolioSignalRApp.Hubs
{
    public class PortfolioHub : Hub
    {
        public async Task NotifyPortfolioUpdate(string message)
        {
            await Clients.All.SendAsync("ReceivePortfolioUpdate", message);
        }
    }
}
