using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SampleClient
{
    public class Logger
    {

        public void Info(string info)
        {
            UnityEngine.Debug.Log(info);

        }

        public void Debug(string info)
        {
            UnityEngine.Debug.Log(info);
        }


        public void Error(string info)
        {
            UnityEngine.Debug.LogError(info);
        }

        public void Error(Exception e)
        {

            UnityEngine.Debug.LogError(e.ToString());
        }

        public void Warn(string info)
        {
            UnityEngine.Debug.LogError(info);
        }
    }
}
