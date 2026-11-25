using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Each button has one hook; both mouse/touch and keyboard/controller submit work.
public class AudioButton : MonoBehaviour, IPointerClickHandler, ISubmitHandler
{
    public void OnPointerClick(PointerEventData data) { if(data.button==PointerEventData.InputButton.Left)Click(); }
    public void OnSubmit(BaseEventData data)=>Click();
    void Click() {var b=GetComponent<Button>();if(b!=null && b.IsInteractable())AdventureAudio.Play(SoundEvent.Click);}
}
