﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Glader.Essentials
{
	/// <summary>
	/// The implementation of adaptation between <see cref="Button"/> and <see cref="IUIButton"/>.
	/// </summary>
	public sealed class UnityButtonUIButtonAdapterImplementation : BaseUnityUIAdapterImplementation, IUIButton
	{
		private UnityEngine.UI.Button UnityButton { get; }

		/// <inheritdoc />
		protected override string LoggableComponentName => UnityButton.gameObject.name;

		/// <inheritdoc />
		public UnityButtonUIButtonAdapterImplementation([NotNull] Button unityButton)
		{
			UnityButton = unityButton ?? throw new ArgumentNullException(nameof(unityButton));
		}

		/// <inheritdoc />
		public void AddOnClickListener(Action action)
		{
			if(action == null) throw new ArgumentNullException(nameof(action));

			UnityButton.onClick.AddListener(() => action());
		}

		/// <inheritdoc />
		public void AddOnClickListenerAsync(Func<Task> action)
		{
			if(action == null) throw new ArgumentNullException(nameof(action));

			//Supporting async button events from the Unity engine button is abit complex.
			AddOnClickListener(() => AsyncUnityEngineButtonCallbackHandler(action));
		}

		/// <inheritdoc />
		public bool IsInteractable
		{
			get => UnityButton.interactable;
			set => UnityButton.interactable = value;
		}

		public void SimulateClick(bool eventsOnly)
		{
			if(eventsOnly)
				UnityButton.onClick?.Invoke();
			else
				ExecuteEvents.Execute(UnityButton.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
		}

		private void AsyncUnityEngineButtonCallbackHandler(Func<Task> action)
		{
			if(action == null) throw new ArgumentNullException(nameof(action));

			//When this is called, the button has been clicked and we need async button handling.
			//This will call the async Method, get the task and create a coroutine that awaits it (for exception handling purposes)

			//We can't use the Button MonoBehaviour because it might be deactivated, we have to use a global behaviour
			UnityAsyncHelper.UnityUIAsyncContinuationBehaviour.StartCoroutine(AsyncCallbackHandler(action()));
		}
	}
}
