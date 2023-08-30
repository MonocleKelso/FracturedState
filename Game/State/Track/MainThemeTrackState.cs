using UnityEngine;

namespace FracturedState.Game.Music
{
    public class MainThemeTrackState : TrackState
    {
        AudioClip mainTheme;

        public MainThemeTrackState(AudioClip mainTheme, AudioSource intro, AudioSource loop) : base(intro, loop)
        {
            this.mainTheme = mainTheme;
        }

        public override void Enter()
        {
            intro.Stop();
            loop.Stop();
            if (mainTheme != null)
            {
                loop.clip = mainTheme;
                loop.volume = MusicManager.Instance.Volume;
                loop.Play();
            }
        }
    }
}