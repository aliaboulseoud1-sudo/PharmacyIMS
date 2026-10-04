namespace PharmacyIMS.Data
{
    public static class DbInitializer
    {
        private static readonly string[] Roles = { "Admin", "Pharmacist" };

        private static readonly (string Email, string Password, string Role)[] DefaultUsers =
        {
            ("admin@pharmacy.com", "Admin@123456", "Admin"),
            ("user@pharmacy.com", "User@123456", "Pharmacist")
        };

        public static async Task SeedRolesAndUsersAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DbInitializer));

            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    var result = await roleManager.CreateAsync(new IdentityRole(role));
                    if (result.Succeeded)
                    {
                        logger.LogInformation("Seeded role: {Role}", role);
                    }
                    else
                    {
                        logger.LogError("Failed to create role {Role}: {Errors}", role,
                            string.Join(", ", result.Errors.Select(e => e.Description)));
                    }
                }
            }

            foreach (var (email, password, role) in DefaultUsers)
            {
                var existingUser = await userManager.FindByEmailAsync(email);

                if (existingUser == null)
                {
                    var user = new IdentityUser
                    {
                        UserName = email,
                        Email = email,
                        EmailConfirmed = true
                    };

                    var createResult = await userManager.CreateAsync(user, password);

                    if (createResult.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, role);
                        logger.LogInformation("Seeded default user {Email} with role {Role}", email, role);
                    }
                    else
                    {
                        logger.LogError("Failed to seed user {Email}: {Errors}", email,
                            string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    }
                }
                else if (!await userManager.IsInRoleAsync(existingUser, role))
                {
                    await userManager.AddToRoleAsync(existingUser, role);
                    logger.LogInformation("Added missing role {Role} to existing user {Email}", role, email);
                }
            }
        }
    }
}
