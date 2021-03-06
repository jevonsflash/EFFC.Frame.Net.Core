﻿using EFFC.Frame.Net.Base.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFFC.Frame.Net.Unit.DB
{
    public interface IDBAccessInfo
    {
        DBStatus CurrentStatus { get; }
        DBType MyType { get; }
        void Open(string connString,params object[] p);
        void Close();
    }
}
