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
using System.Threading;
using System.ComponentModel;
using System.IO.Ports;
using System.Text;
using System.Runtime.CompilerServices;
using Ninject;
using SMSSender.Entities;
using System.Diagnostics;

namespace SMSSender.ConcreteSmsEngines
{
    internal class NokiaStyleSmsEngine : ISmsEngine
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private const int maxWait = 10000;
        private SerialPort port = null;
        private string portname;
        private int baudrate;
        private Parity parity;
        private int databits;
        private StopBits stopbits;
        private const int maxSmsLen = 160;

        private bool responseReceived;
        private string response;

        public NokiaStyleSmsEngine(string name, string portname, int baudrate, Parity parity, int databits, StopBits stopbits)
        {
            this.Name = name;
            this.portname = portname;
            this.baudrate = baudrate;
            this.parity = parity;
            this.databits = databits;
            this.stopbits = stopbits;
        }

        public void Send(string phoneNumber, string message)
        {
            try
            {
                port = new SerialPort(portname, baudrate, parity, databits, stopbits);
                port.DataReceived += new SerialDataReceivedEventHandler(port_DataReceived);
                port.DtrEnable = true;
                port.RtsEnable = true;
                port.ReadTimeout = 1000;
                port.Handshake = Handshake.RequestToSend;
                port.NewLine = "\r\n";

                log.Debug(string.Format("Opening COM Port {0}", portname));
                port.Open();
                log.Debug(string.Format("COM Port {0} open", portname));

                //SendCommand("AT", "OK");
                log.Debug("Sending command AT+CMGF=1");
                SendCommand("AT+CMGF=1", "OK");
                log.Debug("OK");

                if (message.Length > maxSmsLen)
                    message = message.Substring(0, maxSmsLen);

                log.Debug("Sending command AT+CMGS");
                SendCommand("AT+CMGS=\"" + phoneNumber + "\"", ">");
                log.Debug("OK");

                log.Debug("Sending message");
                SendCommand(message + (char)(26), "OK");
                log.Debug("OK");
            }
            finally
            {
                if (port != null)
                {
                    if (port.IsOpen)
                    {
                        log.Debug(string.Format("Closing COM Port {0}", portname));
                        port.Close();
                        log.Debug(string.Format("COM Port {0} closed", portname));
                    }
                    port = null;
                }
            }

            Thread.Sleep(4000); //puff puff... take some rest, otherwise something might go wrong.
        }
        
        private void SendCommand(string command, string responseToWaitFor)
        {
            UnsetReceived();
            port.DiscardOutBuffer();
            port.WriteLine(command);

            if (!string.IsNullOrEmpty(responseToWaitFor))
            {
                var start = DateTime.Now;

                //wait until expected message arrives, or maxWait seconds anyway
                while (DateTime.Now.Subtract(start).TotalMilliseconds < maxWait)
                {
                    if (responseReceived)
                    {
                        if (response.Contains(responseToWaitFor))
                            break;
                        else
                            UnsetReceived();
                    }
                    Thread.Sleep(200);
                }

                if (!responseReceived)
                    throw new SystemException("Protocol error");

                UnsetReceived();
            }
        }

        void port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            string r = port.ReadExisting().Trim();
            SetReceived(r);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void UnsetReceived()
        {
            responseReceived = false;
            response = null;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void SetReceived(string response)
        {
            this.response = response;
            responseReceived = true;
        }

        public string Name { get; set; }

        #region IDisposable Members

        public void Dispose()
        {
            if ((port != null) && (port.IsOpen))
            {
                port.Close();
                port = null;
            }
        }

        #endregion
    }
}