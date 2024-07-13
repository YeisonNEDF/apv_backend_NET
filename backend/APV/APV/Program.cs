using APV;

var builder = WebApplication.CreateBuilder(args);

//Confi del Startup
var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);

var app = builder.Build();

//Confi del Startup
startup.Configure(app, app.Environment);

app.Run();
