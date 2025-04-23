using AO;

public class UIStatusManager : Component //System<UIStatusManager>
{
    public static UIStatusManager instance;

    public UIText mainText;
    public UIText subText;

    private const string mainTextString = "UI Text - MainText";
    private const string subTextString = "UI Text - SubText";

    // ===============================================================================================================================

    public override void Awake()
    {
        instance = this;

        mainText = Entity.FindByName(mainTextString).GetComponent<UIText>();
        subText = Entity.FindByName(subTextString).GetComponent<UIText>();
    }

    public void UpdateMenus()
    {
        switch (GameManagerBomberman.instance.currentMatchState)
        {
            case GameMatchState.Idle:
                mainText.Text = "Waiting for more players...";
                subText.Text = "Players: " + GameManagerBomberman.instance.playersInQueue.Count + "/" + GameManagerBomberman.MIN_PLAYER_COUNT;
                break;

            case GameMatchState.StartingUp:
                mainText.Text = "Match Starts in: " + GameManagerBomberman.instance.queueCountdownTimer;
                subText.Text = "Players: " + GameManagerBomberman.instance.playersInQueue.Count;
                break;

            case GameMatchState.Countdown:
                mainText.Text = "Ready?! " + GameManagerBomberman.instance.matchCountdownTimer + "...";
                subText.Text = "Players remaining: " + GameManagerBomberman.instance.playersInCurrentMatch.Count;
                break;

            case GameMatchState.Playing:
                mainText.Text = "Fight!";
                subText.Text = "Players remaining: " + GameManagerBomberman.instance.playersInCurrentMatch.Count;
                break;

            default:
                mainText.Text = "Unknown match state.";
                subText.Text = "Please wait.";
                break;
        }
    }

    // ===============================================================================================================================
}
