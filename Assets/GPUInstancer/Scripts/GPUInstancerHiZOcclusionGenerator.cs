using UnityEngine;
using UnityEngine.UI;

namespace GPUInstancer
{
    public class GPUInstancerHiZOcclusionGenerator : MonoBehaviour
    {
        public bool debuggerEnabled = false;
        public bool debuggerGUIOnTop = false;
        [Range(0f, 0.1f)]
        public float debuggerOverlay = 0;

        [HideInInspector]
        public RenderTexture hiZDepthTexture;
        [HideInInspector]
        public Texture unityDepthTexture;
        [HideInInspector]
        public Vector2 hiZTextureSize;
        [HideInInspector]
        public bool isVREnabled;

        private Camera _mainCamera;
        private Vector2 _previousScreenSize;
        private bool _isInvalid;

        // Debugger:
        private RawImage _hiZDebugDepthTextureGUIImage;
        private GameObject _hiZOcclusionDebuggerGUI;
        private bool _debuggerGUIOnTopCached;
        private float _debuggerOverlayCached;
        
        #region MonoBehaviour Methods

        private void Awake()
        {
            hiZTextureSize = Vector2.zero;
            GPUInstancerConstants.SetupComputeTextureUtils();
        }

        private void OnEnable()
        {
#if UNITY_2018_1_OR_NEWER // The SRP classes exist only in Unity 2018 and later. 
            if (GPUInstancerConstants.gpuiSettings.isLWRP || GPUInstancerConstants.gpuiSettings.isHDRP)
#if UNITY_2019_1_OR_NEWER // In Unity 2019, the SRP classes are removed from "Experimental".
                UnityEngine.Rendering.RenderPipelineManager.endCameraRendering += OnEndCameraRenderingSRP;
#else
                // Also, the "endCameraRendering" method was added in 2019, so we use beginCameraRendering to get the depth info from the previous frame.
                UnityEngine.Experimental.Rendering.RenderPipeline.beginCameraRendering += OnEndCameraRendering;
#endif
            else
#endif
                Camera.onPostRender += OnEndCameraRendering;
        }

        private void OnDisable()
        {
#if UNITY_2018_1_OR_NEWER
            if (GPUInstancerConstants.gpuiSettings.isLWRP || GPUInstancerConstants.gpuiSettings.isHDRP)
#if UNITY_2019_1_OR_NEWER
                UnityEngine.Rendering.RenderPipelineManager.endCameraRendering -= OnEndCameraRenderingSRP;
#else
                UnityEngine.Experimental.Rendering.RenderPipeline.beginCameraRendering -= OnEndCameraRendering;
#endif
            else
#endif
                Camera.onPostRender -= OnEndCameraRendering;

            if (hiZDepthTexture != null)
            {
                hiZDepthTexture.Release();
                hiZDepthTexture = null;
            }
        }

        #endregion


        #region Public Methods

        public void Initialize(Camera occlusionCamera = null)
        {
            _isInvalid = false;

            _mainCamera = occlusionCamera != null ? occlusionCamera : gameObject.GetComponent<Camera>();

            if (_mainCamera == null)
            {
                Debug.LogError("GPUI Hi-Z Occlision Culling Generator failed to initialize: camera not found.");
                _isInvalid = true;
                return;
            }

#if UNITY_2017_2_OR_NEWER
            if (UnityEngine.XR.XRSettings.enabled)
#else
            if (UnityEngine.VR.VRSettings.enabled)
#endif
            {
                isVREnabled = true;

#if UNITY_2018_3_OR_NEWER
                // Set the correct vr rendering mode if it is 2018.3 or later automatically
                GPUInstancerConstants.gpuiSettings.vrRenderingMode = UnityEngine.XR.XRSettings.stereoRenderingMode == UnityEngine.XR.XRSettings.StereoRenderingMode.SinglePass ? 0 : 1;
#endif

#if UNITY_2017_2_OR_NEWER
                _previousScreenSize = new Vector2(UnityEngine.XR.XRSettings.eyeTextureWidth, UnityEngine.XR.XRSettings.eyeTextureHeight);
#else
                _previousScreenSize = new Vector2(UnityEngine.VR.VRSettings.eyeTextureWidth, UnityEngine.VR.VRSettings.eyeTextureHeight);
#endif

                if (_mainCamera.stereoTargetEye != StereoTargetEyeMask.Both)
                {
                    Debug.LogError("GPUI Hi-Z Occlision works only for cameras that render to Both eyes. Disabling Occlusion Culling.");
                    _isInvalid = true;
                    return;
                }

            }
            else
            {
                isVREnabled = false;
                _previousScreenSize = new Vector2(_mainCamera.pixelWidth, _mainCamera.pixelHeight);
            }

            _mainCamera.depthTextureMode |= DepthTextureMode.Depth;

            CreateHiZDepthTexture();

        }

