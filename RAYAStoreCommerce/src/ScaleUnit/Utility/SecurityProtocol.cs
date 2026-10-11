namespace RR.Utility
{
    using System.Net;
    public static class SecurityProtocol
    {
        public static void TLS()
        {
            //ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls |
            // SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
        }
    }
}
