using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json;
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalCount { get; set; } // Sau khi filter
    public int Page { get; set; }
    public int PageSize { get; set; }
}


public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(this IQueryable<T> query, int page, int pageSize)
    {
        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new PagedResult<T> { Items = items, TotalCount = totalCount, Page = page, PageSize = pageSize };
    }
    
    public static IQueryable<T> ApplyDynamicFilter<T>(this IQueryable<T> query, Dictionary<string, object> filters)
    {
        if (filters == null || filters.Count == 0)
            return query;

        int page = 1;
        int pageSize = 10;

        if (filters.TryGetValue("Page", out var pg) && pg is JsonElement pageElement)
        {
            if (pageElement.ValueKind == JsonValueKind.Number)
                page = pageElement.GetInt32();
            else if (pageElement.ValueKind == JsonValueKind.String && int.TryParse(pageElement.GetString(), out var parsedPage))
                page = parsedPage;
        }

        if (filters.TryGetValue("PageSize", out var ps) && ps is JsonElement sizeElement)
        {
            if (sizeElement.ValueKind == JsonValueKind.Number)
                pageSize = sizeElement.GetInt32();
            else if (sizeElement.ValueKind == JsonValueKind.String && int.TryParse(sizeElement.GetString(), out var parsedSize))
                pageSize = parsedSize;
        }

        // Xử lý các điều kiện đặc biệt: StartTime / EndTime
        filters.TryGetValue("StartTime", out var startTimeRaw);
        filters.TryGetValue("EndTime", out var endTimeRaw);

        DateTime? startTime = TryParseDate(startTimeRaw);
        DateTime? endTime = TryParseDate(endTimeRaw);

        var actualFilters = filters
            .Where(kvp => !string.Equals(kvp.Key, "Page", StringComparison.OrdinalIgnoreCase) &&
                          !string.Equals(kvp.Key, "PageSize", StringComparison.OrdinalIgnoreCase) &&
                          !string.Equals(kvp.Key, "StartTime", StringComparison.OrdinalIgnoreCase) &&
                          !string.Equals(kvp.Key, "EndTime", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

        var param = Expression.Parameter(typeof(T), "x");
        Expression? combined = null;
        foreach (var kvp in actualFilters)
        {
            var prop = typeof(T).GetProperty(kvp.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (prop == null || !prop.CanRead) continue;

            if (!TryConvertValue(kvp.Value, prop.PropertyType, out var convertedValue))
                continue;

            var member = Expression.Property(param, prop.Name);
            Expression constant = Expression.Constant(convertedValue, prop.PropertyType);
            Expression body;

            if (prop.PropertyType == typeof(string))
            {
                var method = typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
                body = Expression.Call(member, method, constant);
            }
            else if (Nullable.GetUnderlyingType(prop.PropertyType) != null)
            {
                var hasValue = Expression.Property(member, "HasValue");
                var value = Expression.Property(member, "Value");

                // Ép kiểu constant nếu chưa khớp với value
                if (value.Type != constant.Type)
                {
                    constant = Expression.Convert(constant, value.Type);
                }

                var equal = Expression.Equal(value, constant);
                body = Expression.AndAlso(hasValue, equal);
            }
            else
            {
                // Ép kiểu nếu member.Type và constant.Type không khớp
                if (member.Type != constant.Type)
                {
                    constant = Expression.Convert(constant, member.Type);
                }

                body = Expression.Equal(member, constant);
            }

            combined = combined == null ? body : Expression.AndAlso(combined, body);
        }


        // Áp dụng StartTime và EndTime (nếu có)
        var dateProp = typeof(T).GetProperty("CreatedAt") ?? typeof(T).GetProperty("TimeCreated");
        if (dateProp != null && dateProp.PropertyType == typeof(DateTime))
        {
            var dateMember = Expression.Property(param, dateProp.Name);

            if (startTime != null)
            {
                var startConst = Expression.Constant(startTime.Value);
                var startCond = Expression.GreaterThanOrEqual(dateMember, startConst);
                combined = combined == null ? startCond : Expression.AndAlso(combined, startCond);
            }

            if (endTime != null)
            {
                var endConst = Expression.Constant(endTime.Value);
                var endCond = Expression.LessThanOrEqual(dateMember, endConst);
                combined = combined == null ? endCond : Expression.AndAlso(combined, endCond);
            }
        }

        if (combined != null)
        {
            var predicate = Expression.Lambda<Func<T, bool>>(combined, param);
            query = query.Where(predicate);
        }

        return query;
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
                // Nếu extract ra vẫn là JsonElement, cố gắng ép kiểu boolean
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