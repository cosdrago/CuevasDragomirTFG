using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RedBjorn.SuperTiles.Multiplayer.UI
{
    /// <summary>
    /// UI panel which provides UI login elements
    /// </summary>
    public class LoginUI : MonoBehaviour
    {
        public TMP_InputField Nickname;
        public TMP_Dropdown ServerName;
        public Button LoginButton;

        public void Show(string nickname, string[] servers, string server)
        {
            Nickname.placeholder.GetComponentInChildren<TextMeshProUGUI>().text = nickname;
            var index = -1;
            ServerName.options = new List<TMP_Dropdown.OptionData>();
            for (int i = 0; i < servers.Length; i++)
            {
                if (servers[i] == server)
                {
                    index = i;
                }
                ServerName.options.Add(new TMP_Dropdown.OptionData { text = servers[i] });
            }
            ServerName.value = index != -1 ? index : 0;
            ServerName.RefreshShownValue();
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public string GetNickname()
        {
            var nickname = Nickname.text;
            if (string.IsNullOrEmpty(nickname))
            {
                nickname = Nickname.placeholder.GetComponentInChildren<TextMeshProUGUI>().text;
            }
            return nickname;
        }

        public string GetServername()
        {
            return ServerName.options[ServerName.value].text;
        }
    }
}
