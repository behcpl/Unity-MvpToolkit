using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Behc.Mvp.Components
{
    public abstract class Curtain : MonoBehaviour, IPointerClickHandler, IPointerDownHandler
    {
        public enum TriggerModeType
        {
            ANY_BUTTON_DOWN,
            LEFT_BUTTON_DOWN,
            RIGHT_BUTTON_DOWN,
            MIDDLE_BUTTON_DOWN,
            ANY_BUTTON_CLICK,
            LEFT_BUTTON_CLICK,
            RIGHT_BUTTON_CLICK,
            MIDDLE_BUTTON_CLICK,
        }

        public UnityEvent OnTrigger = new UnityEvent();
        public TriggerModeType TriggerMode = TriggerModeType.ANY_BUTTON_CLICK;

        public abstract void Setup(bool visible, int order);
        public abstract void Show(int order);
        public abstract void Switch(int newOrder, int previousOrder);
        public abstract void Hide();


        public void OnPointerClick(PointerEventData eventData)
        {
            switch (TriggerMode)
            {
                case TriggerModeType.ANY_BUTTON_CLICK:
                case TriggerModeType.LEFT_BUTTON_CLICK when eventData.button == PointerEventData.InputButton.Left:
                case TriggerModeType.RIGHT_BUTTON_CLICK when eventData.button == PointerEventData.InputButton.Right:
                case TriggerModeType.MIDDLE_BUTTON_CLICK when eventData.button == PointerEventData.InputButton.Middle:
                    OnTrigger.Invoke();
                    break;
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            switch (TriggerMode)
            {
                case TriggerModeType.ANY_BUTTON_DOWN:
                case TriggerModeType.LEFT_BUTTON_DOWN when eventData.button == PointerEventData.InputButton.Left:
                case TriggerModeType.RIGHT_BUTTON_DOWN when eventData.button == PointerEventData.InputButton.Right:
                case TriggerModeType.MIDDLE_BUTTON_DOWN when eventData.button == PointerEventData.InputButton.Middle:
                    OnTrigger.Invoke();
                    break;
            }
        }
    }
}