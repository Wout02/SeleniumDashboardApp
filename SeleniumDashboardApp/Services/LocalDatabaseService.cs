using SeleniumDashboardApp.Models;
using SQLite;

namespace SeleniumDashboardApp.Services
{
    public class LocalDatabaseService
    {
        private readonly SQLiteAsyncConnection _db;

        public LocalDatabaseService(string dbPath)
        {
            _db = new SQLiteAsyncConnection(dbPath);
            _db.CreateTableAsync<TestRun>().Wait();
        }

        public Task<List<TestRun>> GetTestRunsAsync() => _db.Table<TestRun>().ToListAsync();
        public Task<int> SaveTestRunAsync(TestRun run) => _db.InsertAsync(run);
        public Task<int> DeleteAllAsync() => _db.DeleteAllAsync<TestRun>();
    }
}