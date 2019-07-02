using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleClient
{
    public class Logger
    {
        private Logger()
        {

        }
        private static readonly Logger _instance = new Logger();
        public static Logger Instance
        {
            get { return _instance; }
        }

        public string Text { get; set; }

        public void Info(string info)
        {
            Text += "\n" + info;
            UnityEngine.Debug.Log(info);

        }

        public void Debug(string info)
        {
            Text += "\n" + info;
            UnityEngine.Debug.Log(info);
        }


        public void Error(string info)
        {
            Text += "\n" + info;
            UnityEngine.Debug.LogError(info);            
        }

        public void Error(Exception e)
        {
            Text += "\n" + e.ToString();
            UnityEngine.Debug.LogError(e.ToString());
        }

        public void Warn(string info)
        {
            Text += "\n" + info;
            UnityEngine.Debug.LogError(info);
        }
    }
}
