namespace PharmacyIMS.Data
{
    public static class DbInitializer
    {
        private const string SeedSectionName = "InitialSeed";

        private static readonly string[] Roles = { "Admin", "Pharmacist" };

        private static readonly (string SectionKey, string Role)[] SeedAccounts =
        {
            ("AdminUser", "Admin"),
            ("StaffUser", "Pharmacist")
        };

        public static async Task SeedRolesAndUsersAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var configuration = serviceProvider.GetRequiredService<IConfiguration>();
            var logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(nameof(DbInitializer));

            await SeedRolesAsync(roleManager, logger);
            await SeedUsersAsync(userManager, configuration, logger);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
        {
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
        }

        private static async Task SeedUsersAsync(
            UserManager<IdentityUser> userManager,
            IConfiguration configuration,
            ILogger logger)
        {
            foreach (var (sectionKey, role) in SeedAccounts)
            {
                var section = configuration.GetSection($"{SeedSectionName}:{sectionKey}");
                var email = section["Email"]?.Trim();
                var password = section["Password"];

                if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                {
                    logger.LogWarning(
                        "Skipping seed for role {Role}: '{Section}:{Key}' needs both Email and Password in configuration.",
                        role, SeedSectionName, sectionKey);
                    continue;
                }

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
