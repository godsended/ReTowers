using System;
using System.Collections.Generic;
using Core.Client;
using Core.Contracts;
using Mirror;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;
using UnityEngine;

namespace MainMenu.Registration
{
    public class PlayfabManager : MonoBehaviour
    {
        public static PlayfabManager instance;
        
        [Header("UI")] 
        public TextMeshProUGUI messageText;
        public TMP_InputField emailInput;
        public TMP_InputField usernameInput;
        public TMP_InputField passwordInput;
        public TMP_InputField confirmpasswordInput;
        public TMP_InputField emailForPassResetInput;
        public static int winCount;

        public static string playerId;
        
        [SerializeField] 
        private GameObject authWindow;
        [SerializeField] 
        private GameObject welcomeWindow;

        private void Start()
        {
            if (PlayerPrefs.HasKey("email") && PlayerPrefs.HasKey("password")) 
            {
                emailInput.text = PlayerPrefs.GetString("email");
                passwordInput.text = PlayerPrefs.GetString("password");
            }
        }

        public void RegisterButton()
        {
            if (passwordInput.text.Length < 6)
            {
                messageText.text = "Password too short!";
                messageText.color = Color.red;
                return;
            }
            if (confirmpasswordInput.text != passwordInput.text)
            {
                messageText.text = "Passwords are not equal";
                messageText.color = Color.red;
                return;
            }
            
            var request = new RegisterPlayFabUserRequest
            {
                Username = usernameInput.text,
                DisplayName = usernameInput.text,
                Email = emailInput.text,
                Password = passwordInput.text,
                RequireBothUsernameAndEmail = true
            };
                PlayFabClientAPI.RegisterPlayFabUser(request, OnRegisterSuccess, OnError);
        }

        public void LoginWithEmailButton()
        {
            var request = new LoginWithEmailAddressRequest
            {
                Email = emailInput.text,
                Password = passwordInput.text,
                InfoRequestParameters = new GetPlayerCombinedInfoRequestParams
                {
                    GetPlayerProfile = true
                }
            };
            PlayFabClientAPI.LoginWithEmailAddress(request, OnLoginSuccess, OnError);
        }
        
        public void LoginWithUsernameButton()
        {
            var request = new LoginWithPlayFabRequest
            {
                Username = usernameInput.text,
                Password = passwordInput.text
            };
            PlayFabClientAPI.LoginWithPlayFab(request, OnLoginSuccess, OnError);
        }

        public void ResetPasswordButton()
        {
            var request = new SendAccountRecoveryEmailRequest
            {
                Email = emailForPassResetInput.text,
                TitleId = "E10CA"
            };
            PlayFabClientAPI.SendAccountRecoveryEmail(request, OnPasswordReset, OnError);
        }

        public static void TakeAwayEnergy(int energy)
        {
            MainClient.SetEnergyCount(MainClient.GetEnergyCount() - energy);
            SendEnergy(MainClient.GetEnergyCount());
            PlayerInfoUI.instance.UpdatePlayerInfo();
        }

        public static void SendEnergy(int energy)
        {
            var request = new UpdatePlayerStatisticsRequest
            {
                Statistics = new List<StatisticUpdate>
                {
                    new StatisticUpdate
                    {
                        StatisticName = "Energy",
                        Value = energy
                    }
                }
            };
            PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdate, OnLeaderboardError);
            MainClient.SetEnergyCount(energy);
            PlayerInfoUI.instance.UpdatePlayerInfo();
        }

        public static void SendLeaderboard(int winCount)
        {
            var request = new UpdatePlayerStatisticsRequest
            {
                Statistics = new List<StatisticUpdate>
                {
                    new StatisticUpdate
                    {
                        StatisticName = "WinsCount",
                        Value = winCount
                    }
                }
            };
            PlayFabClientAPI.UpdatePlayerStatistics(request, OnLeaderboardUpdate, OnLeaderboardError);
        }

        public void GetLeaderboard()
        {
            var request = new GetLeaderboardRequest
            {
                StatisticName = "WinsCount",
                StartPosition = 0,
                MaxResultsCount = 10
            };
            PlayFabClientAPI.GetLeaderboard(request, OnLeaderboardGet, OnError);
        }
        
