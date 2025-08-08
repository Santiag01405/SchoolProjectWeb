using Microsoft.AspNetCore.Mvc;
using SchoolProjectWeb.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace SchoolProjectWeb.Controllers
{
    public class CourseController : Controller
    {
        private readonly HttpClient _httpClient;

        public CourseController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://SchoolProject123.somee.com/");
        }

        private int? GetSchoolIdFromSession()
        {
            return HttpContext.Session.GetInt32("SchoolId");
        }

        private string? GetTokenFromSession()
        {
            return HttpContext.Session.GetString("UserToken");
        }

        private void SetAuthorizationHeader(string? token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        private bool IsSessionValid()
        {
            return !string.IsNullOrEmpty(GetTokenFromSession()) && GetSchoolIdFromSession().HasValue;
        }

        // 🔹 LISTA DE CURSOS
        [HttpGet]
        public async Task<IActionResult> ListCourses()
        {
            if (!IsSessionValid())
            {
                return RedirectToAction("Login", "Login");
            }

            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();

            var response = await _httpClient.GetAsync($"api/courses?schoolId={schoolId}");
            if (!response.IsSuccessStatusCode)
            {
                // Si la API no responde con éxito, devolvemos una lista vacía
                return View(new List<CourseViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var coursesFromApi = JsonSerializer.Deserialize<List<Course>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Course>();

            // Obtener la lista de profesores para mostrar sus nombres
            var teachers = await GetTeachersAsync();
            var teacherDictionary = teachers.ToDictionary(t => t.UserID, t => t.UserName);

            var coursesViewModel = coursesFromApi.Select(c => new CourseViewModel
            {
                CourseID = c.CourseID,
                Name = c.Name,
                Description = c.Description,
                DayOfWeek = c.DayOfWeek,
                UserID = c.UserID,
                // Asignar el nombre del profesor basado en el UserID
                TeacherName = teacherDictionary.GetValueOrDefault(c.UserID, "Desconocido")
            }).ToList();

            return View(coursesViewModel);
        }

        // 🔹 CREAR CURSO (GET)
        [HttpGet]
        public async Task<IActionResult> CreateCourse()
        {
            if (!IsSessionValid())
            {
                return RedirectToAction("Login", "Login");
            }

            var teachers = await GetTeachersAsync();
            var model = new CourseViewModel { Teachers = teachers };
            return View(model);
        }

        // 🔹 CREAR CURSO (POST)
        [HttpPost]
        public async Task<IActionResult> CreateCourse(CourseViewModel model)
        {
            if (!IsSessionValid())
            {
                return RedirectToAction("Login", "Login");
            }

            if (!ModelState.IsValid)
            {
                model.Teachers = await GetTeachersAsync();
                return View(model);
            }

            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();

            var payload = new
            {
                name = model.Name,
                description = model.Description,
                dayOfWeek = model.DayOfWeek,
                userID = model.UserID,
                schoolID = schoolId.Value
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/courses", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Curso creado correctamente.";
                return RedirectToAction("ListCourses");
            }

            TempData["Error"] = "Error al crear el curso.";
            model.Teachers = await GetTeachersAsync();
            return View(model);
        }

        // 🔹 EDITAR CURSO (GET)
        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            if (!IsSessionValid())
            {
                return RedirectToAction("Login", "Login");
            }

            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();

            var response = await _httpClient.GetAsync($"api/courses/{id}?schoolId={schoolId}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Curso no encontrado o no tiene permisos.";
                return RedirectToAction("ListCourses");
            }

            var json = await response.Content.ReadAsStringAsync();
            var course = JsonSerializer.Deserialize<Course>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            var teachers = await GetTeachersAsync();
            var model = new CourseViewModel
            {
                CourseID = course.CourseID,
                Name = course.Name,
                Description = course.Description,
                DayOfWeek = course.DayOfWeek,
                UserID = course.UserID,
                Teachers = teachers
            };

            return View(model);
        }

        // 🔹 EDITAR CURSO (POST)
        [HttpPost]
        public async Task<IActionResult> EditCourse(CourseViewModel model)
        {
            if (!IsSessionValid())
            {
                return RedirectToAction("Login", "Login");
            }
            if (!ModelState.IsValid)
            {
                model.Teachers = await GetTeachersAsync();
                return View(model);
            }

            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();

            var payload = new
            {
                courseID = model.CourseID,
                name = model.Name,
                description = model.Description,
                dayOfWeek = model.DayOfWeek,
                userID = model.UserID,
                schoolID = schoolId.Value
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"api/courses/{model.CourseID}?schoolId={schoolId}", content);
            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Curso editado correctamente.";
                return RedirectToAction("ListCourses");
            }

            TempData["Error"] = "Error al editar el curso.";
            model.Teachers = await GetTeachersAsync();
            return View(model);
        }

        // 🔹 ELIMINAR CURSO
        [HttpPost]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            if (!IsSessionValid())
            {
                return RedirectToAction("Login", "Login");
            }

            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();

            var response = await _httpClient.DeleteAsync($"api/courses/{id}?schoolId={schoolId}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Curso eliminado correctamente.";
            }
            else
            {
                TempData["Error"] = "Error al eliminar el curso.";
            }

            return RedirectToAction("ListCourses");
        }

        private async Task<List<User>> GetTeachersAsync()
        {
            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();
            var response = await _httpClient.GetAsync($"api/users?schoolId={schoolId}");

            if (response.IsSuccessStatusCode)
            {
                var users = await response.Content.ReadFromJsonAsync<List<User>>();
                // Filtramos por RoleID = 2 que representa a los profesores
                return users?.Where(u => u.RoleID == 2).ToList() ?? new List<User>();
            }

            return new List<User>();
        }
    }
}