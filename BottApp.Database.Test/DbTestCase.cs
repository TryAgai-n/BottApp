namespace BottApp.Database.Test;

public class DbTestCase : IDisposable
{
    private Fixture _fixture;
    private Fixture Fixture => _fixture ??= Fixture.Create();


    internal DatabaseContainer DatabaseContainer => Fixture.DatabaseContainer;


    public void Dispose()
    {
        _fixture?.Dispose();
    }
}