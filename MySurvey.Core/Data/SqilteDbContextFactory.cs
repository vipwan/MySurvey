// Licensed to the MySurvey.Core under one or more agreements.
// The MySurvey.Core licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.EntityFrameworkCore;
namespace MySurvey.Core.Data;

/// <summary>
/// Creates an instance of ApplicationDbContext configured to use a SQLite database. It sets the database source to
/// 'MySurvey.db'.
/// </summary>
public class SqilteDbContextFactory
{
    public static ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlite("Data Source=MySurvey.db");
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
