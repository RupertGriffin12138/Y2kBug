public interface ISaveStore
{
    bool TryLoad(out string json);
    void Save(string json);
    void Wipe();
}
