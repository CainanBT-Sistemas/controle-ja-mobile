using CommunityToolkit.Mvvm.Messaging.Messages;

namespace controle_ja_mobile.Helpers
{
    public class ItemSelectedMessage : ValueChangedMessage<object>
    {
        public ItemSelectedMessage(object value) : base(value) { }
    }
}