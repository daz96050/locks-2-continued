using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using UnityEngine;
using Verse;

namespace Locks2.Core
{
    public partial class LockConfig
    {
        public class ConfigRuleWorkType : IConfigRule
        {
            public bool enabled;
            public WorkTypeDef requiredWorkType;
            public bool skillFilterEnabled;
            public int minSkillLevel = 1;

            private string buffer = "1";

            public override float Height => (enabled ? 75 + (skillFilterEnabled ? 50 : 25) : 54) + 15;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override bool Allows(Pawn pawn)
            {
                if (!enabled || requiredWorkType == null) return false;
                if (pawn.workSettings == null) return false;
                if (pawn.WorkTypeIsDisabled(requiredWorkType)) return false;
                if (!pawn.workSettings.WorkIsActive(requiredWorkType)) return false;

                if (skillFilterEnabled && requiredWorkType.relevantSkills != null && requiredWorkType.relevantSkills.Count > 0)
                {
                    var primarySkill = requiredWorkType.relevantSkills[0];
                    var skill = pawn.skills?.GetSkill(primarySkill);
                    if (skill == null || skill.Level < minSkillLevel) return false;
                }

                return true;
            }

            public override IConfigRule Duplicate()
            {
                return new ConfigRuleWorkType
                {
                    enabled = enabled,
                    requiredWorkType = requiredWorkType,
                    skillFilterEnabled = skillFilterEnabled,
                    minSkillLevel = minSkillLevel
                };
            }

            public override void DoContent(IEnumerable<Pawn> pawns, Rect rect, Action notifySelectionBegan,
                Action notifySelectionEnded)
            {
                var before = enabled;
                Text.Font = GameFont.Small;
                Widgets.CheckboxLabeled(rect.TopPartPixels(25), "Locks2WorkTypeFilter".Translate(), ref enabled);
                Text.Font = GameFont.Tiny;

                if (enabled)
                {
                    Widgets.Label(rect.TopPartPixels(50).BottomPartPixels(25), "Locks2WorkTypeFilterBody".Translate());
                    var rowRect = rect.TopPartPixels(75).BottomPartPixels(25);

                    string workLabel = requiredWorkType != null ? requiredWorkType.labelShort : "Locks2WorkTypeNone".Translate();
                    if (Widgets.ButtonText(rowRect, workLabel))
                    {
                        notifySelectionBegan.Invoke();
                        var allWorkTypes = DefDatabase<WorkTypeDef>.AllDefs
                            .Where(w => w.visible);
                        ITab_Lock.currentSelector = new Selector_WorkTypeSelection(allWorkTypes, wt =>
                        {
                            requiredWorkType = wt;
                            Notify_Dirty();
                            Find.CurrentMap.reachability.ClearCache();
                        }, true, notifySelectionEnded);
                    }

                    rowRect.y += 25;
                    Text.Font = GameFont.Tiny;
                    Widgets.CheckboxLabeled(rowRect, "Locks2WorkTypeSkillFilter".Translate(), ref skillFilterEnabled);

                    if (skillFilterEnabled)
                    {
                        rowRect.y += 25;
                        Text.Font = GameFont.Tiny;
                        Widgets.Label(rowRect.LeftHalf(), "Locks2WorkTypeMinSkill".Translate());
                        Text.Font = GameFont.Small;
                        Widgets.TextFieldNumeric(rowRect.RightHalf(), ref minSkillLevel, ref buffer, 0, 20);
                    }
                }

                if (before != enabled) Notify_Dirty();
            }

            public override void ExposeData()
            {
                Scribe_Values.Look(ref enabled, "enabled", true);
                Scribe_Defs.Look(ref requiredWorkType, "requiredWorkType");
                Scribe_Values.Look(ref skillFilterEnabled, "skillFilterEnabled");
                Scribe_Values.Look(ref minSkillLevel, "minSkillLevel", 1);
            }
        }
    }
}
