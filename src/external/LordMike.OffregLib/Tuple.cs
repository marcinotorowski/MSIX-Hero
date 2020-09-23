namespace OffregLib
{
    /// <summary>
    /// Basic tuple to replicate that of .NET 3 and later
    /// </summary>
    /// <typeparam name="K">Type item 1</typeparam>
    /// <typeparam name="V">Type item 2</typeparam>
    internal class Tuple<K, V>
    {
        public K Item1 { get; set; }
        public V Item2 { get; set; }

        public Tuple()
        {
            
        }

        public Tuple(K item1, V item2)
        {
            Item1 = item1;
            Item2 = item2;
        }
    }
}
