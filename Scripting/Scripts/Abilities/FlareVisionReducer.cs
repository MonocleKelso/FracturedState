using System.Collections;
using System.Linq;
using Code.Game.Management;
using FracturedState.Game;
using FracturedState.Game.Data;
using FracturedState.Game.Network;
using UnityEngine;

namespace FracturedState.Scripting
{
    public class FlareVisionReducer : MutatorAbility
    {
        private const float TotalTime = 10;
        private float time;
        private readonly float startVision;

        public FlareVisionReducer(UnitManager owner) : base(owner)
        {
            startVision = owner.Data.VisionRange;
        }

        public override void ExecuteAbility()
        {
            CoroutineRunner.RunCoroutine(ReduceVision());
        }

        private IEnumerator ReduceVision()
        {
            Transform shroud = null;
            SphereCollider detector = null;
            if (Owner.IsMine || Owner.IsFriendly)
            {
                var t = Object.FindObjectsOfType<ShroudFollower>().SingleOrDefault(s => s.Target == Owner.transform);
                if (t != null)
                {
                    shroud = t.transform;
                }

                var d = Owner.transform.GetChildByName("Vision");
                if (d != null)
                {
                    detector = d.GetComponent<SphereCollider>();
                }
            }
            yield return new WaitForSeconds(TotalTime);
            while (Owner != null && time < TotalTime)
            {
                time += Time.deltaTime;
                var vision = Mathf.Lerp(startVision, 0, time / TotalTime);
                
                if (shroud != null)  shroud.localScale = new Vector3(vision * 2.1f, vision * 2.1f, 0);
                if (detector != null) detector.radius = vision;
                
                if (FracNet.Instance.IsHost && time > TotalTime)
                {
                    Owner.NetMsg.CmdTakeDamage(1, null, Weapon.DummyName);
                }

                yield return null;
            }
        }
    }
}