using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CashDiscipline.Module.ServiceReference1;

namespace CashDiscipline.Module.Logic.SqlServer
{
    public class SsisUtil
    {
        public static string GetMessageText(SsisMessage[] messages)
        {
            string messagesText = string.Empty;
            foreach (var message in messages)
            {
                if (messagesText != string.Empty)
                    messagesText += "\r\n";
                messagesText += string.Format("{0}: {1}: {2}", 
                    message.MessageType, message.MessageSourceType, message.Message);
            }
            messagesText = messagesText.Replace("\r\n\r\n", "\r\n");
            return messagesText;
        }
    }
}
