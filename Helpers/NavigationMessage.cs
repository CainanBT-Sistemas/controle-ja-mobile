using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace controle_ja_mobile.Helpers
{
    public class NavigationMessage : ValueChangedMessage<string>
    {
        public NavigationMessage(string targetPage) : base(targetPage)
        {
        }
    }
}
