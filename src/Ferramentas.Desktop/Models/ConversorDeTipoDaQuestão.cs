using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Ferramentas.Desktop.Models.Enumerados;

namespace Ferramentas.Desktop.Models;

public class ConversorDeTipoDaQuestão : Dictionary<TipoDaQuestão, IDataTemplate>, IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is TipoDaQuestão s)
            return this[s];

        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}