using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

namespace Application.Common.Constants;

public class AppJsonOptions
{
    public static readonly JsonSerializerOptions Default = new JsonSerializerOptions()
    {
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.BasicLatin, UnicodeRanges.Cyrillic, UnicodeRanges.CurrencySymbols),
        PropertyNameCaseInsensitive = true
    };
}
