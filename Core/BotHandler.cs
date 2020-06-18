using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;
using TicTacToeTG.Core.Types;

namespace TicTacToeTG.Core
{
    public class BotHandler
    {
        TelegramBotClient client;
        public List<BotUser> users { get; set; }
        public BotHandler(TelegramBotClient client, List<BotUser> botUsers)
        {
            this.client = client;
            users = botUsers;
        }

        private async void HandleGameCallbackAsync(CallbackQuery callback, BotUser user)
        {
            if (user != null && user.currentGame != null)
            {
                int callbackPosition = Convert.ToInt32(callback.Data);
                MoveResult result = user.currentGame.Move(callbackPosition, user);
                if (result.CanMove && !result.GameFinished)
                {
                    await client.EditMessageTextAsync(user.chatId, user.gameMessageId, "Turn: " + user.currentGame.GetOpponent(user).username,
                        default, default, user.currentGame.gameKeyboard);

                    await client.EditMessageTextAsync(user.currentGame.GetOpponent(user).chatId, user.currentGame.GetOpponent(user).gameMessageId,
                        "Turn: " + user.currentGame.GetOpponent(user).username, default, default, user.currentGame.gameKeyboard);
                }
                else if (result.Standoff)
                {
                    await client.EditMessageTextAsync(user.chatId, user.gameMessageId, "Standoff, good job!");

                    await client.EditMessageTextAsync(user.currentGame.GetOpponent(user).chatId, user.currentGame.GetOpponent(user).gameMessageId,
                        "Standoff, good job!");

                    user.currentGame.EndGame();
                }
                else if (result.GameFinished)
                {
                    await client.EditMessageTextAsync(user.chatId, user.gameMessageId, user.username + " winning this game");

                    await client.EditMessageTextAsync(user.currentGame.GetOpponent(user).chatId, user.currentGame.GetOpponent(user).gameMessageId,
                        user.username + " winning this game");

                    user.currentGame.EndGame();
                }
                else await client.AnswerCallbackQueryAsync(callback.Id, "You can't do this");
            }
        }

        public async void HandleCallbackAsync(CallbackQuery callback, BotUser user)
        {
            if (user != null)
            {
                if (callback.Data == "exit" && user.currentGame != null)
                {
                    user.step = 0;
                    await client.SendTextMessageAsync(user.currentGame.GetOpponent(user).chatId, "Your opponent was left");
                    await client.SendTextMessageAsync(user.currentGame.GetOpponent(user).chatId, BotResponses.GetHelpMessage());
                    user.currentGame.EndGame();
                    await client.SendTextMessageAsync(user.chatId, BotResponses.GetHelpMessage());
                }
                else if (callback.Data == "exit" && user.currentGame == null)
                {
                    user.step = 0;
                    await client.SendTextMessageAsync(user.chatId, BotResponses.GetHelpMessage());
                }
                else if (callback.Data == "accept" && user.currentGame != null)
                {
                     Message receiverMsg = await client.SendTextMessageAsync(user.chatId, "Game started\nFirst turn " + user.currentGame.GetOpponent(user).username,
                         default, default, default, default, BotResponses.GetGameKeyboard());

                     Message initiatorMsg = await client.SendTextMessageAsync(
                         user.currentGame.GetOpponent(user).chatId, user.username + " accepted game, game started\nFirst turn " + user.currentGame.GetOpponent(user).username,
                         default, default, default, default, BotResponses.GetGameKeyboard());

                     user.gameMessageId = receiverMsg.MessageId;
                     user.currentGame.GetOpponent(user).gameMessageId = initiatorMsg.MessageId;
                     user.currentGame.Start();
                }
                else if (callback.Data == "cancel" && user.currentGame != null)
                {
                     user.currentGame.EndGame();
                     await client.SendTextMessageAsync(user.currentGame.GetOpponent(user).chatId, "User cancel game :(");
                     await client.SendTextMessageAsync(user.currentGame.GetOpponent(user).chatId, BotResponses.GetHelpMessage());
                     await client.SendTextMessageAsync(user.chatId, "You canceled game");
                }
                else HandleGameCallbackAsync(callback, user);
            }
        }


        public async void ExecuteCommandAsync(Message msg, BotUser user)
        {
            if (user != null)
            {
                if (msg.Type == Telegram.Bot.Types.Enums.MessageType.Text)
                {
                    try
                    {
                        switch (user.step)
                        {
                            case -1:
                                break;
                            case 0:
                                if (msg.Text == "/start")
                                {
                                    await client.SendTextMessageAsync(msg.Chat.Id, BotResponses.GetStartMessage(), Telegram.Bot.Types.Enums.ParseMode.Html);
                                    await client.SendTextMessageAsync(msg.Chat.Id, BotResponses.GetHelpMessage());
                                }
                                else if (msg.Text == "/help")
                                {
                                    await client.SendTextMessageAsync(msg.Chat.Id, BotResponses.GetHelpMessage());
                                }
                                else if (msg.Text == "/newgame")
                                {
                                    if (user.username != null)
                                    {
                                        await client.SendTextMessageAsync(msg.Chat.Id, BotResponses.GetNewGameMessage(),
                                            default, default, default, default, BotResponses.GetExitKeyboard());
                                        user.step++;
                                        user.targetCommand = msg.Text;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(msg.Chat.Id, BotResponses.GetUsernameErrorText());
                                    }
                                }
                                break;
                            case 1:
                                if (user.targetCommand == "/newgame")
                                {
                                    BotUser target = users.Find(x => x.username == msg.Text);
                                    if (target != null)
                                    {
                                        if (target.currentGame == null)
                                        {
                                            await client.SendTextMessageAsync(target.chatId, msg.From.Username + " invites you to play a game",
                                                default, default, default, default, BotResponses.GetAcceptOrCancelKeyboard());
                                            await client.SendTextMessageAsync(msg.Chat.Id, "Invite sended", default, default, default, default, BotResponses.GetExitKeyboard());
                                            user.currentGame = new Game(user, target);
                                            target.currentGame = user.currentGame;
                                            user.step++;
                                        }
                                        else await client.SendTextMessageAsync(msg.Chat.Id, BotResponses.GetUsersIsBusyMessage(),
                                            default, default, default, default, BotResponses.GetExitKeyboard());
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(msg.Chat.Id, BotResponses.GetUserNotExistMessage(),
                                            default, default, default, default, BotResponses.GetExitKeyboard());
                                    }
                                }
                                break;
                            case 2:
                                if (user.targetCommand == "/newgame")
                                {
                                    await client.SendTextMessageAsync(msg.Chat.Id, BotResponses.GetWaitMessage(),
                                        default, default, default, default, BotResponses.GetExitKeyboard());
                                }
                                break;
                            default:
                                await client.SendTextMessageAsync(msg.Chat.Id, "Undefined command");
                                break;
                        };
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                else
                {
                    await client.SendTextMessageAsync(msg.Chat.Id, "Undefined command");
                }
            }
        }
    }
}
