﻿using Data.Interfaces;
using Data.Structures.Account;
using Data.Structures.Player;
using System;
using System.Linq;
using Utility;

namespace Communicate.Logics
{
    public class PlayerLogic : Global
    {
        public static void DeletePlayer(ISession session, int index, string password)
        {
            //Player p = session.GetPlayer(index);

            //var result = await ApiService
            //    .SendDeletePlayer(p.PlayerId, password);

            bool result = false;

            // todo send packet delete response
            FeedbackService
                .SendDeletePlayer(session, index, result);
        }

        public static void PlayerSelected(ISession session, int playerIndex)
        {
            session.Player = session.Account.Players.FirstOrDefault(player => player.Index == playerIndex);

            if (session.Player == null)
                return;

            session.Player.Session = session;

            PlayerService.InitPlayer(session.Player);
            FeedbackService.SendInitailData(session);

            //Player player = session
            //    .GetPlayer(playerIndex);

            //session.SetSelectPlayer(player);

            //MapService
            //    .EnterWorld(player);

            //PlayerService
            //    .EnterWorld(player);
        }

        public static void OptionSetting(ISession session, SettingOption setting)
        {
            AccountService
                .SetSettingOption(session, setting);

            // todo
            // send broadcast player data
            PlayerService
                .OnUpdateSetting(session);
            // send broadcast equipment data & effect
            // send broadcast skill and status
            // send broadcast Update Qigong
            // send update world time
        }

        public static void PlayerMoved(Player player, float x1, float y1, float z1, float x2, float y2, float z2, float distance, int target)
        {
            Log.Debug($"PlayerMoved: {x1}, {y1}, {z1},{x2}, {y2}, {z2}, {distance}, {target}");
            PlayerService.PlayerMoved(player, x1, y1, z1, x2, y2, z2, distance, target);
            FeedbackService.PlayerMoved(player, x1, y1, z1, x2, y2, z2, distance, target);

            //PartyService.SendMemberPositionToPartyMembers(player);
        }

        public static void SelectNpc(ISession session, int statisticId)
        {
            /*Npc npc = session
                .GetSelectedPlayer()
                .GetMap()
                .GetNpc(statisticId);

            FeedbackService
                .SelectNpc(session, npc);*/
        }

        public static void PlayerEnterWorld(Player player)
        {
            //MapService.PlayerEnterWorld(player);
            //PlayerService.PlayerEnterWorld(player);
            //ControllerService.PlayerEnterWorld(player);
            FeedbackService.OnPlayerEnterWorld(player.Session, player);
        }
    }
}
