using FracturedState.Game.Data;
using UnityEngine;

namespace FracturedState.Game.AI
{
    public class RuhkCharge : UnitMoveState
    {
        private readonly UnitManager target;
        private readonly Vector3 origDestination;
        private bool overLeapMax;
        private bool underLeapMin;
        private bool leaping;
        private Coroutine leapFinishWait;

        private static readonly string[] Grunts = { "Grunt01", "Grunt02", "Grunt03", "Grunt04", "Grunt05", "Grunt06", "Grunt07"};
        
        public RuhkCharge(UnitManager owner, Vector3 destination, UnitManager target) : base(owner, destination)
        {
            this.target = target;
            origDestination = destination;
        }

        public override void Enter()
        {

            IsActive = true;

            CalcLeapState();

            // if we're under leap min then move to target
            if (underLeapMin)
            {
                Destination = target.transform.position;
                base.Enter();
            }
            // if we're over leap max then move to a destination within leap distance
            else if (overLeapMax)
            {
                var toTarget = Owner.transform.position - origDestination;
                Destination = origDestination + toTarget.normalized * (RuhkAttackState.LeapMaxDistance * 0.9f);
                base.Enter();
            }
            // otherwise we're within range so charge
            else
            {
                leaping = true;
                var rand = Grunts[Random.Range(0, Grunts.Length)];
                var grunt = DataUtil.LoadBuiltInSound($"Ruhk/{rand}");
                Owner.GetComponent<AudioSource>().PlayOneShot(grunt);
                var leapRight = Owner.transform.GetChildByName("RuhkChargeRight");
                var leapLeft = Owner.transform.GetChildByName("RuhkChargeLeft");
                if (leapRight != null && leapLeft != null)
                {
                    var rightSys = leapRight.GetComponent<ParticleSystem>();
                    var leftSys = leapLeft.GetComponent<ParticleSystem>();
                    if (rightSys != null && leftSys != null)
                    {
                        rightSys.Play();
                        leftSys.Play();
                    }
                }
                Owner.AnimControl.Play(RuhkAttackState.LeapAnimation, PlayMode.StopAll);
            }

            if (Owner.IsMine)
            {
                MusicManager.Instance.AddCombatUnit();
                Owner.Squad?.RegisterCombatUnit();
            }
        }

        public override void Execute()
        {
            if (target != null && target.IsAlive)
            {
                // if we're waiting for the leap animation to finish then do nothing
                if (leapFinishWait != null)
                    return;

                // if leaping then move towards desination in a straight line
                if (leaping)
                {
                    if (leapFinishWait == null)
                    {
                        Owner.transform.LookAt(origDestination);
                        Owner.transform.position = Vector3.MoveTowards(Owner.transform.position, origDestination, RuhkAttackState.LeapSpeed * Time.deltaTime);
                        if ((Owner.transform.position - origDestination).sqrMagnitude <= ConfigSettings.Instance.Values.CloseEnoughThreshold)
                        {
                            OnArrival();
                        }
                    }
                }
                // otherwise path normally
                else
                {
                    if ((Owner.transform.position - target.transform.position).magnitude <= Owner.ContextualWeapon.Range)
                    {
                        Owner.StateMachine.ChangeState(new RuhkAttackState(new AttackStatePackage(Owner, target)));
                    }
                    else
                    {
                        base.Execute();
                    }
                }
            }
            else
            {
                Owner.StateMachine.ChangeState(new UnitIdleState(Owner));
            }
        }

        protected override void AttackMoveEnemySearch()
        {
            // intentionally empty so we don't stop units
        }
        
        public override void Exit()
        {
            base.Exit();
            if (Owner.IsMine)
            {
                MusicManager.Instance.RemoveCombatUnit();
                Owner.Squad.UnregisterCombatUnit();
            }
            if (leapFinishWait != null)
            {
                Owner.StopCoroutine(leapFinishWait);
            }
        }

        protected override void OnArrival()
        {
            // if we were pathing to a leap position then leap
            if (overLeapMax && !leaping)
            {
                leaping = true;
                Owner.AnimControl.Play(RuhkAttackState.LeapAnimationMoving, PlayMode.StopAll);
            }
            // if we were finishing a leap then play animation
            else if (leaping)
            {
                if (leapFinishWait == null)
                {
                    leapFinishWait = Owner.StartCoroutine(LeapFinish());
                }
            }
            // otherwise switch back to attacking the target
            else
            {
                Owner.StateMachine.ChangeState(new RuhkAttackState(new AttackStatePackage(Owner, target)));
            }
        }

        private System.Collections.IEnumerator LeapFinish()
        {
            Owner.AnimControl.Play(RuhkAttackState.LeapFinishAnimation, PlayMode.StopAll);
            var landRight = Owner.transform.GetChildByName("RuhkLandingRight");
            var landLeft = Owner.transform.GetChildByName("RuhkLandingLeft");
            if (landRight != null && landLeft != null)
            {
                var rightSys = landRight.GetComponent<ParticleSystem>();
                var leftSys = landLeft.GetComponent<ParticleSystem>();
                if (rightSys != null && leftSys != null)
                {
                    rightSys.Play();
                    leftSys.Play();
                }
            }
            yield return new WaitForSeconds(Owner.AnimControl.GetClip(RuhkAttackState.LeapFinishAnimation).length);
            leapFinishWait = null;
            Owner.StateMachine.ChangeState(new RuhkAttackState(new AttackStatePackage(Owner, target)));
        }

        private void CalcLeapState()
        {
            var distance = (Owner.transform.position - origDestination).magnitude;
            overLeapMax = distance > RuhkAttackState.LeapMaxDistance;
            underLeapMin = distance < RuhkAttackState.LeapMinDistance;
        }
    }
}