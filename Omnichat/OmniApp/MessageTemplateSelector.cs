using System.Windows;
using System.Windows.Controls;

namespace OmniApp
{
    public class MessageTemplateSelector : DataTemplateSelector
    {
        public DataTemplate UserTemplate { get; set; }
        public DataTemplate ClientTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is Message message)
            {
                if (message.Source == "User" || message.Source == "System")
                {
                    return UserTemplate;
                }
                else 
                {
                    return ClientTemplate;
                }
            }

            return base.SelectTemplate(item, container);
        }
    }
}
