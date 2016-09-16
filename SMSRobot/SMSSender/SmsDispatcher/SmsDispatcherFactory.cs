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
using System.Xml;
using SMSSender.ConcreteSmsEngines;
using System.IO.Ports;
using System.IO;

namespace SMSSender.SmsDispatcher
{
    public static class SmsDispatcherFactory
    {
        public static SmsDispatcher CreateInstance()
        {

            List<IAsyncSmsEngine> list = new List<IAsyncSmsEngine>();
            XmlDocument xml = new XmlDocument();
            string filename = Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "SmsEngines.xml");
            xml.Load(filename);
            //XmlNode enginesNode = xml.GetElementsByTagName("Engines")[0];
            XmlNodeList engines = xml.GetElementsByTagName("Engine");
            foreach (XmlElement node in engines)
            {
                var name = node.Attributes["Name"].Value;
                var concreteEngineClassName = node.GetElementsByTagName("ConcreteEngineClassName")[0].InnerText;
                var port = node.GetElementsByTagName("Port")[0].InnerText;
                var baudrate = node.GetElementsByTagName("Baudrate")[0].InnerText;
                var parity = node.GetElementsByTagName("Parity")[0].InnerText;
                var dataBits = node.GetElementsByTagName("DataBits")[0].InnerText;
                var stopBits = node.GetElementsByTagName("StopBits")[0].InnerText;

                list.Add(new AsyncSmsEngine(
                    ConcreteEngineFactory.CreateEngine(
                    concreteEngineClassName,
                    name,
                    port,
                    Convert.ToInt32(baudrate),
                    (Parity)Enum.Parse(typeof(Parity), parity, true),
                    Convert.ToInt32(dataBits),
                    (StopBits)Enum.Parse(typeof(StopBits), stopBits, true))));
            }

            return new SmsDispatcher(list, 10000);
        }
    }
}
