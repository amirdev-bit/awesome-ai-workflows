using System;
using Oathsunder.Combat.Input;
using Oathsunder.Controls;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Oathsunder.Gameplay.Controls
{
    /// <summary>
    /// Runs "press the new button" rebinding for one logical button, then writes the result into the player's
    /// <see cref="ControlProfile"/> (stealing the control from any other button so there are never duplicates)
    /// and persists it. The remap screen (Phase 12) drives this component.
    /// </summary>
    public sealed class InteractiveRebinder : MonoBehaviour
    {
        [SerializeField] private PlayerInputSource _source;

        private InputActionRebindingExtensions.RebindingOperation _operation;

        /// <summary>True while waiting for the player to press a control.</summary>
        public bool IsRebinding => _operation != null;

        /// <summary>
        /// Starts listening for a new control for <paramref name="button"/>, replacing the binding at
        /// <paramref name="bindingIndex"/> (0 = gamepad slot, 1 = keyboard slot in the default profile).
        /// </summary>
        /// <param name="onFinished">Called with the button that lost the control (None when nothing was displaced), or null when cancelled.</param>
        public void Begin(InputButtons button, int bindingIndex, Action<InputButtons?> onFinished)
        {
            Cancel();
            var action = _source.ActionFor(button);
            if (action == null || bindingIndex < 0 || bindingIndex >= action.bindings.Count)
            {
                onFinished?.Invoke(null);
                return;
            }

            string previousPath = action.bindings[bindingIndex].effectivePath;
            action.Disable();
            _operation = action.PerformInteractiveRebinding(bindingIndex)
                .WithControlsExcluding("<Mouse>/position")
                .WithControlsExcluding("<Mouse>/delta")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnMatchWaitForAnother(0.1f)
                .OnComplete(operation =>
                {
                    string newPath = action.bindings[bindingIndex].effectivePath;
                    Finish();
                    var profile = _source.Profile;
                    InputButtons displaced = profile.Rebind(button, newPath, previousPath);
                    _source.ApplyProfile(profile);
                    onFinished?.Invoke(displaced);
                })
                .OnCancel(operation =>
                {
                    Finish();
                    action.Enable();
                    onFinished?.Invoke(null);
                })
                .Start();
        }

        /// <summary>Cancels a pending rebind.</summary>
        public void Cancel()
        {
            _operation?.Cancel();
            Finish();
        }

        private void Finish()
        {
            _operation?.Dispose();
            _operation = null;
        }

        private void OnDisable() => Cancel();
    }
}
