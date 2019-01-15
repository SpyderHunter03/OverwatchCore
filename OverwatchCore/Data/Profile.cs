namespace OverwatchCore.Data
{
    public class Profile
    {
        public string Platform { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public string URLName { get; set; }
        public int PlayerLevel { get; set; }
        public string Portrait { get; set; }
        public bool IsPublic { get; set; }
    }
}