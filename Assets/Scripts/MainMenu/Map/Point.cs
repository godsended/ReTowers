using UnityEngine;
using UnityEngine.UI;

public class Point : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Animator _animation;

    public void SetPointActiv(bool state) 
    {
        _button.interactable = state;
        if (_animation != null)
        {
            _animation.enabled = state;
        }
    }
}
