using System.Collections.Generic;
using System.Text;

namespace ArtPipeline.Editor
{
    /// <summary>
    /// Validation results container
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; } = true;
        public List<string> Errors { get; set; } = new List<string>();
        public List<string> Warnings { get; set; } = new List<string>();
        public List<string> Suggestions { get; set; } = new List<string>();

        public void AddError(string error)
        {
            Errors.Add(error);
            IsValid = false;
        }

        public void AddWarning(string warning) => Warnings.Add(warning);

        public void AddSuggestion(string suggestion) => Suggestions.Add(suggestion);

        public override string ToString()
        {
            StringBuilder sb = new();

            if (!IsValid)
            {
                _ = sb.AppendLine("❌ VALIDATION FAILED:");
                foreach (string error in Errors)
                {
                    _ = sb.AppendLine($"   ⚠ {error}");
                }
            }

            if (Warnings.Count > 0)
            {
                _ = sb.AppendLine("⚠ WARNINGS:");
                foreach (string warning in Warnings)
                {
                    _ = sb.AppendLine($"   • {warning}");
                }
            }

            if (Suggestions.Count > 0)
            {
                _ = sb.AppendLine("💡 SUGGESTIONS:");
                foreach (string suggestion in Suggestions)
                {
                    _ = sb.AppendLine($"   • {suggestion}");
                }
            }

            if (IsValid && Warnings.Count == 0 && Suggestions.Count == 0)
            {
                _ = sb.AppendLine("✅ VALIDATION PASSED");
            }

            return sb.ToString();
        }

        public string ToShortString()
        {
            if (!IsValid)
            {
                return $"❌ Failed ({Errors.Count} errors)";
            }

            if (Warnings.Count > 0)
            {
                return $"⚠ Passed with {Warnings.Count} warnings";
            }

            return "✅ Passed";
        }
    }
}
