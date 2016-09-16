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
using NHibernate.Criterion;

namespace SMSSender.EQO
{
    public class GetSmsState
    {
        public SmsState GetStateById(string id)
        {
            Sms sms = null;

            using (var s = NHHelper.OpenSession())
            {
                using (var tx = s.BeginTransaction())
                {
                    sms = s.CreateCriteria(typeof(Sms))
                        .Add(Expression.IdEq(id))
                        .UniqueResult<Sms>();
                    tx.Commit();
                }

                if (sms == null)
                    return SmsState.Unknown;

                return sms.State;
            }
        }

        public Dictionary<string, SmsState> GetStateById(string[] ids)
        {
            var ids_int = ids.Select(x => Convert.ToInt32(x)).ToArray();
            IList<Sms> sms_list = null;

            using (var s = NHHelper.OpenSession())
            {
                using (var tx = s.BeginTransaction())
                {
                    sms_list = s.CreateCriteria(typeof(Sms))
                        .Add(Expression.In("Id", ids_int))
                        .List<Sms>();

                    tx.Commit();
                }
            }

            return sms_list.ToDictionary(x => x.Id.ToString(), x => x.State);
        }
    }
}
