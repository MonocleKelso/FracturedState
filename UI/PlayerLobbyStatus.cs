using System;
using System.Linq;
using FracturedState.Game.Data;
using FracturedState.Game.Management;
using FracturedState.Game.Network;
using UnityEngine;
using UnityEngine.UI;

namespace FracturedState.UI
{
    public enum LobbySlotStatus { Open, Closed, AI, Human }

    public class PlayerLobbyStatus : MonoBehaviour
    {
        private const string ClosedLabel = "lbl.closedslot";
        private const string OpenLabel = "lbl.openslot";

        public LobbySlotStatus Status { get; private set; }
        public Team OwnerTeam { get; private set; }

        [SerializeField] private Image avatar;
        [SerializeField] private Text playerName;
        [SerializeField] private FactionSelect factionPicker;
        [SerializeField] private Toggle readyToggle;
        [SerializeField] private Button kickPlayer;
        [SerializeField] private Button swapSlotStatus;
        [SerializeField] private Dropdown teamPicker;
        [SerializeField] private TeamColorSelect colorPicker;
        [SerializeField] private Sprite[] nameBackgrounds;

        private void Start()
        {
            if (!SkirmishVictoryManager.IsMultiPlayer)
                SetStatus(LobbySlotStatus.Open);
            
            swapSlotStatus.onClick.AddListener(ChangeSlotStatus);
        }

        public void SetTeam(Team team)
        {
            OwnerTeam = team;
            SetName(OwnerTeam.PlayerName);
            factionPicker.Populate(team.IsHuman);
            factionPicker.DropDown.onValueChanged.RemoveListener(SendFactionChoice);
            factionPicker.DropDown.onValueChanged.AddListener(SendFactionChoice);
            factionPicker.DropDown.interactable = (FracNet.Instance.IsHost && !OwnerTeam.IsHuman) || FracNet.Instance.LocalTeam == OwnerTeam;
            colorPicker.DropDown.onValueChanged.RemoveListener(SendColorChoice);
            colorPicker.DropDown.onValueChanged.AddListener(SendColorChoice);
            colorPicker.DropDown.interactable = (FracNet.Instance.IsHost && !OwnerTeam.IsHuman) || FracNet.Instance.LocalTeam == OwnerTeam;
            colorPicker.SetTeam(team);
            SetFaction(OwnerTeam.FactionIndex);
            kickPlayer.gameObject.SetActive(FracNet.Instance.IsHost && FracNet.Instance.LocalTeam != team);
            if (FracNet.Instance.IsHost)
            {
                kickPlayer.onClick.RemoveListener(KickPlayer);
                kickPlayer.onClick.AddListener(KickPlayer);
            }
            swapSlotStatus.gameObject.SetActive(false);
            avatar.gameObject.SetActive(true);
            avatar.sprite = AvailableAvatars.Instance.Avatars[team.AvatarIndex];
            factionPicker.gameObject.SetActive(true);
            colorPicker.gameObject.SetActive(true);
            teamPicker.gameObject.SetActive(true);
            teamPicker.value = team.LobbySlotIndex;
            team.UpdateSide(team.LobbySlotIndex);
            teamPicker.interactable = FracNet.Instance.IsHost && !team.IsHuman || team == FracNet.Instance.LocalTeam;
            teamPicker.onValueChanged.RemoveListener(SendSideChoice);
            if (teamPicker.interactable)
            {
                teamPicker.onValueChanged.AddListener(SendSideChoice);
            }
            if (team.IsHuman)
            {
                SetStatus(LobbySlotStatus.Human);
                SetReady(FracNet.Instance.IsHost);
                readyToggle.interactable = FracNet.Instance.LocalTeam == team;
                readyToggle.isOn = team.IsReady;
                if (FracNet.Instance.IsHost)
                {
                    readyToggle.gameObject.SetActive(FracNet.Instance.LocalTeam != team);
                }
                else
                {
                    // set active for non hosts (host is always in first slot)
                    readyToggle.gameObject.SetActive(team.LobbySlotIndex != 0);
                }
                if (FracNet.Instance.LocalTeam == team)
                {
                    readyToggle.onValueChanged.RemoveListener(SetTeamReady);
                    readyToggle.onValueChanged.AddListener(SetTeamReady);
                }
            }
            else
            {
                SetStatus(LobbySlotStatus.AI);
                SetReady(true);
                readyToggle.interactable = false;
                readyToggle.gameObject.SetActive(false);
            }
        }

