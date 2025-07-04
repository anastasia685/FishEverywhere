using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MinigamePlayerMovement : MonoBehaviour
{
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float acceleration;

    protected Vector2 moveInput;

    Rigidbody rb;

    bool isDone = false;

    //SFX
    public AudioClip FailSFX;
    public AudioClip WinSFX;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY |
            RigidbodyConstraints.FreezeRotationZ |
            RigidbodyConstraints.FreezePositionY;

        rb.maxLinearVelocity = moveSpeed;

    }

    void OnEnable()
    {
        GameManager.PlayerControls.MinigamePlayer.Move.performed += OnMovePerformed;
        GameManager.PlayerControls.MinigamePlayer.Move.canceled += OnMoveCancelled;
        GameManager.PlayerControls.MinigamePlayer.Cancel.performed += OnEscapePressed;
    }

    void OnDisable()
    {
        GameManager.PlayerControls.MinigamePlayer.Move.performed -= OnMovePerformed;
        GameManager.PlayerControls.MinigamePlayer.Move.canceled -= OnMoveCancelled;
        GameManager.PlayerControls.MinigamePlayer.Cancel.performed -= OnEscapePressed;
    }

    void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }
    void OnMoveCancelled(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }

    void OnEscapePressed(InputAction.CallbackContext ctx)
    {
        RewardManager.Instance.ClearPendingReward();
        MySceneManager.Instance.LoadScene(MySceneManager.Instance.GetCurrentScene());
    }

    void FixedUpdate()
    {
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        if (moveDirection.magnitude > 0)
        {
            // apply acceleration-based force
            rb.AddForce(moveDirection * acceleration * Time.fixedDeltaTime, ForceMode.Force);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isDone) return;

        switch (other.gameObject.tag)
        {
            case "Finish":
                //SFX
                AudioManager.Instance.playSFX(WinSFX);

                CatchableSO caughtItem = RewardManager.Instance.GetPendingItem();
                if (caughtItem != null)
                {
                    Debug.Log("You caught a " + RewardManager.Instance.GetPendingSize() + "cm " + RewardManager.Instance.GetPendingRarity() + " " + caughtItem.itemName + "!");
                    FishCatchingUI.Instance.StartAnimation(caughtItem, RewardManager.Instance.GetPendingRarity(), RewardManager.Instance.GetPendingSize());
                }
                else
                {
                    Debug.Log("No reward found. Something went wrong.");
                    MySceneManager.Instance.LoadScene(MySceneManager.Instance.GetCurrentScene());
                }
                break;

            case "Enemy":
                //SFX
                AudioManager.Instance.playSFX(FailSFX);

                Debug.Log("The fish got away!");
                RewardManager.Instance.ClearPendingReward();
                MySceneManager.Instance.LoadScene(MySceneManager.Instance.GetCurrentScene());
                break;
        }
    }
}
