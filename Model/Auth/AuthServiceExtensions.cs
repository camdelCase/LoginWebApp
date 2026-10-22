using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.Runtime;
using LoginWebApp.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LoginWebApp.Model.Auth
{
    public static class AuthServiceExtensions
    {
        /// <summary>
        /// Registers the IAuthService and JWT bearer validation for the provider named in Auth:Provider.
        /// Local: users in SQL Server, tokens issued by this API and signed with Auth:Local:SigningKey.
        /// Cognito: users in a Cognito user pool, tokens issued by Cognito.
        /// </summary>
        public static IServiceCollection AddAppAuthentication(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            var section = configuration.GetSection(AuthOptions.SectionName);
            var options = section.Get<AuthOptions>() ?? new AuthOptions();
            services.Configure<AuthOptions>(section);

            var authentication = services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);

            if (options.Provider == AuthProvider.Local)
            {
                if (environment.IsProduction())
                {
                    throw new InvalidOperationException("Auth:Provider 'Local' is for development and testing only. Use 'Cognito' in production.");
                }

                // HS256 requires a key of at least 256 bits
                if (options.Local.SigningKey.Length < 32)
                {
                    throw new InvalidOperationException("Auth:Local:SigningKey must be at least 32 characters. Set it with: dotnet user-secrets set \"Auth:Local:SigningKey\" \"<value>\"");
                }

                services.AddDbContext<AppDbContext>(db =>
                    db.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
                services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
                services.AddScoped<IUserService, UserService>();
                services.AddSingleton<ITokenService, TokenService>();
                services.AddScoped<IAuthService, LocalAuthService>();

                authentication.AddJwtBearer(jwt =>
                {
                    jwt.MapInboundClaims = false;
                    jwt.TokenValidationParameters = new()
                    {
                        ValidIssuer = options.Local.Issuer,
                        ValidAudience = options.Local.Audience,
                        IssuerSigningKey = TokenService.CreateSigningKey(options.Local.SigningKey),
                        NameClaimType = AuthClaimTypes.Username,
                        RoleClaimType = AuthClaimTypes.Groups
                    };
                });
            }
            else
            {
                var cognito = options.Cognito;
                if (string.IsNullOrWhiteSpace(cognito.Region) || string.IsNullOrWhiteSpace(cognito.UserPoolId) || string.IsNullOrWhiteSpace(cognito.ClientId))
                {
                    throw new InvalidOperationException("Auth:Cognito:Region, UserPoolId and ClientId must all be set when Auth:Provider is 'Cognito'.");
                }

                // InitiateAuth, SignUp and ConfirmSignUp are public Cognito APIs, so no AWS credentials are needed
                services.AddSingleton<IAmazonCognitoIdentityProvider>(_ =>
                    new AmazonCognitoIdentityProviderClient(new AnonymousAWSCredentials(), RegionEndpoint.GetBySystemName(cognito.Region)));
                services.AddScoped<IAuthService, CognitoAuthService>();

                authentication.AddJwtBearer(jwt =>
                {
                    // Issuer and signing keys come from the pool's OpenID metadata
                    jwt.Authority = cognito.Authority;
                    jwt.MapInboundClaims = false;
                    jwt.TokenValidationParameters = new()
                    {
                        // Cognito access tokens have no "aud" claim; client_id is checked below instead
                        ValidateAudience = false,
                        NameClaimType = AuthClaimTypes.Username,
                        RoleClaimType = AuthClaimTypes.Groups
                    };
                    jwt.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            var principal = context.Principal!;
                            if (principal.FindFirst(AuthClaimTypes.TokenUse)?.Value != "access")
                            {
                                context.Fail("Only Cognito access tokens are accepted.");
                            }
                            else if (principal.FindFirst(AuthClaimTypes.ClientId)?.Value != cognito.ClientId)
                            {
                                context.Fail("Token was issued to a different app client.");
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
            }

            return services;
        }
    }
}