        #endregion


        #region Private Methods

#if UNITY_2019_1_OR_NEWER
        // SRP callback signature is different in 2019.1, but we only need the camera. 
        // Using this method to direct the callback to the main method. Unity 2018 has the same signature with Camera.onPostRender, so this is not relevant.
        private void OnEndCameraRenderingSRP(UnityEngine.Rendering.ScriptableRenderContext context, Camera camera)
        {
            OnEndCameraRendering(camera);
        }
#endif

        private void OnEndCameraRendering(Camera camera)
        {
//#if UNITY_EDITOR
//            UnityEngine.Profiling.Profiler.BeginSample("GPUInstancerHiZOcclusionGenerator.OnEndCameraRendering");
//#endif

            if (_isInvalid || _mainCamera == null || camera != _mainCamera)
                return;

            if (hiZDepthTexture == null)
            {
                Debug.LogWarning("GPUI HiZ Depth texture is null where it should not be. Recreating it.");
                CreateHiZDepthTexture();
            }

            HandleScreenSizeChange();

            if (unityDepthTexture == null)
                unityDepthTexture = Shader.GetGlobalTexture("_CameraDepthTexture");

            if (unityDepthTexture != null) // will be null the first time this runs in Unity 2018 SRP since we have to use beginCameraRendering there.
            {
                if (isVREnabled && GPUInstancerConstants.gpuiSettings.vrRenderingMode == 1)
                {
                    if (_mainCamera.stereoActiveEye == Camera.MonoOrStereoscopicEye.Left)
                        GPUInstancerUtility.CopyTextureWithComputeShader(unityDepthTexture, hiZDepthTexture, 0);
                    else if (GPUInstancerConstants.gpuiSettings.testBothEyesForVROcclusion)
                        GPUInstancerUtility.CopyTextureWithComputeShader(unityDepthTexture, hiZDepthTexture, (int)hiZTextureSize.x / 2);
                }
                else
                    GPUInstancerUtility.CopyTextureWithComputeShader(unityDepthTexture, hiZDepthTexture, 0);
            }

//#if UNITY_EDITOR
//            UnityEngine.Profiling.Profiler.EndSample();
//#endif

            HandleHiZDebugger();
        }

        private void CreateHiZDepthTexture()
        {
            if (isVREnabled)
            {
#if UNITY_2017_2_OR_NEWER
                hiZTextureSize.x = UnityEngine.XR.XRSettings.eyeTextureWidth;
                hiZTextureSize.y = UnityEngine.XR.XRSettings.eyeTextureHeight;
#else
                hiZTextureSize.x = UnityEngine.VR.VRSettings.eyeTextureWidth;
                hiZTextureSize.y = UnityEngine.VR.VRSettings.eyeTextureHeight;
#endif
                if (GPUInstancerConstants.gpuiSettings.testBothEyesForVROcclusion)
                    hiZTextureSize.x *= 2;
            }
            else
            {
                hiZTextureSize.x = _mainCamera.pixelWidth;
                hiZTextureSize.y = _mainCamera.pixelHeight;
            }

            if (hiZTextureSize.x <= 0 || hiZTextureSize.y <= 0)
            {
                if (hiZDepthTexture != null)
                {
                    hiZDepthTexture.Release();
                    hiZDepthTexture = null;
                }

                Debug.LogError("Cannot create GPUI HiZ Depth Texture for occlusion culling: Screen size is too small.");
                return;
            }

            if (hiZDepthTexture != null)
                hiZDepthTexture.Release();

            hiZDepthTexture = new RenderTexture((int)hiZTextureSize.x, (int)hiZTextureSize.y, 0, RenderTextureFormat.RFloat, RenderTextureReadWrite.Linear);
            hiZDepthTexture.name = "GPUIHiZDepthTexture";
            hiZDepthTexture.filterMode = FilterMode.Point;
            hiZDepthTexture.useMipMap = false;
            hiZDepthTexture.enableRandomWrite = true;
            hiZDepthTexture.Create();
            hiZDepthTexture.hideFlags = HideFlags.HideAndDontSave;

        }

