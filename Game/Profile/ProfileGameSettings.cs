namespace FracturedState.Game
{
    [System.Serializable]
    public class ProfileGameSettings
    {
        public enum ReflectionQualityOptions
        {
            Ultra = 128,
            High = 64,
            Medium = 32,
            Low = 16,
            Off = 0
        }

        public static ProfileGameSettings DefaultSettings
        {
            get
            {
                return new ProfileGameSettings()
                {
                    ReflectionQuality = ReflectionQualityOptions.Low,
                    MusicVolume = 0.75f,
                    EffectsVolume = 0.4f,
                    UIVolume = 0.3f
                };
            }
        }

        public ReflectionQualityOptions ReflectionQuality { get; set; }
        public float MusicVolume { get; set; }
        public float EffectsVolume { get; set; }
        public float UIVolume { get; set; }
    }
}