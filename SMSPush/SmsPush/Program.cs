// Copyright (C) 2011-2012.
// Author: Marcello Esposito (esposito.marce@gmail.com)
//
// This file is part of SMSRobot.
//   
// SMSRobot is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//   
// SMSRobot is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//   
// You should have received a copy of the GNU General Public License
// along with SMSRobot; see the file COPYING. If not, see
// <http://www.gnu.org/licenses/> and write to esposito.marce@gmail.com.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SmsPush
{
    class Program
    {
        private const int maxSmsLen = 160;
        static int Main(string[] args)
        {
            List<Sms> smss = GetSmss(args);

            if (smss.Count > 0)
            {
                using (var client = new SMSEnqueuer.SMSEnqueuerWcfClient())
                {
                    foreach (var sms in smss)
                    {
                        client.SendSMS(sms.PhoneNumber, sms.Message);
                        Console.WriteLine("Sent: " + sms.PhoneNumber + " - " + sms.Message);
                    }
                }
            }
            else
                Console.WriteLine("No SMSs have been sent.");

            return 0;
        }

        private static List<Sms> GetSmss(string[] args)
        {
            var L = new List<Sms>();

            //Command line?
            if (args.Count() >= 2)
            {
                string phoneNumber = args[0];
                string message = args[1];

                L.Add(
                    new Sms()
                    {
                        PhoneNumber = phoneNumber,
                        Message = message
                    });
            }

            //Input file?
            if (args.Count() == 0)
            {
                while (Console.In.Peek() >= 0)
                {
                    string line = Console.ReadLine();
                    line = line.Trim();
                    if ((line.Length == 0) || (line[0] == '#') || (line[0] == '/'))
                        continue;

                    string[] v = line.Split(new char[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries);

                    if (v.Length == 2)
                    {
                        L.Add(
                            new Sms()
                            {
                                PhoneNumber = v[0],
                                Message = v[1]
                            });
                    }
                    else
                    {
                        Console.WriteLine("Skipped invalid line : " + line);
                    }
                }
            }

            string regExpPattern = "(^[+0-9]{1}[0-9]{7,14}$)";
            Regex regex = new Regex(regExpPattern);

            foreach (var sms in L)
            {
                if (!regex.IsMatch(sms.PhoneNumber))
                {
                    Console.WriteLine("Invalid phone number: " + sms.PhoneNumber);
                    sms.PhoneNumber = null;
                }

                if (sms.Message.Length > maxSmsLen)
                {
                    Console.WriteLine("Message too long: " + sms.Message.Substring(0, maxSmsLen) + "...");
                    sms.PhoneNumber = null;
                }
            }

            L = L.Where(x => x.PhoneNumber != null).ToList();

            return L;
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Usage: " + Environment.GetCommandLineArgs()[0] + " phoneNumber textMessage");
            Console.WriteLine("Usage: " + Environment.GetCommandLineArgs()[0] + " < inputText");
        }
    }
}
