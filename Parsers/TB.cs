using EVE_Bot.Models;
using EVE_Bot.Scripts;
using EVE_Bot.Searchers;
using read_memory_64_bit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EVE_Bot.Parsers
{
    static public class TB
    {
        static public List<TargetInBar> GetInfo()
        {
            var TargetInBarEntry = UITreeReader.GetUITrees("TargetInBar", 6, true);
            if (TargetInBarEntry == null)
                return null;

            List<TargetInBar> TargetsInBar = new List<TargetInBar>();

            for (int i = 0; i < TargetInBarEntry.children.Length; i++)
            {
                TargetInBar Target = new TargetInBar();

                var (XTarget, YTarget) = General.GetCoordsEntityOnScreen(TargetInBarEntry.children[i]);

                Target.Pos.x = XTarget + 55;
                Target.Pos.y = YTarget + 55;

                var valueStr = TargetInBarEntry.children[i].children[1].children.Last().dictEntriesOfInterest["_setText"].ToString();
                int value;
                int.TryParse(string.Join("", valueStr.Where(c => char.IsDigit(c))), out value);

                Target.Distance.value = value;

                Target.Distance.measure = valueStr.Split().Last();

                Target.Name = TargetInBarEntry.children[i].children[1].children[0].dictEntriesOfInterest["_setText"].ToString();

                if (TargetInBarEntry.children[i].children[0].children[0].children[3].pythonObjectTypeName == "ActiveTargetOnBracket")
                {
                    Target.AimOnTargetLocked = true;
                }

                //effects
                if (TargetInBarEntry.children[i].children.Last().children[0].children != null
                    &&
                    TargetInBarEntry.children[i].children.Last().children[0].children[0].pythonObjectTypeName == "Weapon")
                {
                    Target.WeaponWorking = true;
                }

                TargetsInBar.Add(Target);
            }
            return TargetsInBar;
        }
    }
}
