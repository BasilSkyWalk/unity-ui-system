using System;

namespace GOC.UISystem
{
    public readonly struct ScreenId : IEquatable<ScreenId>
    {
        public static readonly ScreenId None = new ScreenId(string.Empty);

        public readonly string Key;

        public ScreenId(string key)
        {
            Key = key ?? string.Empty;
        }

        public bool IsNone => string.IsNullOrEmpty(Key);

        public bool Equals(ScreenId other) => string.Equals(Key, other.Key, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is ScreenId other && Equals(other);
        public override int GetHashCode() => Key != null ? Key.GetHashCode() : 0;
        public override string ToString() => Key ?? string.Empty;

        public static bool operator ==(ScreenId left, ScreenId right) => left.Equals(right);
        public static bool operator !=(ScreenId left, ScreenId right) => !left.Equals(right);

        public static implicit operator string(ScreenId id) => id.Key;
    }
}
