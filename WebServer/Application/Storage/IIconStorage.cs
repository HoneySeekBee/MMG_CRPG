namespace Application.Storage
{
    public interface IIconStorage
    {
        Task SaveAsync(string key, Stream content, string contentType, CancellationToken ct = default);
        Task<bool> ExistsAsync(string key, CancellationToken ct = default);
        Task DeleteAsync(string key, CancellationToken ct = default);
        string GetPublicUrl(string key, int version);
    }
}
