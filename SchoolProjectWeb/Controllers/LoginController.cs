using Microsoft.AspNetCore.Mvc;
using SchoolProjectWeb.Models;
using System.Text;
using System.Text.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

public class LoginController : Controller
{
    private readonly HttpClient _httpClient;

    public LoginController(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient();
        _httpClient.BaseAddress = new Uri("https://SchoolProject123.somee.com/");
    }

    // Añade este método a tu LoginController.cs
    [HttpGet]
    public IActionResult Logout()
    {
        // Limpia todos los datos de la sesión
        HttpContext.Session.Clear();
        // Redirige al usuario a la vista de login
        return RedirectToAction("Login", "Login");
    }

    [HttpGet]
    public IActionResult Login()
    {
        HttpContext.Session.Clear();
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var payload = new
        {
            email = model.EmailOrUserName,
            password = model.Password
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // 🔹 Paso 1: Llamar al login para obtener el token
        var response = await _httpClient.PostAsync("api/auth/login", content);

        if (response.IsSuccessStatusCode)
        {
            var responseJson = await response.Content.ReadAsStringAsync();
            var authResponse = JsonSerializer.Deserialize<AuthResponse>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (authResponse == null || string.IsNullOrEmpty(authResponse.Token))
            {
                ModelState.AddModelError("", "Respuesta de login inválida.");
                return View(model);
            }

            // Decodificar el token para obtener el UserID
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(authResponse.Token);
            var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "UserID")?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                ModelState.AddModelError("", "No se pudo obtener el UserID del token.");
                return View(model);
            }

            // 🔹 Paso 2: Llamar a la API de usuario para obtener los detalles, incluido el schoolid
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authResponse.Token);
            var userResponse = await _httpClient.GetAsync($"api/users/{userId}");

            if (userResponse.IsSuccessStatusCode)
            {
                var userJson = await userResponse.Content.ReadAsStringAsync();
                var userDetails = JsonSerializer.Deserialize<User>(userJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (userDetails != null && userDetails.SchoolID.HasValue && userDetails.RoleID == 2)
                {
                    // Guardar los datos en la sesión
                    HttpContext.Session.SetInt32("SchoolId", userDetails.SchoolID.Value);
                    HttpContext.Session.SetInt32("UserID", userDetails.UserID);
                    HttpContext.Session.SetString("UserName", userDetails.UserName);
                    HttpContext.Session.SetString("email", userDetails.Email);
                    HttpContext.Session.SetString("UserToken", authResponse.Token);

                    return RedirectToAction("Index", "Admin");
                }
                else
                {
                    ModelState.AddModelError("", "El usuario no tiene permisos de administrador o el ID del colegio es inválido.");
                    return View(model);
                }
            }
            else
            {
                ModelState.AddModelError("", "Error al obtener los detalles del usuario.");
                return View(model);
            }
        }
        else
        {
            ModelState.AddModelError("", "Credenciales inválidas.");
            return View(model);
        }


    }
}