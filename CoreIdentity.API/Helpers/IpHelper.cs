using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace CoreIdentity.API.Helpers
{
    public class IpHelper
    {
        public static string GetServerIpAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return string.Empty;
        }

        public static string GetUserIPAddress(IHttpContextAccessor _httpContextAccessor)
        {
            string ipAddress = _httpContextAccessor.HttpContext?.Request?.Headers?["X-Forwarded-For"].ToString();
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.MapToIPv4().ToString();
            }

            return ipAddress;
        }

        public static string GetUserIPAddress(HttpContext _httpContext)
        {
            string ipAddress = _httpContext?.Request?.Headers?["X-Forwarded-For"].ToString();
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                ipAddress = _httpContext?.Connection?.RemoteIpAddress?.MapToIPv4().ToString();
            }

            return ipAddress;
        }
    }
}
