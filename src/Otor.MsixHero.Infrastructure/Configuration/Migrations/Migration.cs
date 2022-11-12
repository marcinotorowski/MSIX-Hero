using System.Collections.Generic;
using System.Linq;

namespace Otor.MsixHero.Infrastructure.Configuration.Migrations
{
    public class Migration
    {
        private readonly Configuration _configuration;

        public Migration(Configuration configuration)
        {
            this._configuration = configuration;
        }

        public void Migrate()
        {
            var migrations = new List<ConfigurationMigration>
            {
                new InitialMigration(this._configuration),
                new SigningMigration(this._configuration)
            };

            foreach (var migration in migrations
                         .Where(m => !m.TargetRevision.HasValue || m.TargetRevision <= 0 || m.TargetRevision > this._configuration.Revision)
                         .OrderBy(m => m.TargetRevision ?? int.MinValue))
            {
                migration.Migrate();
            }
        }
    }
}
