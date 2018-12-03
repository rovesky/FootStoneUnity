using FootStone.GrainInterfaces;
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
    public class IceLogger : Ice.Logger
    {
        public Ice.Logger cloneWithPrefix(string prefix)
        {
            throw new NotImplementedException();
        }

        public void error(string message)
        {
            Debug.LogError(message);
        }

        public string getPrefix()
        {
            return "";
        }

        public void print(string message)
        {
            Debug.Log(message);
        }

        public void trace(string category, string message)
        {
            Debug.Log(message);
        }

        public void warning(string message)
        {
            Debug.Log(message);
        }
    }
    public class NetworkIce
    {
        private static NetworkIce _instance;

        /// <summary>
        /// 私有化构造函数，使得类不可通过new来创建实例
        /// </summary>
        private NetworkIce() { }

        private List<Action>     actions = new List<Action>();
        private Ice.Communicator communicator;

        public SessionFactoryPrx SessionFactoryPrx { get; private set; }
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
                initData.properties.setProperty("Ice.Trace.Network", "2");
                initData.properties.setProperty("Ice.Default.Timeout", "15");
               // initData.properties.setProperty("SessionFactory.Proxy", "SessionFactory:default -h "+ IP + " -p 12000");
                initData.properties.setProperty("Ice.Default.Locator", "FootStone/Locator:default -h " + IP + " -p " + port);

                initData.logger = new IceLogger();
                initData.dispatcher = delegate (System.Action action, Ice.Connection connection)
                {
                    lock (this)
                    {
                        actions.Add(action);
                    }
                };

                communicator = Ice.Util.initialize(initData);
                SessionFactoryPrx = SessionFactoryPrxHelper.uncheckedCast(communicator.stringToProxy("sessionFactory"));
               // SessionFactoryPrx = SessionFactoryPrxHelper.uncheckedCast(communicator.propertyToProxy("SessionFactory.Proxy"));


                Thread thread = new Thread(new ThreadStart(() =>
                {
                    Communicator.waitForShutdown();
                }));
                thread.Start();
            }
            catch (System.Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
             
        public async Task<SessionPrx> CreateSession(string name)
        {
            SessionPrx = await SessionFactoryPrx.CreateSessionAsync("name1", "");
            Connection connection = await SessionPrx.ice_getConnectionAsync();
            Console.WriteLine("session connection: ACM=" + 
                JsonUtility.ToJson(connection.getACM())
                + ",Endpoint=" + JsonUtility.ToJson(connection.getEndpoint()));

            var adapter = communicator.createObjectAdapter("");

            // Register the callback receiver servant with the object adapter               
            var proxy = SessionPushPrxHelper.uncheckedCast(adapter.addWithUUID(new SessionPushI()));
            adapter.addFacet(new PlayerPushI(), proxy.ice_getIdentity(), "playerPush");
            // Associate the object adapter with the bidirectional connection.
            connection.setAdapter(adapter);

            // Provide the proxy of the callback receiver object to the server and wait for
            // shutdown.               
            await SessionPrx.AddPushAsync(proxy);
            return SessionPrx;
        }

    }

    internal class PlayerPushI : PlayerPushDisp_
    {
        public override void hpChanged(int hp, Current current = null)
        {
            Debug.Log("hp changed:" + hp);
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
