using UnityEngine;

public class HeadBobAnimator : MonoBehaviour
{
    private enum AnimationsStrings
    {
        Idle,
        Walk,
        Crouch,
        Sprint_Start,
        Sprint,
    }

    private Animator anim;
    private PlayerController playerController;

    int currentStateHash = 0;

    private void Start()
    {
        anim = GetComponent<Animator>();
        playerController = PlayerController.Instance;
    }

    private void Update()
    {
        string desiredStateName;

        bool isMoving = playerController.isMoving;
        bool isSprinting = playerController.isSprinting;
        PlayerController.CrouchState crouchState = playerController.crouchState;

        if (isMoving && isSprinting && crouchState == PlayerController.CrouchState.Standing)
            desiredStateName = AnimationsStrings.Sprint.ToString();
        else if (isMoving && crouchState == PlayerController.CrouchState.Standing)
            desiredStateName = AnimationsStrings.Walk.ToString();
        else if (isMoving && (crouchState == PlayerController.CrouchState.Crouching || crouchState == PlayerController.CrouchState.Crawling))
            desiredStateName = AnimationsStrings.Crouch.ToString();
        else
            desiredStateName = AnimationsStrings.Idle.ToString();

        int desiredHash = Animator.StringToHash(desiredStateName);

        if (currentStateHash != desiredHash)
        {
            anim.CrossFade(desiredHash, 0.25f, 0, 0f);
            currentStateHash = desiredHash;
        }
    }
}
