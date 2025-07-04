using System.Drawing;
using UnityEngine;

public class FolllowTargetMovement : MonoBehaviour
{
    [SerializeField] float sceneBoundary = 5f;

    // --- values set by config function
    float minSpeed, maxSpeed;
    float directionChangeInterval;
    float burstChance, burstDuration;
    float centerPullStrength = 0.15f;
    float scoreDecayRate = 0.15f; // How fast score drops when not touching
    // ---

    const float perturbationAmount = 0.4f;


    float scoreIncreaseRate = 0.2f; // How fast score increases when touching

    public Transform progressBarFill;

    private float maxScore = 1f;
    private float score = 0.5f; // Start at mid-point
    private bool isTouchingTarget = false;

    private Vector3 targetDirection;
    private float currentSpeed;
    private float changeDirectionTimer;
    private bool isBursting = false;
    private float burstTimer = 0f;

    private Vector3 initialScale;
    private bool isDone = false;

    bool PerfectCatch = true;

    //SFX
    AudioSource ReelingSourceSFX;
    [SerializeField] AudioClip FailSFX;
    [SerializeField] AudioClip WinSFX;

    void Start()
    {
        SetParams();


        ChooseNewDirection();

        if (progressBarFill != null)
        {
            initialScale = progressBarFill.localScale;
        }

        //SFX
        ReelingSourceSFX = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isDone) return;

        UpdateScore();

        if (isBursting)
        {
            burstTimer -= Time.deltaTime;
            if (burstTimer <= 0f)
            {
                isBursting = false;
                currentSpeed = Random.Range(minSpeed, maxSpeed * 0.65f);
            }
        }


        changeDirectionTimer -= Time.deltaTime;
        if (changeDirectionTimer <= 0f)
        {
            ChooseNewDirection();
        }

        Vector3 newPosition = transform.position + targetDirection * currentSpeed * Time.deltaTime;

        // Boundary reflection with subtle center influence
        bool bounced = false;

        if (Mathf.Abs(newPosition.x) >= sceneBoundary)
        {
            targetDirection.x *= -1; // Reverse direction
            newPosition.x = Mathf.Clamp(newPosition.x, -sceneBoundary, sceneBoundary);
            bounced = true;
        }
        if (Mathf.Abs(newPosition.z) >= sceneBoundary)
        {
            targetDirection.z *= -1; // Reverse direction
            newPosition.z = Mathf.Clamp(newPosition.z, -sceneBoundary, sceneBoundary);
            bounced = true;
        }

        // Apply center bias only on boundary collision
        if (bounced)
        {
            targetDirection += new Vector3(Random.Range(-perturbationAmount, perturbationAmount), 0f, Random.Range(-perturbationAmount, perturbationAmount));
            targetDirection.Normalize();
        }

