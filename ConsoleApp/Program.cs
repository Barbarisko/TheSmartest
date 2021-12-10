using System;
using System.Net.Http;
using System.Threading.Tasks;
using gRPC_v3;
using Grpc.Net.Client;

namespace ConsoleApp
{
    class Program
    {
        private static string SERVER_ADDRESS = "http://localhost:5000";
        private const string endgame = "This was the last question";

        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress(SERVER_ADDRESS, new GrpcChannelOptions()
            {
                HttpHandler = GetHttpClientHandler()
            });

            SendSystemMessage("Welcome to THE SMARTEST Game. " +
                "\nWrite down your name: ");
            var username = Console.ReadLine();

            var question = "";
            var client = new Game.GameClient(channel);
            var end = true;
            while (end)
            {
                try
                {
                    if (string.IsNullOrEmpty(question))
                    {
                        var firstQuestion = await client.SendAnswerAsync(new Answer { Question = "", Username = username });
                        if (firstQuestion.Question == endgame)
                        {
                            CreateColorLabel(firstQuestion.Message);
                            end = false;
                            continue;
                        }
                        else
                        {
                            question = firstQuestion.Question;
                            CreateColorLabel(firstQuestion.Message);
                        }
                    }

                    SendSystemMessage("Answer's killin (`exit` to kill game): ");
                    var answer = Console.ReadLine();

                    if (answer == "exit")
                    {
                        var reply = await client.SendAnswerAsync(new Answer 
                        { Question = question, 
                            Message = answer, 
                            Username = username });

                        Environment.Exit(-1);                       

                    }
                    else
                    {
                        var requst = new Answer { Question = question, Message = answer, Username = username };
                        var reply = await client.SendAnswerAsync(requst);

                        Console.WriteLine(reply.Message);
                        question = reply.Question;
                    }
                }
                catch
                {
                    Console.WriteLine("Smth wrong with server!");
                }
            }
        }

        private static HttpClientHandler GetHttpClientHandler()
        {
            var httpHandler = new HttpClientHandler();
            // Return `true` to allow certificates that are untrusted/invalid
            httpHandler.ServerCertificateCustomValidationCallback =
                HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
            return httpHandler;
        }
        private static void CreateColorLabel(string text)
        {
            Random r = new Random();
            Console.BackgroundColor = (ConsoleColor)r.Next(0, 16);
            Console.WriteLine($"{text}");
            Console.BackgroundColor = ConsoleColor.Black;
        }
        private static void SendSystemMessage(string text)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"{text}");
            Console.ResetColor();
        }
    }
}