using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Logging;

namespace gRPC_v3
{
    public class GameService : Game.GameBase
    {
        private Dictionary<string, int> _players = new();
        private Dictionary<string, string> _questionsAnswers = new();
        private int questionindex = 0;
        private const string endgame = "This was the last question"; 
        //private Dictionary<string, int> _winnertable = new();
        public GameService()
        {
            _questionsAnswers.Add("How many meters in kilometer?", "1000");
            //_questionsAnswers.Add("Does Rzhevskiy tell anecdotes?", "No, he whistles");
            _questionsAnswers.Add("Do you need to water the cactus", "No");
        }

        private bool IsCorrectAnswer(Answer answer)
        {
            if (_questionsAnswers[answer.Question].ToLower() == answer.Message.ToLower())
            {
                _players[answer.Username] += 1;
                return true;
            }
            return false;
        }
        private bool IsLastQuestion(Answer answer)
        {
            if (_questionsAnswers[answer.Question] == _questionsAnswers.Values.LastOrDefault())
            {
                return true;
            }
            return false;
        }
        
        private void IsEnd(Answer answer)
        {
            if (answer.Message.ToLower() == "exit")
            {
                throw new Exception("Player disconnected");
            }
        }

        private string GetRandomQuestion()
        {
            if( questionindex == _questionsAnswers.Count)
            {
                return endgame;
            }
            var question = _questionsAnswers.ElementAt(questionindex).Key;

            questionindex += 1;
            return question;
        }

        public override Task<Reply> SendAnswer(Answer answer, ServerCallContext context)
        {
            var reply = new Reply();
            var username = answer.Username;

            if (_players.ContainsKey(username) is false)
            {
                _players.Add(username, 0);

                var question = GetRandomQuestion();
                reply.Question = question;
                reply.Message = $"Welcome, {username}! Your first question is: {question}";
            }
            else
            {
                try
                {
                    var nextRandomQuestion = GetRandomQuestion();
                    IsEnd(answer);
                    if (IsLastQuestion(answer))
                    {
                        reply.Question = endgame;

                        reply.Message = $"Congrats! Its {IsCorrectAnswer(answer)}! \n You've got {_players[username]} points in total. Bye-bye!";
                    }
                    else
                    {
                        reply.Question = nextRandomQuestion;

                        reply.Message = $"{username}!"
                                        + $"Your answer is {IsCorrectAnswer(answer)}"
                                        + $" and now you have {_players[username]} points!"
                                        + "\n"
                                        + $"Next question: {nextRandomQuestion}";
                    }
                }
                catch(Exception e)
                {
                    Console.BackgroundColor = ConsoleColor.DarkMagenta;
                    Console.WriteLine(e.Message);
                    Console.ResetColor();

                    reply.Question = "endgame";
                }

            }
                return Task.FromResult(reply);

        }
    }
}