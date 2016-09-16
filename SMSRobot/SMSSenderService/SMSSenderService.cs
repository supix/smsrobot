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
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using SMSSender;
using SMSSender.SmsDispatcher;

namespace SMSSenderService
{
    public partial class SMSSenderService : ServiceBase
    {
        private SmsDispatcher dispatcher = null;

        public SMSSenderService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();

            if (dispatcher == null)
                NHHelper.Configure(false);
            else
                dispatcher.Dispose();

            dispatcher = SmsDispatcherFactory.CreateInstance();
            dispatcher.Start();
        }

        protected override void OnStop()
        {
            if (dispatcher != null)
            {
                dispatcher.Stop();
                dispatcher.Dispose();
                dispatcher = null;
            }
        }
    }
}
