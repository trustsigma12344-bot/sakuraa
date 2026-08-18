using System;
using SakuraaCastingMod.VR.Interaction;
using UnityEngine;

namespace SakuraaCastingMod.VR.UtilMenu.Utility;

public class KeyboardController : MonoBehaviour
{
	private enum State
	{
		Disabled,
		Opening,
		Open,
		Closing
	}

	public static KeyboardController Instance;

	public string currentInput = "";

	public Action<string> OnKeyPressed;

	public Action OnEnterPressed;

	public Action OnBackspacePressed;

	private Vector3 _startPos = new Vector3(0f, -0.005f, 0f);

	private Vector3 _startRot = Vector3.zero;

	private Vector3 _startScale = new Vector3(7f, 7f, 7f);

	private Vector3 _endPos = new Vector3(0f, -0.01767f, 0.003f);

	private Vector3 _endRot = new Vector3(-30.648f, 0f, 0f);

	private Vector3 _endScale = new Vector3(20f, 20f, 20f);

	private float _animDuration = 0.8f;

	private float _currentAnimTime = 0f;

	private State _currentState = State.Disabled;

	public bool IsKeyboardActive => _currentState != State.Disabled;

	private void Awake()
	{
		Instance = this;
		base.transform.localPosition = _startPos;
		base.transform.localEulerAngles = _startRot;
		base.transform.localScale = _startScale;
		base.gameObject.SetActive(value: false);
		_currentState = State.Disabled;
	}

	private void Start()
	{
		InitializeKeys();
	}

	private void Update()
	{
		if (_currentState != State.Opening)
		{
			if (_currentState == State.Closing)
			{
				_currentAnimTime += Time.deltaTime;
				float num = Mathf.Clamp01(_currentAnimTime / _animDuration);
				ApplyAnimation(1f - num);
				if (num >= 1f)
				{
					_currentState = State.Disabled;
					base.gameObject.SetActive(value: false);
				}
			}
		}
		else
		{
			_currentAnimTime += Time.deltaTime;
			float num2 = Mathf.Clamp01(_currentAnimTime / _animDuration);
			ApplyAnimation(num2);
			if (num2 >= 1f)
			{
				_currentState = State.Open;
				ApplyAnimation(1f);
			}
		}
	}

	private void ApplyAnimation(float t)
	{
		float y = Mathf.Lerp(_startPos.y, _endPos.y, t);
		float num = 0.62f;
		float t2 = 0f;
		float z = _startPos.z;
		if (t > num)
		{
			t2 = (t - num) / (1f - num);
			z = Mathf.Lerp(_startPos.z, _endPos.z, t2);
		}
		base.transform.localPosition = new Vector3(_startPos.x, y, z);
		base.transform.localEulerAngles = Vector3.Lerp(_startRot, _endRot, t2);
		base.transform.localScale = Vector3.Lerp(_startScale, _endScale, t2);
	}

	public void OpenKeyboard()
	{
		if (_currentState != State.Open && _currentState != State.Opening)
		{
			base.gameObject.SetActive(value: true);
			_currentState = State.Opening;
			_currentAnimTime = 0f;
		}
	}

	public void CloseKeyboard()
	{
		if (_currentState != State.Disabled && _currentState != State.Closing)
		{
			_currentState = State.Closing;
			_currentAnimTime = 0f;
		}
	}

	private void InitializeKeys()
	{
		foreach (Transform item in base.transform)
		{
			SetupKey(item.gameObject);
		}
	}

	private void SetupKey(GameObject keyObj)
	{
		keyObj.layer = 18;
		if (UtilMenuController.Instance != null && !UtilMenuController.Instance.requiredLayerObjs.Contains(keyObj))
		{
			UtilMenuController.Instance.requiredLayerObjs.Add(keyObj);
		}
		if (keyObj.GetComponent<Collider>() == null)
		{
			BoxCollider boxCollider = keyObj.AddComponent<BoxCollider>();
			boxCollider.isTrigger = true;
		}
		UtilFingerButton utilFingerButton = keyObj.GetComponent<UtilFingerButton>();
		if (utilFingerButton == null)
		{
			utilFingerButton = keyObj.AddComponent<UtilFingerButton>();
		}
		utilFingerButton.allowLeftHand = true;
		if (UtilMenuMain.Instance != null)
		{
			utilFingerButton.unpressedMaterial = UtilMenuMain.Instance.buttonMat;
			utilFingerButton.pressedMaterial = UtilMenuMain.Instance.selectedBtnMat;
			utilFingerButton.UpdateColor();
		}
		string keyName = keyObj.name;
		utilFingerButton.SetButtonListener(delegate
		{
			HandleInput(keyName);
		});
	}

	private void HandleInput(string keyName)
	{
		if (_currentState != State.Open)
		{
			return;
		}
		HapticEngine.Play(HapticPreset.KeyboardKey);
		string text = keyName.ToLower().Trim();
		switch (text)
		{
		case "enter":
			OnEnterPressed?.Invoke();
			CloseKeyboard();
			break;
		case "backspace":
			if (currentInput.Length > 0)
			{
				currentInput = currentInput.Substring(0, currentInput.Length - 1);
				OnBackspacePressed?.Invoke();
				OnKeyPressed?.Invoke(currentInput);
			}
			break;
		default:
			if (text.Length == 1)
			{
				currentInput += text;
				OnKeyPressed?.Invoke(currentInput);
			}
			break;
		case "space":
			currentInput += " ";
			OnKeyPressed?.Invoke(currentInput);
			break;
		}
	}

	public void ClearInput()
	{
		currentInput = "";
		OnKeyPressed?.Invoke(currentInput);
	}
}
