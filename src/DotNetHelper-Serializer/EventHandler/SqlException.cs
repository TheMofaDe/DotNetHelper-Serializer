﻿using System;

namespace DotNetHelper_Serializer.EventHandler
{

    public class SqlExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; }
        public string Sql { get; } = "";

        public SqlExceptionEventArgs(Exception error,string sql)
        {
            Exception = error;
            Sql = sql;
        }
        public SqlExceptionEventArgs(Exception error)
        {
            Exception = error;
        }
    }
}
