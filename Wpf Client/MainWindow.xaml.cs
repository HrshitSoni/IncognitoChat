using System.Windows;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Wpf_Client;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    HubConnection connection;
    public List<string> messageList = new List<string>();
    public MainWindow()
    {
        InitializeComponent();

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

        var timer = new DispatcherTimer();
        timer.Interval = TimeSpan.FromSeconds(30); 
        timer.Tick += Timer_Tick;

        timer.Start();
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
                messageList.Add(newMessage);
            });
        });

        try
        {
            await connection.StartAsync();
            Messages.Items.Add("Connection started");
            OpenConnection.IsEnabled = false;
            SendBtn.IsEnabled = true;
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
            await connection.InvokeAsync("SendMessage", UserInput.Text, MessageInput.Text, timeString);
            MessageInput.Text = "";
        }
        catch (Exception ex)
        {
            Messages.Items.Add(ex);
        }
    }
   //.......................................... Time Delay code ..........................//

    private void Timer_Tick(object sender, EventArgs e)
    {
        if (Messages.Items.Count > 1)
        {
            Messages.Items.RemoveAt(1);
            Messages.Items.Refresh();
        }
    }



    // ......................................... XAML UI code.............................//
    private void UserInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        TextBox textBox = (TextBox)sender;
        if (textBox.Text == "User Name?")
        {
            textBox.Foreground = Brushes.Gray;
        }
        else
        {
            textBox.Foreground = Brushes.Black;
        }
    }

    private void UserInput_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        TextBox textBox = (TextBox)sender;
        if(textBox.Text == "User Name?")
        {
            textBox.Text = "";
        }
    }

    private void MessageInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        TextBox textBox = (TextBox)sender;
        if (textBox.Text == "Enter your message here...")
        {
            textBox.Foreground = Brushes.Gray;
        }
        else
        {
            textBox.Foreground = Brushes.Black;
        }
    }

    private void MessageInput_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {

        TextBox textBox = (TextBox)sender;
        if (textBox.Text == "Enter your message here...")
        {
            textBox.Text = "";
        }
    }
}

public class MessageItem
{
    public string user { get; set; }
    public string messageString { get; set; }
    public TimeSpan time { get; set; }
}