using Microsoft.AspNetCore.SignalR;

namespace BlazorApp1.Hubs
{
    public class ChatHub:Hub
    {
       public Task SendMessage(string user , string message,string time)
        {
            return Clients.All.SendAsync("RecieveMessage", user, message,time);
        }
    }
}
