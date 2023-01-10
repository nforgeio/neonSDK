using System;
using System.Collections.Generic;

using Neon.Common;
using Neon.Deployment;

namespace DeploymentTests
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            SendTeamsMessage();
        }

        private static void SendEmail()
        {
            var notifyClient = new NotifyClient();

            var attachments = new List<string>()
            {
                @"C:\Temp\hello-world.txt"
            };

            notifyClient.SendMail(
                to:              "jeff@lill.io",
                subject:         "HELLO WORLD!",
                attachmentPaths: attachments,
                bodyAsHtml:      true,
                body:
@"
<p>
Hello Jeff,
</p>
<p>
This is from the NotifyClient.  <b>We hope you like it!</b>
</p>
--Management
");
        }

        private static void SendTeamsMessage()
        {
            var profileClient = new MaintainerProfileClient();

            var devopsChannelUri = profileClient.GetSecretValue("TEAM_DEVBOT_CHANNEL[value]", vault: "user-devbot");

            var notifyClient = new NotifyClient();
            var card =
@"
{
  ""@type"": ""MessageCard"",
  ""@context"": ""https://schema.org/extensions"",
  ""themeColor"": ""00FF00"",
  ""summary"": ""TEST MESSAGE (jeff)"",
  ""sections"": [
    {
      ""activityTitle"": ""Activity Title"",
      ""activitySubtitle"": ""Activity Subtitle""
    }
  ]
}
";

            notifyClient.SendTeamsMessage(devopsChannelUri, cardJson: card);
        }
    }
}
