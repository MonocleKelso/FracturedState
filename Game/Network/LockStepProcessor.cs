using System;
using System.Collections.Generic;
using FracturedState.Game.Management;
using UnityEngine;

namespace FracturedState.Game.Network
{
    public sealed class LockStepProcessor
    {
        private static LockStepProcessor instance;
        public static LockStepProcessor Instance => instance ?? (instance = new LockStepProcessor());

        private uint LastMessageId { get; set; }
        private readonly Dictionary<uint, ILockStepMessage> messages;
        private readonly List<uint> pendingMessages;

        private LockStepProcessor()
        {
            messages = new Dictionary<uint, ILockStepMessage>();
            pendingMessages = new List<uint>();
            LastMessageId = 0;
        }

        public uint GetNextMessageId()
        {
            return LastMessageId++;
        }

        public void AddMessage(ILockStepMessage msg)
        {
            if (messages.ContainsKey(msg.Id))
            {
                throw new FracturedStateException("A synchronization error has occurred. A message was sent with a duplicate id.");
            }
            messages.Add(msg.Id, msg);
        }

        public void ReceiveMessage(GlobalNetworkActions receiver, uint msgId)
        {
            receiver.LocalTeam.AddReceivedMessage(msgId);
            if (!pendingMessages.Contains(msgId))
                pendingMessages.Add(msgId);
            ProcessMessages();
        }

        private void ProcessMessages()
        {
            if (pendingMessages.Count == 0) return;
            
            pendingMessages.Sort();

            while (pendingMessages.Count > 0)
            {
                var msg = pendingMessages[0];
                var canProcess = true;
                for (var i = 0; i < SkirmishVictoryManager.SkirmishTeams.Count; i++)
                {
                    var team = SkirmishVictoryManager.SkirmishTeams[i];
                    if (team.IsHuman && team.Connected)
                    {
                        if (!team.HasReceivedMessage(msg))
                        {
                            canProcess = false;
                            break;
                        }
                    }
                }
                if (canProcess)
                {
                    pendingMessages.RemoveAt(0);
                    foreach (var team in SkirmishVictoryManager.SkirmishTeams)
                    {
                        if (!team.IsHuman || !team.Connected) continue;
                        GlobalNetworkActions.GetActions(team).RpcProcessLockStepMsg(msg);
                    }
                }
                else
                {
                    // stop processing messages because at least 1 player hasn't confirmed receipt
                    break;
                }
            }
        }
        
        public void ProcessMessage(uint msgId)
        {
            ILockStepMessage msg;
            if (!messages.TryGetValue(msgId, out msg))
            {
                throw new FracturedStateException($"A synchronization error has occurred. A request to process a message was sent but we don't have that message. Message Id: {msgId}");
            }

            messages.Remove(msgId);
            try
            {
                msg.Process();
            }
            catch (Exception e)
            {
                ErrorHandler.HandleSyncError(new SyncException(e));
#if UNITY_EDITOR
                Debug.LogError(e.Message + "\n" + e.StackTrace);
#endif
            }
        }
    }
}