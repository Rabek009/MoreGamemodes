using Hazel;
using UnityEngine;

namespace MoreGamemodes
{
    static class ExtendedMessageWriter
    {
        public static void WriteColor(this MessageWriter writer, Color color)
        {
            writer.Write(color.r);
            writer.Write(color.g);
            writer.Write(color.b);
            writer.Write(color.a);
        }

        public static Color ReadColor(this MessageReader reader)
        {
            float r = reader.ReadSingle();
            float g = reader.ReadSingle();
            float b = reader.ReadSingle();
            float a = reader.ReadSingle();
            return new Color(r, g, b, a);
        }
    }
}