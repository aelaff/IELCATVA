using System.Collections.Generic;

[System.Serializable]
public class Root
{
    public int Category;
    public List<Word2> Words;
}

[System.Serializable]
public class Word2
{
    public int wordID;
    public string Turkish;
    public string EnglishMeaning;
    public int Category;
    public int Difficulty;
}

[System.Serializable]
public class Word
{
    public int WordId { get; set; }
    public int CategoryId { get; set; }
    public string Turkish { get; set; }
    public string EnglishMeaning { get; set; }
    public int Difficulty { get; set; }
}
