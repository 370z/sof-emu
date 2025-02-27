﻿using Communicate;
using Data.Structures.Template.Server;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Server;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Utility;

namespace GameServer.Networks
{
    public class Server
    {
        protected int ServerId;
        protected string ServerName;
        protected string ServerAddess;

        protected Dictionary<int, ChannelModel> Channels;
        protected Dictionary<string, long> ConnectionTimes;

        protected Dictionary<int, IScsServer> ChannelServers;
        

        public Server()
        {
            GetServerData();
            GetChannels();
            Initilize();

            SendServerInfoToApi();
        }

        /// <summary>
        /// 
        /// </summary>
        private void GetServerData()
        {
            // todo server data
            ServerId = GameServer
                .Config["gameserver"]
                .Configs["server"]
                .GetInt("id");

            ServerName = GameServer
                .Config["gameserver"]
                .Configs["server"]
                .GetString("name");

            ServerAddess = GameServer
                .Config["gameserver"]
                .Configs["server"]
                .GetString("ip");
        }

        /// <summary>
        /// 
        /// </summary>
        private void GetChannels()
        {
            Channels = new Dictionary<int, ChannelModel>();

            int channelNumber = GameServer
                .Config["gameserver"]
                .Configs["channel"]
                .GetInt("count");

            string[] channelNames = GameServer
                .Config["gameserver"]
                .Configs["channel"]
                .GetString("name")
                .Split(',');

            string[] channelPorts = GameServer
                .Config["gameserver"]
                .Configs["channel"]
                .GetString("port")
                .Split(',');

            string[] channelTypes = GameServer
                .Config["gameserver"]
                .Configs["channel"]
                .GetString("type")
                .Split(',');

            string[] channelMaxPlayers = GameServer
                .Config["gameserver"]
                .Configs["channel"]
                .GetString("max_player")
                .Split(',');

            for (int i = 0; i < channelNumber; i++)
            {
                ChannelModel channel = new ChannelModel(
                    i + 1,
                    channelNames[i],
                    int.Parse(channelPorts[i]),
                    int.Parse(channelTypes[i]),
                    int.Parse(channelMaxPlayers[i]), 
                    0
                );

                Channels.Add(channel.Id, channel);
                // todo send post server data to ApiServer
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void SendServerInfoToApi()
        {
            List<ChannelModel> channels = Channels
                .Values
                .ToList();

            ServerModel model = new ServerModel(
                ServerId,
                ServerName,
                ServerAddess,
                channels);

            Global
                .ApiService
                .SendServerInfo(model);
        }

        /// <summary>
        /// 
        /// </summary>
        private void Initilize()
        {
            ChannelServers = new Dictionary<int, IScsServer>();
            ConnectionTimes = new Dictionary<string, long>();

            for (int i = 1; i <= Channels.Count; i++)
            {
                var channelModel = Channels[i];
                var channel = ScsServerFactory.CreateServer(new ScsTcpEndPoint(channelModel.Port));
                channel.ClientConnected += ChannelClientConnected;
                channel.ClientDisconnected += ChannelClientDisconnected;
                channel.Start();

                ChannelServers.Add(channel.GetHashCode(), channel);

                Log.Info($"Start channel {channelModel.Id} at port: {channelModel.Port}");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChannelClientConnected(object sender, ServerClientEventArgs e)
        {
            string ip = Regex.Match(e.Client.RemoteEndPoint.ToString(), "([0-9]+).([0-9]+).([0-9]+).([0-9]+)").Value;
            
            Log.Info("Client connected!");

            if (ConnectionTimes.ContainsKey(ip))
            {
                if (Funcs.GetCurrentMilliseconds() - ConnectionTimes[ip] < 2000)
                {
                    ConnectionTimes.Remove(ip);
                    Log.Info("TcpServer: FloodAttack prevent! Ip " + ip + " added to firewall");
                    return;
                }
                ConnectionTimes[ip] = Funcs.GetCurrentMilliseconds();
            }
            else
                ConnectionTimes.Add(ip, Funcs.GetCurrentMilliseconds());

            IScsServer channel = ChannelServers[((IScsServer)sender).GetHashCode()];
            _ = new Session(e.Client, channel);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChannelClientDisconnected(object sender, ServerClientEventArgs e)
        {
            Log.Info("Client disconnected!");
        }
    }
}
