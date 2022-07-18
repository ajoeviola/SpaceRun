using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    // Public inspector variables
    [Header("Game Manager Refererences")]
    public GameObject heroShip;
    public WorldGenerator worldGenerator;

    // Private instance variables
    private int score = 0;
    private int highScore = 0;
    private bool isActive = false;
    private bool beatHighScore = false;
    private TransformAnimator heroShipTransformAnimator;
    private TransformAnimator worldAnchorTransformAnimator;

    void Start()
    {
        // Cap the frame rate to 150 fps and disable v-sync
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 150;

        // Cache the references
        heroShipTransformAnimator = heroShip.GetComponent<TransformAnimator>();
        worldAnchorTransformAnimator = WorldAnchor.Instance.transform.GetComponent<TransformAnimator>();

        // Retrieve the high score from PlayerPrefs
        highScore = PlayerPrefs.GetInt("HighScore", 0);
    }

    public void StartGame()
    {
        // Reset the cloud spawner
        CloudSpawner.Instance.Reset();

        // Animate both the hero ship and the world anchor to 0, 0, 0
        heroShipTransformAnimator.targetPosition = Vector3.zero;
        worldAnchorTransformAnimator.targetPosition = Vector3.zero;

        // Enable animation for both target transforms
        heroShipTransformAnimator.isAnimating = true;
        worldAnchorTransformAnimator.isAnimating = true;

        // Initialize the score to 0m
        UIManager.Instance.Set3DText(0, "0 m");

        // Begin playing gameplay game music on Loop
        AudioSource backgroundSource = AudioManager.Instance.GetSource(0);
        backgroundSource.volume = 0.3f;
        backgroundSource.loop = true;
        AudioManager.Instance.Play(0, 0);

        // Stop the hero ship animation once it's done
        heroShipTransformAnimator.OnceAnimationComplete(TransformAnimator.Properties.Position, () =>
        {
            // Disable the hero ship's animator and enable movement
            heroShipTransformAnimator.isAnimating = false;

            // Enable hero ship movement
            HeroShip heroShipScript = heroShip.GetComponent<HeroShip>();
            heroShipScript.isMoving = true;
        });

        // Once the world anchor has finished animating, generate the world
        worldAnchorTransformAnimator.OnceAnimationComplete(TransformAnimator.Properties.Position, () =>
        {
            // Begin scrolling the primary game scroller
            BuildingScroller primaryScroller = worldGenerator.GetBuildingScrollerAtIndex(0);
            primaryScroller.isScrolling = true;

            // Enable world generation on the world generator
            worldGenerator.isGenerating = true;

            // Enable the game
            isActive = true;
        });
    }

    public void ResetGame()
    {
        // Stop the game and reset the score
        int runScore = score;
        isActive = false;
        score = 0;

        // Empty the player score so it doesn't show in the distance
        UIManager.Instance.Set3DText(0, "");

        // Enable animation for both target transforms
        heroShipTransformAnimator.isAnimating = true;
        worldAnchorTransformAnimator.isAnimating = true;

        // Reset the transform animator position of both animators
        heroShipTransformAnimator.targetPosition = heroShipTransformAnimator.GetStartPosition();
        worldAnchorTransformAnimator.targetPosition = worldAnchorTransformAnimator.GetStartPosition();

        // Stop the hero ship animation once it's done
        heroShipTransformAnimator.OnceAnimationComplete(TransformAnimator.Properties.Position, () =>
        {
            // Disable the hero ship's animator
            heroShipTransformAnimator.isAnimating = false;
        });

        // Stop playing gameplay music
        AudioManager.Instance.GetSource(0).Stop();

        // Wait for the world anchor to finish animating
        worldAnchorTransformAnimator.OnceAnimationComplete(TransformAnimator.Properties.Position, () =>
        {
            // Disable hero ship movement
            HeroShip heroShipScript = heroShip.GetComponent<HeroShip>();
            heroShipScript.isMoving = false;

            // Reset the hero ship's position to 0, 0, 0
            heroShip.transform.position = Vector3.zero;

            // Reset the world generator's trails
            worldGenerator.InitializeTrails();

            // Hide the game UI
            UIManager.Instance.HidePanel(1, () =>
            {
                // Write the score message to the game over screen
                UIManager.Instance.SetText(1, "You traveled a total of " + runScore.ToString("N0") + " meters.");

                // Write the high score message to the game over screen
                UIManager.Instance.SetText(2, "You " + (beatHighScore ? "successfully" : "were unable to") + " beat your high score run of " + highScore.ToString("N0") + " meters.");

                // Display the Game Over UI panel
                UIManager.Instance.DisplayPanel(2);

                // Reset the beat high score state
                beatHighScore = false;
            });
        });
    }

    public void IncrementScore(int amount = 1)
    {
        // Increment the score
        score += amount;

        // Update the UI with the new score which is a number formatte with commas every 3 digits
        UIManager.Instance.Set3DText(0, score.ToString("N0") + " m");

        // Check if the score is greater than the high score
        if (score > highScore)
        {
            // Set the high score to the new score
            highScore = score;

            // Set the beat high score flag to true
            beatHighScore = true;

            // Save the high score to PlayerPrefs
            PlayerPrefs.SetInt("HighScore", highScore);
        }
    }

    void FixedUpdate()
    {
        // Check if the game is active
        if (isActive)
        {
            // Increment the score
            IncrementScore(1);
        }
    }
}
