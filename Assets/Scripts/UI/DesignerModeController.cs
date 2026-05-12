using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

namespace EscapeRoom.UI
{
    [AddComponentMenu("Escape Room/UI/Designer Mode Controller")]
    [DisallowMultipleComponent]
    public sealed class DesignerModeController : MonoBehaviour
    {
        [Serializable]
        private sealed class DesignerHintElement
        {
            public GameObject target;
            public string title = "Designer Hint";

            [TextArea(2, 5)]
            public string hint = "Write this object's designer hint here.";
        }

        [Header("Input")]
        [SerializeField] private InputActionProperty inspectAction;
        [SerializeField] private XRRayInteractor rayInteractor;
        [SerializeField] private Transform fallbackRayOrigin;
        [SerializeField] private float fallbackRayDistance = 12f;
        [SerializeField] private LayerMask fallbackRayMask = ~0;
        [SerializeField] private QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore;

        [Header("Flow")]
        [SerializeField] private DesignerHintElement[] elements = new DesignerHintElement[0];
        [SerializeField] private bool advanceAfterInspect = true;
        [SerializeField] private bool loopAfterLastElement = false;

        [Header("Intro Message")]
        [SerializeField] private string introTitle = "Designer Mode";

        [TextArea(2, 5)]
        [SerializeField] private string introMessage = "Use the ray and press B to inspect the highlighted object.";

        [SerializeField] private float introDuration = -1f;

        [Header("Feedback")]
        [SerializeField] private FeedbackUIController feedbackUI;
        [SerializeField] private FeedbackUIController.FeedbackKind hintKind = FeedbackUIController.FeedbackKind.Info;
        [SerializeField] private float hintDuration = -1f;

        [Header("Highlight")]
        [SerializeField] private Color highlightColor = new Color(1f, 0.48f, 0.05f, 1f);
        [SerializeField] private bool includeChildRenderers = true;
        [SerializeField] private bool useEmission = true;
        [SerializeField] private float emissionIntensity = 1.5f;

        [Header("Debug")]
        [SerializeField] private bool debugLogRayHits = false;

        private const string BaseColorProperty = "_BaseColor";
        private const string ColorProperty = "_Color";
        private const string EmissionProperty = "_EmissionColor";

        private HighlightSnapshot[] highlightedRenderers = new HighlightSnapshot[0];
        private InputAction fallbackInspectAction;
        private int currentIndex;
        private bool designerModeActive;

        private struct HighlightSnapshot
        {
            public Renderer renderer;
            public Material[] materials;
            public Color[] baseColors;
            public Color[] colors;
            public Color[] emissionColors;
            public bool[] hadBaseColor;
            public bool[] hadColor;
            public bool[] hadEmission;
        }

        private void Reset()
        {
            feedbackUI = FindFirstObjectByType<FeedbackUIController>();
            rayInteractor = FindFirstObjectByType<XRRayInteractor>();
            fallbackRayOrigin = Camera.main != null ? Camera.main.transform : null;
        }

        private void OnEnable()
        {
            EnableInspectAction();
        }

        private void OnDisable()
        {
            EndDesignerMode();
            DisableInspectAction();
        }

        private void Update()
        {
            if (!designerModeActive || !WasInspectPressedThisFrame())
            {
                return;
            }

            TryInspectCurrentElement();
        }

        public void BeginDesignerMode()
        {
            designerModeActive = true;
            currentIndex = 0;
            EnableInspectAction();
            ApplyCurrentHighlight();

            FeedbackUIController target = ResolveFeedbackUI();
            if (target != null && !string.IsNullOrWhiteSpace(introMessage))
            {
                target.Show(FeedbackUIController.FeedbackKind.Info, introTitle, introMessage, ResolveDisplayDuration(introDuration), true);
            }
        }

        public void EndDesignerMode()
        {
            designerModeActive = false;
            RestoreHighlight();
        }

        public void InspectCurrentElement()
        {
            if (!designerModeActive)
            {
                return;
            }

            ShowCurrentHintAndAdvance();
        }

        private void TryInspectCurrentElement()
        {
            DesignerHintElement element = GetCurrentElement();
            if (element == null || element.target == null)
            {
                LogRayDebug("No current designer element or target.");
                return;
            }

            if (!IsRayPointingAt(element.target))
            {
                return;
            }

            ShowCurrentHintAndAdvance();
        }

