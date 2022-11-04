using TradingTrainer.Model;

namespace TradingTrainer.DAL
{
    public interface ISearchResultRepositry
    {
        Task SaveSearchResultAsync(SearchResult result);
        Task<List<SearchResult>?> GetAllKeywordsAsync();
        Task<SearchResult?> GetOneKeywordAsync(string keyWord);
        Task<bool> FindMatchAsync(string word);

        void DeleteSearchResult(string symbol);
    }
}
