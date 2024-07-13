using APV.Entities.MailTrap;
using APV.Entities.MailTrap.InterfazCorreo;
using APV.Helper;
using AutoMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;


namespace APV
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }


        public void ConfigureServices(IServiceCollection services)
        {
            
            services.AddAutoMapper(typeof(Startup));

            //Configuramos la conexion a la BD
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            //Configuramos el JWT
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opciones => opciones.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(Configuration["JWT"])),
                    ClockSkew = TimeSpan.Zero
                });

            services.AddControllers();
            services.AddEndpointsApiExplorer();

            //Configuracion mailtrap
            services.AddControllersWithViews();
            services.Configure<EnvioCorreo>(Configuration.GetSection("Smtp"));
            services.AddTransient<IEnvioCorreo, EnviarEmail>();


            //Configuracion de CORS
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins("http://localhost:3000").AllowAnyMethod().AllowAnyHeader();
                });
            });

            // Registrar HashPassword
            services.AddScoped<HashPassword>();
            //Generar Token
            services.AddScoped<GenerarToken>();

            //Configuramos el servicio de Identity
            //services.AddIdentity<IdentityUser, IdentityRole>()
            //.AddEntityFrameworkStores<ApplicationDbContext>()
            //.AddDefaultTokenProviders();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) 
        {
            if(env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

    }
}
