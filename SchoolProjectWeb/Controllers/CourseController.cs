using Microsoft.AspNetCore.Mvc;
using SchoolProjectWeb.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;

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

        // 🔹 LISTA
        [HttpGet]
        public async Task<IActionResult> ListCourses()
        {
            var response = await _httpClient.GetAsync("api/courses");
            if (!response.IsSuccessStatusCode)
                return View(new List<Course>());

            var json = await response.Content.ReadAsStringAsync();
            var courses = JsonSerializer.Deserialize<List<Course>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<Course>();

            return View(courses);
        }

        // 🔹 CREAR (GET)
        [HttpGet]
        public IActionResult CreateCourse()
        {
            return View();
        }

        // 🔹 CREAR (POST)
        [HttpPost]
        public async Task<IActionResult> CreateCourse(Course model)
        {
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/courses/create", content);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("ListCourses");

            TempData["Error"] = "Error al crear el curso.";
            return View(model);
        }

        // 🔹 EDITAR (GET)
        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            var response = await _httpClient.GetAsync($"api/courses/{id}");
            if (!response.IsSuccessStatusCode)
                return RedirectToAction("ListCourses");

            var json = await response.Content.ReadAsStringAsync();
            var course = JsonSerializer.Deserialize<Course>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return View(course);
        }

        // 🔹 EDITAR (POST)
        [HttpPost]
        public async Task<IActionResult> EditCourse(Course model)
        {
            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"api/courses/{model.CourseID}", content);
            if (response.IsSuccessStatusCode)
                return RedirectToAction("ListCourses");

            TempData["Error"] = "Error al editar el curso.";
            return View(model);
        }

        // 🔹 ELIMINAR
        [HttpPost]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/courses/{id}");

            if (response.IsSuccessStatusCode)
                TempData["Success"] = "Curso eliminado correctamente.";
            else
                TempData["Error"] = "Error al eliminar el curso.";

            return RedirectToAction("ListCourses");
        }
    }
}
