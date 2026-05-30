using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Verse;

namespace Locks2.Core
{
    public static class Extensions
    {
        private static readonly ConcurrentDictionary<int, int> _pawnSignature = new ConcurrentDictionary<int, int>();
        private static readonly ConcurrentDictionary<int, LockComp> _cache = new ConcurrentDictionary<int, LockComp>();

        public static LockConfig GetConfig(this Building door)
        {
            if (_cache.TryGetValue(door.thingIDNumber, out var comp)) return comp.config;
            comp = door.GetComp<LockComp>();
            if (comp == null)
            {
                if (Finder.debug)
                {
                    var message = string.Format("LOCKS2: Partially patched door {0}, {1}", door.def.defName, door);
                    Log.Warning(message);
                }

                return null;
            }

            if (comp.config == null)
            {
                comp.config = new LockConfig { door = door };
                comp.config.Initailize();
            }

            _cache[door.thingIDNumber] = comp;
            return comp.config;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetKey(this Pawn pawn)
        {
            int hash;
            unchecked
            {
                hash = pawn.GetSignature();
                hash = pawn.thingIDNumber.GetHashCode() ^ (hash << 1);
            }
            return hash;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetSignature(this Pawn pawn, bool dirty = false)
        {
            int signature;
            if (dirty || !_pawnSignature.TryGetValue(pawn.thingIDNumber, out signature))
            {
                signature = Rand.Int;
                _pawnSignature[pawn.thingIDNumber] = signature;
            }
            return signature;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Notify_Dirty(this Pawn pawn)
        {
            var _ = pawn.GetSignature(true);
        }
    }
}