using System;

namespace GOC.UISystem
{
    public readonly struct PopupId : IEquatable<PopupId>
    {
        public static readonly PopupId None = new PopupId(string.Empty);

        public readonly string Key;

        public PopupId(string key)
        {
            Key = key ?? string.Empty;
        }

        public bool IsNone => string.IsNullOrEmpty(Key);

        public bool Equals(PopupId other) => string.Equals(Key, other.Key, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is PopupId other && Equals(other);
        public override int GetHashCode() => Key != null ? Key.GetHashCode() : 0;
        public override string ToString() => Key ?? string.Empty;

        public static bool operator ==(PopupId left, PopupId right) => left.Equals(right);
        public static bool operator !=(PopupId left, PopupId right) => !left.Equals(right);

        public static implicit operator string(PopupId id) => id.Key;
    }
}
