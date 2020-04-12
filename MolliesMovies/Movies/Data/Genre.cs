namespace MolliesMovies.Movies.Data
{
    public class Genre
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
    }

    public class MovieGenre
    {
        public int Id { get; set; }
        
        public int MovieId { get; set; }
        
        public int GenreId { get; set; }
        
        public virtual Genre Genre { get; set; }
    }
}