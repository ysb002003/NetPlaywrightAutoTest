namespace AutoTest;

/// <summary>
/// Sort ListView rows by column text.
/// </summary>
internal sealed class ListViewItemComparer : System.Collections.IComparer
{
    private readonly int _columnIndex;
    private readonly bool _ascending;

    public ListViewItemComparer(int columnIndex, bool ascending)
    {
        _columnIndex = columnIndex;
        _ascending = ascending;
    }

    public int Compare(object? x, object? y)
    {
        var a = x as ListViewItem;
        var b = y as ListViewItem;
        if (a is null && b is null)
        {
            return 0;
        }

        if (a is null)
        {
            return -1;
        }

        if (b is null)
        {
            return 1;
        }

        var sa = CellText(a, _columnIndex);
        var sb = CellText(b, _columnIndex);
        var cmp = string.Compare(sa, sb, StringComparison.OrdinalIgnoreCase);
        return _ascending ? cmp : -cmp;
    }

    private static string CellText(ListViewItem item, int col)
    {
        if (col <= 0)
        {
            return item.Text;
        }

        return col < item.SubItems.Count ? item.SubItems[col].Text : string.Empty;
    }
}
