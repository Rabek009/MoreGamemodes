using UnityEngine;

namespace MoreGamemodes
{
    public class Display : CustomNetObject
    {
        public Display(string text, Vector2 position)
        {
            CreateNetObject(text, position, CustomObjectTypes.Display);
        }
    }
}