using FootStone.GrainInterfaces;
using Ice;
using Newtonsoft.Json;
//using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SampleClient;

namespace FootStone.Core.Client
{
    public static class  Test
    {
        public  static int HpChangeCount = 0;
    }
    public class NetworkClientIce
    {
        //     private static NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private static SampleClient.Logger logger = new SampleClient.Logger();


        private static NetworkClientIce _instance;
      
        public  NetworkClientIce() { }

        private List<Action>     actions = new List<Action>();

        public ObjectAdapter Adapter { get; private set; }
        private Ice.Communicator communicator;

  
        public static NetworkClientIce Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new NetworkClientIce();
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

        public void Init(string ip, int port)
        {
            try
            {
                Ice.InitializationData initData = new Ice.InitializationData();

                initData.properties = Ice.Util.createProperties();
                //initData.properties.setProperty("Ice.ACM.Client.Heartbeat", "Always");
                //initData.properties.setProperty("Ice.RetryIntervals", "-1");
                initData.properties.setProperty("Ice.FactoryAssemblies", "client");
                initData.properties.setProperty("Ice.Trace.Network", "1");
                initData.properties.setProperty("Ice.Default.Timeout", "15000");
                //    initData.properties.setProperty("SessionFactory.Proxy", "SessionFactory:default -h "+ IP + " -p " + port +" -t 10000");
                initData.properties.setProperty("Ice.Default.Locator", "FootStone/Locator:default -h " + ip + " -p " + port);

               // initData.logger = new NLoggerI(LogManager.GetLogger("Ice"));
                //initData.dispatcher = delegate (System.Action action, Ice.Connection connection)
                //{
                //    lock (this)
                //    {
                //        actions.Add(action);
                //    }                
                //};

                communicator = Ice.Util.initialize(initData);
                Adapter = communicator.createObjectAdapter("");

                Thread thread = new Thread(new ThreadStart(() =>
                {
                    Communicator.waitForShutdown();
                    logger.Info("ice closed!");

                }));
                thread.Start();
            }
            catch (System.Exception ex)
            {
                logger.Error(ex);
            }
        }

        public void Fini()
        {
            Communicator.shutdown();
            logger.Info("ice shutdown!");
        }
             
        public async Task<ISessionPrx> CreateSession(string account)
        {
            ISessionFactoryPrx sessionFactoryPrx = null;
            try
            {

                sessionFactoryPrx = ISessionFactoryPrxHelper
                   .uncheckedCast(communicator.stringToProxy("sessionFactory").ice_connectionId(account).ice_timeout(15000)) ;         

            }
            catch (Ice.NotRegisteredException)
            {
              //  var query = IceGrid.QueryPrxHelper.checkedCast(communicator.stringToProxy("FootStone/Query"));
                //   hello = HelloPrxHelper.checkedCast(query.findObjectByType("::Demo::Hello"));
            }
                

            await sessionFactoryPrx.ice_pingAsync();

            //logger.Debug(communicator.proxyToString(sessionFactoryPrx));

            var sessionPrx = (ISessionPrx)(await sessionFactoryPrx
                .CreateSessionAsync(account, "")).ice_connectionId(account);
          
            Connection connection = await sessionPrx.ice_getConnectionAsync();
            connection.setACM(30, Ice.ACMClose.CloseOff, Ice.ACMHeartbeat.HeartbeatAlways);
            connection.setCloseCallback( _ =>
            {
                logger.Warn($"{account} connecton closed!");
            });

            logger.Debug(connection.getInfo().connectionId+" session connection: ACM=" +
                JsonConvert.SerializeObject(connection.getACM())
                + ",Endpoint=" + connection.getEndpoint().ToString());
            

                  // Register the callback receiver servant with the object adapter     
                  //  var adapter = communicator.createObjectAdapter("");
            var proxy = ISessionPushPrxHelper.uncheckedCast(Adapter.addWithUUID(new SessionPushI()));
            Adapter.addFacet(new PlayerPushI(account), proxy.ice_getIdentity(), "playerPush");
            Adapter.addFacet(new ZonePushI(account), proxy.ice_getIdentity(), "zonePush");
            // Associate the object adapter with the bidirectional connection.
            connection.setAdapter(Adapter);

            // Provide the proxy of the callback receiver object to the server and wait for
            // shutdown.               
            await sessionPrx.AddPushAsync(proxy);
            return sessionPrx;
        }

    }

    internal class ZonePushI : IZonePushDisp_
    {
        private string name;

        private int count = 0;

        public ZonePushI(string name)
        {
            this.name = name;
        }
   //     private  NLog.Logger logger = LogManager.GetCurrentClassLogger();
        private static SampleClient.Logger logger = new SampleClient.Logger();
        public override void ZoneSync(byte[] data, Current current = null)
        {
            count++;
            if (count % 330 == 0)
            {
                logger.Info(name + " zone sync:" + count);
            }
        }
    }

    internal class PlayerPushI : IPlayerPushDisp_
    {
        private string name;
       // private NLog.Logger logger = LogManager.GetCurrentClassLogger();

        private static SampleClient.Logger logger = new SampleClient.Logger();
        public PlayerPushI(string name)
        {
            this.name = name;
        }
               
        public override void hpChanged(int hp, Current current = null)
        {

            Test.HpChangeCount++;
            if (Test.HpChangeCount % 1000 == 0)
            {
                logger.Info(name + " hp changed::" + Test.HpChangeCount);
            }

            //      logger.Info(name+" hp changed:" + hp);
        }
    }

    internal class SessionPushI : ISessionPushDisp_
    {
        public override void SessionDestroyed(Current current = null)
        {
            throw new NotImplementedException();
        }
    }
}
