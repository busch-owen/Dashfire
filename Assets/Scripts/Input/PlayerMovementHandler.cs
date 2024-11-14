using System;
using UnityEngine;

public class PlayerMovementHandler : MonoBehaviour
{
    private CharacterControls _characterControls;
    
    private void OnEnable()
    {
        if (_characterControls != null) return;

        _characterControls = new CharacterControls();
    }

    private void OnDestroy()
    {
        Destroy(this);
    }
}
