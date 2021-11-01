using System;

namespace GenomeTools.ChemistryLibrary.IO.Cram.Models
{
    public class TagId : IEquatable<TagId>
    {
        public string Tag { get; }
        public char ValueType { get; }

        public TagId(string tag, char valueType)
        {
            Tag = tag;
            ValueType = valueType;
        }

        public override string ToString()
        {
            return Tag;
        }

        public bool Equals(TagId other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Tag == other.Tag && ValueType == other.ValueType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TagId)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Tag, ValueType);
        }

        public static bool operator ==(TagId left, TagId right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TagId left, TagId right)
        {
            return !Equals(left, right);
        }
    }
}