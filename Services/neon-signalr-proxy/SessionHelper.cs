using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Neon.Common;
using Neon.Cryptography;
using Neon.Diagnostics;
using System;
using System.Linq;

namespace NeonSignalRProxy
{
    /// <summary>
    /// Helper class for getting <see cref="Session"/> info.
    /// </summary>
    public class SessionHelper
    {
        private AesCipher              cipher;
        private ILogger<SessionHelper> logger;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="cipher"></param>
        /// <param name="loggerFactory"></param>
        public SessionHelper(
            AesCipher cipher,
            ILoggerFactory loggerFactory = null)
        {
            this.cipher = cipher;
            this.logger = loggerFactory?.CreateLogger<SessionHelper>();
        }

        /// <summary>
        /// Gets the SignalR session from the <see cref="HttpContext"/>.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public Session GetSession(HttpContext context)
        {
            if (context.Request.Cookies.TryGetValue(GetCookieName(context), out var cookieString))
            {
                try
                {
                    var decrypted = cipher.DecryptStringFrom(cookieString);
                    var session   = NeonHelper.JsonDeserialize<Session>(decrypted);

                    return session;
                }
                catch (Exception e)
                {
                    logger?.LogErrorEx(() => "Error getting existing session");
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the correct cookie name for a given host.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public string GetCookieName(HttpContext context)
        {
            return $"{Service.SessionCookiPrefix}.{CryptoHelper.ComputeMD5String(context.Request.Host.Host)}";
        }
    }
}
