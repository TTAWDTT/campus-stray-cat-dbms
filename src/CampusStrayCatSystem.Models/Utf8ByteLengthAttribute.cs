using System.ComponentModel.DataAnnotations;
using System.Text;

namespace CampusStrayCatSystem.Models {
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)] public sealed class Utf8ByteLengthAttribute : ValidationAttribute {
        public int MaximumBytes { get; }

        public Utf8ByteLengthAttribute(int maximumBytes) { MaximumBytes = maximumBytes; }

        public override bool IsValid(object? value) => value is not string text || Encoding.UTF8.GetByteCount(text) <= MaximumBytes;
    }
}
