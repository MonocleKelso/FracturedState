using UnityEngine;

namespace FracturedState.Game.Music
{
    [System.Serializable]
    public class LoopableTrack
    {
        [SerializeField]
        private AudioClip introTrack;
        [SerializeField]
        private AudioClip loopTrack;

        public AudioClip LoopTrack => loopTrack;
        public AudioClip IntroTrack => introTrack;
    }

    [System.Serializable]
    public class FactionMusic
    {
        [SerializeField]
        private string factionName;
        public string FactionName => factionName;

        public AudioClip[] AmbientTracks;
        public LoopableTrack[] CombatTracks;
        public AudioClip WinTrack;
        public AudioClip LoseTrack;
    }
}