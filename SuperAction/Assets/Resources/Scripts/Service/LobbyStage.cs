using System.Threading.Tasks;
using UnityEngine;

namespace Resources.Scripts.Service
{
    public class LobbyStage : MonoBehaviour
    {
        public async Task ReadySurvivalMode(PlayerData playerData)
        {
            var stage = StageLoader.Instance.LoadStage(new StageData()
            {
                StageType = StageType.Test,
                Participants = new[]
                {
                    new ParticipantData()
                    {
                        Owner = playerData,
                        Color = Color.cyan,
                        ControllerType = 1,
                    },
                },
            });
            
            await Game.Instance.StartGame(stage);
        }
    }
}
