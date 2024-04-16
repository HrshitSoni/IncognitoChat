using System.Windows;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace Wpf_Client;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    HubConnection connection;
    private Dictionary<string, string> messageTimespan;
    public MainWindow()
    {
        InitializeComponent();

        messageTimespan = new Dictionary<string, string>();

        connection = new HubConnectionBuilder().WithUrl("https://localhost:7156/chathub").WithAutomaticReconnect().Build();

        connection.Reconnecting += (sender) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                var newMessage = "Attempting to reconnect";
                Messages.Items.Add(newMessage);

            });
            return Task.CompletedTask;
        };

        connection.Reconnected += (sender) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                var newMessage = "Reconnected to the server";
                Messages.Items.Add(newMessage);
            });
            return Task.CompletedTask;
        };

        connection.Closed += (sender) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                var newMessage = "Connection closed";
                Messages.Items.Add(newMessage);
                OpenConnection.IsEnabled = true;
                SendBtn.IsEnabled = false;
            });
            return Task.CompletedTask;
        };
    }

    private async void OpenConnectionBtn_Click(object sender, RoutedEventArgs e)
    {
        var timeString = DateTime.Now.ToString("HH,mm,ss");
        connection.On<string, string,TimeSpan>("RecieveMessage", (user, message,time) =>
        {
            this.Dispatcher.Invoke(() =>
            {
                var newMessage = $"{user} : {message}  {time}";
                Messages.Items.Add(newMessage);
                messageTimespan.Add(newMessage, timeString);
            });
        });

        try
        {
            await connection.StartAsync();
            Messages.Items.Add("Connection started");
            OpenConnection.IsEnabled = false;
            SendBtn.IsEnabled = true;

          await StartMessageExpirationTimer();
        }
        catch (Exception ex)
        {
            Messages.Items.Add(ex);
        }
    }

    private async void SendBtn_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var timeString = DateTime.Now.ToString("HH:mm:ss");
            await connection.InvokeAsync("SendMessage", "WPF Client", MessageInput.Text, timeString);
        }
        catch (Exception ex)
        {
            Messages.Items.Add(ex);
        }
    }

    private async Task StartMessageExpirationTimer()
    {
        await Task.Run(async () =>
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(30)); 
                ExpireMessages();
            }
        });
    }

    private void ExpireMessages()
    {
        var expiredMessages = new List<string>();
        
        var currTime = DateTime.Now;
        foreach (var message in messageTimespan)
        {
            var time = DateTime.ParseExact(message.Value, "HH,mm,ss", null);
            if(currTime - time > TimeSpan.FromSeconds(300))
            {
                expiredMessages.Add(message.Key);
            }
        }

        foreach(var expiredMessage in expiredMessages)
        {
            var index = Messages.Items.IndexOf(expiredMessage);
            if (index != -1)
            {
                Dispatcher.Invoke(() => Messages.Items.RemoveAt(index));
            }
        }

    }
}