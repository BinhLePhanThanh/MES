using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
using System.Collections;
using System.Text;
using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Query;
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}


public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query, int page, int pageSize)
    {
        if (query.Provider is IAsyncQueryProvider)
        {
            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize)
                                   .Take(pageSize)
                                   .ToListAsync();
            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
        else
        {
            var list = query.ToList();
            var totalCount = list.Count;
            var items = list.Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .ToList();
            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
    }


    public static async Task<IQueryable<T>> ApplyDynamicFilter<T>(
    this IQueryable<T> query,
    Dictionary<string, object> filters)
    {
        if (filters == null || filters.Count == 0)
            return query;

        // Helper local để ghép AndAlso khi combined có thể null
        Expression AndAlso(Expression? left, Expression right) => left == null ? right : Expression.AndAlso(left, right);

        // Lấy StartTime / EndTime chung (global)
        filters.TryGetValue("StartTime", out var startTimeRaw);
        filters.TryGetValue("EndTime", out var endTimeRaw);
        DateTime? startTime = TryParseDate(startTimeRaw);
        DateTime? endTime = TryParseDate(endTimeRaw);

        var actualFilters = filters
            .Where(kvp => !kvp.Key.EndsWith("_Mode", StringComparison.OrdinalIgnoreCase) &&
                          !string.Equals(kvp.Key, "Page", StringComparison.OrdinalIgnoreCase) &&
                          !string.Equals(kvp.Key, "PageSize", StringComparison.OrdinalIgnoreCase) &&
                          !string.Equals(kvp.Key, "StartTime", StringComparison.OrdinalIgnoreCase) &&
                          !string.Equals(kvp.Key, "EndTime", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        var param = Expression.Parameter(typeof(T), "x");
        Expression? combined = null;

        bool hasFuzzy = false;
        List<Func<T, bool>> fuzzyFilters = new();

        foreach (var kvp in actualFilters)
        {
            var key = kvp.Key;

            // --- Xử lý postfix _startDate / _endDate cho từng field ---
            if (key.EndsWith("_startDate", StringComparison.OrdinalIgnoreCase) ||
                key.EndsWith("_endDate", StringComparison.OrdinalIgnoreCase))
            {
                bool isStart = key.EndsWith("_startDate", StringComparison.OrdinalIgnoreCase);
                var fieldName = key.Substring(0, key.Length - (isStart ? "_startDate".Length : "_endDate".Length));

                if (string.IsNullOrWhiteSpace(fieldName))
                    continue;

                var dateProp = typeof(T).GetProperty(fieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (dateProp == null || !dateProp.CanRead)
                    continue;

                // Only support DateTime / DateTime? here
                var underlying = Nullable.GetUnderlyingType(dateProp.PropertyType);
                bool isNullableDate = underlying == typeof(DateTime);
                bool isPlainDate = dateProp.PropertyType == typeof(DateTime);

                if (!isPlainDate && !isNullableDate)
                    continue;

                var dt = TryParseDate(kvp.Value);
                if (dt == null)
                    continue;

                var dateMember = Expression.Property(param, dateProp.Name);

                Expression dateCondition;
                if (isNullableDate)
                {
                    // member.HasValue && member.Value >=/<= constant
                    var hasValue = Expression.Property(dateMember, "HasValue");
                    var valueProp = Expression.Property(dateMember, "Value");
                    var constExpr = Expression.Constant(dt.Value, typeof(DateTime));
                    var cmp = isStart
                        ? Expression.GreaterThanOrEqual(valueProp, constExpr)
                        : Expression.LessThanOrEqual(valueProp, constExpr);
                    dateCondition = Expression.AndAlso(hasValue, cmp);
                }
                else // DateTime
                {
                    var constExpr = Expression.Constant(dt.Value, typeof(DateTime));
                    dateCondition = isStart
                        ? (Expression)Expression.GreaterThanOrEqual(dateMember, constExpr)
                        : (Expression)Expression.LessThanOrEqual(dateMember, constExpr);
                }

                combined = combined == null ? dateCondition : Expression.AndAlso(combined, dateCondition);
                continue; // đã xử lý key này, chuyển key khác
            }

            // --- Các filter bình thường ---
            var prop = typeof(T).GetProperty(kvp.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null || !prop.CanRead) continue;

            var member = Expression.Property(param, prop.Name);

            if (prop.PropertyType == typeof(string))
            {
                string modeKey = kvp.Key + "_Mode";
                string mode = filters.TryGetValue(modeKey, out var modeVal) ? modeVal?.ToString() ?? "" : "Normal";

                if (!TryConvertValue(kvp.Value, typeof(string), out var convertedValue))
                    continue;

                string searchValue = convertedValue?.ToString() ?? "";

                if (mode.Equals("Fuzzy", StringComparison.OrdinalIgnoreCase))
                {
                    hasFuzzy = true;
                    var normSearch = TextUtils.NormalizeFuzzy(searchValue);
                    int searchLen = normSearch.Length;
                    const int c = 2;
                    int tolerance = searchLen  / 2 + c;

                    fuzzyFilters.Add(item =>
                    {
                        var propValue = prop.GetValue(item)?.ToString() ?? "";
                        var normValue = TextUtils.NormalizeFuzzy(propValue);
                        int valueLen = normValue.Length;
                        if (searchLen == 0 || valueLen == 0) return false;

                        int winLen = Math.Min(valueLen, searchLen + c);

                        // Trường hợp ngắn hơn hoặc bằng window size → so trực tiếp
                        if (valueLen <= winLen)
                            return TextUtils.LevenshteinDistance(normValue, normSearch) <= tolerance;

                        // Trượt cửa sổ, dừng ngay khi tìm được match
                        ReadOnlySpan<char> span = normValue.AsSpan();
                        for (int start = 0; start <= valueLen - winLen; start++)
                        {
                            if (TextUtils.LevenshteinDistance(span.Slice(start, winLen).ToString(), normSearch) <= tolerance)
                                return true;
                        }
                        return false;
                    });

                    continue; // đã xử lý fuzzy, không cần thêm vào combined
                }

                else
                {
                    // Normal search
                    var toLowerMethod = typeof(string).GetMethod(nameof(string.ToLower), System.Type.EmptyTypes)!;
                    var trimMethod = typeof(string).GetMethod(nameof(string.Trim), System.Type.EmptyTypes)!;

                    var lowered = Expression.Call(SafeStringMember(member), toLowerMethod);
                    var trimmed = Expression.Call(lowered, trimMethod);

                    var constNorm = Expression.Constant(searchValue.Trim().ToLowerInvariant());
                    var containsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;

                    combined = AndAlso(combined, Expression.Call(trimmed, containsMethod, constNorm));
                }
            }
            else
            {
                if (!TryConvertValue(kvp.Value, prop.PropertyType, out var convertedValue))
                    continue;

                var constant = Expression.Constant(convertedValue, prop.PropertyType);
                combined = combined == null ? Expression.Equal(member, constant) : Expression.AndAlso(combined, Expression.Equal(member, constant));
            }
        }

        // --- Global CreatedAt / TimeCreated filtering (unchanged) ---
        var datePropGlobal = typeof(T).GetProperty("CreatedAt") ?? typeof(T).GetProperty("TimeCreated");
        if (datePropGlobal != null && datePropGlobal.PropertyType == typeof(DateTime))
        {
            var dateMember = Expression.Property(param, datePropGlobal.Name);

            if (startTime != null)
            {
                var startConst = Expression.Constant(startTime.Value, typeof(DateTime));
                var startCond = Expression.GreaterThanOrEqual(dateMember, startConst);
                combined = combined == null ? startCond : Expression.AndAlso(combined, startCond);
            }

            if (endTime != null)
            {
                var endConst = Expression.Constant(endTime.Value, typeof(DateTime));
                var endCond = Expression.LessThanOrEqual(dateMember, endConst);
                combined = combined == null ? endCond : Expression.AndAlso(combined, endCond);
            }
        }

        // Apply normal filters in EF
        if (combined != null)
        {
            var predicate = Expression.Lambda<Func<T, bool>>(combined, param);
            query = query.Where(predicate);
        }

        // Only load from DB if fuzzy filter exists
        if (hasFuzzy)
        {
            var list = await query.ToListAsync();
            foreach (var fuzzy in fuzzyFilters)
                list = list.Where(fuzzy).ToList();

            return list.AsQueryable(); // in-memory IQueryable
        }

        return query; // still EF IQueryable
    }




    private static Expression SafeStringMember(Expression member)
    {
        var nullConst = Expression.Constant(string.Empty, typeof(string));
        return Expression.Condition(
            Expression.Equal(member, Expression.Constant(null, typeof(string))),
            nullConst,
            member
        );
    }

    private static Expression AndAlso(Expression? left, Expression right)
    {
        return left == null ? right : Expression.AndAlso(left, right);
    }

    public static class TextUtils
    {
        public static int LevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s)) return t.Length;
            if (string.IsNullOrEmpty(t)) return s.Length;

            var dp = new int[s.Length + 1, t.Length + 1];
            for (int i = 0; i <= s.Length; i++) dp[i, 0] = i;
            for (int j = 0; j <= t.Length; j++) dp[0, j] = j;

            for (int i = 1; i <= s.Length; i++)
            {
                for (int j = 1; j <= t.Length; j++)
                {
                    int cost = (s[i - 1] == t[j - 1]) ? 0 : 1;
                    dp[i, j] = Math.Min(
                        Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                        dp[i - 1, j - 1] + cost
                    );
                }
            }
            return dp[s.Length, t.Length];
        }


        public static string NormalizeQuick(string? input)
        {
            return (input ?? "").Trim().ToLowerInvariant();
        }

        public static string NormalizeFuzzy(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var lower = input.Trim().ToLowerInvariant();
            var normalized = lower.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var ch in normalized)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            }
            return Regex.Replace(sb.ToString(), @"\s+", "");

        }
        public static int LevenshteinDistance(ReadOnlySpan<char> source, ReadOnlySpan<char> target, int tolerance)
        {
            int n = source.Length;
            int m = target.Length;

            // Nếu chênh lệch độ dài lớn hơn tolerance thì bỏ luôn
            if (Math.Abs(n - m) > tolerance)
                return tolerance + 1;

            // Luôn để n >= m để giảm kích thước mảng
            if (n < m)
            {
                var tempSpan = source;
                source = target;
                target = tempSpan;
                n = source.Length;
                m = target.Length;
            }

            var prevRow = new int[m + 1];
            var currRow = new int[m + 1];

            for (int j = 0; j <= m; j++)
                prevRow[j] = j;

            for (int i = 1; i <= n; i++)
            {
                currRow[0] = i;

                // Giới hạn window theo tolerance
                int start = Math.Max(1, i - tolerance);
                int end = Math.Min(m, i + tolerance);

                // Nếu start > 1, set giá trị trước đó = tolerance+1 (ngoài phạm vi)
                if (start > 1)
                    currRow[start - 1] = tolerance + 1;

                int minInRow = tolerance + 1;

                for (int j = start; j <= end; j++)
                {
                    int cost = source[i - 1] == target[j - 1] ? 0 : 1;
                    currRow[j] = Math.Min(
                        Math.Min(prevRow[j] + 1,     // delete
                                 currRow[j - 1] + 1), // insert
                        prevRow[j - 1] + cost         // replace
                    );

                    if (currRow[j] < minInRow)
                        minInRow = currRow[j];
                }

                // Nếu hàng này toàn giá trị > tolerance thì bỏ luôn
                if (minInRow > tolerance)
                    return tolerance + 1;

                // Hoán đổi mảng
                var tmp = prevRow;
                prevRow = currRow;
                currRow = tmp;
            }

            return prevRow[m];
        }
    }


    private static DateTime? TryParseDate(object? value)
    {
        try
        {
            if (value is JsonElement json)
            {
                value = ExtractJsonValue(json);
            }

            if (value is string s && DateTime.TryParse(s, out var dt))
                return dt;

            if (value is DateTime dt2)
                return dt2;

            return null;
        }
        catch
        {
            return null;
        }
    }


    private static bool TryConvertValue(object value, System.Type targetType, out object? result)
    {
        try
        {
            if (value is JsonElement jsonElement)
            {
                var extracted = ExtractJsonValue(jsonElement);
                if (extracted is JsonElement innerJson)
                {
                    switch (innerJson.ValueKind)
                    {
                        case JsonValueKind.True:
                            value = true;
                            break;
                        case JsonValueKind.False:
                            value = false;
                            break;
                        case JsonValueKind.Number:
                            if (innerJson.TryGetInt32(out var intVal))
                                value = intVal;
                            else if (innerJson.TryGetDouble(out var dblVal))
                                value = dblVal;
                            break;
                        case JsonValueKind.String:
                            value = innerJson.GetString();
                            break;
                        default:
                            value = innerJson.ToString();
                            break;
                    }
                }
                else
                {
                    value = extracted;
                }
            }

            if (value == null)
            {
                result = null;
                return !targetType.IsValueType || Nullable.GetUnderlyingType(targetType) != null;
            }

            var nonNullableType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            if (nonNullableType == typeof(string))
            {
                result = value.ToString();
                return true;
            }

            if (nonNullableType.IsEnum)
            {
                result = Enum.Parse(nonNullableType, value.ToString()!, ignoreCase: true);
                return true;
            }

            if (nonNullableType == typeof(Guid))
            {
                result = Guid.Parse(value.ToString()!);
                return true;
            }

            if (nonNullableType == typeof(DateTime))
            {
                result = DateTime.Parse(value.ToString()!);
                return true;
            }
            if (nonNullableType == typeof(bool))
            {
                if (value is bool b)
                {
                    result = b;
                    return true;
                }
                if (value is string s)
                {
                    if (bool.TryParse(s, out var b2))
                    {
                        result = b2;
                        return true;
                    }
                }
                result = null;
                return false;
            }

            result = Convert.ChangeType(value, nonNullableType);
            return true;
        }
        catch
        {
            result = null;
            return false;
        }
    }

    private static object? ExtractJsonValue(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? l :
                                    element.TryGetDouble(out var d) ? d : element.GetDecimal(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => element.ToString()
        };
    }



}