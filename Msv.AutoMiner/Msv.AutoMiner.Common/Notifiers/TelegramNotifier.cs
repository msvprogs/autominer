using System;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Msv.AutoMiner.Common.Notifiers
{
    public class TelegramNotifier : INotifier
    {
        private readonly ITelegramBotClient m_Client;
        private readonly ITelegramNotifierStorage m_Storage;
        private readonly string[] m_UserWhiteList;

        public TelegramNotifier(ITelegramBotClient client, ITelegramNotifierStorage storage, string[] userWhiteList)
        {
            m_Client = client ?? throw new ArgumentNullException(nameof(client));
            m_Storage = storage ?? throw new ArgumentNullException(nameof(storage));
            m_UserWhiteList = userWhiteList ?? throw new ArgumentNullException(nameof(userWhiteList));
        }

        public void SendMessage(string message) 
            => SendMessageToSubscribers(message);

        private void SendMessageToSubscribers(string message) 
            => Task.WaitAll(m_Storage.GetReceiverIds(m_UserWhiteList)
                .Select(x => m_Client.SendTextMessageAsync(new ChatId(x), message, ParseMode.Html))
                .Cast<Task>()
                .ToArray());
    }
}