        public void GetStatistics()
        {
            List<string> statNames = new List<string>() {"WinsCount", "Energy" };

            PlayFabClientAPI.GetPlayerStatistics(
                new GetPlayerStatisticsRequest()
                {
                    StatisticNames = statNames
                },
                OnGetStatistics,
                onGetStatisticsError => Debug.Log("error"));
        }

        private void Awake()
        {
            instance = this;
        }

        private static void OnLeaderboardUpdate(UpdatePlayerStatisticsResult updatePlayerStatisticsResult)
        {
            Debug.Log("Statistics updated");
        }

        private static void OnGetStatistics(GetPlayerStatisticsResult result)
        {
            List<string> namesStatistics = new List<string>();
            for(int i = 0; i < result.Statistics.Count; i++) 
            {
                namesStatistics.Add(result.Statistics[i].StatisticName);

                switch (result.Statistics[i].StatisticName) 
                {
                    case "WinsCount":
                        MainClient.SetWinCount(result.Statistics[i].Value);
                        break;
                    case "Energy":
                        MainClient.SetEnergyCount(result.Statistics[i].Value);
                        break;
                }
            }

            if (!namesStatistics.Contains("WinsCount"))
                SendLeaderboard(0);
            if (!namesStatistics.Contains("Energy"))
            {
                SendEnergy(20);
                MainClient.SetEnergyCount(20);
            }
            PlayerInfoUI.instance.UpdatePlayerInfo();

            MenuInterfaceController.ChangeStateInteface(true);
        }

        private void OnRegisterSuccess(RegisterPlayFabUserResult result)
        {
            SaveDataForLogin(emailInput.text, passwordInput.text);

            MenuInterfaceController.ChangeStateInteface(false);
            messageText.color = Color.white;
            messageText.text = "Registered and logged in!";
            authWindow.SetActive(false);
            playerId = result.PlayFabId;
            MainClient.SetUsername(result.Username);
            PlayerInfoUI.instance.UpdatePlayerInfo();
            
            NetworkClient.Send(new AuthDto
            {
                Login = result.Username,
                Name = result.Username,
                IsGuest = false,
                LastLoginTime = DateTime.MinValue.ToString()
            });
            SendLeaderboard(0);
            SendEnergy(20);  
            Invoke("GetStatistics", 3);
            Core.Cards.LibraryCards.GetCardsFromDataBase();
            PlayerInfoUI.instance.UpdatePlayerInfo();
        }

        private void OnLoginSuccess(LoginResult result)
        {
            SaveDataForLogin(emailInput.text, passwordInput.text);

            MenuInterfaceController.ChangeStateInteface(false);
            messageText.color = Color.white;
            messageText.text = "Logged in successfully";
            authWindow.SetActive(false);
            
            playerId = result.PlayFabId;
            MainClient.SetUsername(result.InfoResultPayload.PlayerProfile.DisplayName);

            NetworkClient.Send(new AuthDto
            {
                Login = result.InfoResultPayload.PlayerProfile.PlayerId,
                Name = result.InfoResultPayload.PlayerProfile.DisplayName,
                IsGuest = false,
                LastLoginTime = result.LastLoginTime.ToString()
                
            });
            Invoke("GetStatistics", 3);
            PlayerInfoUI.instance.UpdatePlayerInfo();
            Core.Cards.LibraryCards.GetCardsFromDataBase();
        }

        private void OnPasswordReset(SendAccountRecoveryEmailResult result)
        {
            messageText.color = Color.white;
            messageText.text = "Password reset mail sent!";
        }

        private void OnError(PlayFabError error)
        {
            messageText.color = Color.red;
            messageText.text = error.ErrorMessage;
            Debug.Log(error.GenerateErrorReport());
        }
        
        private static void OnLeaderboardError(PlayFabError error)
        {
            Debug.Log(error.GenerateErrorReport());
        }

        public static void OnLeaderboardGet(GetLeaderboardResult result)
        {
            foreach (var item in result.Leaderboard)
            {
                Debug.Log(item.Position + " " + item.DisplayName + " " + item.StatValue);
            }
        }

        private void SaveDataForLogin(string email, string passwrod) 
        {
            PlayerPrefs.SetString("email", email);
            PlayerPrefs.SetString("password", passwrod);
            PlayerPrefs.Save();
        }
        
    }
}
