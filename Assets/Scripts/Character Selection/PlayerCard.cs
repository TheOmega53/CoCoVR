using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerCard : MonoBehaviour
{
    [SerializeField] private CharacterDatabase characterDatabase;

    [SerializeField] private GameObject visuals;

    [SerializeField] private Image characterIconImage;

    [SerializeField] private TMP_Text playerNameText;

    [SerializeField] private TMP_Text CharacterNameText;


    public void UpdateDisplay(CharacterSelectState state)
    {
        if(state.CharacterId != -1)
        {
            var character = characterDatabase.GetCharacterById(state.CharacterId);
            characterIconImage.sprite = character.Icon;
            characterIconImage.enabled = true;
            CharacterNameText.text = character.DisplayName;
        }
        else
        {
            characterIconImage.enabled = false;
        }

        playerNameText.text = state.IsLockedIn ? $"Player {state.ClientId + 1}" : $"Player {state.ClientId + 1} (Picking...)";
        visuals.SetActive(true);
    }

    public void DisableDisplay()
    {
        visuals.SetActive(false);
    }
}
