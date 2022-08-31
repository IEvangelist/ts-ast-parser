using System.Text.RegularExpressions;
using Zu.TypeScript.TsTypes;

namespace Zu.TypeScript.TsParser
{
    public static class Core
    {
        // from core.ts

        public static int BinarySearch(int[] array, int value, Func<int, int, int>? comparer = null, int? offset = null)
        {
            if (array == null || array.Length == 0)
            {
                return -1;
            }
            var low = offset ?? 0;
            var high = array.Length - 1;
            comparer = comparer ?? ((v1, v2) => v1 < v2 ? -1 : (v1 > v2 ? 1 : 0));
            while (low <= high)
            {
                var middle = low + ((high - low) >> 1);
                var midValue = array[middle];
                if (comparer(midValue, value) == 0)
                {
                    return middle;
                }
                else
                if (comparer(midValue, value) > 0)
                {
                    high = middle - 1;
                }
                else
                {
                    low = middle + 1;
                }
            }
            return ~low;
        }
        public static bool PositionIsSynthesized(int pos)
        {
            // This is a fast way of testing the following conditions:
            //  pos == null || pos == null || isNaN(pos) || pos < 0;
            return !(pos >= 0);
        }

        public static ScriptKind EnsureScriptKind(string fileName, ScriptKind scriptKind)
        {
            // Using scriptKind as a condition handles both:
            // - 'scriptKind' is unspecified and thus it is `null`
            // - 'scriptKind' is set and it is `Unknown` (0)
            // If the 'scriptKind' is 'null' or 'Unknown' then we attempt
            // to get the ScriptKind from the file name. If it cannot be resolved
            // from the file name then the default 'TS' script kind is returned.
            var sk = scriptKind != ScriptKind.Unknown ? scriptKind : GetScriptKindFromFileName(fileName);
            return sk != ScriptKind.Unknown ? sk : ScriptKind.Ts;
        }
        public static ScriptKind GetScriptKindFromFileName(string fileName) =>
            Path.GetExtension(fileName)?.ToLower() switch
            {
                ".js" => ScriptKind.Js,
                ".jsx" => ScriptKind.Jsx,
                ".ts" => ScriptKind.Ts,
                ".tsx" => ScriptKind.Tsx,
                _ => ScriptKind.Unknown
            };

        public static string NormalizePath(string path)
        {
            path = NormalizeSlashes(path);
            var rootLength = GetRootLength(path);
            var root = path[..rootLength];
            var normalized = GetNormalizedParts(path, rootLength);
            if (normalized.Any())
            {
                var joinedParts = root + string.Join(DirectorySeparator.ToString(), normalized);//.join(directorySeparator);
                return PathEndsWithDirectorySeparator(path) ? joinedParts + DirectorySeparator : joinedParts;
            }
            else
            {
                return root;
            }
        }
        public static string NormalizeSlashes(string path) =>
            Regex.Replace(path, "/\\/ g", "/");

        public static int GetRootLength(string path)
        {
            if (path.charCodeAt(0) == (int)CharacterCodes.Slash)
            {
                if (path.charCodeAt(1) != (int)CharacterCodes.Slash)
                {
                    return 1;
                }
                var p1 = path.IndexOf("/", 2, StringComparison.Ordinal);
                if (p1 < 0)
                {
                    return 2;
                }
                var p2 = path.IndexOf("/", p1 + 1, StringComparison.Ordinal);
                return p2 < 0 ? p1 + 1 : p2 + 1;
            }
            if (path.charCodeAt(1) == (int)CharacterCodes.Colon)
            {
                return path.charCodeAt(2) == (int)CharacterCodes.Slash ? 3 : 2;
            }
            if (path.LastIndexOf("file:///", 0, StringComparison.Ordinal) == 0)
            {
                return "file:///".Length;
            }
            var idx = path.IndexOf("://", StringComparison.Ordinal);
            return idx != -1 ? idx + "://".Length : 0;
        }
        public static char DirectorySeparator = '/';
        public static int DirectorySeparatorCharCode = (int)CharacterCodes.Slash;
        public static List<string> GetNormalizedParts(string normalizedSlashedPath, int rootLength)
        {
            var parts = normalizedSlashedPath[rootLength..].Split(DirectorySeparator);
            List<string> normalized = new();
            foreach (var part in parts)
            {
                if (part != ".")
                {
                    if (part == ".." && normalized.Count > 0 && LastOrUndefined(normalized) != "..")
                    {
                        normalized.pop();
                    }
                    else
                    {
                        // A part may be an empty string (which is 'falsy') if the path had consecutive slashes,
                        // e.g. "path//file.ts".  Drop these before re-joining the parts.
                        if (part != null)
                        {
                            normalized.Add(part);
                        }
                    }
                }
            }
            return normalized;
        }
        public static T? LastOrUndefined<T>(List<T> array) where T : class =>
            array != null && array.Any()
                ? array.Last()
                : default;

        public static bool PathEndsWithDirectorySeparator(string path) =>
            path.charCodeAt(path.Length - 1) == DirectorySeparatorCharCode;

        public static bool FileExtensionIs(string path, string extension) =>
            path.Length > extension.Length && EndsWith(path, extension);

        public static bool EndsWith(string str, string suffix)
        {
            var expectedPos = str.Length - suffix.Length;
            return expectedPos >= 0 && str.IndexOf(suffix, expectedPos, StringComparison.Ordinal) == expectedPos;
        }

        public static Diagnostic CreateFileDiagnostic(SourceFile file, int start, int length, DiagnosticMessage message, params string[] arguments)
        {
            var end = start + length;
            if (file != null)
            {
               
            }
            var text = GetLocaleSpecificMessage(message);
            if (arguments.Length > 0) // 4)
            {
                //text = formatStringFromArgs(text, arguments, 4);
            }
            return new Diagnostic
            {
                File = file,
                Start = start,
                Length = length,

                MessageText = text,
                Category = message?.Category ?? DiagnosticCategory.Unknown,
                Code = message?.Code ?? 0,
            };
        }

        public static string GetLocaleSpecificMessage(DiagnosticMessage message)
        {
            return "localizedDiagnosticMessages";// && localizedDiagnosticMessages[message.key] || message.message;
        }

    }
}
