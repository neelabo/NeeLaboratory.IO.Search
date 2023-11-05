namespace NeeLaboratory.IO.Search
{
    public interface ISearchItem
    {
        SearchValue GetValue(SearchPropertyProfile profile);
    }

}
