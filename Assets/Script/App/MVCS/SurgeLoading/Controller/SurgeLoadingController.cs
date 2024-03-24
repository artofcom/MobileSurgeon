using System.Collections;
using UnityEngine;
using Core.Events;
using Unity.Services.Core;
using Unity.Services.Authentication;
using System.Threading.Tasks;
using Unity.Services.Analytics;
using System;

namespace App.MVCS
{
    public class SurgeLoadingController : IController
    {
        //  Events ----------------------------------------
        EventsGroup Events = new EventsGroup();

        //  Properties ------------------------------------
        SurgeLoadingModel _model;
        SurgeLoadingView _view;
        SurgeLoadingService _service;
        SurgeContext _context;

        bool IsConfigLoaded = false;
        bool IsServiceInitialized = false;

        bool IsReadyToLogin => IsConfigLoaded && IsServiceInitialized;
        bool IsReadyToSceneTransit = false;

        const string GUESTID_KEY = "GuestId";
        string GuestId = string.Empty;
        const string GuestPW = "Qq654098&*";
        bool IsNewGuestLogin = false;

        //  Methods ---------------------------------------
        //
        public SurgeLoadingController(SurgeLoadingModel model, SurgeLoadingView view, SurgeLoadingService service, SurgeContext context)
        {
            _model = model;
            _view = view;
            _service = service;
            _context = context;

            _view.StartCoroutine(coInit());

            AsyncInitService();

            Events.RegisterEvent("SurgeLoadingView_OnBtnContinueClicked", SurgeLoadingView_OnBtnContinueClicked);
            Events.RegisterEvent("SurgeLoadingView_OnBtnGuestLoginClicked", SurgeLoadingView_OnBtnGuestLoginClicked);
            Events.RegisterEvent("SurgeLoadingView_OnBtnSignUpSubmitClicked", SurgeLoadingView_OnBtnSignUpSubmitClicked);
        }


        // Event Receivers --------------------------------
        //
        void SurgeLoadingView_OnBtnContinueClicked(object data)
        {
            if (!IsReadyToLogin)
            {
                Debug.LogWarning("Initializing App....");
                return;
            }
            AsyncSignIn(_view.LoginID, _view.LoginPassword);
        }

        void SurgeLoadingView_OnBtnGuestLoginClicked(object data)
        {
            if(!IsReadyToLogin)
            {
                Debug.LogWarning("Initializing App....");
                return;
            }

            AsyncGuestLogin();
        }

        void SurgeLoadingView_OnBtnSignUpSubmitClicked(object data)
        {
            if (!IsReadyToLogin)
            {
                Debug.LogWarning("Initializing App....");
                return;
            }

            AsyncSignUp(_view.SignUpID, _view.SignUpPassword);
        }


        //  Loaders --------------------------------
        //
        IEnumerator coInit()
        {
            yield return new WaitUntil(() => _context.Initialized == true);

            _view.StartCoroutine(_context.AnimCtrlFetcher.CoLoadControllerBundle(
                (successed) =>
                {
                    UnityEngine.Assertions.Assert.IsTrue(successed == true);
                    IsConfigLoaded = true;

                }));
        }

        IEnumerator coUpdateProgressBar()
        {
            _view.SwitchToProgressViewGroup();

            const float LoadingViewMinTime = 0.5f;
            float fStartTime = Time.time;

            while (Time.time - fStartTime <= LoadingViewMinTime)
            {
                float progress = (Time.time - fStartTime) / LoadingViewMinTime;
                _view.RefreshProgressBar(progress);
                yield return null;
            }
        }



        //  Auth Section ---------------------------------------
        //
        async void AsyncInitService()
        {
            await UnityServices.InitializeAsync();

            AnalyticsService.Instance.StartDataCollection();

            SetupEvents();

            IsServiceInitialized = true;

            if(!_context.IsSignedOut)
                await SignUserWithCustomTokenWithAutoRefresh();
        }
        async void AsyncGuestLogin()
        {
            await SignInGuestAsync();
        }
        async void AsyncSignIn(string email, string pw)
        {
            await SignInWithUsernamePasswordAsync(email, pw);
        }
        async void AsyncSignUp(string email, string pw)
        {
            await SignUpWithUsernamePassword(email, pw);
        }

        IEnumerator coCheckToTransitView()
        {
            while(true)
            {
                if (IsReadyToSceneTransit)
                    break;

                yield return null;
            }

            yield return _view.StartCoroutine(coUpdateProgressBar());

            EventSystem.DispatchEvent("SurgeLoading_OnSignInSuccessed", (object)true);
        }

        void SetupEvents()
        {
            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log($"PlayerID : {AuthenticationService.Instance.PlayerInfo.Username}");
                Debug.Log($"PlayerName : {AuthenticationService.Instance.PlayerName}");
                Debug.Log($"Access Token : {AuthenticationService.Instance.AccessToken}");

                if(IsNewGuestLogin && !string.IsNullOrEmpty(GuestId))
                {
                    PlayerPrefs.SetString(GUESTID_KEY, GuestId);
                    IsNewGuestLogin = false;
                }

                _view.StartCoroutine(coCheckToTransitView());
            };

            AuthenticationService.Instance.SignInFailed += (err) =>
            {
                var presentData = new MessageDialogView.PresentData();
                presentData.Message = "SignInFailed : " + err.Message;
                _context.TriggerDialog("messageDialog", presentData, (returnedData) => { });
            };

            AuthenticationService.Instance.SignedOut += () =>
            {
                Debug.Log("Player has singed out.");

                _context.IsSignedOut = true;
                EventSystem.DispatchEvent("OnSignedOut");
            };

