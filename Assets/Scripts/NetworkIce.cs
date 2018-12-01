﻿using FootStone.GrainInterfaces;
using Ice;
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

        private List<Action>     actions = new List<Action>();
        private Ice.Communicator communicator;

        public SessionPrx SessionPrx { get; private set; }
        public AccountPrx accountPrx { get; private set; }

        public PlayerPrx PlayerPrx {  get ;  set; }


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

        public Communicator Communicator
        {
            get
            {
                return communicator;
            }

            set
            {
                communicator = value;
            }
        }

        public string PlayerId { get; internal set; }

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

        public async void Init(string IP,int port)
        {
            try
            {
                Ice.InitializationData initData = new Ice.InitializationData();

                initData.properties = Ice.Util.createProperties();
                //initData.properties.setProperty("Ice.ACM.Client.Heartbeat", "Always");
                //initData.properties.setProperty("Ice.RetryIntervals", "-1");
                initData.properties.setProperty("Ice.FactoryAssemblies", "client");
                initData.properties.setProperty("Ice.Trace.Network", "0");
                initData.properties.setProperty("Ice.Default.Timeout", "15");
                //    initData.properties.setProperty("SessionFactory.Proxy", "SessionFactory:default -h "+ IP + " -p " + port +" -t 10000");
                initData.properties.setProperty("Ice.Default.Locator", "FootStone/Locator:default -h " + IP + " -p " + port);
                 
                initData.dispatcher = delegate (System.Action action, Ice.Connection connection)
                {
                    lock (this)
                    {
                        actions.Add(action);
                    }                
                };

                communicator = Ice.Util.initialize(initData);

                SessionPrx = SessionPrxHelper.uncheckedCast(communicator.stringToProxy("session"));

                var adapter = communicator.createObjectAdapter("");

                //
                // Register the callback receiver servant with the object adapter
                //
                var proxy = SessionPushPrxHelper.uncheckedCast(adapter.addWithUUID(new SessionPushI()));

                //
                // Associate the object adapter with the bidirectional connection.
                //
                (await SessionPrx.ice_getConnectionAsync()).setAdapter(adapter);

                //
                // Provide the proxy of the callback receiver object to the server and wait for
                // shutdown.
                //
                await SessionPrx.AddPushAsync(proxy);

                Thread thread = new Thread(new ThreadStart(() =>
                {
                    Communicator.waitForShutdown();
                }));
                thread.Start();
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
            }
        }
             

    }

    internal class SessionPushI : SessionPushDisp_
    {
        public override void SessionDestroyed(Current current = null)
        {
            throw new NotImplementedException();
        }
    }
}
