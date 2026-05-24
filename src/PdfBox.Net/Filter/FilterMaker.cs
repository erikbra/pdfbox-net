/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 */

namespace PdfBox.Net.Filter;

public static class FilterMaker
{
    public static FilterFactory CreateFactory()
    {
        return FilterFactory.Instance;
    }
}
