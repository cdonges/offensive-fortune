namespace offensive_fortune;

public interface IFortuneService
{
    /// <summary>
    /// Gets a random fortune using simple text-based parsing with ROT13.
    /// </summary>
    Task<string> GetRandom2(string folderName);

    /// <summary>
    /// Gets a random fortune using binary Unix fortune format parsing.
    /// </summary>
    Task<string> GetRandom(string folderName);
}
