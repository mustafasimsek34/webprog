using FitnessCenterManagement.Models;
using Microsoft.AspNetCore.Identity;

namespace FitnessCenterManagement.Data
{
    public static class DbSeeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Seed Roles
            await SeedRolesAsync(roleManager);

            // Seed Admin User
            await SeedAdminUserAsync(userManager);

            // Seed Sample Data
            await SeedSampleDataAsync(context, userManager);
        }

        private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            string[] roles = { "Admin", "Member" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }

        private static async Task SeedAdminUserAsync(UserManager<ApplicationUser> userManager)
        {
            var adminEmail = "b241210383@sakarya.edu.tr";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true,
                    RegistrationDate = DateTime.Now
                };

                var result = await userManager.CreateAsync(adminUser, "sau");

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }
        }

        private static async Task SeedSampleDataAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            // Seed Gym
            if (!context.Gyms.Any())
            {
                context.Gyms.Add(new Gym
                {
                    Name = "Premium Fitness Center",
                    Location = "Sakarya University Campus, Serdivan/Sakarya",
                    WorkingHoursStart = "06:00",
                    WorkingHoursEnd = "23:00",
                    Description = "State-of-the-art fitness facility with experienced trainers",
                    ContactPhone = "0264-295-5000"
                });
                await context.SaveChangesAsync();
            }

            // Seed Services
            if (!context.Services.Any())
            {
                var services = new List<Service>
                {
                    new Service { Name = "Yoga", Description = "Relaxing yoga sessions for flexibility and mindfulness", Duration = 60, Price = 150.00m, IsActive = true },
                    new Service { Name = "Pilates", Description = "Core strengthening and body conditioning", Duration = 45, Price = 120.00m, IsActive = true },
                    new Service { Name = "Personal Training", Description = "One-on-one fitness coaching", Duration = 60, Price = 200.00m, IsActive = true },
                    new Service { Name = "CrossFit", Description = "High-intensity functional training", Duration = 60, Price = 180.00m, IsActive = true },
                    new Service { Name = "Zumba", Description = "Fun dance fitness workout", Duration = 50, Price = 100.00m, IsActive = true }
                };
                context.Services.AddRange(services);
                await context.SaveChangesAsync();
            }

            // Seed Trainers
            if (!context.Trainers.Any())
            {
                var trainers = new List<Trainer>
                {
                    new Trainer 
                    { 
                        Name = "Ahmet Yılmaz", 
                        Email = "ahmet.yilmaz@fitness.com", 
                        PhoneNumber = "0555-123-4567",
                        Biography = "pilates ve fitnessta 10 yıl tecrübe sahibi sertifikalı eğitmen",
                        IsAvailable = true
                    },
                    new Trainer 
                    { 
                        Name = "Ayşe Demir", 
                        Email = "ayse.demir@fitness.com", 
                        PhoneNumber = "0555-234-5678",
                        Biography = "CrossFit Seviye 2 eğitmeni ve beslenme uzmanı",
                        IsAvailable = true
                    },
                    new Trainer 
                    { 
                        Name = "Mehmet Kaya", 
                        Email = "mehmet.kaya@fitness.com", 
                        PhoneNumber = "0555-345-6789",
                        Biography = "Güç ve kondisyon konusunda uzman kişisel antrenör",
                        IsAvailable = true
                    }
                };
                context.Trainers.AddRange(trainers);
                await context.SaveChangesAsync();

                // Assign services to trainers
                var yoga = context.Services.First(s => s.Name == "Yoga");
                var pilates = context.Services.First(s => s.Name == "Pilates");
                var personalTraining = context.Services.First(s => s.Name == "Personal Training");
                var crossfit = context.Services.First(s => s.Name == "CrossFit");
                var zumba = context.Services.First(s => s.Name == "Zumba");

                var ahmet = context.Trainers.First(t => t.Name == "Ahmet Yılmaz");
                var ayse = context.Trainers.First(t => t.Name == "Ayşe Demir");
                var mehmet = context.Trainers.First(t => t.Name == "Mehmet Kaya");

                var trainerServices = new List<TrainerService>
                {
                    new TrainerService { TrainerId = ahmet.Id, ServiceId = yoga.Id },
                    new TrainerService { TrainerId = ahmet.Id, ServiceId = pilates.Id },
                    new TrainerService { TrainerId = ayse.Id, ServiceId = crossfit.Id },
                    new TrainerService { TrainerId = ayse.Id, ServiceId = zumba.Id },
                    new TrainerService { TrainerId = mehmet.Id, ServiceId = personalTraining.Id },
                    new TrainerService { TrainerId = mehmet.Id, ServiceId = crossfit.Id }
                };
                context.TrainerServices.AddRange(trainerServices);

                // Add trainer availabilities (Monday to Friday, 9 AM to 6 PM)
                var availabilities = new List<TrainerAvailability>();
                foreach (var trainer in trainers)
                {
                    for (int day = 1; day <= 5; day++) // Monday to Friday
                    {
                        availabilities.Add(new TrainerAvailability
                        {
                            TrainerId = trainer.Id,
                            DayOfWeek = (DayOfWeek)day,
                            StartTime = new TimeSpan(9, 0, 0),
                            EndTime = new TimeSpan(18, 0, 0),
                            IsActive = true
                        });
                    }
                }
                context.TrainerAvailabilities.AddRange(availabilities);
                await context.SaveChangesAsync();
            }

            // Seed a sample member user
            var memberEmail = "member@example.com";
            var memberUser = await userManager.FindByEmailAsync(memberEmail);
            
            if (memberUser == null)
            {
                memberUser = new ApplicationUser
                {
                    UserName = memberEmail,
                    Email = memberEmail,
                    FullName = "Sample Member",
                    EmailConfirmed = true,
                    RegistrationDate = DateTime.Now
                };

                var result = await userManager.CreateAsync(memberUser, "member123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(memberUser, "Member");
                }
            }
        }
    }
}
