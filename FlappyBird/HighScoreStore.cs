using System.Globalization;

namespace FlappyBird;

internal sealed class HighScoreStore(string path)
{
    public bool TryLoad(out int score)
    {
        score = 0;
        try
        {
            string text = File.ReadAllText(path);
            if (int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int value)
                && value >= 0)
            {
                score = value;
                return true;
            }

            return false;
        }
        catch (FileNotFoundException)
        {
            return true;
        }
        catch (DirectoryNotFoundException)
        {
            return true;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            return false;
        }
    }

    public bool TrySave(int score, out int savedScore)
    {
        savedScore = score;
        string temporaryPath = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using var updateLock = new FileStream(path + ".lock", FileMode.OpenOrCreate,
                FileAccess.ReadWrite, FileShare.None);
            if (TryLoad(out int currentScore) && currentScore >= score)
            {
                savedScore = currentScore;
                return true;
            }

            File.WriteAllText(temporaryPath, score.ToString(CultureInfo.InvariantCulture));
            File.Move(temporaryPath, path, overwrite: true);
            return true;
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException)
        {
            return false;
        }
        finally
        {
            try
            {
                File.Delete(temporaryPath);
            }
            catch (Exception error) when (error is IOException or UnauthorizedAccessException)
            {
                // A failed cleanup must not interrupt the game.
            }
        }
    }
}
