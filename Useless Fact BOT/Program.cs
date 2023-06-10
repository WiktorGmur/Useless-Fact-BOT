using Discord.Commands;
using Discord.WebSocket;
using Discord;
using Newtonsoft.Json;



using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json;

public class FactModule : ModuleBase<SocketCommandContext>
{
    [Command("fact")]
    public async Task GetFactAsync()
    {
        // send typing indicator to show bot is working
        await Context.Channel.TriggerTypingAsync();

        // get a random fact from the API
        string fact = await GetRandomFactAsync();

        // send the fact as a message
        await ReplyAsync(fact);
    }

    private async Task<string> GetRandomFactAsync()
    {
        // make a request to the API
        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri("https://uselessfacts.jsph.pl");
            HttpResponseMessage response = await client.GetAsync("/random.json?language=en");

            // read the response content as a string
            string content = await response.Content.ReadAsStringAsync();

            // parse the JSON response to a C# object
            dynamic data = JsonConvert.DeserializeObject(content);

            // return the fact string
            return data.text;
        }
    }
}

public class Bot
{
    private DiscordSocketClient _client;
    private CommandService _commands;

    public async Task MainAsync()
    {
        // create the Discord client and command service
        _client = new DiscordSocketClient();
        _commands = new CommandService();

        _client.Log += (msg) =>
        {
            Console.WriteLine(msg.Message);
            return Task.CompletedTask;
        };


        // register the command module
        await _commands.AddModuleAsync<FactModule>(null);

        // add the command execution handler
        _client.MessageReceived += HandleCommandAsync;

        // add the command execution handler for successful command execution
        _commands.CommandExecuted += CommandExecutedAsync;

        // log the bot in and start listening for messages
        string token = "";
        await _client.LoginAsync(TokenType.Bot, token);
        await _client.StartAsync();
        await _client.SetGameAsync("!rfact to get a random fact");

        // keep the program running until it is stopped manually
        await Task.Delay(-1);
    }

    private async Task HandleCommandAsync(SocketMessage message)
    {
        Console.WriteLine($"Received message: {message.Content}");
        Console.WriteLine($"Command received in channel: {message.Channel.Name}");


        // ignore system messages and messages from bots
        if (!(message is SocketUserMessage msg) || msg.Author.IsBot)
        {
            Console.WriteLine($"Ignoring message from bot or system message");
            return;
        }

        // check if the message is a command
        int argPos = 0;
        if (msg.HasStringPrefix("!", ref argPos))
        {
            Console.WriteLine($"Detected command: {msg.Content}");
            // execute the command and handle any errors
            var context = new SocketCommandContext(_client, msg);
            var result = await _commands.ExecuteAsync(context, argPos, null);
            if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
            {
                Console.WriteLine(result.ErrorReason);
            }
        }
        else
        {
            Console.WriteLine($"Message is not a command");
        }
    }


    private async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
    {
        if (result.IsSuccess)
        {
            Console.WriteLine($"Command \"{command.Value.Name}\" executed successfully.");
        }
    }
}

class Program
{
    static void Main(string[] args)
        => new Bot().MainAsync().GetAwaiter().GetResult();
}
