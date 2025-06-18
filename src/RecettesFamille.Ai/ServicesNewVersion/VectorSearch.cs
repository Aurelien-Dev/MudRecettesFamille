namespace RecettesFamille.Ai.ServicesNewVersion;

public class VectorSearch
{
    public static IEnumerable<(RecetteVector, double)> SearchSimilar(float[] inputEmbedding, IEnumerable<RecetteVector> vectors, double minScore = 0.7)
    {
        return vectors
            .Select(v => (v, Score: CosineSimilarity(v.Vector, inputEmbedding)))
            .Where(x => x.Score >= minScore)
            .OrderByDescending(x => x.Score);
    }

    private static double CosineSimilarity(float[] a, float[] b)
    {
        double dot = 0, magA = 0, magB = 0;
        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }

        return dot / (Math.Sqrt(magA) * Math.Sqrt(magB) + 1e-8);
    }
}