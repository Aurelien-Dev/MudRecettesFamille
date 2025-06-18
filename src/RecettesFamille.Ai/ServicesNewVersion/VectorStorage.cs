namespace RecettesFamille.Ai.ServicesNewVersion;

public class VectorStorage
{
    public static void Save(string folder, RecetteVector vec)
    {
        var path = Path.Combine(folder, $"{vec.RecipeId}.vec");
        using var fs = File.Create(path);
        using var bw = new BinaryWriter(fs);

        bw.Write(vec.RecipeId);
        bw.Write(vec.RecipeName);
        bw.Write(string.Join(", ", vec.Tags));
        bw.Write(vec.Vector.Length);
        foreach (var f in vec.Vector)
            bw.Write(f);
    }

    public static RecetteVector Load(string path)
    {
        using var fs = File.OpenRead(path);
        using var br = new BinaryReader(fs);

        var id = br.ReadString();
        var recipeName = br.ReadString();
        var len = br.ReadInt32();
        var embedding = new float[len];
        for (int i = 0; i < len; i++)
            embedding[i] = br.ReadSingle();

        return new RecetteVector { RecipeId = Convert.ToInt32(id), RecipeName = recipeName, Vector = embedding };
    }

    public static IEnumerable<RecetteVector> LoadAll(string folder)
        => Directory.EnumerateFiles(folder, "*.vec").Select(Load);
}
