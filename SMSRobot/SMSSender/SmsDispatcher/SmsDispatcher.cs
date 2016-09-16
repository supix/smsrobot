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
using System.Threading;
using SMSSender.Entities;
using NHibernate.Criterion;
using System.Diagnostics;

namespace SMSSender.SmsDispatcher
{
    public class SmsDispatcher : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        Random rand;
        IEnumerable<IAsyncSmsEngine> engines;
        Thread thread;
        int interval;
        bool terminated;
        DateTime? lastCycled;
        readonly EventLog eventLog;

        public SmsDispatcher(IEnumerable<IAsyncSmsEngine> engines, int interval)
        {
            this.engines = engines;
            this.interval = interval;

            foreach (var e in engines)
            {
                e.Sent += new SendNotification(e_Sent);
                e.SendError += new SendNotification(e_SendError);
            }

            lastCycled = null;
            rand = new Random();
        }

        void e_SendError(IAsyncSmsEngine sender, ISms sms)
        {
            using (var session = NHHelper.OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    sms.SentToDeviceAt = null;
                    session.Update(sms);
                    tx.Commit();
                }
            }
        }

        void e_Sent(IAsyncSmsEngine sender, ISms sms)
        {
            using (var session = NHHelper.OpenSession())
            {
                using (var tx = session.BeginTransaction())
                {
                    sms.SendTime = DateTime.Now;
                    sms.SentByDeviceId = sender.Name;
                    session.Update(sms);
                    tx.Commit();
                }
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (Active)
                Stop();

            foreach (var e in engines)
                e.Dispose();
        }

        #endregion

        public void Start()
        {
            if (thread == null)
            {
                terminated = false;
                thread = new Thread(doYourJob);
                thread.Start();
            }
        }

        public void Stop()
        {
            if (thread != null)
            {
                terminated = true;
                Thread.Sleep(2000);                
                thread.Abort();
                thread = null;
            }
        }

        public DateTime? LastCycled
        {
            get { return lastCycled; }
        }

        public bool Active
        {
            get
            {
                return thread != null;
            }
            set
            {
                if (value)
                    Start();
                else
                    Stop();
            }
        }

        void doYourJob()
        {
            if (engines.Count() == 0)
                Stop();

            //Mutex mut = new Mutex(false, "SMSSenderDbKey");

            try
            {
                while (!terminated)
                {
                    IList<Sms> sms_list = null;
                    IList<IAsyncSmsEngine> freeEngines = null;

                    lastCycled = DateTime.Now;

                    using (var session = NHHelper.OpenSession())
                    {
                        using (var tx = session.BeginTransaction())
                        {
                            sms_list = session.CreateCriteria(typeof(Sms))
                                .Add(
                                    Restrictions.Or(
                                        Restrictions.IsNull("SentToDeviceAt"),
                                        Restrictions.Le("SentToDeviceAt", DateTime.Now.AddMinutes(-1))))
                                .Add(Restrictions.IsNull("SendTime"))
                                .AddOrder(new Order("EnqueueTime", true))
                                .List<Sms>();
                        }
                    }

                    if (sms_list.Count > 0)
                        log.Debug(string.Format("{0} SMS(s) fetched", sms_list.Count));

                    int i = 0;
                    while ((sms_list != null) && (i < sms_list.Count))
                    {
                        if (terminated)
                            break;

                        freeEngines = engines.Where(x => !x.Busy).ToList();

                        if (freeEngines.Count > 0)
                        {
                            //randomize freeEngine list to increase robustness
                            freeEngines = randomizeList(freeEngines);

                            List<Sms> toBeUpdated = new List<Sms>();
                            for (int j = 0; j < freeEngines.Count && i < sms_list.Count; j++)
                            {
                                var sms = sms_list[i++];
                                sms.SentToDeviceAt = DateTime.Now;
                                sms.SentByDeviceId = freeEngines[j].Name;
                                toBeUpdated.Add(sms);
                                freeEngines[j].Send(sms);
                                log.Debug(string.Format("SMS {0} sent to engine {1}", sms.Id, sms.SentByDeviceId));
                            }

                            if (toBeUpdated.Count > 0)
                            {
                                using (var session = NHHelper.OpenSession())
                                {
                                    using (var tx = session.BeginTransaction())
                                    {
                                        foreach (var sms in toBeUpdated)
                                        {
                                            session.Update(sms);
                                        }

                                        tx.Commit();
                                    }
                                }
                            }
                        }
                        else //all engines are busy
                        {
                            if (!terminated)
                                Thread.Sleep(500);
                        }
                    }

                    if (!terminated)
                    {
                        if ((sms_list == null) || (sms_list.Count == 0)) //there are no pending sms
                            if (interval > 0)
                            {
                                DateTime waitUntil = DateTime.Now.AddMilliseconds(interval);
                                while ((!terminated) && (DateTime.Now < waitUntil))
                                {
                                    Thread.Sleep(1000);
                                }
                            }
                            else
                                Stop();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("ERROR", ex);
                Thread.Sleep(30000);
                thread = null;
                Start();
                Thread.CurrentThread.Abort();
            }
        }

        private IList<IAsyncSmsEngine> randomizeList(IList<IAsyncSmsEngine> freeEngines)
        {
            List<IAsyncSmsEngine> newList = new List<IAsyncSmsEngine>();

            while (freeEngines.Count > 0)
            {
                int r = rand.Next(freeEngines.Count);
                newList.Add(freeEngines[r]);
                freeEngines.RemoveAt(r);
            }

            return newList;
        }

        #region IDisposable Members

        void IDisposable.Dispose()
        {
            Stop();
            foreach (var e in engines)
                e.Dispose();
            eventLog.Close();
        }

        #endregion
    }
}
