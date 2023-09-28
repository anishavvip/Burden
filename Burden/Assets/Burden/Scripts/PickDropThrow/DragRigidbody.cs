using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DragRigidbody : MonoBehaviour
{
    float force = 600;
    float damping = 6;
    float distance = 3f;
    Transform jointTrans;
    float dragDepth;
    PlayerController player;
    [HideInInspector] public Rigidbody Rigidbody;
    bool isDrag = false;
    bool isClicked = false;

    private void Awake()
    {
        Rigidbody = GetComponent<Rigidbody>();
    }
    void Start()
    {
        player = ExampleManager.GetPlayer();
    }

    private void SendDataToServer()
    {
        if (!player.hasGameBegun) return;
        GrabDetails SyncData = new GrabDetails();
        SyncData.name = ExampleManager.Instance.Avatar.ToString();

        SyncData.xPos = transform.position.x;
        SyncData.yPos = transform.position.y;
        SyncData.zPos = transform.position.z;

        SyncData.xRot = transform.localRotation.x;
        SyncData.yRot = transform.localRotation.y;
        SyncData.zRot = transform.localRotation.z;
        SyncData.wRot = transform.localRotation.w;
        SyncData.isDrag = isDrag;
        SyncData.isClicked = isClicked;

        ExampleManager.CustomServerMethod("itemGrab", new object[] { SyncData });
    }
    private void Update()
    {
        if (!player.hasGameBegun) return;
        SendDataToServer();
    }
    void OnMouseDown()
    {
        isClicked = true;
        HandleInputBegin(Input.mousePosition);
        player.DragRigidbody = this;
    }

    void OnMouseUp()
    {
        isClicked = false;
        HandleInputEnd(Input.mousePosition);
    }
    void OnMouseDrag()
    {
        isDrag = true;
        HandleInput(Input.mousePosition);
    }

    public void HandleInputBegin(Vector3 screenPosition)
    {
        var ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, distance))
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Interactive"))
            {
                dragDepth = CameraPlane.CameraToPointDepth(Camera.main, hit.point);
                jointTrans = AttachJoint(hit.rigidbody, hit.point);
            }
        }
    }

    public void HandleInput(Vector3 screenPosition)
    {
        if (jointTrans == null)
            return;
        var worldPos = Camera.main.ScreenToWorldPoint(screenPosition);
        jointTrans.position = CameraPlane.ScreenToWorldPlanePoint(Camera.main, dragDepth, screenPosition);
    }

    public void HandleInputEnd(Vector3 screenPosition)
    {
        Destroy(jointTrans.gameObject);
        player.DragRigidbody = null;
        isDrag = false;
    }

    Transform AttachJoint(Rigidbody rb, Vector3 attachmentPosition)
    {
        GameObject go = new GameObject("Attachment Point");
        go.hideFlags = HideFlags.HideInHierarchy;
        go.transform.position = attachmentPosition;

        var newRb = go.AddComponent<Rigidbody>();
        newRb.isKinematic = true;

        var joint = go.AddComponent<ConfigurableJoint>();
        joint.connectedBody = rb;
        joint.configuredInWorldSpace = true;
        joint.xDrive = NewJointDrive(force, damping);
        joint.yDrive = NewJointDrive(force, damping);
        joint.zDrive = NewJointDrive(force, damping);
        joint.slerpDrive = NewJointDrive(force, damping);
        joint.rotationDriveMode = RotationDriveMode.Slerp;

        return go.transform;
    }

    private JointDrive NewJointDrive(float force, float damping)
    {
        JointDrive drive = new JointDrive();
        drive.mode = JointDriveMode.Position;
        drive.positionSpring = force;
        drive.positionDamper = damping;
        drive.maximumForce = Mathf.Infinity;
        return drive;
    }
    private void OnCollisionEnter(Collision col)
    {
        Interactable interactable = col.collider.gameObject.GetComponentInParent<Interactable>();
        if (interactable != null)
            if (interactable.canPlaceObjectsInside) 
                gameObject.transform.SetParent(col.gameObject.transform, true);
    }
    private void OnCollisionExit(Collision col)
    {
        gameObject.transform.parent = null;
    }
}
