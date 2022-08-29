using EVE_Bot.Configs;
using EVE_Bot.Controllers;
using EVE_Bot.Models;
using EVE_Bot.Searchers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace EVE_Bot.Scripts
{
    static public class FV
    {
        static public Random r = new Random();
        static public int AvgDeley = Config.AverageDelay;
        static ModulesInfo ModulesInfo = new ModulesInfo();


        static public void StartFarmingFV()
        {
            SecScriptsForFV.CheckReadyForFV();

            FarmFV();
        }

        static public void FarmFV()
        {
            //- на каждой миссии: выбрал цель, взял орбиту вокруг цель по умолчанию(110 + км),
            //      включил скоростной режим и модули, отметил цель атаки, повторил всё тоже самое на остальных окнах


        }



    }
}
