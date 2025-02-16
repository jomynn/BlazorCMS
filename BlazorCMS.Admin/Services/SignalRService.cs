using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace BlazorCMS.Admin.Services
{
    public class SignalRService
    {
        private HubConnection _hubConnection;
        private readonly NavigationManager _navigation;

        public SignalRService(NavigationManager navigation)
        {
            _navigation = navigation;
            _hubConnection = new HubConnectionBuilder()
                .WithUrl(_navigation.BaseUri + "hub")
                .WithAutomaticReconnect() // ✅ Auto Reconnect
            .Build();
        }

        public async Task ConnectAsync()
        {
            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                await _hubConnection.StartAsync();
            }
        }

        public async Task DisconnectAsync()
        {
            if (_hubConnection.State == HubConnectionState.Connected)
            {
                await _hubConnection.StopAsync();
            }
        }

        public async Task ReconnectAsync()
        {
            await DisconnectAsync();
            await ConnectAsync();
        }
    }
}
