using FootStone.Client;
using FootStone.GrainInterfaces;
using FootStone.ProtocolNetty;
using Ice;
using Newtonsoft.Json;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace SampleClient
{

    internal class ZonePushI : IZonePushDisp_,IServerPush
    {      
        private int count = 0;

        public ZonePushI()
        {
           
        }
       // private NLog.Logger logger = LogManager.GetCurrentClassLogger();
        private static SampleClient.Logger logger = SampleClient.Logger.Instance;

        private SessionPushI sessionPushI;
        private string account;

        public override void RecvData(byte[] data, Current current = null)
        {
            count++;
            if (count % 33 == 0)
            {
                logger.Debug($"{account} RecvData:" + count);
            }
        }

        public string GetFacet()
        {
            return typeof(IZonePushPrx).Name;
        }       

        public void setSessionPushI(SessionPushI sessionPushI)
        {
            this.sessionPushI = sessionPushI;
        }

        public void setAccount(string account)
        {
            this.account = account;
        }
    }

    internal class PlayerPushI : IPlayerPushDisp_, IServerPush
    {    
    //    private NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        private static SampleClient.Logger logger = SampleClient.Logger.Instance;

        private SessionPushI sessionPushI;
        private string account;

        public PlayerPushI()
        {

        }

        public string GetFacet()
        {
           return typeof(IPlayerPushPrx).Name;
        }

        public void setSessionPushI(SessionPushI sessionPushI)
        {
            this.sessionPushI = sessionPushI;
        }

        public override void hpChanged(int hp, Current current = null)
        {
           // logger.Info(account + "begin hp changed::" + NetworkNew.HpChangeCount);
            Interlocked.Add(ref NetworkNew.HpChangeCount,1);
            if ((NetworkNew.HpChangeCount / NetworkNew.playerCount) > NetworkNew.lastHpChange)
            {
                Interlocked.Add(ref NetworkNew.lastHpChange, 1);
                logger.Info(account + " hp changed::" + NetworkNew.HpChangeCount);
            }
        }

        public void setAccount(string account)
        {
            this.account = account;
        }
    }

 

    public class NetworkNew
    {
        public  static int HpChangeCount = 0;
        public static int lastHpChange = 0;

        public static int recvDataCount = 0;
        public static int lastRecvData = 0;

        public static int playerCount = 0;

      //  private static NLog.Logger logger = LogManager.GetCurrentClassLogger();
        private static SampleClient.Logger logger = SampleClient.Logger.Instance;
        public async Task Test(string ip, int port, int count, ushort startIndex, bool needNetty)
        {
            logger.Info($"begin test,count:${count},startIndex:{startIndex},needNetty:{needNetty}");

            var client = new FSClientBuilder()
                .IceOptions(iceOptions =>
                {
                    iceOptions.EnableDispatcher = false;
                    iceOptions.PushObjects = new List<Ice.Object>();
                    iceOptions.PushObjects.Add(new PlayerPushI());
                    iceOptions.PushObjects.Add(new ZonePushI());
                })
                .NettyOptions(nettyOptions =>
                {
                    nettyOptions.Port = 8007;
                })
                .Build();

            //启动主线程
            //Thread thread = new Thread(new ThreadStart(async () =>
            //{
            //    do
            //    {
            //        client.Update();

            //        await Task.Delay(33);

            //     //   Thread.Sleep(33);
      
            //    } while (true);
            //}));
            //thread.Start();


            await client.StartAsync();

            for (ushort i = startIndex; i < startIndex + count; ++i)
            {
                var sessionId = "session" + i;
                var session = await client.CreateSession(ip, port, sessionId);             
                RunSession(session, i, 20, needNetty);
                playerCount++;
                await Task.Delay(10);
            }
            logger.Info("all session created:" + count);
        }

        private  async Task RunSession(IFSSession session,ushort index, int count,bool needNetty)
        {
            var account = "account" + index;
            var password = "111111";
            var playerName = "player" + index;

            try
            {
                session.OnDestroyed += (sender, e) =>
                {
                    logger.Info($"session:{session.GetId()} destroyed!");
                };


                //注册账号           
                var accountPrx = session.UncheckedCast(IAccountPrxHelper.uncheckedCast);
                try
                {
                    await accountPrx.RegisterRequestAsync(account, new RegisterInfo(account, password));
                    logger.Debug("RegisterRequest ok:" + account);
                }
                catch (Ice.Exception ex)
                {
                    logger.Debug("RegisterRequest fail:" + ex.Message);
                }

                //账号登录
                await accountPrx.LoginRequestAsync(account, password);
                logger.Debug("LoginRequest ok:" + account);

                //选择服务器         
                var worldPrx = session.UncheckedCast(IWorldPrxHelper.uncheckedCast);
                List<ServerInfo> servers = await worldPrx.GetServerListRequestAsync();

                if (servers.Count == 0)
                {
                    Console.Error.WriteLine("server list is empty!");
                    return;
                }

                //选取第一个服务器
                var serverId = servers[0].id;

                //获取角色列表
                List<PlayerShortInfo> players = await worldPrx.GetPlayerListRequestAsync(serverId);
                var playerPrx = session.UncheckedCast(IPlayerPrxHelper.uncheckedCast);

                //如果角色列表为0，创建新角色
                if (players.Count == 0)
                {
                    var playerId = await playerPrx.CreatePlayerRequestAsync(serverId, new PlayerCreateInfo(playerName, 1));
                    players = await worldPrx.GetPlayerListRequestAsync(serverId);
                }
                //选择第一个角色
                await playerPrx.SelectPlayerRequestAsync(players[0].playerId);


                //获取角色信息
                var playerInfo = await playerPrx.GetPlayerInfoAsync();
                logger.Debug($"{account} playerInfo:" + JsonConvert.SerializeObject(playerInfo));

                if (needNetty)
                {
                    await RunNetty(session, playerInfo, index);
                  //  await RunZone(session, playerInfo, index);
                }

                var roleMasterPrx = session.UncheckedCast(IRoleMasterPrxHelper.uncheckedCast);
                logger.Info($"{account} playerPrx begin!");
                MasterProperty property;
                for (int i = 0; i < count; ++i)
                {
                    await playerPrx.SetPlayerNameAsync(playerName + "_" + i);
                    await Task.Delay(3000);
                    property = await roleMasterPrx.GetPropertyAsync();
                    await Task.Delay(5000);
                    //     Console.Out.WriteLine("property" + JsonConvert.SerializeObject(property));
                    playerInfo = await playerPrx.GetPlayerInfoAsync();
                    logger.Info($"{account} playerInfo:" + JsonConvert.SerializeObject(playerInfo));

                    await Task.Delay(10000);
                }
            
                logger.Info($"{account} playerPrx end!");
            }
            catch (System.Exception e)
            {
                logger.Error(account + ":" + e.ToString());
            }
            finally
            {
                session.Destory();
            }
        }

        private async Task RunNetty(IFSSession session,PlayerInfo playerInfo,int index)
        {
            var zonePrx = session.UncheckedCast(IZonePrxHelper.uncheckedCast);

            System.Timers.Timer moveTimer = null;
            System.Timers.Timer pingTimer = null;
            
            //绑定Zone
            var endPoint = await zonePrx.BindZoneAsync(playerInfo.zoneId, playerInfo.playerId);
            var gameServerId = ProtocolNettyUtility.Endpoint2GameServerId(endPoint.ip, endPoint.port);

            var channel = await session.CreateStreamChannel();
            channel.eventRecvData += (data) =>
            {

                Interlocked.Add(ref recvDataCount, 1);
                if ((recvDataCount / playerCount) > lastRecvData)
                {
                    Interlocked.Add(ref lastRecvData, 10);
                    logger.Info($"session{index} recv data,size:{data.Length},count:{recvDataCount}");
                }
             
            };
            await channel.BindGameServer(playerInfo.playerId, gameServerId);

            //进入Zone
            await zonePrx.PlayerEnterAsync();

            //发送move消息
            //moveTimer = new System.Timers.Timer();
            //moveTimer.AutoReset = true;
            //moveTimer.Interval = 500;
            //moveTimer.Enabled = true;
            //moveTimer.Elapsed += (_1, _2) =>
            //{
            //    var data = channel.Allocator.DirectBuffer(16);
            //    data.WriteUnsignedShort((ushort)MessageType.Data);
            //    data.WriteUnsignedShort((ushort)14);
            //    data.WriteUnsignedShort((ushort)index);

            //    var move = new Move();
            //    move.direction = 1;
            //    move.speed = 10;
            //    move.point.x = 10.6f;
            //    move.point.y = 300.1f;
            //    move.Encoder(data);
            //    channel.WriteAndFlushAsync(data);

            //    //  logger.Debug($"send move!");
            //};
            //moveTimer.Start();

            //发送ping消息
            pingTimer = new System.Timers.Timer();
            pingTimer.AutoReset = true;
            pingTimer.Interval = 10000;
            pingTimer.Enabled = true;
            pingTimer.Elapsed += async (_1, _2) =>
            {
                logger.Debug($"send ping!");
                var pingTime = await channel.Ping(DateTime.Now.Ticks);
                var now = DateTime.Now.Ticks;
                var value = (now - pingTime) / 10000;
                logger.Debug($"ping:{value}ms");
            };
            pingTimer.Start();

        }

        private async Task RunZone(IFSSession session, PlayerInfo playerInfo, int index)
        {
            var zonePrx = session.UncheckedCast(IZonePrxHelper.uncheckedCast);

            System.Timers.Timer moveTimer = null;

            //进入Zone
            var endPoint = await zonePrx.BindZoneAsync(playerInfo.zoneId, playerInfo.playerId);
            //  await zonePrx.PlayerEnterAsync();

            //发送move消息
            moveTimer = new System.Timers.Timer();
            moveTimer.AutoReset = true;
            moveTimer.Interval = 500;
            moveTimer.Enabled = true;
            moveTimer.Elapsed += (_1, _2) =>
            {
                byte[] data = new byte[14];
                zonePrx.SendData(data);
            };
            moveTimer.Start();        
        }
    }
}