        private void ShowCurrentHintAndAdvance()
        {
            DesignerHintElement element = GetCurrentElement();
            if (element == null || element.target == null)
            {
                AdvanceToNextElement();
                return;
            }

            FeedbackUIController target = ResolveFeedbackUI();
            if (target != null)
            {
                string title = string.IsNullOrWhiteSpace(element.title) ? "Designer Hint" : element.title;
                target.Show(hintKind, title, element.hint, ResolveDisplayDuration(hintDuration), true);
            }

            if (advanceAfterInspect)
            {
                AdvanceToNextElement();
            }
        }

        private void AdvanceToNextElement()
        {
            if (elements == null || elements.Length == 0)
            {
                RestoreHighlight();
                return;
            }

            int nextIndex = currentIndex + 1;
            if (nextIndex >= elements.Length)
            {
                if (!loopAfterLastElement)
                {
                    RestoreHighlight();
                    return;
                }

                nextIndex = 0;
            }

            currentIndex = nextIndex;
            ApplyCurrentHighlight();
        }

        private DesignerHintElement GetCurrentElement()
        {
            if (elements == null || elements.Length == 0)
            {
                return null;
            }

            currentIndex = Mathf.Clamp(currentIndex, 0, elements.Length - 1);
            return elements[currentIndex];
        }

        private bool WasInspectPressedThisFrame()
        {
            InputAction action = inspectAction.action;
            if (action != null && action.WasPressedThisFrame())
            {
                return true;
            }

            if (fallbackInspectAction != null && fallbackInspectAction.WasPressedThisFrame())
            {
                return true;
            }

            if (Keyboard.current != null && Keyboard.current.bKey.wasPressedThisFrame)
            {
                return true;
            }

            return Gamepad.current != null && Gamepad.current.buttonEast.wasPressedThisFrame;
        }

        private bool IsRayPointingAt(GameObject target)
        {
            ResolveRayInteractor();

            if (rayInteractor != null && rayInteractor.TryGetCurrent3DRaycastHit(out RaycastHit xrHit))
            {
                bool matches = HitBelongsToTarget(xrHit.collider, target);
                LogRayDebug($"XR ray hit '{xrHit.collider.name}', current target='{target.name}', matches={matches}.");
                return matches;
            }

            Transform origin = rayInteractor != null
                ? rayInteractor.rayOriginTransform != null ? rayInteractor.rayOriginTransform : rayInteractor.transform
                : fallbackRayOrigin != null
                ? fallbackRayOrigin
                : Camera.main != null ? Camera.main.transform : null;
            if (origin == null)
            {
                LogRayDebug("No ray interactor and no fallback ray origin.");
                return false;
            }

            Ray ray = new Ray(origin.position, origin.forward);
            float rayDistance = rayInteractor != null
                ? Mathf.Max(0.01f, rayInteractor.maxRaycastDistance)
                : Mathf.Max(0.01f, fallbackRayDistance);
            if (!Physics.Raycast(ray, out RaycastHit hit, rayDistance, fallbackRayMask, triggerInteraction))
            {
                LogRayDebug($"Fallback ray from '{origin.name}' hit nothing, current target='{target.name}'.");
                return false;
            }

            bool fallbackMatches = HitBelongsToTarget(hit.collider, target);
            LogRayDebug($"Fallback ray from '{origin.name}' hit '{hit.collider.name}', current target='{target.name}', matches={fallbackMatches}.");
            return fallbackMatches;
        }

        private static bool HitBelongsToTarget(Collider hitCollider, GameObject target)
        {
            if (hitCollider == null || target == null)
            {
                return false;
            }

            Transform hitTransform = hitCollider.transform;
            Transform targetTransform = target.transform;
            return hitTransform == targetTransform ||
                   hitTransform.IsChildOf(targetTransform) ||
                   targetTransform.IsChildOf(hitTransform);
        }

        private void ApplyCurrentHighlight()
        {
            RestoreHighlight();

            DesignerHintElement element = GetCurrentElement();
            if (element == null || element.target == null)
            {
                return;
            }

            Renderer[] renderers = includeChildRenderers
                ? element.target.GetComponentsInChildren<Renderer>(true)
                : element.target.GetComponents<Renderer>();

            highlightedRenderers = new HighlightSnapshot[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                highlightedRenderers[i] = CaptureAndHighlight(renderers[i]);
            }
        }

