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
using SMSSender.Entities;
using System.Threading;
using System.Runtime.CompilerServices;

namespace SMSSender.SmsDispatcher
{
    internal class AsyncSmsEngine : IAsyncSmsEngine
    {
        private readonly ISmsEngine smsEngine;
        private ISms sms;
        Thread thread = null;
        private bool busy;

        public AsyncSmsEngine(ISmsEngine smsEngine)
        {
            this.smsEngine = smsEngine;
            busy = false;
        }

        #region IAsyncSmsEngine Members

        virtual public string Name
        {
            get
            {
                return smsEngine.Name;
            }
            set
            {
                smsEngine.Name = value;
            }
        }

        public bool Busy { get { return busy; } }

        public event SendNotification Sent;

        public event SendNotification SendError;

        #endregion

        #region ISmsEngine Members

        [MethodImpl(MethodImplOptions.Synchronized)]
        virtual public void Send(ISms sms)
        {
            if (busy)
                throw new NotSupportedException("Cannot send: engine is busy");

            busy = true;
            this.sms = sms;
            
            thread = new Thread(doYourJob);
            thread.Start();
        }

        private void doYourJob()
        {
            busy = true;
            try
            {
                smsEngine.Send(sms.PhoneNumber, sms.Message);
                if (Sent != null)
                    Sent(this, sms);
            }
            catch { }

            SetBusyFalse();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void SetBusyFalse()
        {
            busy = false;
            thread.Abort();
        }

        #endregion

        #region IDisposable Members

        virtual public void Dispose()
        {
            smsEngine.Dispose();
            if (thread != null)
            {
                thread.Abort();
                thread = null;
            }
        }

        #endregion
    }
}
