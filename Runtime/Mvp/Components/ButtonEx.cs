﻿using System.Collections;
using Behc.Utils;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace Behc.Mvp.Components
{
    public class ButtonEx : MonoBehaviour, IMoveHandler, IPointerDownHandler, IPointerUpHandler,
        IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler, IPointerClickHandler, ISubmitHandler
	{
		public enum StateType
		{
			DISABLED,
			NORMAL,
			HIGHLIGHTED,
			PRESSED,
		}

		public StateType State { get; private set; } = StateType.NORMAL;
		public bool Highlighted { get; private set; }
		public bool Pressed { get; private set; }
		public bool Selected { get; private set; }

		public bool Toggled
		{
			get => _toggled;
			set
			{
				if (_toggled != value)
				{
					_toggled = value;
					_dirty = true;
					NotifyIfDirty();
				}
			}
		}

		public bool Interactable
		{
			get => _interactable;
			set
			{
				if (_interactable != value)
				{
					_interactable = value;
					_dirty = true;
					UpdateState();
					NotifyIfDirty();
				}
			}
		}

		public float HoverDelay => _options.IsNull() ? 0.2f : _options.HoverDelay;
		public float LongPressDuration => _options.IsNull() ? 0.5f : _options.LongPressDuration;

        public UnityEvent onClick = new UnityEvent();
        public UnityEvent onStateChange = new UnityEvent();
        public UnityEvent onHover = new UnityEvent();
        public UnityEvent onPointerEnter = new UnityEvent();
        public UnityEvent onPointerExit = new UnityEvent();
        public UnityEvent onLongPress = new UnityEvent();
        public UnityEvent onLongPressCancel = new UnityEvent();

#pragma warning disable CS0649
		[SerializeField] private ButtonOptions _options;
		[SerializeField] private bool _interactable = true;
		[SerializeField] private bool _selectOnHighlight;
		[SerializeField] private bool _selectOnClick;
#pragma warning restore CS0649

		private bool _dirty;
		private bool _skipNotify;
		private bool _toggled;

		private bool _longPressFired;
		private Coroutine _hoverCoroutine;
		private Coroutine _longPressCoroutine;

		private void OnDisable()
		{
			StopCoroutines();

			_dirty = false;
			_skipNotify = false;

			_longPressFired = false;

			Highlighted = false;
			Pressed = false;
			Selected = false;
			State = Interactable ? StateType.NORMAL : StateType.DISABLED;
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			if (eventData.pointerId > 0)
			{
				return;
			}

			Highlighted = true;
			UpdateState();

			_skipNotify = true;
			if (_selectOnHighlight)
				EventSystem.current.SetSelectedGameObject(gameObject);

			onPointerEnter.Invoke();
			_skipNotify = false;

			_hoverCoroutine = StartCoroutine(HoverWait());

			NotifyIfDirty();
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			if (eventData.pointerId > 0)
			{
				return;
			}

			StopCoroutines();

			Highlighted = false;
			UpdateState();

			_skipNotify = true;
			onPointerExit.Invoke();
			_skipNotify = false;

			NotifyIfDirty();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			if (eventData.pointerId > 0)
			{
				return;
			}

			if (eventData.button == PointerEventData.InputButton.Left)
			{
				Pressed = true;
				_skipNotify = true;

				if (_selectOnClick)
					EventSystem.current.SetSelectedGameObject(gameObject);

				UpdateState();

				_skipNotify = false;
				NotifyIfDirty();

				_longPressCoroutine = StartCoroutine(LongPressWait());
			}
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			if (eventData.pointerId > 0)
			{
				return;
			}

			if (eventData.button == PointerEventData.InputButton.Left)
			{
				if (_longPressCoroutine != null)
				{
					StopCoroutine(_longPressCoroutine);
					_longPressCoroutine = null;
					onLongPressCancel.Invoke();
				}

				Pressed = false;
				UpdateState();

				_longPressFired = false;

				NotifyIfDirty();
			}
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (eventData.pointerId > 0)
			{
				return;
			}

			if (eventData.button == PointerEventData.InputButton.Left && Interactable && !_longPressFired)
				onClick.Invoke();
		}

		public void OnSubmit(BaseEventData eventData)
		{
			if (Interactable && !_longPressFired)
				onClick.Invoke();
		}

		public void OnMove(AxisEventData eventData)
		{
			//TODO: add method to navigate? delegate to parent?
		}

		public void OnSelect(BaseEventData eventData)
		{
			if (!Selected)
			{
				Selected = true;
				_dirty = true;
				NotifyIfDirty();
			}
		}

		public void OnDeselect(BaseEventData eventData)
		{
			if (Selected)
			{
				Selected = false;
				_dirty = true;
				NotifyIfDirty();
			}
		}

		private void UpdateState()
		{
			StateType state = !Interactable ? StateType.DISABLED : Highlighted ? Pressed ? StateType.PRESSED : StateType.HIGHLIGHTED : StateType.NORMAL;
			if (state != State)
			{
				State = state;
				_dirty = true;
			}
		}

		private void NotifyIfDirty()
		{
			if (_dirty && !_skipNotify)
			{
#if BEHC_MVPTOOLKIT_VERBOSE
                Debug.Log($"GameObject: {gameObject.name} state:{State} selected:{Selected} toggled:{Toggled}");
#endif
				onStateChange.Invoke();
				_dirty = false;
			}
		}

		private IEnumerator HoverWait()
		{
			yield return new WaitForSecondsRealtime(HoverDelay); //TODO: cache
			onHover.Invoke();
			_hoverCoroutine = null;
		}

		private IEnumerator LongPressWait()
		{
			yield return new WaitForSecondsRealtime(LongPressDuration); //TODO: cache
			onLongPress.Invoke();
			_longPressCoroutine = null;
		}

		private void StopCoroutines()
		{
			if (_hoverCoroutine != null)
			{
				StopCoroutine(_hoverCoroutine);
				_hoverCoroutine = null;
			}

			if (_longPressCoroutine != null)
			{
				StopCoroutine(_longPressCoroutine);
				_longPressCoroutine = null;
				onLongPressCancel.Invoke();
			}
		}
	}
}
