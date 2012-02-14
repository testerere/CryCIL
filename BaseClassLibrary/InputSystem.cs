﻿using System.Runtime.CompilerServices;

using System.Collections.Generic;

namespace CryEngine
{
	public class InputSystem
	{
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		extern internal static void _RegisterAction(string actionName);

		public static void RegisterAction(string actionName, InputActionDelegate actionDelegate)
		{
			if (inputActionDelegates == null)
				inputActionDelegates = new Dictionary<string, InputActionDelegate>();

			actionName = actionName.ToLower();
			if (!inputActionDelegates.ContainsKey(actionName))
			{
				inputActionDelegates.Add(actionName, actionDelegate);

				_RegisterAction(actionName);
			}
			else
				Console.LogAlways("[Warning] Attempted to register duplicate input action {0}", actionName);
		}

		public delegate void InputActionDelegate(ActionActivationMode activationMode, float value);

		public static void OnActionTriggered(string action, ActionActivationMode activationMode, float value)
		{
			if (inputActionDelegates.ContainsKey(action))
				inputActionDelegates[action](activationMode, value);
			else
				Console.LogAlways("Attempted to invoke unregistered action {0}", action);
		}

		private static Dictionary<string, InputActionDelegate> inputActionDelegates;
	}

	public enum ActionActivationMode
	{
		Invalid = 0,
		/// <summary>
		/// Used when the action key is pressed
		/// </summary>
		OnPress,
		/// <summary>
		/// Used when the action key is released
		/// </summary>
		OnRelease,
		/// <summary>
		/// Used when the action key is held
		/// </summary>
		OnHold,
		Always,

		Retriggerable,
		NoModifiers,
		ConsoleCmd,
		/// <summary>
		/// Used when analog compare op succeeds
		/// </summary>
		AnalogCmd
	}
}
