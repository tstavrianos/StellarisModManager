using System;
using System.Collections.Generic;
using Serilog;

namespace Paradox.Common.Models
{
    public sealed class SupportedVersion: IComparable<SupportedVersion>, IEquatable<SupportedVersion>
    {
        public int Major { get; }

        public int Minor { get; }

        public int Patch { get; }
        private readonly ILogger _logger;

        public SupportedVersion(string source, ILogger logger = null)
        {
            this._logger = logger;
            var ver = source.Split('.');

            this.Major = int.MaxValue;
            this.Minor = int.MaxValue;
            this.Patch = int.MaxValue;

            try
            {
                this.Major = ver[0] == "*" ? int.MaxValue : int.Parse(ver[0]);
                if (ver.Length <= 1) return;
                this.Minor = ver[1] == "*" ? int.MaxValue : int.Parse(ver[1]);
                if (ver.Length > 2)
                    this.Patch = ver[2] == "*" ? int.MaxValue : int.Parse(ver[2]);
            }
            catch (Exception e)
            {
                this._logger?.Error(e, source);
            }
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

        public int CompareTo(SupportedVersion other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            var majorComparison = this.Major.CompareTo(other.Major);
            if (majorComparison != 0) return majorComparison;
            var minorComparison = this.Minor.CompareTo(other.Minor);
            return minorComparison != 0 ? minorComparison : this.Patch.CompareTo(other.Patch);
        }

        public static bool operator >(SupportedVersion a, SupportedVersion b)
        {
            return a.CompareTo(b) > 0;
        }

        public static bool operator <(SupportedVersion a, SupportedVersion b)
        {
            return a.CompareTo(b) < 0;
        }
        
        public static bool operator ==(SupportedVersion a, SupportedVersion b)
        {
            return a.CompareTo(b) == 0;
        }
        
        public static bool operator !=(SupportedVersion a, SupportedVersion b)
        {
            return a.CompareTo(b) != 0;
        }
        
        public bool Equals(SupportedVersion other)
        {
            return this.Major == other.Major && this.Minor == other.Minor && this.Patch == other.Patch;
        }

        public override bool Equals(object obj)
        {
            return ReferenceEquals(this, obj) || obj is SupportedVersion other && this.Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Major, this.Minor, this.Patch);
        }
    }
}