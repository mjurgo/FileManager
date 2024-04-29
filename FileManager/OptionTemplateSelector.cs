using System.Windows;
using System.Windows.Controls;
using Engine.Config;

namespace FileManager;

public class OptionTemplateSelector : DataTemplateSelector
{
    public DataTemplate TextTemplate { get; set; }
    public DataTemplate SelectTypeTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
        if (item is Option option)
        {
            return option.Type switch
            {
                OptionType.Text => TextTemplate,
                OptionType.Select => SelectTypeTemplate,
                _ => throw new ArgumentOutOfRangeException()
            };
        }
        return base.SelectTemplate(item, container);
    }
}