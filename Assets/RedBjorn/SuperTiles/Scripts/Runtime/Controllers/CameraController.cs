using System.Collections;
using UnityEngine;

namespace RedBjorn.SuperTiles
{
    /// <summary>
    /// Controller of camera movement bahaviour
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        float Eps = 0.1f;
        [SerializeField]
        float MoveSpeed = 1f;
        [SerializeField]
        Vector3 TargetOffset = new Vector3(0f, 10f, -10f);

        Vector3? HoldPosition;
        Vector3? ClickPosition;
        Coroutine MovingToCoroutine;

        public Vector3 LookPosition
        {
            get
            {
                var mouseRay = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
                var plane = new Plane(Vector3.up, Vector3.zero);
                float enter = 0f;
                if (plane.Raycast(mouseRay, out enter))
                {
                    return mouseRay.GetPoint(enter);
                }
                return Vector3.zero;
            }
        }

        void LateUpdate()
        {
            if (InputController.GetOnWorldDownFree)
            {
                StopMoving();
                HoldPosition = InputController.GroundPositionCameraOffset;
                ClickPosition = transform.position;
            }
            else if (InputController.GetOnWorldUpFree)
            {
                HoldPosition = null;
                ClickPosition = null;
            }
            UpdatePosition();
        }

        void OnDisable()
        {
            InputController.LockUp = false;
        }

        public void MoveTo(Vector3 groundPosition)
        {
            StopMoving();
            if (gameObject.activeInHierarchy)
            {
                MovingToCoroutine = StartCoroutine(MovingTo(groundPosition));
            }
        }

        void UpdatePosition()
        {
            if (HoldPosition.HasValue)
            {
                var delta = HoldPosition.Value - InputController.GroundPositionCameraOffset;
                transform.position += delta;
                transform.position = ClickPosition.Value + delta;
                if (!InputController.LockUp)
                {
                    InputController.LockUp = delta.sqrMagnitude > Eps;
                }
            }
            else
            {
                InputController.LockUp = false;
            }
        }

        void StopMoving()
        {
            if (MovingToCoroutine != null)
            {
                StopCoroutine(MovingToCoroutine);
                MovingToCoroutine = null;
            }
        }

        IEnumerator MovingTo(Vector3 groundPosition)
        {
            var delta = new Vector3(groundPosition.x, 0f, groundPosition.z) + TargetOffset - transform.position;
            var step = delta * MoveSpeed;
            var targetPos = transform.position + delta;
            var direction = targetPos - transform.position;
            while (step.x * direction.x + step.z * direction.z > 0f)
            {
                transform.position = transform.position + step * Time.deltaTime;
                direction = targetPos - transform.position;
                yield return null;
            }
            transform.position = targetPos;
            MovingToCoroutine = null;
        }
    }
}
