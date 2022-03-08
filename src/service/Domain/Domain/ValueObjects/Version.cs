using System.Linq;
using System.Text;
using CQRS.Mediatr.Lite.SDK.Domain;

namespace Microsoft.FeatureFlighting.Core.Domain.ValueObjects
{
    public class Version: ValueObject
    {
        public int MajorVersion { get; private set; }
        public int MinorVersion { get; private set;}

        public Version()
        {
            MajorVersion = 1;
            MinorVersion = 0;
        }

        public Version(string version)
        {
            if (string.IsNullOrWhiteSpace(version))
            {
                MajorVersion = 1;
                MinorVersion = 0;
                return;
            }

            string[] splitVersion = version.Split('.');
            if (int.TryParse(splitVersion.First(), out int majorVersion))
                MajorVersion = majorVersion;
            else
                MajorVersion = 1;

            if (int.TryParse(splitVersion.Count() > 1 ? splitVersion.Last() : "0", out int minorVersion))
                MinorVersion = minorVersion;
            else
                MinorVersion = 0;
        }

        public void UpdateMajor()
        {
            MajorVersion++;
            MinorVersion = 0;
        }

        public void UpdateMinor()
        {   
            MinorVersion++;
        }

        public override string ToString()
        {
            return new StringBuilder()
                .Append(MajorVersion)
                .Append(".")
                .Append(MinorVersion)
                .ToString();
        }
    }
}
