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
using System.IO.Ports;

namespace SMSSender.ConcreteSmsEngines
{
    public static class ConcreteEngineFactory
    {
        public static ISmsEngine CreateEngine(
            string concreteEngineClassName,
            string name,
            string port,
            int baudrate,
            Parity parity,
            int dataBits,
            StopBits stopBits)
        {
            switch (concreteEngineClassName)
            {
                case "NokiaStyleSmsEngine":
                    return new NokiaStyleSmsEngine(
                        name,
                        port,
                        baudrate,
                        parity,
                        dataBits,
                        stopBits);
                default:
                    throw new NotSupportedException("Unsupported concrete sms engine: " + concreteEngineClassName);
            }
        }
    }
}
