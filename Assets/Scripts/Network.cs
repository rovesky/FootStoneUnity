using DotNetty.Transport.Channels;
using FootStone.Core.Client;
using FootStone.GrainInterfaces;
using FootStone.ProtocolNetty;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SampleClient
{
    public class Network
    {
        //  private static Logger logger = LogManager.GetCurrentClassLogger();

        private static SampleClient.Logger logger = SampleClient.Logger.Instance;

        private string parseHost(string endPoint)
        {
            var strs = endPoint.Split(' ');
            for (int i = 0; i < strs.Length; ++i)
            {
                if (strs[i] == "-h")
                {
                    return strs[i + 1];
                }
            }
            return "";
        }
      

    //    private  List<NetworkClientIce> clients = new List<NetworkClientIce>();

        public   async Task Test(int count, ushort startIndex, bool needNetty)
        {
            logger.Info($"begin test,count:${count},startIndex:{startIndex},needNetty:{needNetty}" );
            NetworkClientIce clientIce = new NetworkClientIce();
            clientIce.Init("192.168.0.128", 4061);
         //   clients.Add(clientIce);

            NetworkClientNetty clientNetty = null;
            if (needNetty)
            {
                clientNetty = new NetworkClientNetty();
                clientNetty.Init();
              //  var channel = await clientNetty.ConnectNettyAsync("192.168.0.128", 8007, "1234");
              //  logger.Info("netty connect ok!" );
            }

            Run(1, 20, clientIce, clientNetty);

            //for (ushort i = startIndex; i < startIndex + count; ++i)
            //{
            //    if (i % 100 == 0)
            //    {
            //        clientIce = new NetworkClientIce();
            //        clientIce.Init("192.168.0.128", 4061);
            //        clients.Add(clientIce);
            //    }
            //    Run(i, 20, clientIce, clientNetty);
            //    await Task.Delay(20);
            //}
            logger.Info("all session created:" + count);
        }

        private  async Task Run(ushort index, int repeatCount, NetworkClientIce iceClient, NetworkClientNetty netty)
        {
            try
            {
                var sessionId = "session" + index;
                var account = "account" + index;
                var password = "111111";
                var playerName = "player" + index;

                //创建连接
                var sessionPrx = await iceClient.CreateSession(sessionId);
                //  Console.Out.WriteLine("NetworkIce.Instance.CreateSession ok:"+ account);

                //注册账号
                var accountPrx = AccountPrxHelper.uncheckedCast(sessionPrx, "account");
                try
                {
                    await accountPrx.RegisterRequestAsync(account, new RegisterInfo(account, password));
                    logger.Debug("RegisterRequest ok:" + account);
                }
                catch (Exception ex)
                {
                    logger.Debug("RegisterRequest fail:" + ex.Message);
                }

                //账号登录
                await accountPrx.LoginRequestAsync(account, password);
                logger.Debug("LoginRequest ok:" + account);

                //选择服务器
                var worldPrx = WorldPrxHelper.uncheckedCast(sessionPrx, "world");
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
                var playerPrx = IPlayerPrxHelper.uncheckedCast(sessionPrx, "player");

                //如果角色列表为0，创建新角色
                if (players.Count == 0)
                {
                    var playerId = await playerPrx.CreatePlayerRequestAsync(serverId, new PlayerCreateInfo(playerName, 1));
                    players = await worldPrx.GetPlayerListRequestAsync(serverId);
                }
                //选择第一个角色
                await playerPrx.SelectPlayerRequestAsync(players[0].playerId);

                var roleMasterPrx = IRoleMasterPrxHelper.uncheckedCast(sessionPrx, "roleMaster");
                var zonePrx = IZonePrxHelper.uncheckedCast(sessionPrx, "zone");

                //获取角色信息
                var playerInfo = await playerPrx.GetPlayerInfoAsync();
             //   logger.Debug($"{account} playerInfo:" + JsonConvert.SerializeObject(playerInfo));

                IChannel channel = null;
                System.Timers.Timer moveTimer = null;
                System.Timers.Timer pingTimer = null;
                if (netty != null)
                {
                    //连接Netty
                    var host = parseHost(sessionPrx.ice_getConnection().getEndpoint().ToString());
                    logger.Debug("ConnectNetty begin(" + host + ")");
                    channel = await netty.ConnectNettyAsync(host, 8007, playerInfo.playerId);
                    logger.Debug("ConnectNetty end(" + host + ")");

                    //绑定Zone
                    var endPoint = await zonePrx.BindZoneAsync(playerInfo.zoneId, playerInfo.playerId);
                    var gameServerId = ProtocolNettyUtility.Endpoint2GameServerId(endPoint.ip, endPoint.port);
                    await netty.BindGameServer(channel, playerInfo.playerId, gameServerId);

                    //进入Zone
                    await zonePrx.PlayerEnterAsync();

                    //发送move消息
                    moveTimer = new System.Timers.Timer();
                    moveTimer.AutoReset = true;
                    moveTimer.Interval = 500;
                    moveTimer.Enabled = true;
                    moveTimer.Elapsed += (_1, _2) =>
                    {
                        netty.SendMove(channel, index);
                    };
                    moveTimer.Start();

                    //发送ping消息
                    pingTimer = new System.Timers.Timer();
                    pingTimer.AutoReset = true;
                    pingTimer.Interval = 2000;
                    pingTimer.Enabled = true;
                    pingTimer.Elapsed += (_1, _2) =>
                    {
                        netty.SendPing(channel, index);
                    };
                    pingTimer.Start();
                }

                logger.Info($"{account} playerPrx begin!");
                MasterProperty property;
                for (int i = 0; i < repeatCount; ++i)
                {
                    await playerPrx.SetPlayerNameAsync(playerName + "_" + i);
                    await Task.Delay(3000);
                    property = await roleMasterPrx.GetPropertyAsync();
                    await Task.Delay(5000);
                    //     Console.Out.WriteLine("property" + JsonConvert.SerializeObject(property));
                    playerInfo = await playerPrx.GetPlayerInfoAsync();
                    await Task.Delay(10000);
                }
              //  logger.Info($"{account} playerInfo:" + JsonConvert.SerializeObject(playerInfo));

                logger.Info($"{account} playerPrx end!");
                sessionPrx.begin_Destroy();
                if (channel != null)
                {
                    await channel.CloseAsync();
                }

                if (moveTimer != null)
                {
                    moveTimer.Stop();
                }

                if (pingTimer != null)
                {
                    pingTimer.Stop();
                }
                iceClient.Fini();

            }
            catch (System.Exception e)
            {
                logger.Error(e);
            }
        }
    }
}
