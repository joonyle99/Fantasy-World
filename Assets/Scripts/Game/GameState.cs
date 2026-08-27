namespace FantasyWorld.Game
{
    /// <summary>
    /// 앱 전역 흐름 상태. 씬 단위 흐름(타이틀/로딩/플레이 등)을 표현한다.
    /// 세부 상태 로직이 필요하면 GameStateController 대신 StateMachine{T}로 승격한다.
    /// </summary>
    public enum GameState
    {
        Booting,
        Title,
        Loading,
        Playing,
        Paused,
        Result,
    }
}
