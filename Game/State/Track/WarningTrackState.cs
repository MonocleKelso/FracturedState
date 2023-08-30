using UnityEngine;

namespace FracturedState.Game.Music
{
    public class WarningTrackState : TrackState
    {
        private readonly AudioClip warningTrack;

        public WarningTrackState(AudioClip warningTrack, AudioSource intro, AudioSource loop)
            : base(intro, loop)
        {
            this.warningTrack = warningTrack;
        }

        public override void Enter()
        {
            intro.Stop();
            loop.Stop();
            intro.volume = MusicManager.Instance.Volume;
            loop.volume = 0;
            intro.clip = warningTrack;
            intro.Play();
        }
    }
}