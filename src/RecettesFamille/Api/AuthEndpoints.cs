using Microsoft.AspNetCore.Identity;
using RecettesFamille.Data;

namespace RecettesFamille.Api
{
    /// <summary>
    /// Extension methods pour enregistrer les endpoints d'authentification
    /// </summary>
    public static class AuthEndpoints
    {
        /// <summary>
        /// Enregistre tous les endpoints d'authentification
        /// </summary>
        public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
        {
            // Endpoint pour le login
            app.MapPost("/auth/login", HandleLogin)
                .DisableAntiforgery()
                .WithName("AuthLogin");

            // Endpoint pour le register
            app.MapPost("/auth/register", HandleRegister)
                .DisableAntiforgery()
                .WithName("AuthRegister");

            // Endpoint pour le logout
            app.MapPost("/auth/logout", HandleLogout)
                .DisableAntiforgery()
                .WithName("AuthLogout");

            // Endpoint pour la mise à jour du profil
            app.MapPost("/auth/update-profile", HandleUpdateProfile)
                .DisableAntiforgery()
                .RequireAuthorization()
                .WithName("AuthUpdateProfile");

            return app;
        }

        /// <summary>
        /// Gère la connexion de l'utilisateur
        /// </summary>
        private static async Task<IResult> HandleLogin(
            HttpContext context,
            SignInManager<ApplicationUser> signInManager,
            ILogger<Program> logger)
        {
            try
            {
                var form = await context.Request.ReadFormAsync();
                var email = form["email"].ToString();
                var password = form["password"].ToString();
                var rememberMeStr = form["rememberMe"].ToString();
                var returnUrl = form["returnUrl"].ToString();

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    logger.LogWarning("Login attempt with empty credentials");
                    return Results.Redirect($"/login?error=empty");
                }

                bool rememberMe = !string.IsNullOrEmpty(rememberMeStr) && 
                                  (rememberMeStr.Equals("true", StringComparison.OrdinalIgnoreCase) || 
                                   rememberMeStr.Equals("on", StringComparison.OrdinalIgnoreCase));

                var result = await signInManager.PasswordSignInAsync(email, password, isPersistent: rememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    logger.LogInformation("User {Email} logged in successfully", email);
                    var redirectTo = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;
                    return Results.Redirect(redirectTo);
                }
                else if (result.RequiresTwoFactor)
                {
                    logger.LogWarning("User {Email} requires two-factor authentication", email);
                    return Results.Redirect("/login?error=2fa");
                }
                else if (result.IsLockedOut)
                {
                    logger.LogWarning("User {Email} account is locked out", email);
                    return Results.Redirect("/login?error=locked");
                }
                else
                {
                    logger.LogWarning("Invalid login attempt for {Email}", email);
                    return Results.Redirect("/login?error=invalid");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception during login");
                return Results.Redirect("/login?error=server");
            }
        }

        /// <summary>
        /// Gère l'inscription d'un nouvel utilisateur
        /// </summary>
        private static async Task<IResult> HandleRegister(
            HttpContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<Program> logger)
        {
            try
            {
                var form = await context.Request.ReadFormAsync();
                var email = form["email"].ToString();
                var password = form["password"].ToString();
                var confirmPassword = form["confirmPassword"].ToString();
                var returnUrl = form["returnUrl"].ToString();

                if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
                {
                    logger.LogWarning("Register attempt with empty credentials");
                    return Results.Redirect($"/register?error=empty");
                }

                if (password != confirmPassword)
                {
                    logger.LogWarning("Register attempt with mismatched passwords for {Email}", email);
                    return Results.Redirect($"/register?error=mismatch");
                }

                var user = new ApplicationUser 
                { 
                    UserName = email, 
                    Email = email 
                };

                var result = await userManager.CreateAsync(user, password);

                if (result.Succeeded)
                {
                    logger.LogInformation("User {Email} created successfully", email);
                    await userManager.AddToRoleAsync(user, "Reader");
                    await signInManager.SignInAsync(user, isPersistent: false);
                    logger.LogInformation("User {Email} signed in after registration", email);

                    var redirectTo = string.IsNullOrWhiteSpace(returnUrl) ? "/" : returnUrl;
                    return Results.Redirect(redirectTo);
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    logger.LogWarning("Failed to create user {Email}: {Errors}", email, errors);
                    var errorParam = Uri.EscapeDataString(errors);
                    return Results.Redirect($"/register?error=creation&details={errorParam}");
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception during registration");
                return Results.Redirect("/register?error=server");
            }
        }

        /// <summary>
        /// Gère la déconnexion de l'utilisateur
        /// </summary>
        private static async Task<IResult> HandleLogout(
            HttpContext context,
            SignInManager<ApplicationUser> signInManager,
            ILogger<Program> logger)
        {
            try
            {
                var userName = context.User?.Identity?.Name ?? "Unknown";
                await signInManager.SignOutAsync();
                logger.LogInformation("User {UserName} logged out", userName);
                return Results.Redirect("/login");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception during logout");
                return Results.Redirect("/login");
            }
        }

        /// <summary>
        /// Gère la mise à jour du profil utilisateur
        /// </summary>
        private static async Task<IResult> HandleUpdateProfile(
            HttpContext context,
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ILogger<Program> logger)
        {
            try
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null)
                {
                    logger.LogWarning("Update profile attempted but user not found");
                    return Results.Redirect("/profile?error=notfound");
                }

                var form = await context.Request.ReadFormAsync();
                var phoneNumber = form["phoneNumber"].ToString();

                var currentPhone = await userManager.GetPhoneNumberAsync(user);
                if (phoneNumber != currentPhone)
                {
                    var setPhoneResult = await userManager.SetPhoneNumberAsync(user, phoneNumber);
                    if (!setPhoneResult.Succeeded)
                    {
                        logger.LogWarning("Failed to set phone number for user {UserId}", user.Id);
                        return Results.Redirect("/profile?error=update");
                    }
                }

                await signInManager.RefreshSignInAsync(user);
                logger.LogInformation("User {UserId} updated their profile", user.Id);
                return Results.Redirect("/profile?success=true");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception during profile update");
                return Results.Redirect("/profile?error=server");
            }
        }
    }
}
