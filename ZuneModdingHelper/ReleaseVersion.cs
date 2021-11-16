using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ZuneModdingHelper
{
    public sealed class ReleaseVersion : ICloneable, IComparable, IComparable<ReleaseVersion?>, IEquatable<ReleaseVersion?>
    {
        /// <summary>
        /// Gets the value of the major component of the version number for the current <see cref="ReleaseVersion"/> object.
        /// </summary>
        public int Major { get; set; }

        /// <summary>
        /// Gets the value of the minor component of the version number for the current <see cref="ReleaseVersion"/> object.
        /// </summary>
        public int Minor { get; set; }

        /// <summary>
        /// Gets the value of the build component of the version number for the current <see cref="ReleaseVersion"/> object.
        /// </summary>
        public int Build { get; set; }

        /// <summary>
        /// Gets the value of the revision component of the version number for the current <see cref="ReleaseVersion"/> object.
        /// </summary>
        public int Revision { get; set; }

        private Phase _Phase = Phase.Unknown;
        /// <summary>
        /// Gets the value of the release phase component of the version number for the current <see cref="ReleaseVersion"/> object.
        /// </summary>
        public Phase Phase
        {
            set => _Phase = value;
            get
            {
                if (_Phase == Phase.Unknown)
                {
#if DEBUG
                    _Phase = Phase.Debug;
#else
                    _Phase = Phase.Production;
#endif
                }
                return _Phase;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseVersion"/> class.
        /// </summary>
        public ReleaseVersion()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReleaseVersion"/> class with the specified major,
        /// minor, build, revision, and release phase.
        /// </summary>
        /// <param name="major">
        /// The major version number.
        /// </param>
        /// <param name="minor">
        /// The minor version number.
        /// </param>
        /// <param name="build">
        /// The build version number.
        /// </param>
        /// <param name="revision">
        /// The revision version number.
        /// </param>
        /// <param name="phase">
        /// The release phase.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="major"/>, <paramref name="minor"/>, <paramref name="build"/>, or <paramref name="revision"/> is less than zero.
        /// </exception>
        public ReleaseVersion(int major, int minor, int build = 0, int revision = 0, Phase phase = Phase.Unknown)
        {
            Guard.IsGreaterThanOrEqualTo(major, 0, nameof(major));
            Guard.IsGreaterThanOrEqualTo(minor, 0, nameof(minor));
            Guard.IsGreaterThanOrEqualTo(build, 0, nameof(build));
            Guard.IsGreaterThanOrEqualTo(revision, 0, nameof(revision));

            Major = major;
            Minor = minor;
            Build = build;
            Revision = revision;
            Phase = phase;
        }

        /// <summary>
        /// Returns a new <see cref="ReleaseVersion"/> object whose value is the same as the current <see cref="ReleaseVersion"/>
        /// object.
        /// </summary>
        /// <returns>
        /// A new <see cref="object"/> whose values are a copy of the current <see cref="ReleaseVersion"/> object.
        /// </returns>
        public object Clone()
        {
            return new ReleaseVersion(Major, Minor, Build, Revision, Phase);
        }

        /// <summary>
        /// Compares the current <see cref="ReleaseVersion"/> object to a specified object and returns
        /// an indication of their relative values.
        /// </summary>
        /// <param name="version">
        /// An object to compare, or null.
        /// </param>
        /// <returns>
        /// A signed integer that indicates the relative values of the two objects, as shown
        /// in the following table.
        /// Return value – Meaning
        /// Less than zero – The current <see cref="ReleaseVersion"/> object is a version before <paramref name="version"/>.
        /// Zero – The current <see cref="ReleaseVersion"/> object is the same version as <paramref name="version"/>.
        /// Greater than zero – The current <see cref="ReleaseVersion"/> object is a version subsequent
        /// to <paramref name="version"/>, or <paramref name="version"/> is null.
        /// </returns>
        /// <exception cref="NullReferenceException">
        /// <paramref name="version"/> is not of type <see cref="ReleaseVersion"/>.
        /// </exception>
        public int CompareTo(object? version)
        {
            if (version == null)
            {
                return 1;
            }

            if (version is ReleaseVersion v)
            {
                return CompareTo(v);
            }

            throw new ArgumentException(nameof(version) + " must be of type " + nameof(ReleaseVersion));
        }

        /// <summary>
        /// Compares the current <see cref="ReleaseVersion"/> object to a specified <see cref="ReleaseVersion"/> object
        /// and returns an indication of their relative values.
        /// </summary>
        /// <param name="value">
        /// A <see cref="ReleaseVersion"/> object to compare to the current <see cref="ReleaseVersion"/> object, or null.
        /// </param>
        /// <returns>
        /// A signed integer that indicates the relative values of the two objects, as shown
        /// in the following table.
        /// Return value – Meaning
        /// Less than zero – The current <see cref="ReleaseVersion"/> object is a version before <paramref name="version"/>.
        /// Zero – The current <see cref="ReleaseVersion"/> object is the same version as <paramref name="version"/>.
        /// Greater than zero – The current <see cref="ReleaseVersion"/> object is a version subsequent
        /// to <paramref name="version"/>, or <paramref name="version"/> is null.
        /// </returns>
        public int CompareTo(ReleaseVersion? value)
        {
            return
                ReferenceEquals(value, this) ? 0 :
                value is null ? 1 :
                Major != value.Major ? (Major > value.Major ? 1 : -1) :
                Minor != value.Minor ? (Minor > value.Minor ? 1 : -1) :
                Build != value.Build ? (Build > value.Build ? 1 : -1) :
                Revision != value.Revision ? (Revision > value.Revision ? 1 : -1) :
                Phase != value.Phase? ((byte)Phase > (byte)value.Phase ? 1 : -1) :
                0;
        }

        /// <summary>
        /// Returns a value indicating whether the current <see cref="ReleaseVersion"/> object is equal
        /// to a specified object.
        /// </summary>
        /// <param name="obj">
        /// An object to compare with the current <see cref="ReleaseVersion"/> object, or null.
        /// </param>
        /// <returns>
        /// <c>true</c> if the current <see cref="ReleaseVersion"/> object and <paramref name="obj"/>
        /// are both <see cref="ReleaseVersion"/> objects,
        /// and every component of the current <see cref="ReleaseVersion"/> object matches the corresponding
        /// component of <paramref name="obj"/>; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object? obj)
        {
            return Equals(obj as ReleaseVersion);
        }

        /// <summary>
        /// Returns a value indicating whether the current <see cref="ReleaseVersion"/> object and a specified
        /// <see cref="ReleaseVersion"/> object represent the same value.
        /// </summary>
        /// <param name="obj">
        /// A <see cref="ReleaseVersion"/> object to compare to the current <see cref="ReleaseVersion"/> object, or null.
        /// </param>
        /// <returns>
        /// <c>true</c> if every component of the current <see cref="ReleaseVersion"/> object matches the corresponding
        /// component of the <paramref name="obj"/> parameter; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(ReleaseVersion? obj)
        {
            return ReferenceEquals(obj, this) ||
                (obj is not null &&
                Major == obj.Major &&
                Minor == obj.Minor &&
                Build == obj.Build &&
                Revision == obj.Revision &&
                Phase == obj.Phase);
        }

        /// <summary>
        /// Returns a hash code for the current <see cref="ReleaseVersion"/> object.
        /// </summary>
        /// <returns>
        /// A 32-bit signed integer hash code.
        /// </returns>
        public override int GetHashCode()
        {
            return HashCode.Combine(Major, Minor, Build, Revision, Phase);
        }

        /// <summary>
        /// Determines whether two specified <see cref="ReleaseVersion"/> objects are equal.
        /// </summary>
        /// <param name="v1">
        /// The first <see cref="ReleaseVersion"/> object.
        /// </param>
        /// <param name="v2">
        /// The second <see cref="ReleaseVersion"/> object.
        /// </param>
        /// <returns>
        /// true if v1 equals v2; otherwise, false.
        /// </returns>
        // Force inline as the true/false ternary takes it above ALWAYS_INLINE size even though the asm ends up smaller
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ReleaseVersion? v1, ReleaseVersion? v2)
        {
            // Test "right" first to allow branch elimination when inlined for null checks (== null)
            // so it can become a simple test
            if (v2 is null)
            {
                // return true/false not the test result https://github.com/dotnet/runtime/issues/4207
                return (v1 is null) ? true : false;
            }

            // Quick reference equality test prior to calling the virtual Equality
            return ReferenceEquals(v2, v1) ? true : v2.Equals(v1);
        }

        /// <summary>
        /// Determines whether the first specified <see cref="ReleaseVersion"/> object is greater than
        /// the second specified <see cref="ReleaseVersion"/> object.
        /// </summary>
        /// <param name="v1">
        /// The first <see cref="ReleaseVersion"/> object.
        /// </param>
        /// <param name="v2">
        /// The second <see cref="ReleaseVersion"/> object.
        /// </param>
        /// <returns>
        /// true if v1 is greater than v2; otherwise, false.
        /// </returns>
        public static bool operator >(ReleaseVersion? v1, ReleaseVersion? v2)
            => v2 < v1;

        /// <summary>
        /// Determines whether the first specified <see cref="ReleaseVersion"/> object is greater than
        /// or equal to the second specified <see cref="ReleaseVersion"/> object.
        /// </summary>
        /// <param name="v1">
        /// The first <see cref="ReleaseVersion"/> object.
        /// </param>
        /// <param name="v2">
        /// The second <see cref="ReleaseVersion"/> object.
        /// </param>
        /// <returns>
        /// true if v1 is greater than or equal to v2; otherwise, false.
        /// </returns>
        public static bool operator >=(ReleaseVersion? v1, ReleaseVersion? v2)
            => v2 <= v1;

        /// <summary>
        /// Determines whether two specified <see cref="ReleaseVersion"/> objects are not equal.
        /// </summary>
        /// <param name="v1">
        /// The first <see cref="ReleaseVersion"/> object.
        /// </param>
        /// <param name="v2">
        /// The second <see cref="ReleaseVersion"/> object.
        /// </param>
        /// <returns>
        /// true if v1 does not equal v2; otherwise, false.
        /// </returns>
        public static bool operator !=(ReleaseVersion? v1, ReleaseVersion? v2)
            => !(v1 == v2);

        /// <summary>
        /// Determines whether the first specified <see cref="ReleaseVersion"/> object is less than the
        /// second specified <see cref="ReleaseVersion"/> object.
        /// </summary>
        /// <param name="v1">
        /// The first <see cref="ReleaseVersion"/> object.
        /// </param>
        /// <param name="v2">
        /// The second <see cref="ReleaseVersion"/> object.
        /// </param>
        /// <returns>
        /// true if v1 is less than v2; otherwise, false.
        /// </returns>
        public static bool operator <(ReleaseVersion? v1, ReleaseVersion? v2)
        {
            if (v1 is null)
            {
                return !(v2 is null);
            }

            return v1.CompareTo(v2) < 0;
        }

        /// <summary>
        /// Determines whether the first specified <see cref="ReleaseVersion"/> object is less than or
        /// equal to the second <see cref="ReleaseVersion"/> object.
        /// </summary>
        /// <param name="v1">
        /// The first <see cref="ReleaseVersion"/> object.
        /// </param>
        /// <param name="v2">
        /// The second <see cref="ReleaseVersion"/> object.
        /// </param>
        /// <returns>
        /// true if v1 is less than or equal to v2; otherwise, false.
        /// </returns>
        public static bool operator <=(ReleaseVersion? v1, ReleaseVersion? v2)
        {
            if (v1 is null)
            {
                return true;
            }

            return v1.CompareTo(v2) <= 0;
        }

        /// <summary>
        /// Converts the string representation of a version number to an equivalent <see cref="ReleaseVersion"/>
        /// object.
        /// </summary>
        /// <param name="input">
        /// A string containing the major, minor, build, and revision numbers, where each
        /// number is delimited with a period character ('.'), followed by a dash character
        /// ('-') and the release phase.
        /// </param>
        /// <returns>
        /// An object that is equivalent to the version number specified in the input parameter.
        /// </returns>
        public static ReleaseVersion Parse(string input)
        {
            ReleaseVersion version = new();
            string versionPart = input;
            int phaseIdx = input.IndexOf('-');
            if (phaseIdx != -1)
            {
                versionPart = input[..phaseIdx];

                string phaseComp = input[(phaseIdx + 1)..];
                if (phaseComp.ToLowerInvariant() == "rc")
                    version.Phase = Phase.ReleaseCandidate;
                else
                    version.Phase = Enum.Parse<Phase>(phaseComp, true);
            }

            string[] components = versionPart.Split('.');
            Guard.IsGreaterThanOrEqualTo(components.Length, 2, nameof(components));

            if (components.Length >= 2)
            {
                version.Major = int.Parse(components[0]);
                version.Minor = int.Parse(components[1]);


                if (components.Length >= 3)
                {
                    version.Build = int.Parse(components[2]);


                    if (components.Length >= 4)
                    {
                        version.Revision = int.Parse(components[3]);
                    }
                }
            }

            return version;
        }

        /// <summary>
        /// Converts the value of the current <see cref="ReleaseVersion"/> object to its equivalent <see cref="string"/>
        /// representation.
        /// </summary>
        /// <returns>
        /// The string representation of the values of the major, minor, build, and
        /// revision components of the current <see cref="ReleaseVersion"/> object, as depicted in the
        /// following format. Each component is separated by a period character ('.'),
        /// and the release phase is appended to the end after a dash character ('-'). Square
        /// brackets ('[' and ']') indicate a component that will not appear in the return
        /// value if the component is not defined: <c>major.minor[.build[.revision]][-phase]</c> For example,
        /// if you create a <see cref="ReleaseVersion"/> object using the constructor <c>Version(1, 1)</c>, the
        /// returned string is <c>"1.1"</c>. If you create a <see cref="ReleaseVersion"/> object using the constructor
        /// <c>Version(1, 3, 4, 2, Phase.Beta)</c>, the returned string is <c>"1.3.4.2-beta"</c>.
        /// </returns>
        public override string ToString() => ToString(true);

        /// <inheritdoc cref="ToString"/>
        /// <param name="includeAllComponents">
        /// When set to <c>true</c>, the string will be formatted as <c>major.minor.build.revision-phase</c>,
        /// even if an optional component is not present.
        /// </param>
        public string ToString(bool includeAllComponents)
        {
            const char numDelim = '.';
            const char phaseDelim = '-';
            string output = Major.ToString() + numDelim + Minor.ToString();

            if (Build > 0 || includeAllComponents)
            {
                output += numDelim + Build.ToString();

                if (Revision > 0 || includeAllComponents)
                {
                    output += numDelim + Revision.ToString();
                }
            }

            if ((Phase != Phase.Unknown && Phase != Phase.Production) || includeAllComponents)
            {
                output += phaseDelim + (Phase == Phase.ReleaseCandidate ? "rc" : Phase.ToString().ToLowerInvariant());
            }

            return output;
        }

        /// <summary>
        /// Tries to convert the string representation of a version number to an equivalent
        /// <see cref="ReleaseVersion"/> object, and returns a value that indicates whether the conversion
        /// succeeded.
        /// </summary>
        /// <param name="input">
        /// A string that contains a version number to convert.
        /// </param>
        /// <param name="result">
        /// When this method returns, contains the <see cref="ReleaseVersion"/> equivalent of the number
        /// that is contained in input, if the conversion succeeded. If input is null, <see cref="String.Empty"/>,
        /// or if the conversion fails, result is null when the method returns.
        /// </param>
        /// <returns>
        /// true if the input parameter was converted successfully; otherwise, false.
        /// </returns>
        public static bool TryParse([NotNullWhen(true)] string? input, [NotNullWhen(true)] out ReleaseVersion? result)
        {
            try
            {
                Guard.IsNotNull(input, nameof(input));
                result = Parse(input);
                return true;
            }
            catch
            {
                result = null;
                return false;
            }
        }
    }

    public enum Phase : byte
    {
        Unknown,
        Debug,
        Alpha,
        Beta,
        ReleaseCandidate,
        Production
    }
}
