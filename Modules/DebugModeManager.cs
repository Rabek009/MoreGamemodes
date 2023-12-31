namespace MoreGamemodes
{
    public static class DebugModeManager
    {
        // これが有効の時、通常のゲームに支障のないデバッグ機能(詳細ログ・ゲーム外でのデバッグ表示など)が有効化される。
        // また、ゲーム内オプションでデバッグモードを有効化することができる。
        public static bool AmDebugger { get; private set; } =
#if DEBUG
    true;
#else
    false;
#endif
        public static void Auth(HashAuth auth, string input)
        {
            // AmDebugger = デバッグビルドである || デバッグキー認証が通った
            AmDebugger = AmDebugger || auth.CheckString(input);
        }
    }
}