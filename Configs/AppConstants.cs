using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace controle_ja_mobile.Configs
{
    public static class AppConstants
    {
        public static string BaseUrl => DeviceInfo.Platform == DevicePlatform.Android
        ? "http://192.168.100.246:8080/controle_ja_api/v1/"
        : "http://localhost:8080/controle_ja_api/v1/";

        public const string AuthStorageKey = "AuthToken";
        public const string UserNameStorageKey = "UserName";
    }
}
