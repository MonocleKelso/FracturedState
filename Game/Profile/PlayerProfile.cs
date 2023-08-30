using System;
using FracturedState.Game.Data;
using System.Collections.Generic;
using UnityEngine.Events;

namespace FracturedState.Game
{
    public static class ProfileManager
    {
        // TODO: Remove edit mode from profile manager
        public static bool EditMode { get; private set; }
        public static int EditedAvatar { get; private set; }

        public static List<PlayerProfile> ProfileList { get; private set; }
        private static PlayerProfile activeProfile;

        public class PlayerProfileChanged : UnityEvent<string, int> { }

        public static UnityEvent<string, int> OnPlayerInfoChanged = new PlayerProfileChanged();

        public static void LoadProfileData()
        {
            ProfileList = new List<PlayerProfile>();
            var profiles = DataUtil.GetFiles(DataLocationConstants.GameRootPath + DataLocationConstants.PlayerProfileDirectory, "dat");
            if (profiles != null)
            {
                foreach (var p in profiles)
                {
                    var profile = DataUtil.DeserializeBinary<PlayerProfile>(p);
                    if (profile != null)
                    {
                        ProfileList.Add(profile);
                    }
                }
            }
            GetActiveProfile();
            EditMode = false;
        }

        public static float GetEffectsVolumeFromProfile()
        {
            return activeProfile?.GameSettings.EffectsVolume ?? 0;
        }

        public static PlayerProfile GetActiveProfile()
        {
            if (ProfileList == null)
            {
                LoadProfileData();
            }

            if (ProfileList.Count > 0)
            {
                activeProfile = ProfileList[0];
            }

            if (activeProfile == null)
            {
                activeProfile = new PlayerProfile();
                ProfileList.Add(activeProfile);
                if (SteamManager.Initialized)
                {
                    EditCurrentProfile();
                    var name = Steamworks.SteamFriends.GetPersonaName();
                    name = name.Replace('|', '-');
                    if (name.Length > 18)
                    {
                        name = name.Substring(0, 18);
                    }
                    activeProfile.SaveSettings(name, activeProfile.BuiltInAvatarIndex);
                    SaveCurrentProfile(activeProfile);
                }
            }
            return new PlayerProfile(activeProfile);
        }

        public static void EditCurrentProfile()
        {
            if (activeProfile == null)
                throw new FracturedStateException("Cannot enter edit mode without an active profile. Call GetActiveProfile() first");

            EditedAvatar = activeProfile.BuiltInAvatarIndex;
            EditMode = true;
        }

        public static void SaveCurrentProfile(PlayerProfile newData)
        {
            if (activeProfile == null)
                throw new FracturedStateException("Cannot save without an active profile. Call GetActiveProfile() first");

            var filePath = DataLocationConstants.GameRootPath + DataLocationConstants.PlayerProfileDirectory;

            var oldFile = "profile_" + activeProfile.PlayerName + activeProfile.BuiltInAvatarIndex + ".dat";
            DataUtil.DeleteFile(filePath + "/" + oldFile);

            var fileName = "profile_" + newData.PlayerName + newData.BuiltInAvatarIndex + ".dat";
            try
            {
                DataUtil.SerializeBinary(newData, filePath, fileName);
            }
            catch (Exception)
            { // do nothing
            }

            var index = ProfileList.IndexOf(activeProfile);
            ProfileList[index] = newData;
            activeProfile = newData;
            
            EditMode = false;
        }

        public static void ResetProfile()
        {
            activeProfile = null;
            LoadProfileData();
            MusicManager.Instance.SetVolume(activeProfile.GameSettings.MusicVolume);
            InterfaceSoundPlayer.UpdateVolume(activeProfile.GameSettings.UIVolume);
            UnitBarkManager.Instance.UpdateVolume(activeProfile.GameSettings.UIVolume);
        }
    }

    [Serializable]
    public class PlayerProfile
    {
        public string PlayerName { get; private set; }
        public int BuiltInAvatarIndex { get; private set; }
        public KeyBindingConfiguration KeyBindConfig { get; }
        public ProfileGameSettings GameSettings { get; }

        public PlayerProfile()
        {
            PlayerName = "New Player";
            BuiltInAvatarIndex = 0;
            KeyBindConfig = KeyBindingConfiguration.DefaultConfiguration;
            GameSettings = ProfileGameSettings.DefaultSettings;
        }

        public PlayerProfile(PlayerProfile copy)
        {
            PlayerName = copy.PlayerName;
            BuiltInAvatarIndex = copy.BuiltInAvatarIndex;
            KeyBindConfig = copy.KeyBindConfig;
            GameSettings = copy.GameSettings;
            if (KeyBindConfig == null)
            {
                KeyBindConfig = KeyBindingConfiguration.DefaultConfiguration;
            }
            if (GameSettings == null)
            {
                GameSettings = ProfileGameSettings.DefaultSettings;
            }
        }

        public void SaveSettings(string playerName, int avatarIndex)
        {
            PlayerName = playerName;
            BuiltInAvatarIndex = avatarIndex;
            ProfileManager.OnPlayerInfoChanged.Invoke(playerName, avatarIndex);
        }
    }
}