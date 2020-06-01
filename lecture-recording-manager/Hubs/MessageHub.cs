using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
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
