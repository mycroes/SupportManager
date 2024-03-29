﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SupportManager.Telegram.Infrastructure;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using User = SupportManager.Telegram.DAL.User;

namespace SupportManager.Telegram
{
    internal class SupportManagerBot
    {
        public delegate Task CommandHandler(CallbackQueryContext context);

        private readonly Dictionary<string, CommandHandler> commandHandlers;
        private readonly Action<object> log;

        private readonly Configuration configuration;
        private readonly ITelegramBotClient botClient;

        public SupportManagerBot(Configuration configuration, ITelegramBotClient botClient, Dictionary<string, CommandHandler> commandHandlers, Action<object> log)
        {
            this.configuration = configuration;
            this.botClient = botClient;
            this.commandHandlers = commandHandlers;
            this.log = log;

            botClient.StartReceiving(async (client, update, ct) =>
            {
                if (update.CallbackQuery is {} cq)
                {
                    await BotClientOnOnCallbackQuery(cq);
                }

                if (update.Message is { } msg)
                {
                    await BotClientOnOnMessage(msg);
                }

            }, (client, exception, ct) =>
            {
                log.Invoke(exception);
            });
        }

        private async Task BotClientOnOnCallbackQuery(CallbackQuery callbackQuery)
        {
            if (callbackQuery.Data == Constants.Ignore) return;

            await ProcessCommand(callbackQuery.Message, callbackQuery.Data);
        }

        private async Task ProcessCommand(Message message, string input)
        {
            using (var context = new CallbackQueryContext(configuration, botClient, message, input))
            {
                if (!commandHandlers.TryGetValue(context.Command, out var handler)) return;
                try
                {
                    await handler(context);
                }
                catch (IncompleteDataException)
                {
                }
                catch (AuthenticationRequiredException)
                {
                    try
                    {
                        await context.Interaction.Write("Please authenticate using '/apikey [API Key]' first.");
                    }
                    catch (Exception)
                    {
                        // Don't worry, be happy
                    }
                }
                catch (Exception ex)
                {
                    log($"[{nameof(ProcessCommand)}] Unhandled exception: {ex}");
                }
            }
        }

        private async Task BotClientOnOnMessage(Message message)
        {
            try
            {
                await ProcessMessage(message);
            }
            catch (IncompleteDataException)
            {
                // User is queried for data, we're doing fine
            }
            catch (Exception ex)
            {
                // Something else went wrong...
                log($"[{nameof(BotClientOnOnMessage)}] Unhandled exception: {ex}");
            }
        }

        private async Task ProcessMessage(Message message)
        {
            if (message?.Type != MessageType.Text) return;

            var words = message.Text.Split(' ');
            var fw = words[0];

            var args = words.Skip(1);
            if (fw[0] != '/') return;
            var verb = fw.Substring(1);

            switch (verb)
            {
                case "start":
                    var me = await botClient.GetMeAsync();
                    var reply = $"Hello I'm *{me.FirstName}*. Please setup your API Key by typing '/apikey [key]'.";
                    await botClient.SendTextMessageAsync(message.Chat.Id, reply, ParseMode.Markdown);
                    break;
                case "apikey":
                    var key = args.First();

                    using (var context = new CallbackQueryContext(configuration, botClient,
                        await botClient.SendTextMessageAsync(message.Chat.Id, "Trying to authenticate ..."), ""))
                    {
                        try
                        {
                            var api = await context.GetApi(key);
                            var details = await api.MyDetails();

                            var user = await context.Db.Users.Where(u => u.ChatId == message.Chat.Id)
                                .SingleOrDefaultAsync() ?? context.Db.Users.Add(new User()).Entity;

                            user.ApiKey = key;
                            user.ChatId = message.Chat.Id;
                            user.SupportManagerUserId = details.Id;

                            await context.Db.SaveChangesAsync();
                            await context.Interaction.Write($"Authenticated as *{details.DisplayName}*");
                        }
                        catch (Exception)
                        {
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Failed to authenticate.");
                        }
                    }
                    break;
                default:
                    if (commandHandlers.ContainsKey(verb))
                        await ProcessCommand(
                            await botClient.SendTextMessageAsync(message.Chat.Id, "Preparing response..."), verb);
                    break;
            }
        }
    }
}