        private HighlightSnapshot CaptureAndHighlight(Renderer renderer)
        {
            Material[] materials = renderer.materials;
            HighlightSnapshot snapshot = new HighlightSnapshot
            {
                renderer = renderer,
                materials = materials,
                baseColors = new Color[materials.Length],
                colors = new Color[materials.Length],
                emissionColors = new Color[materials.Length],
                hadBaseColor = new bool[materials.Length],
                hadColor = new bool[materials.Length],
                hadEmission = new bool[materials.Length],
            };

            Color emissionColor = highlightColor * Mathf.Max(0f, emissionIntensity);
            for (int i = 0; i < materials.Length; i++)
            {
                Material material = materials[i];
                if (material == null)
                {
                    continue;
                }

                snapshot.hadBaseColor[i] = material.HasProperty(BaseColorProperty);
                snapshot.hadColor[i] = material.HasProperty(ColorProperty);
                snapshot.hadEmission[i] = material.HasProperty(EmissionProperty);

                if (snapshot.hadBaseColor[i])
                {
                    snapshot.baseColors[i] = material.GetColor(BaseColorProperty);
                    material.SetColor(BaseColorProperty, highlightColor);
                }

                if (snapshot.hadColor[i])
                {
                    snapshot.colors[i] = material.GetColor(ColorProperty);
                    material.SetColor(ColorProperty, highlightColor);
                }

                if (useEmission && snapshot.hadEmission[i])
                {
                    snapshot.emissionColors[i] = material.GetColor(EmissionProperty);
                    material.EnableKeyword("_EMISSION");
                    material.SetColor(EmissionProperty, emissionColor);
                }
            }

            return snapshot;
        }

        private void RestoreHighlight()
        {
            if (highlightedRenderers == null)
            {
                highlightedRenderers = new HighlightSnapshot[0];
                return;
            }

            for (int i = 0; i < highlightedRenderers.Length; i++)
            {
                RestoreSnapshot(highlightedRenderers[i]);
            }

            highlightedRenderers = new HighlightSnapshot[0];
        }

        private static void RestoreSnapshot(HighlightSnapshot snapshot)
        {
            if (snapshot.renderer == null || snapshot.materials == null)
            {
                return;
            }

            for (int i = 0; i < snapshot.materials.Length; i++)
            {
                Material material = snapshot.materials[i];
                if (material == null)
                {
                    continue;
                }

                if (snapshot.hadBaseColor != null && snapshot.hadBaseColor[i])
                {
                    material.SetColor(BaseColorProperty, snapshot.baseColors[i]);
                }

                if (snapshot.hadColor != null && snapshot.hadColor[i])
                {
                    material.SetColor(ColorProperty, snapshot.colors[i]);
                }

                if (snapshot.hadEmission != null && snapshot.hadEmission[i])
                {
                    material.SetColor(EmissionProperty, snapshot.emissionColors[i]);
                }
            }
        }

        private void EnableInspectAction()
        {
            InputAction action = inspectAction.action;
            if (action != null && !action.enabled)
            {
                action.Enable();
            }

            if (fallbackInspectAction == null)
            {
                fallbackInspectAction = new InputAction(
                    "Designer Mode B Button",
                    InputActionType.Button,
                    "<XRController>{RightHand}/secondaryButton");
            }

            if (!fallbackInspectAction.enabled)
            {
                fallbackInspectAction.Enable();
            }
        }

        private void DisableInspectAction()
        {
            InputAction action = inspectAction.action;
            if (action != null && action.enabled)
            {
                action.Disable();
            }

            if (fallbackInspectAction != null && fallbackInspectAction.enabled)
            {
                fallbackInspectAction.Disable();
            }
        }

        private FeedbackUIController ResolveFeedbackUI()
        {
            if (feedbackUI == null)
            {
                feedbackUI = FeedbackUIController.Instance != null
                    ? FeedbackUIController.Instance
                    : FindFirstObjectByType<FeedbackUIController>();
            }

            return feedbackUI;
        }

        private void ResolveRayInteractor()
        {
            if (rayInteractor != null)
            {
                return;
            }

            XRRayInteractor[] interactors = FindObjectsByType<XRRayInteractor>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            if (interactors == null || interactors.Length == 0)
            {
                return;
            }

            for (int i = 0; i < interactors.Length; i++)
            {
                XRRayInteractor candidate = interactors[i];
                if (candidate != null && candidate.name.IndexOf("Right", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    rayInteractor = candidate;
                    return;
                }
            }

            rayInteractor = interactors[0];
        }

        private static float ResolveDisplayDuration(float duration)
        {
            return duration < 0f ? 0f : duration;
        }

        private void LogRayDebug(string message)
        {
            if (!debugLogRayHits)
            {
                return;
            }

            Debug.Log($"[DesignerModeController] {message}", this);
        }
    }
}