            AuthenticationService.Instance.Expired += () =>
            {
                Debug.Log("Player session expired.");
            };
        }

        async Task SignInGuestAsync()
        {
            try
            {
                string guestId = PlayerPrefs.GetString(GUESTID_KEY);
                if (string.IsNullOrEmpty(guestId))
                {
                    // Try to generate Unique ID per Device as Possible.
                    string deviceId = SystemInfo.deviceUniqueIdentifier;
                    string ticks = DateTime.UtcNow.Ticks.ToString();
                    string mixer = "";
                    for (int q = 0; q < deviceId.Length; ++q)
                    {
                        char code = q < ticks.Length ? (char)((deviceId[q] + ticks[q]) % 26) : (char)(deviceId[q] % 26);
                        code += 'A';
                        mixer += code;
                    }

                    const int safeLen = 19; // unity auth id len ( 3 - 20 )
                    guestId = mixer;        // other possible unique key to consider -> DateTime.UtcNow.Ticks.ToString();
                    guestId = guestId.Length>=safeLen ? guestId.Remove(safeLen, guestId.Length - safeLen) : guestId;
                    GuestId = guestId;

                    IsNewGuestLogin = true;

                    Debug.Log("Guest Id : " + guestId);
                    await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(guestId, GuestPW);
                }
                else
                {
                    await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(guestId, GuestPW);
                }

                Debug.Log("Sign in anonymousley successed.");
                Debug.Log($"PlayerID : {AuthenticationService.Instance.PlayerId}");

                IsReadyToSceneTransit = true;
            }
            catch (AuthenticationException ex)
            {
                var presentData = new MessageDialogView.PresentData();
                presentData.Message = "GuestLogin Auth Error : " + ex.Message;
                _context.TriggerDialog("messageDialog", presentData, (returnedData) => {});
            }
            catch (RequestFailedException ex)
            {
                var presentData = new MessageDialogView.PresentData();
                presentData.Message = "GuestLogin Req Error : " + ex.Message;
                _context.TriggerDialog("messageDialog", presentData, (returnedData) => { });
            }
        }

        async Task SignInWithUsernamePasswordAsync(string username, string password)
        {
            try
            {
                await AuthenticationService.Instance.SignInWithUsernamePasswordAsync(username, password);
                Debug.Log("SignIn is successful.");

                IsReadyToSceneTransit = true;
            }
            catch (AuthenticationException ex)
            {
                var presentData = new MessageDialogView.PresentData();
                presentData.Message = "SignIn Auth Error : " + ex.Message;
                _context.TriggerDialog("messageDialog", presentData, (returnedData) => { _view.ClearInputField(); });
            }
            catch (RequestFailedException ex)
            {
                var presentData = new MessageDialogView.PresentData();
                presentData.Message = "SignIn Req Error : " + ex.Message;
                _context.TriggerDialog("messageDialog", presentData, (returnedData) => { _view.ClearInputField(); });
            }
        }

        async Task SignUpWithUsernamePassword(string username, string password)
        {
            try
            {
                await AuthenticationService.Instance.SignUpWithUsernamePasswordAsync(username, password);
                Debug.Log("SignUp is successful.");

                var presentData = new MessageDialogView.PresentData();
                presentData.Message = "SignUp is successful.";
                _context.TriggerDialog("messageDialog", presentData, (returnedData) =>
                {
                    IsReadyToSceneTransit = true;
                });
            }
            catch (AuthenticationException ex)
            {
                var presentData = new MessageDialogView.PresentData();
                presentData.Message = "Signup Auth Error : " + ex.Message;
                _context.TriggerDialog("messageDialog", presentData, (returnedData) => { _view.ClearInputField(); });
            }
            catch (RequestFailedException ex)
            {
                var presentData = new MessageDialogView.PresentData();
                presentData.Message = "Signup Req Error : " + ex.Message;
                _context.TriggerDialog("messageDialog", presentData, (returnedData) => { _view.ClearInputField(); });
            }
        }

        async Task SignUserWithCustomTokenWithAutoRefresh()
        {
            try
            {
                // Check if a cached player already exists by checking if the session token exists
                if (AuthenticationService.Instance.SessionTokenExists)
                {
                    // This call will sign in the cached player.
                    //var option = new SignInOptions();
                    //option.CreateAccount = false;
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();// option);
                    Debug.Log("Cached user sign in succeeded!");

                    IsReadyToSceneTransit = true;
                }
                else
                {
                    //var userTokens = "";// Fetch the user tokens using your method calls
                    //AuthenticationService.Instance.ProcessAuthenticationTokens(userTokens.AccessToken, userTokens.SessionToken)
                }
                // Shows how to get the playerID
                Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }
            catch (RequestFailedException ex)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }
            catch (Exception ex)
            {
                // Handle exceptions from your method call
                Debug.LogException(ex);
            }
        }

        async Task AddUsernamePasswordAsync(string username, string password)
        {
            try
            {
                await AuthenticationService.Instance.AddUsernamePasswordAsync(username, password);
                Debug.Log("Username and password added.");
            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }
            catch (RequestFailedException ex)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }
        }

        async Task UpdatePasswordAsync(string currentPassword, string newPassword)
        {
            try
            {
                await AuthenticationService.Instance.UpdatePasswordAsync(currentPassword, newPassword);
                Debug.Log("Password updated.");
            }
            catch (AuthenticationException ex)
            {
                // Compare error code to AuthenticationErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }
            catch (RequestFailedException ex)
            {
                // Compare error code to CommonErrorCodes
                // Notify the player with the proper error message
                Debug.LogException(ex);
            }
        }


    }

}