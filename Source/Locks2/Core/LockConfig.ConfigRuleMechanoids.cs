using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;

namespace Locks2.Core
{
    public partial class LockConfig
    {
        public class ConfigRuleMechanoids : IConfigRule
        {
            public bool enabled = true;

            public override float Height => 54 + 15;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override bool Allows(Pawn pawn)
            {
                return enabled && (pawn?.RaceProps?.IsMechanoid ?? false) && (pawn.factionInt?.IsPlayer ?? false);
            }

            public override void DoContent(IEnumerable<Pawn> pawns, Rect rect, Action notifySelectionBegan,
                Action notifySelectionEnded)
            {
                var before = enabled;
                Widgets.CheckboxLabeled(rect.TopPartPixels(54), "Locks2MechanoidsFilter".Translate(), ref enabled);
                if (before != enabled)
                {
                    Notify_Dirty();
                    Find.CurrentMap.reachability.ClearCache();
                }
            }

            public override IConfigRule Duplicate()
            {
                return new ConfigRuleMechanoids { enabled = enabled };
            }

            public override void ExposeData()
            {
                Scribe_Values.Look(ref enabled, "enabled", true);
            }
        }
    }
}
