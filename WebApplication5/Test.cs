public class Test
{
    public int Deneme { get; set; }
    public ICollection<Test2> DenemeList { get; set; }
}

public class Test2
{
    public int SubDeneme { get; set; }
    public string SubDeneme1 { get; set; }

}