using DevExpress.ExpressApp.Model;
using DevExpress.Xpo;

namespace D2NXAF.ExpressApp.SystemModule
{
    [NonPersistent]
    public class MessageBoxTextMessage
    {

        private readonly string _Message;

        [ModelDefault("Caption", " ")]
        [Size(SizeAttribute.Unlimited)]
        public string Message
        {
            get { return _Message; }
        }

        public MessageBoxTextMessage(string Message)
        {
            _Message = Message;
        }
    }
}