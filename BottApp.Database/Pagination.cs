namespace BottApp.Database;

public class Pagination
{
    public Pagination(int page, int limit)
        {
            Page = page;
            Limit = limit;
        }

    public int Page { get;}

        public int Limit { get; }
        
        public int GetSkip()
        {
            return Page * Limit;
        }
    
}