        transform.position = newPosition;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isTouchingTarget = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isTouchingTarget = false;
            PerfectCatch = false;
        }
    }

    void SetParams()
    {
        RarityType pendingRarity = RewardManager.Instance.GetPendingRarity();

        switch (pendingRarity)
        {
            case RarityType.Common:
            default:
                minSpeed = 1.5f;
                maxSpeed = 3.5f;
                directionChangeInterval = 2f;
                burstChance = 0.3f;
                burstDuration = 0.65f;
                scoreDecayRate = 0.1f;
                break;
            case RarityType.Rare:
                minSpeed = 2f;
                maxSpeed = 4.5f;
                directionChangeInterval = 2f;
                burstChance = 0.37f;
                burstDuration = 0.65f;
                scoreDecayRate = 0.12f;
                break;
            case RarityType.Epic:
                minSpeed = 2.5f;
                maxSpeed = 5f;
                directionChangeInterval = 2f;
                burstChance = 0.45f;
                burstDuration = 0.75f;
                scoreDecayRate = 0.14f;
                break;
            case RarityType.Legendary:
                minSpeed = 3f;
                maxSpeed = 5.5f;
                directionChangeInterval = 2f;
                burstChance = 0.48f;
                burstDuration = 0.75f;
                scoreDecayRate = 0.16f;
                break;
            case RarityType.Mythic:
                minSpeed = 3.5f;
                maxSpeed = 6f;
                directionChangeInterval = 1.8f;
                burstChance = 0.5f;
                burstDuration = 0.75f;
                scoreDecayRate = 0.18f;
                break;
        }

        float decayMultiplier = 1.0f - (PlayerStatsManager.Instance.GetTimeScaleMultiplier() - 1.0f); // multiplier 1.1 -> 1 - (1.1 - 1) = 0.9 => decay rate decreased by 0.1
        scoreDecayRate *= decayMultiplier;
    }

    void UpdateScore()
    {
        // Adjust score based on contact with the target
        if (isTouchingTarget)
        {
            score += scoreIncreaseRate * Time.deltaTime;

            //SFX
            if (!ReelingSourceSFX.isPlaying)
            {
                ReelingSourceSFX.Play();
            }
        }
        else
        {
            score -= scoreDecayRate * Time.deltaTime;
            ReelingSourceSFX.Stop();
        }

        // Clamp score between 0 and maxScore
        score = Mathf.Clamp(score, 0, maxScore);

        // Update progress bar scale
        if (progressBarFill != null)
        {
            float barMaxHeight = 12f;
            float newHeight = Mathf.Max((score / maxScore) * barMaxHeight, 0.01f);
            progressBarFill.localScale = new Vector3(initialScale.x, initialScale.y, newHeight);
            progressBarFill.localPosition = new Vector3(progressBarFill.localPosition.x, progressBarFill.localPosition.y, (newHeight - barMaxHeight) / 2);
        }

        // Check win/loss condition
        if (score >= maxScore)
        {
            OnWin();
        }
        else if (score <= 0)
        {
            OnLose();
        }
    }

    void ChooseNewDirection()
    {
        // Pick a new random direction
        targetDirection = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;

        Vector3 toCenter = (Vector3.zero - transform.position).normalized * centerPullStrength;
        targetDirection = (targetDirection + toCenter).normalized;

        // Set a new speed
        currentSpeed = Random.Range(minSpeed, maxSpeed * 0.65f);

        // Chance of a sudden burst
        if (Random.value < burstChance)
        {
            isBursting = true;
            burstTimer = burstDuration;
            currentSpeed = maxSpeed;
        }

        // Reset direction change timer
        changeDirectionTimer = Random.Range(directionChangeInterval * 0.8f, directionChangeInterval * 1.2f);
    }

    void OnWin()
    {
        //SFX
        AudioManager.Instance.playSFX(WinSFX);

        isDone = true;

        CatchableSO caughtItem = RewardManager.Instance.GetPendingItem();
        if (caughtItem != null)
        {
            Debug.Log("You caught a " + RewardManager.Instance.GetPendingSize() + "cm " + RewardManager.Instance.GetPendingRarity() + " " + caughtItem.itemName + "!");
            FishCatchingUI.Instance.StartAnimation(caughtItem, RewardManager.Instance.GetPendingRarity(), RewardManager.Instance.GetPendingSize());
            if (PerfectCatch)
            {
                //AchievementsPageUI.Instance.PerfectCatchUpdate();
                AchievementManager.Instance.UpdateEntries(AchievementType.PerfectCatch);
            }
        }
        else
        {
            Debug.Log("No reward found. Something went wrong.");
            MySceneManager.Instance.LoadScene(MySceneManager.Instance.GetCurrentScene());
        }
    }

    void OnLose()
    {
        //SFX
        AudioManager.Instance.playSFX(FailSFX);

        isDone = true;

        Debug.Log("The fish got away!");
        RewardManager.Instance.ClearPendingReward();
        MySceneManager.Instance.LoadScene(MySceneManager.Instance.GetCurrentScene());
    }
}
