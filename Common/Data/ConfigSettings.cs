
namespace FracturedState.Game.Data
{
    public class ConfigSettings
    {
        public GameConfig Values { get; private set; }

        private static ConfigSettings instance;
        public static ConfigSettings Instance
        {
            get
            {
                if (instance == null)
                    instance = new ConfigSettings();

                return instance;
            }
        }

        private ConfigSettings() { }

        public void LoadDefaultSettings()
        {
            Values = DataUtil.DeserializeXml<GameConfig>(DataLocationConstants.GameRootPath + DataLocationConstants.GameSettingsFile);
        }
    }
}