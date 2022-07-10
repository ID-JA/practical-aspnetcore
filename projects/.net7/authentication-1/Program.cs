using System.Security.Claims;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
var builder = WebApplication.CreateBuilder(args);

builder.Authentication.AddJwtBearer(options => {
    options.IncludeErrorDetails = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "practical aspnetcore",
        ValidAudience = "https://localhost:5001/",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this is custom key for practical aspnetcore sample"))
    };});

var app = builder.Build();

app.MapGet("/", (HttpRequest request) => Results.Text("""
<html>
<body>
Hello, World!

JWT:<br/>
<div id="jwt_content"></div>
<br/><br/>
Response from <a href="/secret">/secret</a>
<div id="message"></div>
<br/><br/>

<button id="jwt">Get Secret</button>


<script>
 let btn = document.getElementById('jwt');
 btn.addEventListener('click', async function() {
    const url = window.location.protocol + '//' + window.location.host  + "/jwt";
    const response = await fetch(url,  {
        headers: { 'Accept': 'application/json' }
    });

    const json = await response.json();
    document.getElementById('jwt_content').textContent = json;

    const url2 = window.location.protocol + '//' + window.location.host  + "/secret";
    const response2 = await fetch(url2,  {
        headers: { 'Accept': 'text/plain', 'Authentication': 'Bearer ' + json, }
    });

    const text =  await response.text();
    alert(text);
 });
</script>

</body>
</html>
""", "text/html"));


app.MapGet("/secret", (ClaimsPrincipal user) => $"Hello {user.Identity?.Name}. This is a secret!")
    .RequireAuthorization( m => m.RequireAuthenticatedUser);

app.MapGet("/jwt", () => Results.Json(GenerateJSONWebToken()));

string GenerateJSONWebToken()    
{    
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this is custom key for practical aspnetcore sample"));    
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);    

    var token = new JwtSecurityToken(issuer:"practical aspnetcore",    
        audience:"https://localhost:5001/",    
        claims: null,
        notBefore: null,    
        expires: DateTime.Now.AddMinutes(120),    
        signingCredentials: credentials);    

    return new JwtSecurityTokenHandler().WriteToken(token);    
}    

app.Run();