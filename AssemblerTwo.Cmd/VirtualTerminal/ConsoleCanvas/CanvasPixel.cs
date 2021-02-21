using System;
using System.Collections.Generic;

namespace ConsoleCanvas
{
    public struct CanvasPixel : IEquatable<CanvasPixel>
    {
        public char Character;
        public bool Visible;

        public CanvasPixel(char character = ' ', bool visible = false)
        {
            Character = character;
            Visible = visible;
        }

        public override bool Equals(object obj)
        {
            if (obj is CanvasPixel)
            {
                return this.Equals((CanvasPixel)obj);
            }
            return false;
        }

        public bool Equals(CanvasPixel other)
        {
            if (Character == other.Character)
            {
                return true;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(CanvasPixel lhs, CanvasPixel rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(CanvasPixel lhs, CanvasPixel rhs)
            => !(lhs == rhs);
    }
}
