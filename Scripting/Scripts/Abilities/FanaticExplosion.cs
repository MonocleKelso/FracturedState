using FracturedState.Game;
using FracturedState.Game.AI;
using FracturedState.Game.Data;
using FracturedState.Game.Network;
using UnityEngine;

namespace FracturedState.Scripting
{
    public class FanaticExplosion : LocationAbility, IMonitorAbility
    {
        private class MoveToPlaceState : UnitMoveState
        {
            private readonly FanaticExplosion explosion;
            public MoveToPlaceState(UnitManager owner, Vector3 destination, FanaticExplosion explosion)
                : base(owner, destination)
            {
                this.explosion = explosion;
            }
            
            public override void Execute()
            {
                base.Execute();
                if ((Owner.transform.position - Destination).magnitude < Range)
                {
                    OnArrival();
                }
            }

            protected override void AttackMoveEnemySearch()
            {
                // intentionally empty so we don't stop units
            }
            
            protected override void OnArrival()
            {
                explosion.Channel();
            }
        }

        private const float Range = 21;
        
        private const float TravelTime = 1;
        private const float ChannelTime = 3;

        private const string EffectPath = "Fanatic/FanaticAstral";
        private const string WeaponName = "FanaticExplosion";
        
        private enum State { Start, Channel, End, Complete }

        private State _currentState;
        private Vector3 _casterPosition;
        private float _startTime;

        private GameObject _projection;
        private Weapon _weapon;

        private ParticleSystem _explosion;
        private const string ExplosionName = "Fanatic/AstralExplosion";
        private const float ExplosionLife = 12;

        private const string ChainEffectName = "Fanatic/AstralChain";
        private GameObject _chainEffect;
        private GameObject _chainDoppleEffect;
        
        private const string AnimationStartName = "AstralProjection";

        private bool _chargeAnimationPlayed;

        private MoveToPlaceState internalState;
        
        public FanaticExplosion(UnitManager caster, Vector3 location, Ability ability) : base(caster, location, ability) { }

        public override void ExecuteAbility()
        {
            if ((caster.transform.position - location).magnitude > Range)
            {
                internalState = new MoveToPlaceState(caster, location, this);
                internalState.Enter();
            }
            else
            {
                Channel();
            }
        }

        private void Channel()
        {
            _weapon = XmlCacheManager.Weapons[WeaponName];
            caster.AcceptInput = false;
            caster.transform.LookAt(location);
            _currentState = State.Start;
            _casterPosition = caster.transform.position;
            _startTime = Time.time;
            caster.AnimControl.Play(AnimationStartName, PlayMode.StopAll);
            caster.GetComponent<AudioSource>().clip = DataUtil.LoadBuiltInSound("Fanatic/Doppleganger_Launch");
            caster.GetComponent<AudioSource>().Play();
            if (caster.IsMine)
            {
                UnitBarkManager.Instance.AbilityBark(ability);
            }
        }
        
