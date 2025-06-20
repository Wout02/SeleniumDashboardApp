using SeleniumDashboardApp.Models;
using SQLite;

namespace SeleniumDashboardApp.Services
{
    public class LocalDatabaseService
    {
        private readonly SQLiteAsyncConnection _db;

        public LocalDatabaseService(string dbPath)
        {
#if DEBUG
            if (File.Exists(dbPath))
                File.Delete(dbPath);
#endif

            _db = new SQLiteAsyncConnection(dbPath);
            _db.CreateTableAsync<LocalTestRun>().Wait();
        }

        public Task<List<LocalTestRun>> GetTestRunsAsync() => _db.Table<LocalTestRun>().ToListAsync();

        public Task<int> SaveTestRunAsync(LocalTestRun run) => _db.InsertAsync(run);

        public Task<int> DeleteAllAsync() => _db.DeleteAllAsync<LocalTestRun>();

        public Task<int> DeleteTestRunByIdAsync(int backendId)
        {
            return _db.Table<LocalTestRun>()
                      .Where(run => run.BackendId == backendId)
                      .DeleteAsync();
        }

        public Task<LocalTestRun?> GetTestRunByIdAsync(int backendId)
        {
            return _db.Table<LocalTestRun>()
                      .Where(run => run.BackendId == backendId)
                      .FirstOrDefaultAsync();
        }
    }
}