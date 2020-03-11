using System.Collections.Generic;

namespace Stellaris.Data
{
    public sealed class SupportedVersion
    {
        public int Major { get; }

        public int Minor { get; }

        public int Patch { get; }

        public SupportedVersion(string source)
        {
            var ver = source.Split('.');

            this.Major = int.MaxValue;
            this.Minor = int.MaxValue;
            this.Patch = int.MaxValue;

            this.Major = ver[0] == "*" ? int.MaxValue : int.Parse(ver[0]);
            if (ver.Length <= 1) return;
            this.Minor = ver[1] == "*" ? int.MaxValue : int.Parse(ver[1]);
            if (ver.Length > 2)
                this.Patch = ver[2] == "*" ? int.MaxValue : int.Parse(ver[2]);
        }

        public SupportedVersion(int maj, int min, int pat)
        {
            this.Major = maj;
            this.Minor = min;
            this.Patch = pat;
        }

        public static SupportedVersion Combine(IEnumerable<SupportedVersion> source)
        {
            var ma = int.MaxValue;
            var mi = int.MaxValue;
            var pa = int.MaxValue;

            foreach (var s in source)
            {
                if (ma > s.Major)
                {
                    ma = s.Major;
                    mi = int.MaxValue;
                    pa = int.MaxValue;
                }
                else if (ma == s.Major)
                {
                    if (mi > s.Minor)
                    {
                        mi = s.Minor;
                        pa = int.MaxValue;
                    }
                    else
                    {
                        if (pa > s.Patch)
                            pa = s.Patch;
                    }
                }
            }

            return new SupportedVersion(ma, mi, pa);
        }

        public override string ToString()
        {
            var mj = this.Major < int.MaxValue ? this.Major.ToString() : "*";
            var mi = this.Minor < int.MaxValue ? this.Minor.ToString() : "*";
            var pa = this.Patch < int.MaxValue ? this.Patch.ToString() : "*";

            return $"{mj}.{mi}.{pa}";
        }

    }
}