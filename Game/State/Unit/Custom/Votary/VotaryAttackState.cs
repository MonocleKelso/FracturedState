using FracturedState.Game.Data;
using FracturedState.Game.Network;
using UnityEngine;

namespace FracturedState.Game.AI
{
    public class VotaryAttackState : CustomState
    {
        private readonly UnitManager target;
        private readonly Weapon weapon;
        private readonly AudioClip[] shootSounds;

        private float deadTargetWaitTime = 3f;
        
        public VotaryAttackState(AttackStatePackage initPackage) : base(initPackage)
        {
            target = initPackage.Target;
            weapon = owner.ContextualWeapon;
            shootSounds = new AudioClip[weapon.SoundEffects.Length];
            for (var i = 0; i < weapon.SoundEffects.Length; i++)
            {
                shootSounds[i] = DataUtil.LoadBuiltInSound(weapon.SoundEffects[i]);
            }
        }

        public override void Execute()
        {
            if (owner == null || !owner.IsAlive) return;
            
            if (FracNet.Instance.IsHost && (owner.NetMsg.OwnerUnit == null || !owner.NetMsg.OwnerUnit.IsAlive))
            {
                owner.NetMsg.CmdTakeDamage(int.MaxValue, null, Weapon.DummyName);
                return;
            }
            
            if (target == null || !target.IsAlive || target.WorldState == Nav.State.Interior)
            {
                owner.IsIdle = true;
                deadTargetWaitTime -= Time.deltaTime;
                if (deadTargetWaitTime <= 0)
                {
                    owner.StateMachine.ChangeState(new UnitIdleState(owner));
                }

                return;
            }

            if (owner.NetMsg.OwnerUnit != null)
            {
                if ((owner.transform.position - owner.NetMsg.OwnerUnit.transform.position).magnitude > 30)
                {
                    owner.StateMachine.ChangeState(new UnitIdleState(owner));
                    return;
                }
            }

            var moveTarget = target.transform.position + Vector3.up;
            
            owner.transform.LookAt(new Vector3(moveTarget.x, owner.transform.position.y, moveTarget.z));
            var toTarget = moveTarget - owner.transform.position;
            
            if (toTarget.magnitude > weapon.Range)
            {
                owner.transform.position = Vector3.MoveTowards(owner.transform.position, moveTarget,
                    owner.Data.Physics.MaxSpeed * Time.deltaTime);
                Space();

                return;
            }

            Space();
            
            if (owner.LastFiredTime + weapon.FireRate > Time.time) return;

            owner.LastFiredTime = Time.time;
            owner.ContextualMuzzleFlash.Play();
            var audio = owner.GetComponent<AudioSource>();
            audio.clip = shootSounds[Random.Range(0, shootSounds.Length)];
            audio.pitch = Random.Range(0.80f, 1.2f);
            audio.Play();

            if (!FracNet.Instance.IsHost) return;

            var damage = target.MitigateDamage(weapon, owner.transform.position);
            target.ProcessDamageInterrupts(damage, owner);
        }

        private void Space()
        {
            var nearby = Physics.OverlapSphere(owner.transform.position, owner.Data.Physics.PathRadius * 1.5f, GameConstants.ExteriorUnitAllMask);
            var count = 0;
            var avg = Vector3.zero;
            foreach (var n in nearby)
            {
                if (n.transform == owner.transform) continue;
                avg += n.transform.position;
                count++;
            }
            if (count == 0) return;
            
            avg /= count;
            var pos = owner.transform.position;
            var y = pos.y;
            pos += (pos - avg).normalized * Time.deltaTime;
            // prevent units from climbing off the ground
            pos.y = y;
            
            var ray = new Ray(pos + Vector3.up * 100, -Vector3.up);
            if (RaycastUtil.RayCheckExterior(ray)) return;
            if (!RaycastUtil.IsUnderTerrain(pos + Vector3.up)) return;
            
            // if we made it here than update unit position to nudge
            owner.transform.position = pos;
        }
    }
}