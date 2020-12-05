using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace LectureRecordingManager.Hubs
{
    public class MessageHub : Hub
    {
        public async Task StatusChange(Message message)
        {
            await Clients.All.SendAsync("StatusChanged", message);
        }
    }
}
