using CommunityToolkit.Mvvm.Messaging.Messages;

namespace controle_ja_mobile.Helpers
{
    public class GlobalRefreshMessage : ValueChangedMessage<bool>
    {
        public GlobalRefreshMessage(bool value = true) : base(value) { }
    }
}