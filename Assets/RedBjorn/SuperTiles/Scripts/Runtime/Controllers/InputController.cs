using RedBjorn.SuperTiles.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RedBjorn.SuperTiles
{
    /// <summary>
    /// Controller which wraps UnityEnity.Input class
    /// </summary>
    public class InputController
    {
        public static bool LockUp;

        public class FrameInfo
        {
            public int Frame;
            public bool OverUI;
            public GameObject OverObject;
            public Vector3 CameraGroundPosition;
        }

        static FrameInfo LastFrame = new FrameInfo();

        static void Validate()
        {
            if (LastFrame.Frame != Time.frameCount)
            {
                LastFrame.Frame = Time.frameCount;
                LastFrame.OverUI = EventSystem.current.IsPointerOverGameObject();
                if (LastFrame.OverUI)
                {
                    LastFrame.OverObject = null;
                }
                else
                {
                    RaycastHit hit;
                    if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 100f))
                    {
                        LastFrame.OverObject = hit.collider.gameObject;
                    }
                    else
                    {
                        LastFrame.OverObject = null;
                    }
                }
                var screemCenterRay = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
                var plane = new Plane(Vector3.up, Vector3.zero);
                float enter = 0f;
                if (plane.Raycast(screemCenterRay, out enter))
                {
                    LastFrame.CameraGroundPosition = screemCenterRay.GetPoint(enter);
                }
                else
                {
                    LastFrame.CameraGroundPosition = Vector3.zero;
                }
            }
        }

        public static GameObject OverGameobject
        {
            get
            {
                Validate();
                return LastFrame.OverObject;
            }
        }

        public static Plane Ground = new Plane(Vector3.up, Vector3.zero);

        public static bool GetOnWorldDownFree
        {
            get
            {
                Validate();
                return !LastFrame.OverUI && Input.GetMouseButtonDown(0);
            }
        }

        public static bool GetOnWorldUpFree
        {
            get
            {
                Validate();
                return !LastFrame.OverUI && Input.GetMouseButtonUp(0);
            }
        }

        public static bool GetOnWorldUp
        {
            get
            {
                Validate();
                return !LockUp && !LastFrame.OverUI && GetOnWorldUpFree;
            }
        }

        public static bool GetOnWorldFree
        {
            get
            {
                Validate();
                return !LastFrame.OverUI && Input.GetMouseButton(0);
            }
        }

        public static bool GetGameHotkeyUp(KeyCode code, bool ignoreConfirmMessage = false)
        {
            return Input.GetKeyUp(code) && (!ConfirmMessageUI.IsActive || ignoreConfirmMessage);
        }

        public static Vector3 CameraGroundPosition
        {
            get
            {
                Validate();
                return LastFrame.CameraGroundPosition;
            }
        }

        public static Vector3 GroundPosition
        {
            get
            {
                var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                var plane = new Plane(Vector3.up, Vector3.zero);
                float enter = 0f;
                if (plane.Raycast(mouseRay, out enter))
                {
                    return mouseRay.GetPoint(enter);
                }
                return Vector3.zero;
            }
        }

        public static Vector3 GroundPositionCameraOffset
        {
            get
            {
                var mouseRay = Camera.main.ScreenPointToRay(Input.mousePosition);
                float enter = 0f;
                if (Ground.Raycast(mouseRay, out enter))
                {
                    return mouseRay.GetPoint(enter) - Camera.main.transform.position;
                }
                return Vector3.zero;
            }
        }
    }
}

