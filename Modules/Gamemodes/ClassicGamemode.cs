namespace MoreGamemodes
{
    public class ClassicGamemode : CustomGamemode
    {
        public ClassicGamemode()
        {
            Gamemode = Gamemodes.Classic;
            PetAction = false;
        }

        public static ClassicGamemode instance;
    }
}