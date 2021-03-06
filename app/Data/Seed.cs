using System.Threading.Tasks;
using app.Entities;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;

namespace app.Data
{
    public static class Seed
    {
        public static async Task SeedDataBase(DataContext context)
        {
            // check if database has any data
            if (await context.Users.AnyAsync()) return;

            await SeedUsers(context);
            await SeedEpumpData(context);

            await context.SaveChangesAsync();
        }

        private static async Task SeedUsers(DataContext context)
        {
            var userData = await File.ReadAllTextAsync("Data/UserSeed.json");
            var users = JsonSerializer.Deserialize<List<AppUser>>(userData);

            // create users of class AppUser
            foreach (var user in users)
            {
                await context.Users.AddAsync(user);
            }
            await context.SaveChangesAsync();
        }

        private static async Task SeedEpumpData(DataContext context)
        {
            var epumpData = await File.ReadAllTextAsync("Data/EpumpDataSeed.json");
            var epumpDataList = JsonSerializer.Deserialize<List<EpumpData>>(epumpData);

            // create epump data of class EpumpData
            foreach (var item in epumpDataList)
            {
                await context.EpumpData.AddAsync(item);

                // Update the user table with the epump FK
                var user = item;
                var check = await context.Users.AnyAsync(u => u.ChatId == user.ChatId);

                if (!check) continue;
                var userToUpdate = await context.Users.FirstOrDefaultAsync(x => x.ChatId == item.ChatId);
                userToUpdate.EpumpDataId = item.ID;
            }
        }
    }
}