        public void Update()
        {
            if ((caster.transform.position - location).magnitude > Range)
            {
                internalState?.Execute();
                return;
            }
            
            switch (_currentState)
            {
                case State.Start:
                    var done = MoveProjection(_casterPosition, location);
                    if (_chainEffect == null)
                    {
                        _chainEffect = DataUtil.LoadBuiltInParticleSystem(ChainEffectName);
                        _chainEffect.SetLayerRecursively(caster.gameObject.layer);
                        _chainEffect.GetComponent<particleAttractorLinear>().target =
                            caster.transform.GetChildByName("Spine2");
                    }

                    if (_chainDoppleEffect == null)
                    {
                        _chainDoppleEffect = DataUtil.LoadBuiltInParticleSystem(ChainEffectName);
                        _chainDoppleEffect.SetLayerRecursively(caster.gameObject.layer);
                        _chainDoppleEffect.GetComponent<particleAttractorLinear>().target =
                            _projection.transform.GetChildByName("Spine2");
                    }
                    _chainEffect.transform.position = _projection.transform.position + Vector3.up;
                    _chainDoppleEffect.transform.position = caster.transform.position + Vector3.up;
                    if (done >= TravelTime)
                    {
                        _startTime = 0;
                        _currentState = State.Channel;
                        _explosion = ParticlePool.Instance.GetSystem(ExplosionName);
                        _explosion.transform.position = location + Vector3.up;
                        _explosion.gameObject.SetLayerRecursively(caster.gameObject.layer);
                        var life = _explosion.GetComponent<Lifetime>() ??
                                   _explosion.gameObject.AddComponent<Lifetime>();
                        life.SetLifetime(ExplosionLife);
                        _explosion.Play();
                    }
                    break;
                case State.Channel:
                    _startTime += Time.deltaTime;
                    if (!_chargeAnimationPlayed)
                    {
                        _projection.GetComponentInChildren<Animation>().Play("AstralCharge", PlayMode.StopAll);
                        _chargeAnimationPlayed = true;
                    }
                    if (_startTime >= ChannelTime)
                    {
                        _currentState = State.End;
                    }
                    break;
                case State.End:
                    DealDamage();
                    _explosion.gameObject.GetComponent<AudioSource>().Play();
                    _currentState = State.Complete;
                    break;
                case State.Complete:
                    caster.StateMachine.ChangeState(new UnitIdleState(caster));
                    break;
                default:
                    return;
            }
        }

        private float MoveProjection(Vector3 start, Vector3 end)
        {
            var t = Time.time - _startTime;
            var p = t / TravelTime;
            GetProjection().transform.position = Vector3.Lerp(start, end, p);
            return p;
        }

        private GameObject GetProjection()
        {
            if (_projection != null) return _projection;
            _projection = DataUtil.LoadBuiltInParticleSystem(EffectPath);
            _projection.SetLayerRecursively(caster.gameObject.layer);
            _projection.transform.position = _casterPosition;
            _projection.transform.LookAt(location);
            _projection.GetComponentInChildren<Animation>().Play("AstralFly", PlayMode.StopAll);
            return _projection;
        }

        private void DealDamage()
        {
            _projection.SetActive(false);
            _chainDoppleEffect.GetComponent<particleAttractorLinear>().enabled = false;
            if (!FracNet.Instance.IsHost) return;
            
            var nearby = Physics.OverlapSphere(location, _weapon.BlastRadius, caster.GetEnemyLayerMask());
            if (nearby == null) return;
            foreach (var n in nearby)
            {
                var unit = n.GetComponent<UnitManager>();
                if (unit == null || !unit.IsAlive) continue;

                var hitChance = Random.Range(0, 101);
                
                if (hitChance > _weapon.Accuracy) continue;
                
                if (!unit.InCover)
                {
                    SendDamage(unit);
                }
                else
                {
                    foreach (var cp in unit.CurrentCover.CoverPoints)
                    {
                        if (cp.name == unit.CurrentCoverPoint.Name)
                        {
                            hitChance += unit.CurrentCoverPoint.GetBonus(cp, location);
                        }
                    }
                    if (hitChance < _weapon.Accuracy)
                    {
                        SendDamage(unit);
                    }
                }
            }
        }

        private void SendDamage(UnitManager unit)
        {
            var dist = (unit.transform.position - location).magnitude;
            var dmg = Mathf.Lerp(_weapon.Damage, _weapon.MinDamage, dist / _weapon.BlastRadius);
            var mDmg = unit.MitigateDamage(_weapon, Mathf.RoundToInt(dmg), location);
            unit.NetMsg.CmdTakeProjectileDamage(mDmg, null, location, _weapon.Name);
        }

        public void Finish()
        {
            if (_projection != null) Object.Destroy(_projection);
            if (_chainEffect != null) Object.Destroy(_chainEffect.gameObject, ExplosionLife);
            if (_chainDoppleEffect != null) Object.Destroy(_chainDoppleEffect.gameObject, ExplosionLife);
            if (_explosion != null && (caster == null || !caster.IsAlive)) _explosion.Stop();
            if (caster != null)
            {
                caster.UseAbility(ability.Name);
                caster.AcceptInput = true;
            }
        }
    }
}