using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class PlayerMovement : MonoBehaviour, IDataPersistence
{
    [SerializeField] float moveSpeed; // was 270f
    [SerializeField] float rotationSpeed = 720f;

    //[SerializeField] float sceneBoundary = 10f;

    [SerializeField] GroundMapper groundMapper;
    [SerializeField] Tilemap[] plantTilemap, grassTilemap, dirtTilemap, hiddenTilemap;

    [SerializeField] Animator animator;
    Vector2 moveInput;

    Rigidbody rb;

    [SerializeField] Transform bobber;

    //SFX
    [SerializeField] AudioClip WhooshSFX;
    [SerializeField] AudioClip SposhSFX;
    float footsteptime = 0;
    [SerializeField] AudioClip[] DirtFootstepsSFX;
    [SerializeField] AudioClip[] GrassFootstepsSFX;
    [SerializeField] AudioClip[] LeafFootstepsSFX;

    //float CastStrength;
    float castStartTime = -1;

    public void LoadData(GameData data)
    {
        // this will be handled by scene manager
    }

    public void SaveData(GameData data)
    {
        // this is to save immediate position
        // (scene manager doesn't have it, it only has spawn position that gets updated when switching between scenes
        data.position = gameObject.transform.position;
    }

    void MovePerformed(InputAction.CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }
    void MoveCancelled(InputAction.CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }
    void CastStarted(InputAction.CallbackContext ctx)
    {
        castStartTime = Time.time;
    }
    void CastEnded(InputAction.CallbackContext ctx)
    {
        if (castStartTime < 0) return;

        // start is only triggered after 0.1sec hold condition is met
        float holdDuration = Mathf.Clamp(Time.time - castStartTime, 0, 0.9f); // cap out at 1 sec
        holdDuration += 0.1f; // get from 0-0.9 to 0.1-1 range

        //Debug.Log($"Cast held for {holdDuration} seconds");

        castStartTime = -1; // reset

        OnCast(holdDuration * 5.0f);
    }
    void InventoryPressed(InputAction.CallbackContext ctx)
    {
        InventoryUI.Instance.ToggleInventory();
    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationY |
            RigidbodyConstraints.FreezeRotationZ;// |
            //RigidbodyConstraints.FreezePositionY;

    }

    private void Start()
    {
        Vector3 spawnPosition = MySceneManager.Instance.GetSpawnPosition();
        Quaternion spawnRotation = MySceneManager.Instance.GetSpawnRotation();

        if (spawnPosition != Vector3.zero)
        {
            transform.position = spawnPosition;
        }
        if (spawnRotation != Quaternion.identity)
        {
            transform.rotation = spawnRotation;
        }
    }

    IEnumerator ThrowBobber(CatchableSO reward, EnvironmentType pendingEnvironment, Vector3 startPos, Vector3 targetPos, float castStrength)
    {
        //SFX
        AudioSource.PlayClipAtPoint(WhooshSFX, Camera.main.transform.position, 1f);
        
        float duration = 0.1f * castStrength; // Time to reach target
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            bobber.position = Vector3.Lerp(startPos, targetPos, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        bobber.position = targetPos;

        //SFX
        AudioSource.PlayClipAtPoint(SposhSFX, Camera.main.transform.position, 1f);

        yield return new WaitForSeconds(0.5f);


        MySceneManager.Instance.SetSpawnPosition(gameObject.transform.position);
        MySceneManager.Instance.SetSpawnRotation(gameObject.transform.rotation);
        switch (pendingEnvironment)
        {
            case EnvironmentType.Forest_Ground:
                MySceneManager.Instance.LoadMinigame("Maze");
                break;
            case EnvironmentType.Forest_Undergrowth:
            default:
                MySceneManager.Instance.LoadMinigame("FollowTarget");
                break;
        }
    }

    void OnCast(float castStrength)
    {
        // disable controls while processing cast request
        GameManager.PlayerControls.Disable();

        Vector3 startPos = bobber.position;
        // where bobber will end up
        //Vector3 targetPos = transform.position + transform.forward * CastStrength;
        Vector3 targetPos = startPos + bobber.forward * castStrength;
        
        //EnvironmentType currEnvironment = groundMapper.GetEnvironment(targetPos);

        EnvironmentType currEnvironment = GetEnvironment(targetPos);

        //Hidden Areas achivement
        for (int i = 0; i < hiddenTilemap.Count(); i++)
        {
            Vector3Int cellPosition = hiddenTilemap[i].WorldToCell(targetPos);
            TileBase hiddenTile = hiddenTilemap[i].GetTile(cellPosition);
            if (hiddenTile != null)
            {
                //AchievementsPageUI.Instance.HiddenFishingUpdate(cellPosition);
                HiddenAreasManager.Instance.MarkEntryCollected(cellPosition);
            }
        }

        RewardManager.Instance.ChooseReward(currEnvironment);
        CatchableSO pendingReward = RewardManager.Instance.GetPendingItem();
        // this will be the same as currEnvironment, but just to be extra sure i guess :d
        // right now pending environment is used to determine which mini-game to open, which probs won't be the case in the future anyways
        EnvironmentType pendingEnvironment = RewardManager.Instance.GetPendingEnvironment();
        if (pendingReward != null)
        {
            StartCoroutine(ThrowBobber(pendingReward, pendingEnvironment, startPos, targetPos, castStrength));   
        }
        else
        {
            Debug.Log("No reward found.");

            // re-enable controls
            GameManager.PlayerControls.Enable();
        }
    }

    private void OnEnable()
    {
        GameManager.PlayerControls.Player.Move.performed += MovePerformed;
        GameManager.PlayerControls.Player.Move.canceled += MoveCancelled;
        //GameManager.PlayerControls.Player.Cast.performed += CastStarted;
        //GameManager.PlayerControls.Player.Cast.canceled += CastEnded;
        GameManager.PlayerControls.Player.Inventory.performed += InventoryPressed;
    }

    private void OnDisable()
    {
        GameManager.PlayerControls.Player.Move.performed -= MovePerformed;
        GameManager.PlayerControls.Player.Move.canceled -= MoveCancelled;
        //GameManager.PlayerControls.Player.Cast.performed -= CastStarted;
        //GameManager.PlayerControls.Player.Cast.canceled -= CastEnded;
        GameManager.PlayerControls.Player.Inventory.performed -= InventoryPressed;
    }


    // cast on mouse 0 pressed.
    /*private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            CastStrength = 0;
        }
        if (Input.GetKeyUp(KeyCode.Mouse0) & CastStrength > 1)
        {
            OnCast();
        }
        if (Input.GetKey(KeyCode.Mouse0) & CastStrength < 10)
        {
            CastStrength += 5 * Time.deltaTime;
        }

        if (Input.GetKeyUp(KeyCode.B))
        {
            BeastiaryPageUI.Instance.ToggleBeastiary();
        }
    }*/

    Vector3 lastMoveDir = Vector3.zero;
    private void Update()
    {
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        Vector3 moveDir = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        if (moveDir.sqrMagnitude > 0.01f)
        {
            if (!moveDir.Equals(lastMoveDir))
            {
                if (moveDir.z < 0 && Mathf.Abs(moveDir.z) > Mathf.Abs(moveDir.x))
                    animator.SetTrigger("walkFront");
                else if (moveDir.z > 0 && Mathf.Abs(moveDir.z) > Mathf.Abs(moveDir.x))
                    animator.SetTrigger("walkBack");
                else if (moveDir.x < 0)
                    animator.SetTrigger("walkLeft");
                else if (moveDir.x > 0)
                    animator.SetTrigger("walkRight");

                lastMoveDir = moveDir;
            }
        }
        else
        {
            if (lastMoveDir != Vector3.zero)
            {
                animator.SetTrigger("idle");
                lastMoveDir = Vector3.zero;
            }
        }
    }
    private void FixedUpdate()
    {
        Vector3 moveDirection = new Vector3(moveInput.x, 0f, moveInput.y).normalized;

        //SFX
        //Get tile to decide on what footsteps to create
        EnvironmentType FootstepsEnvironment = GetEnvironment(transform.position);

        AudioClip[] FootstepsSFX;
        if (FootstepsEnvironment == EnvironmentType.Forest_Plants) FootstepsSFX = LeafFootstepsSFX;
        else if (FootstepsEnvironment == EnvironmentType.Forest_Ground) FootstepsSFX = GrassFootstepsSFX;
        else FootstepsSFX = DirtFootstepsSFX;

        //choose random sound
        int i = Random.Range(0, FootstepsSFX.Length);

        //play sound every 0.5 sec
        if (moveDirection.sqrMagnitude > 0.01f & footsteptime <= 0f)
        {
            footsteptime = 0.5f;
            AudioSource.PlayClipAtPoint(FootstepsSFX[i], Camera.main.transform.position, 1f);
        }
        else if (footsteptime > 0f)
        {
            footsteptime -= Time.deltaTime;
        }

        //rb.velocity = new Vector3(moveDirection.x * moveSpeed * Time.fixedDeltaTime, rb.velocity.y, moveDirection.z * moveSpeed * Time.fixedDeltaTime);
        var increment = new Vector3(moveDirection.x * moveSpeed * Time.fixedDeltaTime, 0, moveDirection.z * moveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(rb.position + increment);
        //Debug.DrawRay(transform.position + Vector3.up * 0.5f, Vector3.down * 1.5f, Color.red, 0.1f);

        // rotate to face moving direction
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            //Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            //rb.rotation = Quaternion.RotateTowards(rb.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }

        /*float angle = -45f * Mathf.Deg2Rad;
        // rotate for ground-space
        float rotatedX = Mathf.Cos(angle) * rb.position.x - Mathf.Sin(angle) * rb.position.z;
        float rotatedZ = Mathf.Sin(angle) * rb.position.x + Mathf.Cos(angle) * rb.position.z;

        // clamp
        rotatedX = Mathf.Clamp(rotatedX, -sceneBoundary, sceneBoundary);
        rotatedZ = Mathf.Clamp(rotatedZ, -sceneBoundary, sceneBoundary);

        // convert back to world space
        float worldX = Mathf.Cos(-angle) * rotatedX - Mathf.Sin(-angle) * rotatedZ;
        float worldZ = Mathf.Sin(-angle) * rotatedX + Mathf.Cos(-angle) * rotatedZ;

        rb.position = new Vector3(worldX, rb.position.y, worldZ);*/
    }

    public EnvironmentType GetEnvironment(Vector3 position)
    {

        for (int i = 0; i < grassTilemap.Count(); i++)
        {
            Vector3Int cellPosition = grassTilemap[i].WorldToCell(position);
            TileBase plantTile = plantTilemap[i].GetTile(cellPosition);
            TileBase grassTile = grassTilemap[i].GetTile(cellPosition);
            TileBase dirtTile = dirtTilemap[i].GetTile(cellPosition);

            if (plantTile != null) return EnvironmentType.Forest_Plants;
            else if (grassTile != null) return EnvironmentType.Forest_Undergrowth;
            else if (dirtTile != null) return EnvironmentType.Forest_Ground;
        }
        return 0;
    }
}
