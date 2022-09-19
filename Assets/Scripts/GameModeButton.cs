using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BurnTheRope
{
    public enum GameMode
    {
        Play,
        Edit
    }
    public class GameModeButton : MonoBehaviour
    {
        public static GameMode gameMode;
        private TMP_Text _buttonText;

        private void Start()
        {
            _buttonText = transform.GetComponentInChildren<TMP_Text>();
            gameMode = GameMode.Play;
            SetButtonText();
        }

        public void OnClick()
        {
            gameMode = gameMode == GameMode.Play ? GameMode.Edit : GameMode.Play;
            SetButtonText();
        }

        private void SetButtonText()
        {
            _buttonText.text = $"{gameMode.ToString()} Mode"; 
        }
    }
}