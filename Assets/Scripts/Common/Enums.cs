namespace BubbleTown
{
    public enum GameMode
    {
        SinglePlayer = 0,
        AIBattle = 1,
        LocalVS = 2
    }

    public enum GameScene
    {
        MainMenu = 0,
        ModeSelect = 1,
        MapSelect = 2,
        Battle = 3,
        Result = 4
    }

    public enum MatchOutcome
    {
        None = 0,
        Player1Win = 1,
        Player2Win = 2,
        AIWin = 3,
        Draw = 4
    }

    public enum CharacterType
    {
        Player = 0,
        AI = 1
    }

    public enum ControlScheme
    {
        PlayerOne = 0,
        PlayerTwo = 1
    }

    public enum ItemType
    {
        None = 0,
        BombCount = 1,
        BombRange = 2,
        MoveSpeed = 3
    }

    public enum SpawnSlot
    {
        Player1 = 0,
        Player2 = 1,
        AI1 = 2,
        AI2 = 3
    }
}
