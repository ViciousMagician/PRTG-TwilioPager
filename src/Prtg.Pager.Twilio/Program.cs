using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Twilio;
using Twilio.Rest.Api.V2010.Account;

namespace PRTG.SMS
{
    class Program
    {
        static void Main(string[] args)
        {
            // Find your Account SID and Auth Token at twilio.com/console
            string accountSid = ConfigurationManager.AppSettings["TwilioAccountSid"];
            string authToken = ConfigurationManager.AppSettings["TwilioAuthToken"];
            var fromNumber = ConfigurationManager.AppSettings["TwilioNumber"];
            //var to = CheckAndFormatPhone(args[0]);
            var body = string.Join(" ", args.Skip(0));

            string[] arr = ConfigurationManager.AppSettings["SMSTo"].Split(',');

            foreach (string s in arr)
            {
                var to = CheckAndFormatPhone(s);
                Console.WriteLine("From: {0}\r\nTo: {1}\r\nMessage: \"{2}\"", fromNumber ?? "NULL", to ?? "NULL", body ?? "NULL");

                if (string.IsNullOrEmpty(fromNumber) || string.IsNullOrEmpty(to) || string.IsNullOrEmpty(body) || string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken))
                {
                    Console.WriteLine("Skipping, argument missing.");
                    return;
                }


                TwilioClient.Init(accountSid, authToken);

                var message = MessageResource.Create(
                    body: body,
                    from: new Twilio.Types.PhoneNumber(fromNumber),
                    to: new Twilio.Types.PhoneNumber(to)
                );
                if (message == null)
                {
                    throw new ApplicationException("Didn't get a response from SMS provider.");
                }

                Console.WriteLine("Message sent: {0} - {1}", message.Status, message.Sid);
            }


        }
        public static string CheckAndFormatPhone(string phone)
        {
            var checkAndFormatPhoneRegex = new Regex(@"^(\+?1[ -.]?)?\(?([0-9]{3})\)?[ -.]?([0-9]{3})[ -.]?([0-9]{4})$", RegexOptions.Compiled);
            if (string.IsNullOrEmpty(phone))
            {
                throw new InvalidDataException("Phone number is not in a valid format.");
            }

            var match = checkAndFormatPhoneRegex.Match(phone);
            if (!match.Success)
            {
                throw new InvalidDataException("Phone number is not in a valid format.");
            }

            return string.Join(string.Empty, "+1", match.Groups[2], match.Groups[3], match.Groups[4]);
        }
    }
}
