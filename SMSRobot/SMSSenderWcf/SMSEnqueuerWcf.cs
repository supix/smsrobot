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
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using SMSSender.SmsEnqueuer;
using SMSSender.Entities;
using SMSSender;
using SMSSender.EQO;

namespace SMSEnqueuerWcf
{
    // NOTE: If you change the class name "Service1" here, you must also update the reference to "Service1" in App.config.
    public class SMSEnqueuerWcf : ISMSEnqueuerWcf
    {
        SmsEnqueuer enqueuer = null;
        #region ISMSSenderWcf Members

        public void SendSMS(string phoneNumber, string message)
        {
            SendSMS2(phoneNumber, message);
        }

        public string SendSMS2(string phoneNumber, string message)
        {
            if (enqueuer == null)
            {
                NHHelper.Configure(false);
                enqueuer = new SmsEnqueuer();
            }

            return enqueuer.Enqueue(phoneNumber, message);
        }

        public SmsState GetSmsStateById(string id)
        {
            var eqo = new GetSmsState();
            return eqo.GetStateById(id);
        }

        public Dictionary<string, SmsState> GetSmsStateByIds(string[] ids)
        {
            var eqo = new GetSmsState();
            return eqo.GetStateById(ids);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (enqueuer != null)
                enqueuer.Dispose();
        }

        #endregion
    }
}
