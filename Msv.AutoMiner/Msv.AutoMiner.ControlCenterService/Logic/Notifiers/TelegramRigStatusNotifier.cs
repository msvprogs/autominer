﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Msv.AutoMiner.ControlCenterService.Logic.Storage.Contracts;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Msv.AutoMiner.ControlCenterService.Logic.Notifiers
{
    public class TelegramRigStatusNotifier : IRigStatusNotifier
    {
        private readonly ITelegramBotClient m_Client;
        private readonly IRigStatusNotifierStorage m_Storage;

        public TelegramRigStatusNotifier(ITelegramBotClient client, IRigStatusNotifierStorage storage)
        {
            m_Client = client ?? throw new ArgumentNullException(nameof(client));
            m_Storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public void NotifyLowVideoUsage(int rigId, int[] samples) 
            => SendMessageToSubscribers(CreateMessage(rigId, "Some video adapters have very low usage", samples, "%"));

        public void NotifyHighVideoTemperature(int rigId, int[] samples)
            => SendMessageToSubscribers(CreateMessage(rigId, "Some video adapters are overheated", samples, "°C"));

        public void NotifyHighInvalidShareRate(int rigId, int[] samples)
            => SendMessageToSubscribers(CreateMessage(rigId, "There are too many invalid shares", samples, "%"));

        public void NotifyUnusualHashRate(int rigId, int[] samples)
            => SendMessageToSubscribers(CreateMessage(rigId, "Current hashrate differs too much from reference one", samples, "%"));

        private void SendMessageToSubscribers(string message) 
            => Task.WaitAll(m_Storage.GetReceiverIds()
                .Select(x => m_Client.SendTextMessageAsync(new ChatId(x), message, ParseMode.Html))
                .Cast<Task>()
                .ToArray());

        private string CreateMessage(int rigId, string problem, IReadOnlyCollection<int> values, string valuePostfix)
        {
            var rig = m_Storage.GetRig(rigId);
            //language=html
            const string messageFormat = @"<b>Warning!</b>
Your rig '{0}' is experiencing the following problem:
<i>{1}</i>
Last {2} measured values: {3}";
            return string.Format(messageFormat, rig.Name, problem, values.Count, string.Join(", ", values.Select(x => x + valuePostfix)));
        }
    }
}
