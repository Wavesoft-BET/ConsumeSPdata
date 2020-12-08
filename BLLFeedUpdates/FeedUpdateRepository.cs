using System;
using System.Data;
using System.Data.SqlClient;
using BOFeedUpdates;

namespace BLLFeedUpdates
{
    public class FeedUpdateRepository
    {
        public static void SaveToDatabase(string json, int sportId)
        {
            using (var db = new FeedUpdatesEntities())
            {
                var pJson = new SqlParameter("@RMQJson", SqlDbType.VarChar) {Value = json};

                //Process data according to your needs here

            }
        }
    }
}
