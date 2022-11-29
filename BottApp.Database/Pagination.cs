namespace BottApp.Database;

public class Pagination
{
    public Pagination(int skip, int take)
        {
            Skip = skip;
            Take = take;
        }


        public int Skip { get;}

        public int Take { get; }

    
}