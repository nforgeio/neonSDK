//-----------------------------------------------------------------------------
// FILE:	    NotifyClient.cs
// CONTRIBUTOR: Jeff Lill
// COPYRIGHT:	Copyright © 2005-2023 by NEONFORGE LLC.  All rights reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mail;
using System.Net.Mime;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Neon.Common;

namespace Neon.Deployment
{
    /// <summary>
    /// <para>
    /// Implements notification operations like sending an email or a Microsoft Teams message.
    /// </para>
    /// <note>
    /// These notifications will be sent from the <b>devbot@neonforge.com</b> user because
    /// that user doesn't enable multi-factor authentication (MFA) whereas our developer
    /// Office accounts do enable MFA.  MFA prevents basic authentication from working.
    /// </note>
    /// </summary>
    public class NotifyClient
    {
        private const string devbotVault = "user-devbot";

        private string      username;
        private string      password;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="username">Optionally specifies the Office 365 username (like: "sally@neonforge.com").</param>
        /// <param name="password">Optionally specifies the password.</param>
        /// <remarks>
        /// This constructor obtains these values from <b>neon-assistant</b> from the 
        /// <b>devbot@neonforge.com</b> user's <b>NEONFORGE_LOGIN</b> secret when not 
        /// specified explicitly.
        /// </remarks>
        public NotifyClient(string username = null, string password = null)
        {
            var profileClient = new MaintainerProfile();

            if (!string.IsNullOrEmpty(username))
            {
                this.username = username;
            }
            else
            {
                this.username = profileClient.GetSecretValue("NEONFORGE_LOGIN[username]", vault: devbotVault);
            }

            if (!string.IsNullOrEmpty(password))
            {
                this.password = password;
            }
            else
            {
                this.password = profileClient.GetSecretValue("NEONFORGE_LOGIN[password]", vault: devbotVault);
            }
        }

        /// <summary>
        /// Sends an email via the <b>devbot@neonforge.com</b> Office 356 account.
        /// </summary>
        /// <param name="to">Specifies the target email addresses separated with commas.</param>
        /// <param name="subject">Specifies the subject line.</param>
        /// <param name="body">Optionally specifies the message body text.</param>
        /// <param name="bodyAsHtml">Optionally indicates that the body text is HTML.</param>
        /// <param name="cc">Optionally specifies the target CC (carbon copy) addresses separated with commas.</param>
        /// <param name="bcc">Optionally specifies the target BCC (blind carbon copy) addresses separated with commas.</param>
        /// <param name="attachmentPaths">Specifies the file paths to any attachments to be included.</param>
        public void SendMail(string to, string subject, string body = null, bool bodyAsHtml = false, string cc = null, string bcc = null, IEnumerable<string> attachmentPaths = null)
        {
            Covenant.Requires<ArgumentNullException>(!string.IsNullOrEmpty(to), nameof(to));

            subject ??= string.Empty;
            body    ??= string.Empty;

            var message = new MailMessage()
            {
                Subject    = subject,
                Body       = body,
                IsBodyHtml = bodyAsHtml
            };

            message.From = new MailAddress(username);
            message.To.Add(to);

            if (!string.IsNullOrEmpty(cc))
            {
                message.CC.Add(cc);
            }

            if (!string.IsNullOrEmpty(bcc))
            {
                message.Bcc.Add(bcc);
            }

            var attachments = new List<Attachment>();

            if (attachmentPaths != null && attachmentPaths.Count() > 0)
            {
                foreach (var attachmentPath in attachmentPaths)
                {
                    var stream      = File.OpenRead(attachmentPath);
                    var contentType = new ContentType() { Name = Path.GetFileName(attachmentPath) };
                    var attachment  = new Attachment(stream, contentType);

                    attachments.Add(attachment);
                    message.Attachments.Add(attachment);
                }
            }

            try
            {
                using (var smtp = new SmtpClient("smtp.office365.com", 587) { Credentials = new NetworkCredential(username, password), EnableSsl = true})
                {
                    smtp.Credentials           = new NetworkCredential(username, password);
                    smtp.UseDefaultCredentials = false;
                    smtp.EnableSsl             = true;
                    smtp.DeliveryMethod        = SmtpDeliveryMethod.Network;

                   smtp.Send(message);
                }
            }
            finally
            {
                // We need to dispose any attachments we created.

                foreach (var attachment in attachments)
                {
                    attachment.Dispose();
                }
            }
        }

        /// <summary>
        /// Sends a message to a Office 365 Teams channel.
        /// </summary>
        /// <param name="channelUri">Specifies the target Teams channel URI.</param>
        /// <param name="cardJson">
        /// <para>
        /// Specifies the message as the legancy <b>MessageCard format:</b>
        /// </para>
        /// <para>
        /// https://learn.microsoft.com/en-us/outlook/actionable-messages/message-card-reference<br/>
        /// https://messagecardplayground.azurewebsites.net/
        /// </para>
        /// <note>
        /// Adaptive Cards are not supported by the Teams Connector at this time.
        /// </note>
        /// </param>
        public void SendTeamsMessage(string channelUri, string cardJson)
        {
            using (var httpClient = new HttpClient())
            {
                var request = new HttpRequestMessage(HttpMethod.Post, channelUri);
                var content = new StringContent(cardJson, Encoding.UTF8);

                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                request.Content = content;

                var response = httpClient.SendAsync(request).Result;

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new DeploymentException($"Teams Message Error: {response.Content.ReadAsStringAsync().Result}");
                }
            }
        }
    }
}
