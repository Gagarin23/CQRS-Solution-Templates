using System;
using System.Linq;
using System.Text.RegularExpressions;
using FluentValidation;

namespace Application.Common.Validators;

public abstract partial class InputValidator<T> : AbstractValidator<T>
{
    /// <summary>
    /// Поиск любого символа кроме букв юникода и тире
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"[^\p{L}\-]")]
    private static partial Regex GetAnyNonLetterSymbolRegex();
    
    /// <summary>
    /// Поиск букв юникода и пробелов
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"[\p{L}\s]")]
    private static partial Regex GetAnyLetterOrWhitespaceSymbolRegex();
    
    /// <summary>
    /// Соответствие формату мобильного телефона
    /// </summary>
    /// <returns></returns>
    [GeneratedRegex(@"\+\d*\(\d*\)\d{3}-\d{2}-\d{2}")]
    private static partial Regex GetIsMobilePhoneRegex();
    
    protected bool ValidateId(long id)
    {
        return id > 0;
    }

    protected bool ValidatePersonName(string name)
    {
        if (name == null)
            throw new ArgumentNullException(nameof(name));
        
        return name != string.Empty
               && char.IsUpper(name[0])
               && !GetAnyNonLetterSymbolRegex().IsMatch(name);
    }

    protected bool ValidateMobilePhone(string mobilePhone)
    {
        if (mobilePhone == null)
            throw new ArgumentNullException(nameof(mobilePhone));
        
        return mobilePhone.Length is > 9 and < 15 
               && !GetAnyLetterOrWhitespaceSymbolRegex().IsMatch(mobilePhone) 
               && GetIsMobilePhoneRegex().IsMatch(mobilePhone);
    }

    protected bool ValidatePassword(string password)
    {
        if (password == null)
            throw new ArgumentNullException(nameof(password));
        
        return password.Length > 7
               && password.Any(char.IsLetter)
               && GetAnyNonLetterSymbolRegex().IsMatch(password);
    }
}
