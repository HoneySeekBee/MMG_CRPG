using Contracts.Protos;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.WebRequestMethods;


namespace Game.Scenes.Login
{
    public class RegistAccount : MonoBehaviour
    {
        public LoginSceneController LoginSceneScript;
        public Button SignUpButton;
        public TMP_InputField RegAccountInput, RegPasswordInput, RegNicknameInput;

        private void Awake()
        {
            if (SignUpButton)
            {
                SignUpButton.onClick.RemoveListener(OnClickSignUp);
                SignUpButton.onClick.AddListener(OnClickSignUp);
            }
        }

        void OnClickSignUp()
        {
            var acc = (RegAccountInput?.text ?? "").Trim();
            var pw = RegPasswordInput?.text ?? "";
            var nn = (RegNicknameInput?.text ?? "").Trim();

            // 클라측 기본 검증
            if (string.IsNullOrEmpty(acc) || string.IsNullOrEmpty(pw) || string.IsNullOrEmpty(nn))
            {
                LoginSceneScript.Popup?.Show("아이디/비밀번호/닉네임을 입력하세요.");
                return;
            }
            if (pw.Length < 8) { LoginSceneScript.Popup?.Show("비밀번호는 8자 이상이어야 합니다."); return; }

            StartCoroutine(CoSignUp(acc, pw, nn));
        }

        IEnumerator CoSignUp(string account, string password, string nickname)
        {
            LoginSceneScript.Spinner?.Show(true);
            if (SignUpButton) SignUpButton.interactable = false;

            var req = new RegisterAuthRequest { Account = account, Password = password, Nickname = nickname };

            bool ok = false;
            string playerId = null, access = null, refresh = null;
            long serverMs = 0;

            yield return LoginSceneScript.Http.Post(ApiRoutes.AuthRegister, req, AuthResponse.Parser, (res) =>
            {
                if (!res.Ok)
                {
                    LoginSceneScript.Spinner?.Show(false);
                    // 상태코드 매핑
                    if (res.StatusCode == 409) LoginSceneScript.Popup?.Show("이미 사용 중인 계정입니다.");
                    else if (res.StatusCode == 400) LoginSceneScript.Popup?.Show("입력 형식이 올바르지 않습니다.");
                    else LoginSceneScript.Popup?.Show($"회원가입 실패: {res.Message}");
                    return;
                }
                ok = true;
                playerId = res.Data.PlayerId;
                access = res.Data.AccessToken;
                refresh = res.Data.RefreshToken;
                serverMs = res.Data.ServerUnixMs;
            });

            if (!ok) { if (SignUpButton) SignUpButton.interactable = true; yield break; }

            // 공통 후처리 (토큰 저장 → 부트스트랩 → 로비)
            yield return LoginSceneScript.AfterAuth(true, playerId, access, refresh, serverMs);
            this.gameObject.SetActive(false);
        }
    }

}