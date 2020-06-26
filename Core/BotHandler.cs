using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using TicTacToeTG.Core.Games;
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
                if (user.currentGame is GameWithPlayer)
                {
                    GameWithPlayer game = (GameWithPlayer)user.currentGame;
                    if (result.CanMove && !result.GameFinished)
                    {
                        await client.EditMessageTextAsync(user.chatId, user.gameMessageId, "Turn: " + game.GetOpponent(user).username,
                            default, default, user.currentGame.gameKeyboard);

                        await client.EditMessageTextAsync(game.GetOpponent(user).chatId, game.GetOpponent(user).gameMessageId,
                            "Turn: " + game.GetOpponent(user).username, default, default, user.currentGame.gameKeyboard);
                    }
                    else await client.AnswerCallbackQueryAsync(callback.Id, "You can't do this");
                    if (result.Tie)
                    {
                        await client.EditMessageTextAsync(user.chatId, user.gameMessageId, "Tie!",
                            default, default, user.currentGame.gameKeyboard);
                        await client.EditMessageTextAsync(game.GetOpponent(user).chatId, game.GetOpponent(user).gameMessageId,
                            "Tie!", default, default, user.currentGame.gameKeyboard);
                        await client.SendTextMessageAsync(user.chatId, BotResponses.GetHelpMessage());
                        await client.SendTextMessageAsync(game.GetOpponent(user).chatId, BotResponses.GetHelpMessage());
                        user.currentGame.EndGame();
                    }
                    else if (result.GameFinished)
                    {
                        await client.EditMessageTextAsync(user.chatId, user.gameMessageId, user.username + " winning this game",
                            default, default, user.currentGame.gameKeyboard);

                        await client.EditMessageTextAsync(game.GetOpponent(user).chatId, game.GetOpponent(user).gameMessageId,
                            user.username + " winning this game", default, default, user.currentGame.gameKeyboard);
                        await client.SendTextMessageAsync(user.chatId, BotResponses.GetHelpMessage());
                        await client.SendTextMessageAsync(game.GetOpponent(user).chatId, BotResponses.GetHelpMessage());

                        user.currentGame.EndGame();
                    }
                }
                //if game with AI
                else
                {
                    MoveResult aiRes = new MoveResult(false, false, false, false);
                    if (result.CanMove && !result.GameFinished)
                    {
                        aiRes = (user.currentGame as TicTacToeAI).AiMove();
                        await client.EditMessageReplyMarkupAsync(user.chatId, user.gameMessageId, user.currentGame.gameKeyboard);
                    }
                    else
                    {
                        if(!result.GameFinished)
                            await client.AnswerCallbackQueryAsync(callback.Id, "You can't do this");
                    }
                    if (result.Tie || aiRes.Tie)
                    {
                        await client.EditMessageTextAsync(user.chatId, user.gameMessageId, "Tie!",
                            default, default, user.currentGame.gameKeyboard);
                        await client.SendTextMessageAsync(user.chatId, BotResponses.GetHelpMessage());
                        user.currentGame.EndGame();
                    }
                    else if (result.GameFinished || aiRes.GameFinished)
                    {
                        if (!result.BotWin && !aiRes.BotWin)
                            await client.EditMessageTextAsync(user.chatId, user.gameMessageId, user.username + " winning this game",
                                default, default, user.currentGame.gameKeyboard);
                        else await client.EditMessageTextAsync(user.chatId, user.gameMessageId, "AI winning this game",
                            default, default, user.currentGame.gameKeyboard);
                        await client.SendTextMessageAsync(user.chatId, BotResponses.GetHelpMessage());
                        user.currentGame.EndGame();
                    }
                }
            }
        }

        public async void HandleCallbackAsync(CallbackQuery callback, BotUser user)
        {
            if (user != null)
            {
                if (callback.Data == "exit" && user.currentGame != null)
                {
                    user.step = 0;
                    if (user.currentGame is GameWithPlayer)
                    {
                        GameWithPlayer game = (GameWithPlayer)user.currentGame;
                        await client.SendTextMessageAsync(game.GetOpponent(user).chatId, "Your opponent was left");
                        await client.SendTextMessageAsync(game.GetOpponent(user).chatId, BotResponses.GetHelpMessage());
                    }
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
                    GameWithPlayer game = (GameWithPlayer)user.currentGame;
                    Message receiverMsg = await client.SendTextMessageAsync(user.chatId, "Game started\nFirst turn " +game.GetOpponent(user).username,
                         default, default, default, default, user.currentGame.gameKeyboard);

                    Message initiatorMsg = await client.SendTextMessageAsync(
                         game.GetOpponent(user).chatId, user.username + " accepted game, game started\nFirst turn " + game.GetOpponent(user).username,
                         default, default, default, default, user.currentGame.gameKeyboard);

                     user.gameMessageId = receiverMsg.MessageId;
                     game.GetOpponent(user).gameMessageId = initiatorMsg.MessageId;
                     user.currentGame.Start();
                }
                else if (callback.Data == "cancel" && user.currentGame != null)
                {
                    GameWithPlayer game = (GameWithPlayer)user.currentGame;
                    user.currentGame.EndGame();
                    await client.SendTextMessageAsync(game.GetOpponent(user).chatId, "User cancel game :(");
                    await client.SendTextMessageAsync(game.GetOpponent(user).chatId, BotResponses.GetHelpMessage());
                    await client.SendTextMessageAsync(user.chatId, "You canceled game");
                    await client.SendTextMessageAsync(user.chatId, BotResponses.GetHelpMessage());
                }
                else HandleGameCallbackAsync(callback, user);
            }
        }

        private async void StartGameWithAi(int difficulty, BotUser user)
        {
            user.currentGame = new TicTacToeAI(user, difficulty);
            Message send = await client.SendTextMessageAsync(user.chatId, "Game started",
                default, default, default, default, user.currentGame.gameKeyboard);
            user.gameMessageId = send.MessageId;
            user.currentGame.Start();
            (user.currentGame as TicTacToeAI).AiMove();
            await client.EditMessageReplyMarkupAsync(user.chatId, user.gameMessageId, user.currentGame.gameKeyboard);
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
                                    await client.SendTextMessageAsync(msg.Chat.Id, BotResponses.GetNewGameMessage(),
                                            default, default, default, default, BotResponses.GetExitKeyboard());
                                    user.step++;
                                    user.targetCommand = msg.Text;
                                }
                                else
                                {
                                    await client.SendTextMessageAsync(msg.Chat.Id, BotResponses.GetHelpMessage());
                                }
                                break;
                            case 1:
                                if (user.targetCommand == "/newgame")
                                {
                                    if (msg.Text == "/player")
                                    {
                                        if (user.username != null)
                                        {
                                            await client.SendTextMessageAsync(msg.Chat.Id, BotResponses.GetNewGameMessageWithPlayer(),
                                                default, default, default, default, BotResponses.GetExitKeyboard());
                                            user.step++;
                                            user.targetCommand = msg.Text;
                                        }
                                        else
                                        {
                                            await client.SendTextMessageAsync(msg.Chat.Id, BotResponses.GetUsernameErrorText(),
                                                default, default, default, default, BotResponses.GetExitKeyboard());
                                        }
                                    }
                                    else if (msg.Text == "/ai")
                                    {
                                        await client.SendTextMessageAsync(user.chatId, BotResponses.GetAIDifficultyMessage(),
                                            default, default, default, default, BotResponses.GetExitKeyboard());
                                        user.step++;
                                        user.targetCommand = msg.Text;
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(user.chatId, BotResponses.GetUndefinedCmdMessage());
                                        await client.SendTextMessageAsync(msg.Chat.Id, BotResponses.GetNewGameMessage(),
                                            default, default, default, default, BotResponses.GetExitKeyboard());
                                    }
                                }
                                break;
                            case 2:
                                if (user.targetCommand == "/player")
                                {
                                    BotUser target = users.Find(x => x.username == msg.Text);
                                    if (target != null)
                                    {
                                        if (target.currentGame == null)
                                        {
                                            await client.SendTextMessageAsync(target.chatId, msg.From.Username + " invites you to play a game",
                                                default, default, default, default, BotResponses.GetAcceptOrCancelKeyboard());
                                            await client.SendTextMessageAsync(msg.Chat.Id, "Invite sended", default, default, default, default, BotResponses.GetExitKeyboard());
                                            user.currentGame = new GameWithPlayer(user, target);
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
                                else if (user.targetCommand == "/ai")
                                {
                                    if (msg.Text == "/easy")
                                    {
                                        StartGameWithAi(0, user);
                                    }
                                    else if (msg.Text == "/medium")
                                    {
                                        StartGameWithAi(1, user);
                                    }
                                    else if (msg.Text == "/hard")
                                    {
                                        StartGameWithAi(2, user);
                                    }
                                    else
                                    {
                                        await client.SendTextMessageAsync(user.chatId, BotResponses.GetUndefinedCmdMessage());
                                        await client.SendTextMessageAsync(user.chatId, BotResponses.GetAIDifficultyMessage(),
                                            default, default, default, default, BotResponses.GetExitKeyboard());
                                    }
                                }
                                break;
                            case 3:
                                if(user.targetCommand=="/player")
                                {
                                    await client.SendTextMessageAsync(user.chatId, "Wait for a player response",
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
