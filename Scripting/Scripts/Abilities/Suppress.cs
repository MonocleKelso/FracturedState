using System.Collections.Generic;
using FracturedState.Game;
using FracturedState.Game.Data;
using UnityEngine;

namespace FracturedState.Scripting
{
    public class Suppress : LocationAbility
    {
        public const string AbilityName = "Suppress";
        
        public Suppress(UnitManager caster, Vector3 location, Ability ability) : base(caster, location, ability) { }

        public override void ExecuteAbility()
        {
            // check to see if we actually clicked on a structure before burning the skill
            var hit = RaycastUtil.RaycastExterior(location + Vector3.up * 100);
            if (hit.collider == null) return;
            
            var structure = hit.collider.transform.GetAbsoluteParent().GetComponent<StructureManager>();
            if (structure == null) return;

            caster.AcceptInput = false;
            caster.UseAbility(ability.Name);
            caster.RemoveAbility(ability.Name);
            if (!caster.Data.CanSuppress || (!caster.IsMine && !caster.AISimulate)) return;
            
            var fp = structure.AllFirePoints;
            if (fp == null || fp.Count == 0) return;

            var points = new List<Transform>(); 
            foreach (var p in fp)
            {
                // don't attack firepoints facing away from us
                if (Vector3.Dot((p.position - caster.transform.position).normalized, p.forward) > 0) continue;
                
                points.Add(p);
            }

            if (points.Count == 0) return;

            var po = points[Random.Range(0, points.Count)];
            caster.NetMsg.CmdSuppressPoint(structure.GetComponent<Identity>().UID, po.name);
        }
    }
}