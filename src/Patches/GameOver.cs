using BattleTech;

namespace TBD.Patches
{
    internal class GameOver
    {
        /// <summary>
        /// Adds a custom game-over event that can be used on mission failure.
        /// </summary>
        [HarmonyPatch(typeof(SimGameState), nameof(SimGameState.ApplyEventAction))]
        public class SimGameState_ApplyEventAction
        {
            [HarmonyPrefix]
            public static bool Prefix(SimGameResultAction action)
            {
                if (action.Type == SimGameResultAction.ActionType.System_PlayVideo && action.value == "mcb_exit_bt")
                {
                    SimGameState simGame = UnityGameInstance.BattleTechGame.Simulation;
                    simGame.CompanyStats.Set("Funds", -10000000);
                    simGame.InterruptQueue.QueueLossOutcome();
                    return false;
                }
                return true;
            }
        }
    }
}
