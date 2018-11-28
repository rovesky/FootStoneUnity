﻿using FootStone.GrainInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Network
{
    public class NetworkIce
    {
        private static NetworkIce _instance;

        /// <summary>
        /// 私有化构造函数，使得类不可通过new来创建实例
        /// </summary>
        private NetworkIce() { }

        private List<Action> actions = new List<Action>();
        private Ice.Communicator communicator;

        public static NetworkIce Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new NetworkIce();
                }
                return _instance;
            }
        }


        public void Update()
        {
            Action[] array;
            lock (this)
            {
                array = actions.ToArray();
                actions.Clear();
            }
            foreach (Action each in array)
            {
                each();
            }
        }

        public void Init()
        {
            try
            {
                Ice.InitializationData initData = new Ice.InitializationData();

                initData.properties = Ice.Util.createProperties();
                initData.properties.setProperty("Ice.ACM.Client.Heartbeat", "Always");
                //initData.properties.setProperty("Ice.RetryIntervals", "-1");
                initData.properties.setProperty("Ice.FactoryAssemblies", "client");
                initData.properties.setProperty("Ice.Trace.Network", "0");
                initData.properties.setProperty("SessionFactory.Proxy", "SessionFactory:default -h 192.168.206.1 -p 12000 -t 5000");

                initData.dispatcher = delegate (System.Action action, Ice.Connection connection)
                {
                    lock (this)
                    {
                        actions.Add(action);
                    }                
                };

                communicator = Ice.Util.initialize(initData);


                Thread thread = new Thread(new ThreadStart(() =>
                {
                    communicator.waitForShutdown();
                }));
                thread.Start();
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }

        public async Task<SessionPrx> createSession()
        {
            var basePrx = communicator.propertyToProxy("SessionFactory.Proxy");
            var factory = SessionFactoryPrxHelper.uncheckedCast(basePrx);
            if (factory == null)
            {
                Debug.LogError("invalid proxy");
                return null;
               
            }
            return await factory.createAsync("1");
        }

    }
}