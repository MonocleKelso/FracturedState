using UnityEngine;

namespace FracturedState.Game.Music
{
    public class SingleTrackState : TrackState
    {
        private readonly AudioClip track;

        public SingleTrackState(AudioClip track, AudioSource intro, AudioSource loop) : base(intro, loop)
        {
            this.track = track;
        }

        public override void Enter()
        {
            intro.Stop();
            loop.Stop();
            intro.volume = MusicManager.Instance.Volume;
            loop.volume = 0;
            intro.clip = track;
            intro.Play();
        }
    }
}