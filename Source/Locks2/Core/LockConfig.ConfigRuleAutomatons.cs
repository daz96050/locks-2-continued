using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using RimWorld;
using UnityEngine;
using Verse;

namespace Locks2.Core
{
    public partial class LockConfig
    {
        public class ConfigRuleAutomatons : IConfigRule
        {
            private static Type automatonType;
            private static Type compAutomatonType;
            private static bool typesResolved;

            public bool enabled = true;

            public static bool IsAvailable => ModsConfig.IsActive("Neronix17.Asimov");

            public override float Height => 54 + 15;

            private static void ResolveTypes()
            {
                if (typesResolved) return;
                typesResolved = true;
                foreach (var mod in LoadedModManager.RunningMods)
                {
                    foreach (var assembly in mod.assemblies.loadedAssemblies)
                    {
                        if (automatonType == null)
                            automatonType = assembly.GetType("Asimov.Automaton", false);
                        if (compAutomatonType == null)
                            compAutomatonType = assembly.GetType("Asimov.Comp_Automaton", false);
                        if (automatonType != null && compAutomatonType != null) return;
                    }
                }
            }

            /// <summary>
            /// Checks if a pawn is an Asimov automaton. Used by other rules to exclude automatons
            /// from generic colonist/drafted checks so they are only handled by this rule.
            /// </summary>
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public static bool IsAutomaton(Pawn pawn)
            {
                if (!IsAvailable) return false;
                ResolveTypes();
                // Check if pawn is an instance of the Automaton class (animal-type automatons)
                if (automatonType != null && automatonType.IsInstanceOfType(pawn))
                    return true;
                // Check if pawn has the Comp_Automaton component (covers humanlike automatons too)
                if (compAutomatonType != null)
                {
                    var comps = pawn.AllComps;
                    for (int i = 0; i < comps.Count; i++)
                    {
                        if (compAutomatonType.IsInstanceOfType(comps[i]))
                            return true;
                    }
                }
                return false;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override bool Allows(Pawn pawn)
            {
                return enabled && IsAutomaton(pawn) && (pawn.factionInt?.IsPlayer ?? false);
            }

            public override void DoContent(IEnumerable<Pawn> pawns, Rect rect, Action notifySelectionBegan,
                Action notifySelectionEnded)
            {
                var before = enabled;
                Widgets.CheckboxLabeled(rect.TopPartPixels(54), "Locks2AutomatonsFilter".Translate(), ref enabled);
                if (before != enabled)
                {
                    Notify_Dirty();
                    Find.CurrentMap.reachability.ClearCache();
                }
            }

            public override IConfigRule Duplicate()
            {
                return new ConfigRuleAutomatons { enabled = enabled };
            }

            public override void ExposeData()
            {
                Scribe_Values.Look(ref enabled, "enabled", true);
            }
        }
    }
}