        public void SendSideChoice(int side)
        {
            if (OwnerTeam == FracNet.Instance.LocalTeam)
            {
                FracNet.Instance.NetworkActions.CmdUpdateSide(side);
            }
            else if (FracNet.Instance.IsHost && !OwnerTeam.IsHuman)
            {
                FracNet.Instance.NetworkActions.CmdUpdateAISide(OwnerTeam.PlayerName, side);
            }
        }

        public void UpdateSide(int side)
        {
            teamPicker.value = side;
        }
        
        private static void SetTeamReady(bool ready)
        {
            if (ready)
            {
                FracNet.Instance.NetworkActions.CmdMakeReady();
            }
        }

        private void KickPlayer()
        {
            if (OwnerTeam != null)
            {
                if (OwnerTeam.IsHuman)
                {
                    var actions = GlobalNetworkActions.GetActions(OwnerTeam);
                    if (actions != null)
                    {
                        actions.connectionToClient.Disconnect();
                    }
                }
                else
                {
                    var remove = FracNet.Instance.RemoveAiTeam(OwnerTeam.PlayerName);
                    if (remove)
                    {
                        FracNet.Instance.NetworkActions.CmdUpdateLobbySlot(transform.GetSiblingIndex(), (int)LobbySlotStatus.Open);
                    }
                }
            }
        }

        private void ChangeSlotStatus()
        {
            if (Status == LobbySlotStatus.Open)
            {
                FracNet.Instance.NetworkActions.CmdUpdateLobbySlot(transform.GetSiblingIndex(), (int)LobbySlotStatus.Closed);
            }
            else if (Status == LobbySlotStatus.Closed)
            {
                FracNet.Instance.AddAiTeam(transform.GetSiblingIndex());
            }
        }

        private void SendColorChoice(int index)
        {
            if (OwnerTeam == FracNet.Instance.LocalTeam)
            {
                FracNet.Instance.NetworkActions.CmdUpdateColor(index);
            }
            else if (!OwnerTeam.IsHuman && FracNet.Instance.IsHost)
            {
                FracNet.Instance.NetworkActions.CmdUpdateAIColor(OwnerTeam.PlayerName, index);
            }
        }

        private void SendFactionChoice(int index)
        {
            if (OwnerTeam == FracNet.Instance.LocalTeam)
            {
                FracNet.Instance.NetworkActions.CmdUpdateFaction(index);
            }
            else if (!OwnerTeam.IsHuman && FracNet.Instance.IsHost)
            {
                FracNet.Instance.NetworkActions.CmdUpdateAIFaction(OwnerTeam.PlayerName, index);
            }
        }

        public void SetStatus(LobbySlotStatus status)
        {
            Status = status;
            if (Status == LobbySlotStatus.Closed || Status == LobbySlotStatus.Open)
            {
                SetName(LocalizedString.GetString(Status == LobbySlotStatus.Closed ? ClosedLabel : OpenLabel));
                readyToggle.gameObject.SetActive(false);
                kickPlayer.gameObject.SetActive(false);
                swapSlotStatus.gameObject.SetActive(FracNet.Instance.IsHost);
                avatar.gameObject.SetActive(false);
                factionPicker.gameObject.SetActive(false);
                colorPicker.gameObject.SetActive(false);
                teamPicker.gameObject.SetActive(false);
                GetComponent<Image>().sprite = nameBackgrounds[nameBackgrounds.Length - 1];
            }
        }

        public void SetName(string pName)
        {
            playerName.text = pName;
        }

        public void SetReady(bool ready)
        {
            readyToggle.isOn = ready;
        }

        public void SetFaction(int faction)
        {
            factionPicker.DropDown.value = faction;
            GetComponent<Image>().sprite = nameBackgrounds[faction];
        }

        public void SetColor(int color)
        {
            colorPicker.DropDown.value = color;
        }

        private void OnDestroy()
        {
            if (factionPicker != null && factionPicker.DropDown != null)
            {
                factionPicker.DropDown.onValueChanged.RemoveAllListeners();
            }
            if (colorPicker != null && colorPicker.DropDown != null)
            {
                colorPicker.DropDown.onValueChanged.RemoveAllListeners();
            }
            if (swapSlotStatus != null)
            {
                swapSlotStatus.onClick.RemoveListener(ChangeSlotStatus);
            }
        }
    }
}