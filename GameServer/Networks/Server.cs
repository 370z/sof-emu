﻿using Data.Models.Server;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.Scs.Server;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Utility;

namespace GameServer.Networks
{
    public class Server
    {
        protected Dictionary<int, ChannelModel> Channels;
        protected Dictionary<string, long> ConnectionTimes;

        public Server()
        {
            GenerateServer();
            GenerateChannels();
            Initilize();
        }


        private void GenerateServer()
        {
            // todo
        }

        private void GenerateChannels()
        {
            Channels = new Dictionary<int, ChannelModel>();

            int channelNumber = Program
                .Config["gameserver"]
                .Configs["channel"]
                .GetInt("count");

            string[] channelNames = Program
                .Config["gameserver"]
                .Configs["channel"]
                .GetString("name")
                .Split(',');

            string[] channelPorts = Program
                .Config["gameserver"]
                .Configs["channel"]
                .GetString("port")
                .Split(',');

            string[] channelTypes = Program
                .Config["gameserver"]
                .Configs["channel"]
                .GetString("type")
                .Split(',');

            string[] channelMaxPlayers = Program
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

        private void Initilize()
        {
            for(int i = 1; i <= Channels.Count; i++)
            {
                var channelModel = Channels[i];
                var channel = ScsServerFactory.CreateServer(new ScsTcpEndPoint(channelModel.Port));
                channel.ClientConnected += ChannelClientConnected;
                channel.ClientDisconnected += ChannelClientDisconnected;
                channel.Start();

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

            Log.Debug($"sender: {sender}");

            //if (ip == "159.253.18.161")
            //  return;

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

            //new Session(e.Client);
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
