using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EscapeRoom.UI
{
    [AddComponentMenu("Escape Room/UI/Escape Room Timer")]
    [DisallowMultipleComponent]
    public sealed class EscapeRoomTimer : MonoBehaviour
    {
        [Header("Timer")]
        [SerializeField] private bool countDown = true;
        [SerializeField] private float startingSeconds = 480f;
        [SerializeField] private bool startOnEnable = false;
        [SerializeField] private bool resetOnEnable = true;

        [Header("Display")]
        [SerializeField] private TMP_Text timerText;
        [SerializeField] private Text legacyTimerText;
        [SerializeField] private string label = "Time";
        [SerializeField] private bool hideDisplayWhenStopped = false;

        [Header("Warnings")]
        [SerializeField] private bool warnNearEnd = true;
        [SerializeField] private float warningThresholdSeconds = 30f;
        [SerializeField] private string warningMessage = "Time is running out.";
        [SerializeField] private string expiredMessage = "Time is up.";

        [Header("Events")]
        [SerializeField] private UnityEvent onWarningThresholdReached;
        [SerializeField] private UnityEvent onTimerExpired;

        private float currentSeconds;
        private bool isRunning;
        private bool warningTriggered;
        private bool expiredTriggered;

        public float CurrentSeconds => currentSeconds;
        public bool IsRunning => isRunning;

        private void Awake()
        {
            currentSeconds = Mathf.Max(0f, startingSeconds);
        }

        private void OnEnable()
        {
            if (resetOnEnable)
            {
                ResetTimer();
            }

            if (startOnEnable)
            {
                StartTimer();
            }
            else
            {
                RefreshDisplay();
            }
        }

        private void Update()
        {
            if (!isRunning)
            {
                return;
            }

            if (countDown)
            {
                TickCountdown();
            }
            else
            {
                currentSeconds += Time.deltaTime;
            }

            RefreshDisplay();
        }

        public void StartTimer()
        {
            isRunning = true;
            expiredTriggered = false;
            RefreshDisplay();
        }

        public void StartTimer(float seconds)
        {
            SetSeconds(seconds);
            StartTimer();
        }

        public void PauseTimer()
        {
            isRunning = false;
            RefreshDisplay();
        }

        public void ResumeTimer()
        {
            StartTimer();
        }

        public void StopTimer()
        {
            isRunning = false;
            RefreshDisplay();
        }

        public void ResetTimer()
        {
            currentSeconds = Mathf.Max(0f, startingSeconds);
            warningTriggered = false;
            expiredTriggered = false;
            RefreshDisplay();
        }

        public void SetSeconds(float seconds)
        {
            currentSeconds = Mathf.Max(0f, seconds);
            warningTriggered = false;
            expiredTriggered = false;
            RefreshDisplay();
        }

        public void AddSeconds(float seconds)
        {
            currentSeconds = Mathf.Max(0f, currentSeconds + seconds);

            if (countDown && currentSeconds > warningThresholdSeconds)
            {
                warningTriggered = false;
            }

            if (currentSeconds > 0f)
            {
                expiredTriggered = false;
            }

            RefreshDisplay();
        }

        private void TickCountdown()
        {
            currentSeconds = Mathf.Max(0f, currentSeconds - Time.deltaTime);

            if (warnNearEnd &&
                !warningTriggered &&
                warningThresholdSeconds > 0f &&
                currentSeconds > 0f &&
                currentSeconds <= warningThresholdSeconds)
            {
                warningTriggered = true;
                FeedbackUIController target = FeedbackUIController.Instance;
                if (target != null)
                {
                    target.Show(FeedbackUIController.FeedbackKind.Warning, "Timer", warningMessage);
                }

                onWarningThresholdReached?.Invoke();
            }

            if (!expiredTriggered && currentSeconds <= 0f)
            {
                expiredTriggered = true;
                isRunning = false;

                FeedbackUIController target = FeedbackUIController.Instance;
                if (target != null)
                {
                    target.Show(FeedbackUIController.FeedbackKind.Error, "Timer", expiredMessage, 0f);
                }

                onTimerExpired?.Invoke();
            }
        }

        private void RefreshDisplay()
        {
            string text = BuildDisplayText();

            if (timerText != null || legacyTimerText != null)
            {
                bool show = !hideDisplayWhenStopped || isRunning;
                if (timerText != null)
                {
                    timerText.text = text;
                    timerText.gameObject.SetActive(show);
                }

                if (legacyTimerText != null)
                {
                    legacyTimerText.text = text;
                    legacyTimerText.gameObject.SetActive(show);
                }

                return;
            }

            FeedbackUIController target = FeedbackUIController.Instance;
            if (target == null)
            {
                return;
            }

            if (hideDisplayWhenStopped && !isRunning)
            {
                target.ClearTimer();
            }
            else
            {
                target.SetTimerText(text);
            }
        }

        private string BuildDisplayText()
        {
            string time = FormatTime(currentSeconds);
            return string.IsNullOrWhiteSpace(label) ? time : $"{label} {time}";
        }

        private static string FormatTime(float seconds)
        {
            TimeSpan timeSpan = TimeSpan.FromSeconds(Mathf.Max(0f, seconds));
            return timeSpan.Hours > 0
                ? $"{timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}"
                : $"{timeSpan.Minutes:00}:{timeSpan.Seconds:00}";
        }
    }
}
