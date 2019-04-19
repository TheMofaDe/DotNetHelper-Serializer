using System;
using System.Reflection;
using DotNetHelper_DeviceInformation;

namespace DotNetHelper_Serializer.Model
{
    [Attribute.SqlTableAttritube(TableName = "LogException")]
    public class ErrorObject
    {
        [Attribute.SqlColumnAttritube(SetPrimaryKey = true, SetAutoIncrementBy = 1)]
        public int Id { get; set; }
        [Attribute.SqlColumnAttritube(SetMaxColumnSize = 200)]
        public string ApplicationName { get; set; }
        public DateTime LogTime { get; set; } = DateTime.Now;
        public string ExceptionType { get; set; }
        [Attribute.SqlColumnAttritube(SetSyncTime = true, SetUtcDateTime = true)]
        public DateTime LogTimeUtc { get; set; } = DateTime.UtcNow;
        [Attribute.SqlColumnAttritube(SetMaxColumnSize = 200)]
        public string DeviceModel { get; set; } = DeviceInformation.Model;
        [Attribute.SqlColumnAttritube(SetMaxColumnSize = 200)]
        public string Platform { get; set; } = DeviceInformation.Platform;
        [Attribute.SqlColumnAttritube(SetMaxColumnSize = 200)]
        public string Version { get; set; } = DeviceInformation.Version;
        public string OsDescription { get; set; } = DeviceInformation.OsDescription;
        [Attribute.SqlColumnAttritube(SetMaxColumnSize = 200)]
        public string Architecture { get; set; } = DeviceInformation.Architecture;
        [Attribute.SqlColumnAttritube(SetMaxColumnSize = 200)]
        public string Manufacturer { get; set; } = DeviceInformation.Manufacturer;
        public string InnerExceptionMessage { get; set; }
        public string StackTrace { get; set; }
        public string ExactStackTrace { get; set; }
        [Attribute.SqlColumnAttritube(SetMaxColumnSize = 500)]
        public string DeviceId { get; set; }
        public string ExceptionMessage { get; set; }
        [Attribute.SqlColumnAttritube(SetMaxColumnSize = 200)]
        public string HelpLink { get; set; }


        public ErrorObject(Exception e)
        {

            ApplicationName = GetApplicationName();
            ExceptionMessage = e.Message ?? null;
            ExceptionType = e.GetType().Name ?? null;

            if (e.InnerException != null)
                InnerExceptionMessage = e.InnerException.Message ?? null;
            if (e.StackTrace != null)
                StackTrace = e.StackTrace.Trim() ?? null;
            if (e.HelpLink != null)
                HelpLink = e.HelpLink ?? null;
            //if (trace != null)
            //    ExactStackTrace  = trace;
        }


        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        /// <returns>System.String.</returns>
        private static string GetApplicationName()
        {
            try
            {
                var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                return assembly?.FullName.Split(',')[0];
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
                return null;
            }
        }



    }

    /// <summary>
    /// An Log Object Thats Used To Store Errors In Database 
    /// </summary>
    [Attribute.SqlTableAttritube(TableName = "Logs")]
    public class LogObject
    {
        [Attribute.SqlColumnAttritube(SetPrimaryKey = true, SetAutoIncrementBy = 1)]
        public int Id { get; set; }
        public string ApplicationName { get; set; } = GetApplicationName();
        public DateTime LogTime = DateTime.Now; //public DateTime LogTime { get; set; } = DateTime.Now; Doesnt work for some odd reason
        public string ExceptionType { get; set; }
        public DateTime LogTimeUtc = DateTime.Now.ToUniversalTime();
        public string Message { get; set; }
        public string DeviceModel { get; set; } = DeviceInformation.Model;
        public string Platform { get; set; } = DeviceInformation.Platform;
        public string Version { get; set; } = DeviceInformation.Version;
        public string DeviceId { get; set; } = DeviceInformation.DeviceId;


        public LogObject(string logs)
        {
            Message = logs;
        }

        /// <summary>
        /// Gets the name of the application.
        /// </summary>
        /// <returns>System.String.</returns>
        private static string GetApplicationName()
        {
            try
            {
                var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                return assembly?.FullName.Split(',')[0];
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
                return null;
            }
        }

    }
}