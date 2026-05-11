using System;
using EscapeRoom.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EscapeRoom.UI
{
    [AddComponentMenu("Escape Room/UI/Password Puzzle Input")]
    [DisallowMultipleComponent]
    public sealed class PasswordPuzzleInput : MonoBehaviour
    {
        [Serializable]
        public sealed class PasswordSubmittedEvent : UnityEvent<string>
        {
        }

        [Header("Input")]
        [SerializeField] private TMP_InputField inputField;
        [SerializeField] private InputField legacyInputField;
        [SerializeField] private TMP_Text displayText;
        [SerializeField] private Text legacyDisplayText;
        [SerializeField] private string expectedPassword = "0000";
        [SerializeField] private int maxLength = 8;
        [SerializeField] private bool caseSensitive = false;
        [SerializeField] private bool trimInput = true;
        [SerializeField] private bool clearInputOnWrongPassword = true;

        [Header("Progress")]
        [SerializeField] private bool setSolvedFlag = true;
        [SerializeField] private ProgressFlagReference solvedFlag;

        [Header("Feedback")]
        [SerializeField] private FeedbackUIController feedbackUI;
        [SerializeField] private bool showInstructionOnEnable = false;
        [SerializeField] private string instructionMessage = "Enter the password.";
        [SerializeField] private string successMessage = "Password accepted.";
        [SerializeField] private string wrongPasswordMessage = "Wrong password.";

        [Header("Events")]
        [SerializeField] private PasswordSubmittedEvent onPasswordSubmitted;
        [SerializeField] private UnityEvent onPasswordCorrect;
        [SerializeField] private UnityEvent onPasswordWrong;

        private string bufferedInput = string.Empty;

        private void Reset()
        {
            inputField = GetComponent<TMP_InputField>();
            legacyInputField = GetComponent<InputField>();
            displayText = GetComponent<TMP_Text>();
            legacyDisplayText = GetComponent<Text>();
        }

        private void OnValidate()
        {
            if (inputField == null)
            {
                inputField = GetComponent<TMP_InputField>();
            }

            if (legacyInputField == null)
            {
                legacyInputField = GetComponent<InputField>();
            }

            if (displayText == null)
            {
                displayText = GetComponent<TMP_Text>();
            }

            if (legacyDisplayText == null)
            {
                legacyDisplayText = GetComponent<Text>();
            }
        }

        private void OnEnable()
        {
            SyncBufferedInputFromField();
            RefreshDisplay();

            if (showInstructionOnEnable)
            {
                ShowInstruction();
            }
        }

        public void SubmitCurrentInput()
        {
            SubmitPassword(GetInputText());
        }

        public void SubmitPassword(string candidatePassword)
        {
            string submittedPassword = Normalize(candidatePassword);
            string targetPassword = Normalize(expectedPassword);
            onPasswordSubmitted?.Invoke(submittedPassword);

            if (PasswordsMatch(submittedPassword, targetPassword))
            {
                HandleCorrectPassword();
            }
            else
            {
                HandleWrongPassword();
            }
        }

        public void AppendCharacter(string character)
        {
            if (string.IsNullOrEmpty(character))
            {
                return;
            }

            string currentText = GetInputText();
            if (maxLength > 0 && currentText.Length >= maxLength)
            {
                return;
            }

            SetInputText(currentText + character);
        }

        public void AppendDigit(int digit)
        {
            AppendCharacter(Mathf.Clamp(digit, 0, 9).ToString());
        }

        public void Backspace()
        {
            string currentText = GetInputText();
            if (string.IsNullOrEmpty(currentText))
            {
                return;
            }

            SetInputText(currentText.Substring(0, currentText.Length - 1));
        }

        public void ClearInput()
        {
            SetInputText(string.Empty);
        }

        public void ShowInstruction()
        {
            FeedbackUIController target = ResolveFeedbackUI();
            if (target != null)
            {
                target.ShowInputInstruction(instructionMessage);
            }
        }

        private void HandleCorrectPassword()
        {
            bool progressWillAnnounce = false;

            if (setSolvedFlag && solvedFlag != null && solvedFlag.IsConfigured && ProgressState.Instance != null)
            {
                progressWillAnnounce = !ProgressState.Instance.HasFlag(solvedFlag);
                ProgressState.Instance.SetTrue(solvedFlag);
            }

            if (!progressWillAnnounce)
            {
                FeedbackUIController target = ResolveFeedbackUI();
                if (target != null)
                {
                    target.ShowSuccess(successMessage);
                }
            }

            onPasswordCorrect?.Invoke();
        }

        private void HandleWrongPassword()
        {
            FeedbackUIController target = ResolveFeedbackUI();
            if (target != null)
            {
                if (string.IsNullOrWhiteSpace(wrongPasswordMessage))
                {
                    target.ShowWrongPassword();
                }
                else
                {
                    target.Show(FeedbackUIController.FeedbackKind.Error, "Wrong password", wrongPasswordMessage);
                }
            }

            if (clearInputOnWrongPassword)
            {
                ClearInput();
            }

            onPasswordWrong?.Invoke();
        }

        private string Normalize(string value)
        {
            string normalized = value ?? string.Empty;
            if (trimInput)
            {
                normalized = normalized.Trim();
            }

            return normalized;
        }

        private bool PasswordsMatch(string submittedPassword, string targetPassword)
        {
            return string.Equals(
                submittedPassword,
                targetPassword,
                caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase);
        }

        private FeedbackUIController ResolveFeedbackUI()
        {
            return feedbackUI != null ? feedbackUI : FeedbackUIController.Instance;
        }

        private string GetInputText()
        {
            if (inputField != null)
            {
                return inputField.text;
            }

            return legacyInputField != null ? legacyInputField.text : bufferedInput;
        }

        private void SetInputText(string value)
        {
            bufferedInput = value ?? string.Empty;

            if (inputField != null)
            {
                inputField.text = bufferedInput;
            }

            if (legacyInputField != null)
            {
                legacyInputField.text = bufferedInput;
            }

            RefreshDisplay();
        }

        private void SyncBufferedInputFromField()
        {
            if (inputField != null)
            {
                bufferedInput = inputField.text ?? string.Empty;
            }
            else if (legacyInputField != null)
            {
                bufferedInput = legacyInputField.text ?? string.Empty;
            }
        }

        private void RefreshDisplay()
        {
            string text = GetInputText();

            if (displayText != null)
            {
                displayText.text = text;
            }

            if (legacyDisplayText != null)
            {
                legacyDisplayText.text = text;
            }
        }
    }
}
