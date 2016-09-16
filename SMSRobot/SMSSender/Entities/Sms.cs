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

namespace SMSSender.Entities
{
    public class Sms : AbstractEntity, ISms
    {
        #region ISms Members

        virtual public string Message { get; set; }
        virtual public string PhoneNumber { get; set; }
        virtual public DateTime EnqueueTime { get; set; }
        virtual public DateTime? SendTime { get; set; }
        virtual public DateTime? SentToDeviceAt { get; set; }
        virtual public string SubmittingUsername { get; set; }
        virtual public string SentByDeviceId { get; set; }
        virtual public SmsState State
        {
            get
            {
                if (!SentToDeviceAt.HasValue)
                    return SmsState.Enqueued;

                if ((SentToDeviceAt.HasValue) && (!SendTime.HasValue))
                    return SmsState.Sending;

                if (SendTime.HasValue)
                    return SmsState.Sent;

                return SmsState.Unknown;
            }
        }

        #endregion
    }
}
