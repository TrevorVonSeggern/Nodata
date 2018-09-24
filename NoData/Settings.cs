using System;
using System.Collections.Generic;
using System.Text;
using CodeTools;

namespace NoData
{
    public enum FilterSecurityTypes
    {
        AllowOnlyVisibleValues,
        AllowFilteringOnPropertiesThatAreNotDisplayed,
        AllowFilteringOnNonExplicitlyExpandedItems,
    }

    [Immutable]
    public class Settings
    {
    }
}
