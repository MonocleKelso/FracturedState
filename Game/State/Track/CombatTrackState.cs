using UnityEngine;

namespace FracturedState.Game.Music
{
    public class CombatTrackState : TrackState
    {
        FactionMusic music;
        LoopableTrack track;
        double transitionTime;

        public CombatTrackState(FactionMusic music, AudioSource intro, AudioSource loop)
            : base(intro, loop)
        {
            this.music = music;
        }

        public override void Enter()
        {
            intro.Stop();
            loop.Stop();
            intro.volume = MusicManager.Instance.Volume;
            loop.volume = 0;
            if (music != null)
            {
                track = music.CombatTracks[Random.Range(0, music.CombatTracks.Length)];
                intro.clip = track.IntroTrack;
                loop.clip = track.LoopTrack;
                transitionTime = AudioSettings.dspTime + (track.IntroTrack.length - track.LoopTrack.length);
                loop.PlayScheduled(transitionTime);
            }
        }

        public override void Execute()
        {
            if (music != null)
            {
                if (AudioSettings.dspTime > transitionTime && intro.volume > 0)
                {
                    loop.volume += 0.1f;
                    intro.volume -= 0.1f;
                    loop.volume = Mathf.Clamp(loop.volume, 0, MusicManager.Instance.Volume);
                }
            }
        }
    }
}