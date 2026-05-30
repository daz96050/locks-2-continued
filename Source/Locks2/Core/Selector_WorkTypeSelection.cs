using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace Locks2.Core
{
    public class Selector_WorkTypeSelection : ISelector
    {
        public IEnumerable<WorkTypeDef> workTypes;
        public Action<WorkTypeDef> onSelect;
        private Vector2 scrollPosition = Vector2.zero;
        private string searchString = "";

        public Selector_WorkTypeSelection(IEnumerable<WorkTypeDef> workTypes, Action<WorkTypeDef> onSelect,
            bool integrated = false, Action closeAction = null) : base(integrated, closeAction)
        {
            this.workTypes = workTypes;
            this.onSelect = onSelect;
        }

        public override void FillContents(Rect inRect)
        {
            GameFont font = Text.Font;
            try
            {
                Rect searchRect = inRect.TopPartPixels(20);
                if (Widgets.ButtonImage(searchRect.LeftPartPixels(20), TexButton.CloseXSmall))
                {
                    Close();
                }
                Text.Font = GameFont.Tiny;
                string searchBuffer = Widgets.TextField(
                        new Rect(searchRect.position + new Vector2(25, 0), searchRect.size - new Vector2(55, 0)),
                        searchString).ToLower().Trim();
                if (searchBuffer != searchString)
                {
                    scrollPosition = Vector2.zero;
                    searchString = searchBuffer;
                }
                inRect.yMin += 25;
                Rect contentRect = new Rect(0, 0, inRect.width - 20, workTypes.Count() * 40);
                Widgets.DrawMenuSection(inRect);
                Widgets.BeginScrollView(inRect, ref scrollPosition, contentRect);
                Rect currentRect = contentRect.TopPartPixels(40);
                currentRect.xMin += 15;
                Text.Font = GameFont.Small;
                foreach (var workType in workTypes)
                {
                    if (searchString.Length > 0 && !workType.labelShort.ToLower().Contains(searchString))
                    {
                        continue;
                    }
                    Widgets.DrawHighlightIfMouseover(currentRect);
                    Widgets.Label(currentRect, workType.labelShort);
                    if (Widgets.ButtonInvisible(currentRect))
                    {
                        onSelect(workType);
                        Close();
                    }
                    currentRect.y += currentRect.height;
                }
                Widgets.EndScrollView();
            }
            finally
            {
                Text.Font = font;
            }
        }
    }
}
