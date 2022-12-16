using UnityEngine;
using UnityEngine.SceneManagement;

public class TakeEnergyOnClick : MonoBehaviour
{
    public void TakeAwayEnergyOnClick(int count) 
    {
        MainMenu.Registration.PlayfabManager.TakeAwayEnergy(count);
    }
    
    public void LoadSceneBattle() 
    {
        SceneManager.LoadScene("Battle");
    } 
}
