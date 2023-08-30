using FracturedState.Game.AI;
using UnityEngine;

namespace FracturedState.Game.Music
{
    public abstract class TrackState : IState
    {
        protected AudioSource intro;
        protected AudioSource loop;

        public TrackState(AudioSource intro, AudioSource loop)
        {
            this.intro = intro;
            this.loop = loop;
        }

        public virtual void Enter() { }

        public virtual void Execute() { }

        public virtual void Exit() { }

        protected void FadeOutLoop()
        {
            if (loop.volume > 0)
            {
                loop.volume -= 0.2f * Time.deltaTime;
            }
            else
            {
                loop.Stop();
            }
        }
    }
}