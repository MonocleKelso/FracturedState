using UnityEngine;

namespace FracturedState.Game.Music
{
    public class AmbientLoopTrackState : TrackState
    {
        private readonly FactionMusic music;
        private float waitTime;
        private bool fadeStarted;
        private readonly bool skipFade;

        public AmbientLoopTrackState(FactionMusic music, AudioSource intro, AudioSource loop, bool skipFade)
            : base(intro, loop)
        {
            this.music = music;
            this.skipFade = skipFade;
        }

        public override void Execute()
        {
            if (MusicManager.Instance.InCombatTrack)
            {
                if (waitTime <= 0)
                {
                    waitTime = 10;
                }
                else
                {
                    waitTime -= Time.deltaTime;
                    return;
                }
            }

            if (loop.isPlaying)
            {
                if (!fadeStarted && !skipFade)
                {
                    waitTime = 10;
                    fadeStarted = true;
                }
                else
                {
                    waitTime -= Time.deltaTime;
                }
                if (waitTime <= 0 || skipFade)
                {
                    FadeOutLoop();
                }
                return;
            }

            if (music != null)
            {
                if (!intro.isPlaying)
                {
                    if (waitTime <= 0)
                    {
                        waitTime = Random.Range(0f, 5f);
                    }

                    waitTime -= Time.deltaTime;
                    if (waitTime <= 0)
                    {
                        intro.clip = music.AmbientTracks[Random.Range(0, music.AmbientTracks.Length)];
                        intro.Play();
                    }
                }
                else if (intro.volume < MusicManager.Instance.Volume)
                {
                    intro.volume += 0.2f * Time.deltaTime;
                }
            }
        }
    }
}