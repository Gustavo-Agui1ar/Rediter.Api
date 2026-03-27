namespace Rediter.Api.Repositories
{
    public class UserRepository : BaseRepository<Models.User>
    {
        public UserRepository(Supabase.Client supabase, Session session) : base(supabase, session)
        {

            public bool DeleteUser(string userId)
            {
                return _session.Execute(conn =>
                {
                    using var cmd = new Npgsql.NpgsqlCommand("DELETE FROM users WHERE id = @id", conn);
                    cmd.Parameters.AddWithValue("id", userId);
                    return cmd.ExecuteNonQuery() > 0;
                });
            }
        }
    }
}