        private void HandleScreenSizeChange()
        {
            if (isVREnabled)
            {
#if UNITY_2017_2_OR_NEWER
                if (_previousScreenSize.x != UnityEngine.XR.XRSettings.eyeTextureWidth || _previousScreenSize.y != UnityEngine.XR.XRSettings.eyeTextureHeight)
                {
                    _previousScreenSize.x = UnityEngine.XR.XRSettings.eyeTextureWidth;
                    _previousScreenSize.y = UnityEngine.XR.XRSettings.eyeTextureHeight;
#else
                if (_previousScreenSize.x != UnityEngine.VR.VRSettings.eyeTextureWidth || _previousScreenSize.y != UnityEngine.VR.VRSettings.eyeTextureHeight)
                {
                    _previousScreenSize.x = UnityEngine.VR.VRSettings.eyeTextureWidth;
                    _previousScreenSize.y = UnityEngine.VR.VRSettings.eyeTextureHeight;
#endif

                    CreateHiZDepthTexture();
                }
            }
            else
            {
                if (_previousScreenSize.x != _mainCamera.pixelWidth || _previousScreenSize.y != _mainCamera.pixelHeight)
                {
                    _previousScreenSize.x = _mainCamera.pixelWidth;
                    _previousScreenSize.y = _mainCamera.pixelHeight;

                    CreateHiZDepthTexture();
                }
            }
        }

        private void HandleHiZDebugger()
        {
#if UNITY_EDITOR
            
            if (debuggerEnabled && _hiZOcclusionDebuggerGUI == null)
                CreateHiZDebuggerCanvas();

            if (!debuggerEnabled && _hiZOcclusionDebuggerGUI != null)
                DestroyImmediate(_hiZOcclusionDebuggerGUI);

            if (!debuggerEnabled)
                return;

            if (debuggerGUIOnTop != _debuggerGUIOnTopCached || debuggerOverlay != _debuggerOverlayCached)
            {
                _hiZOcclusionDebuggerGUI.GetComponent<Canvas>().sortingOrder = debuggerGUIOnTop ? 10000 : -10000;
                _hiZDebugDepthTextureGUIImage.color = new Color(1, 1, 1, 1 - debuggerOverlay);

                _debuggerOverlayCached = debuggerOverlay;
                _debuggerGUIOnTopCached = debuggerGUIOnTop;
            }
                

            if (_hiZOcclusionDebuggerGUI != null && hiZDepthTexture != null)
            {
                _hiZDebugDepthTextureGUIImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, isVREnabled 
                    && GPUInstancerConstants.gpuiSettings.testBothEyesForVROcclusion ? hiZTextureSize.x * 0.5f : hiZTextureSize.x);
                _hiZDebugDepthTextureGUIImage.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, hiZTextureSize.y);
                _hiZDebugDepthTextureGUIImage.texture = hiZDepthTexture;
            }
#endif
        }

        private void CreateHiZDebuggerCanvas()
        {
            _hiZOcclusionDebuggerGUI = new GameObject("GPUI HiZ Occlusion Culling Debugger");
            _debuggerGUIOnTopCached = debuggerGUIOnTop;
            _debuggerOverlayCached = debuggerOverlay;

            // Add and setup the canvas
            Canvas debuggerCanvas = _hiZOcclusionDebuggerGUI.AddComponent<Canvas>();
            debuggerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            debuggerCanvas.pixelPerfect = true; // no antialiasing
            debuggerCanvas.sortingOrder = debuggerGUIOnTop ? 10000 : -10000;
            debuggerCanvas.targetDisplay = 0;

            // Add a raw image to display the HiZ Depth RenderTexture
            GameObject hiZDepthTextureGUI = new GameObject("HiZ Depth Texture");
            hiZDepthTextureGUI.transform.parent = _hiZOcclusionDebuggerGUI.transform;
            _hiZDebugDepthTextureGUIImage = hiZDepthTextureGUI.AddComponent<RawImage>();
            _hiZDebugDepthTextureGUIImage.color = new Color(1, 1, 1, 1 - debuggerOverlay);

            // Setup the image's RectTransform for anchors, pivot and position
            Vector2 bottomRight = new Vector2(0, 0);
            _hiZDebugDepthTextureGUIImage.rectTransform.anchorMin = bottomRight;
            _hiZDebugDepthTextureGUIImage.rectTransform.anchorMax = bottomRight;
            _hiZDebugDepthTextureGUIImage.rectTransform.pivot = bottomRight;
            _hiZDebugDepthTextureGUIImage.rectTransform.position = Vector2.zero;
        }

        #endregion
    }

}