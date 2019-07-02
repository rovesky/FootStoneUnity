using System;
using System.Collections.Generic;
using System.Text;
using Ice;


namespace FootStone.Core.Client
{
    public class NLoggerI : Ice.Logger
    {
       // private NLog.Logger logger;

        public string Prefix { get; set; }


        public NLoggerI()
        {
       
        }

        public void print(string message)
        {
            SampleClient.Logger.Instance.Info(message);
        }

        public void trace(string category, string message)
        {
            SampleClient.Logger.Instance.Info(message);
        }

        public void warning(string message)
        {
            SampleClient.Logger.Instance.Warn(message);
        }

        public void error(string message)
        {
            SampleClient.Logger.Instance.Error(message);
        }

        public string getPrefix()
        {
            return Prefix;

        }
        public Ice.Logger cloneWithPrefix(string prefix)
        {

            return null;
        }
    }
}
