using Microsoft.AspNetCore.Mvc;
using SchoolProjectWeb.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Net.Http.Headers;

namespace SchoolProjectWeb.Controllers
{
    public class AdminController : Controller
    {
        private readonly HttpClient _httpClient;

        public AdminController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://SchoolProject123.somee.com/");
        }

        private int? GetSchoolIdFromSession() => HttpContext.Session.GetInt32("SchoolId");
        private string? GetTokenFromSession() => HttpContext.Session.GetString("UserToken");
        private int? GetUserIdFromSession() => HttpContext.Session.GetInt32("UserID");
        private void SetAuthorizationHeader(string? token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }
        private bool IsSessionValid() => !string.IsNullOrEmpty(GetTokenFromSession()) && GetSchoolIdFromSession().HasValue;
        // Agrega este método al final de tu AdminController
        // Asegúrate de que el nombre de la acción sea "Index" para que cargue la vista por defecto.

        // GET: Muestra la vista con el formulario para crear un usuario
        [HttpGet]
        public IActionResult CreateUser()
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");
            return View(new UserRegisterViewModel());
        }

        // POST: Procesa el formulario de creación de usuario
        [HttpPost]
        public async Task<IActionResult> CreateUser(UserRegisterViewModel model)
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();
            var payload = new
            {
                userName = model.UserName,
                email = model.Email,
                passwordHash = model.Password,
                roleID = model.RoleID,
                schoolID = schoolId.Value
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/auth/register", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Usuario registrado con éxito.";
                return RedirectToAction(nameof(ListUsers));
            }

            // ✅ Lógica corregida para manejar el error de la API
            var errorJson = await response.Content.ReadAsStringAsync();
            var errorMessage = "Error al registrar el usuario.";

            try
            {
                using (JsonDocument doc = JsonDocument.Parse(errorJson))
                {
                    if (doc.RootElement.TryGetProperty("message", out JsonElement messageElement))
                    {
                        errorMessage = messageElement.GetString() ?? errorMessage;
                    }
                }
            }
            catch (JsonException)
            {
                // Si el JSON no es válido, se usa el mensaje de error por defecto
            }

            TempData["Error"] = errorMessage;
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ListUsers()
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();

            var users = new List<User>();
            var response = await _httpClient.GetAsync($"api/users?schoolId={schoolId}");

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                users = JsonSerializer.Deserialize<List<User>>(responseContent, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<User>();
            }

            // ✅ Aquí se pasa la lista de usuarios a la vista.
            // Si la llamada falla, se pasa una lista vacía.
            return View(users);
        }

        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();
            var response = await _httpClient.GetAsync($"api/users/{id}?schoolId={schoolId}");

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var user = JsonSerializer.Deserialize<User>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (user != null)
                {
                    var viewModel = new EditUserViewModel
                    {
                        UserID = user.UserID,
                        UserName = user.UserName,
                        Email = user.Email,
                        RoleID = user.RoleID
                    };
                    return View(viewModel);
                }
            }

            TempData["Error"] = "Usuario no encontrado o no tiene permisos.";
            return RedirectToAction(nameof(ListUsers));
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");
            if (!ModelState.IsValid) return View(model);

            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();
            var payload = new
            {
                userID = model.UserID,
                userName = model.UserName,
                email = model.Email,
                roleID = model.RoleID,
                schoolID = schoolId.Value
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"api/users/{model.UserID}?schoolId={schoolId}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Usuario actualizado con éxito.";
                return RedirectToAction(nameof(ListUsers));
            }

            TempData["Error"] = "Error al actualizar el usuario.";
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();
            var response = await _httpClient.DeleteAsync($"api/users/{id}?schoolId={schoolId}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Usuario eliminado con éxito.";
            }
            else
            {
                TempData["Error"] = "Error al eliminar el usuario.";
            }

            return RedirectToAction(nameof(ListUsers));
        }

        // ----------------------------------------------------------------------
        // VISTAS DE CURSOS
        // ----------------------------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> ListCourses(string search = "")
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();
            var response = await _httpClient.GetAsync($"api/courses?schoolId={schoolId}&search={search}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Error al obtener cursos.";
                return View(new List<CourseViewModel>());
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            var courses = JsonSerializer.Deserialize<List<CourseViewModel>>(responseJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return View(courses);
        }

        [HttpGet]
        public async Task<IActionResult> CreateCourse()
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

            var teachers = await GetTeachersAsync();
            var model = new CourseViewModel { Teachers = teachers };
            return View(model);
        }

        // 🔹 POST: Procesa el formulario para crear un curso
        [HttpPost]
        public async Task<IActionResult> CreateCourse(CourseViewModel model)
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

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

            // 💡 Llamada al endpoint para crear un curso
            var response = await _httpClient.PostAsync("api/courses/create", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Curso creado correctamente.";
                return RedirectToAction("ListCourses");
            }

            TempData["Error"] = "Error al crear el curso.";
            model.Teachers = await GetTeachersAsync();
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();
            var response = await _httpClient.GetAsync($"api/courses/{id}?schoolId={schoolId}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Error al cargar el curso.";
                return RedirectToAction("ListCourses");
            }

            var json = await response.Content.ReadAsStringAsync();
            var course = JsonSerializer.Deserialize<Course>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (course == null)
            {
                TempData["Error"] = "Curso no encontrado.";
                return RedirectToAction("ListCourses");
            }

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




        [HttpPost]
        public async Task<IActionResult> EditCourse(CourseViewModel model)
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");
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
                TempData["Success"] = "Curso actualizado correctamente.";
                return RedirectToAction("ListCourses");
            }

            TempData["Error"] = "Error al actualizar el curso.";
            model.Teachers = await GetTeachersAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

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

        //Ver inscripciones de un curso
        public async Task<IActionResult> ViewStudentsInCourse(int courseId)
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

            try
            {
                SetAuthorizationHeader(GetTokenFromSession());
                var schoolId = GetSchoolIdFromSession();
                var responseStudents = await _httpClient.GetAsync($"api/enrollments/course/{courseId}/students?schoolId={schoolId}");

                if (!responseStudents.IsSuccessStatusCode)
                {
                    TempData["Error"] = $"No hay estudiantes inscritos o error al obtener estudiantes del curso con ID {courseId}.";
                    return RedirectToAction("ListCourses");
                }

                var students = await responseStudents.Content.ReadFromJsonAsync<List<StudentViewModel>>();
                var responseCourse = await _httpClient.GetAsync($"api/courses/{courseId}?schoolId={schoolId}");

                if (!responseCourse.IsSuccessStatusCode)
                {
                    TempData["Error"] = $"Error al obtener información del curso con ID {courseId}.";
                    return RedirectToAction("ListCourses");
                }

                var course = await responseCourse.Content.ReadFromJsonAsync<CourseViewModel>();

                ViewBag.CourseID = courseId;
                ViewBag.CourseName = course?.Name ?? "Curso Desconocido";

                return View(students);
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar estudiantes: {ex.Message}";
                return RedirectToAction("ListCourses");
            }
        }
        private async Task<List<User>> GetTeachersAsync()
        {
            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();
            var response = await _httpClient.GetAsync($"api/users?schoolId={schoolId}");

            if (response.IsSuccessStatusCode)
            {
                var users = await response.Content.ReadFromJsonAsync<List<User>>();
                return users?.Where(u => u.RoleID == 2).ToList() ?? new List<User>();
            }

            return new List<User>();
        }
        //----------------------------------------------------------------------------------------------------------------------------------

        //Crear relacion entre padre e hijo

        [HttpGet]
        public async Task<IActionResult> CreateUserRelationship()
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();
            var response = await _httpClient.GetAsync($"api/users?schoolId={schoolId}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se pudieron cargar los usuarios.";
                return View(new UserRelationshipViewModel());
            }

            var json = await response.Content.ReadAsStringAsync();
            var allUsers = JsonSerializer.Deserialize<List<User>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<User>();

            var model = new UserRelationshipViewModel
            {
                Parents = allUsers.Where(u => u.RoleID == 3).ToList(),
                Children = allUsers.Where(u => u.RoleID == 1).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserRelationship(UserRelationshipViewModel model)
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

            // ✅ Verificamos si los IDs son válidos antes de enviar la solicitud.
            if (model.User1ID <= 0 || model.User2ID <= 0)
            {
                TempData["Error"] = "Debe seleccionar un padre y un hijo.";
                return RedirectToAction("CreateUserRelationship");
            }

            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();

            // ✅ Se corrige el payload con los nombres de propiedad correctos y el tipo de relación.
            var payload = new
            {
                user1ID = model.User1ID,
                user2ID = model.User2ID,
                relationshipType = "Padre-Hijo" // La API espera este valor
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // ✅ Se corrige el endpoint de la API.
            var response = await _httpClient.PostAsync("api/relationships/create", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Relación creada exitosamente.";
            }
            else
            {
                // Manejo de errores más detallado en caso de fallo
                var errorJson = await response.Content.ReadAsStringAsync();
                var errorMessage = "Error al crear la relación.";
                try
                {
                    using (JsonDocument doc = JsonDocument.Parse(errorJson))
                    {
                        if (doc.RootElement.TryGetProperty("message", out JsonElement messageElement))
                        {
                            errorMessage = messageElement.GetString() ?? errorMessage;
                        }
                    }
                }
                catch (JsonException)
                {
                    // Si el JSON no es válido, se usa el mensaje de error por defecto
                }
                TempData["Error"] = errorMessage;
            }

            return RedirectToAction("CreateUserRelationship");
        }

        [HttpGet]
        public async Task<IActionResult> ViewChildren()
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();

            var response = await _httpClient.GetAsync($"api/users?schoolId={schoolId}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Error al obtener la lista de padres.";
                return View(new List<User>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var allUsers = JsonSerializer.Deserialize<List<User>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<User>();
            var parents = allUsers.Where(u => u.RoleID == 3).ToList();

            return View(parents);
        }

        [HttpGet]
        public async Task<IActionResult> ViewChildrenOfParent(int userId)
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();

            var response = await _httpClient.GetAsync($"api/relationships/user/{userId}/children?schoolId={schoolId}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Error al obtener los hijos del padre.";
                return RedirectToAction("ListUsers");
            }

            var json = await response.Content.ReadAsStringAsync();
            // ✅ Se usa el nuevo modelo StudentViewModel para deserializar la respuesta
            var children = JsonSerializer.Deserialize<List<StudentViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<StudentViewModel>();

            var parent = await GetUserByIdAsync(userId);
            ViewBag.ParentName = parent?.UserName ?? "Padre Desconocido";

            return View(children);
        }
        private async Task<User?> GetUserByIdAsync(int userId)
        {
            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();
            var response = await _httpClient.GetAsync($"api/users/{userId}?schoolId={schoolId}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<User>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            return null;
        }
        //---------------------------------------------------------------------------------------------------------------------------------


        [HttpGet]
        public async Task<IActionResult> ListStudents(string search = "")
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();
            var response = await _httpClient.GetAsync($"api/users?schoolId={schoolId}&search={search}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Error al obtener usuarios.";
                return View(new List<User>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var allUsers = JsonSerializer.Deserialize<List<User>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<User>();
            var students = allUsers.Where(u => u.RoleID == 1).ToList();

            if (!string.IsNullOrWhiteSpace(search))
            {
                students = students.Where(u => u.UserName.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return View(students);
        }

        public async Task<IActionResult> ViewEnrollments(int userId)
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

            List<EnrollmentViewModel> enrollments = new();
            try
            {
                SetAuthorizationHeader(GetTokenFromSession());
                var schoolId = GetSchoolIdFromSession();
                var response = await _httpClient.GetAsync($"api/enrollments/user/{userId}?schoolId={schoolId}");

                if (response.IsSuccessStatusCode)
                {
                    enrollments = await response.Content.ReadFromJsonAsync<List<EnrollmentViewModel>>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    TempData["InfoMessage"] = "Este usuario no tiene inscripciones.";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Error al obtener inscripciones: {error}";
                }

                var userResponse = await _httpClient.GetAsync($"api/users/{userId}?schoolId={schoolId}");
                if (userResponse.IsSuccessStatusCode)
                {
                    var user = await userResponse.Content.ReadFromJsonAsync<User>();
                    ViewBag.UserName = user?.UserName ?? "Sin nombre";
                }
                else
                {
                    ViewBag.UserName = "Nombre no disponible";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Error al comunicarse con la API: {ex.Message}";
            }

            ViewBag.UserId = userId;
            return View(enrollments);
        }

        public async Task<IActionResult> AssignCourseToUser(int id)
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

            ViewBag.UserID = id;
            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();

            var courses = await _httpClient.GetFromJsonAsync<List<CourseViewModel>>($"api/courses?schoolId={schoolId}");
            ViewBag.Courses = courses;

            var userResponse = await _httpClient.GetAsync($"api/users/{id}?schoolId={schoolId}");
            if (userResponse.IsSuccessStatusCode)
            {
                var user = await userResponse.Content.ReadFromJsonAsync<User>();
                ViewBag.UserName = user?.UserName ?? "Usuario desconocido";
            }
            else
            {
                ViewBag.UserName = "Usuario desconocido";
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> AssignCourseToUser(int userId, int courseId)
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();
            var enrollment = new
            {
                UserID = userId,
                CourseID = courseId,
                SchoolID = schoolId.Value
            };

            var response = await _httpClient.PostAsJsonAsync("api/enrollments", enrollment);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Usuario inscrito correctamente.";
                return RedirectToAction("ViewEnrollments", new { userId = userId });
            }

            var error = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = $"Error: {error}";
            return RedirectToAction("AssignCourseToUser", new { id = userId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteEnrollment(int enrollmentId, int userId)
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();
            var response = await _httpClient.DeleteAsync($"api/enrollments/{enrollmentId}?schoolId={schoolId}");

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Inscripción eliminada correctamente.";
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                TempData["ErrorMessage"] = $"Error al eliminar inscripción: {error}";
            }

            return RedirectToAction("ViewEnrollments", new { userId = userId });
        }

        //----Notificaciones--------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> SearchUsers(string search = "")
        {
            if (!IsSessionValid()) return Json(new List<User>());

            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();
            var response = await _httpClient.GetAsync($"api/users?schoolId={schoolId}");

            if (!response.IsSuccessStatusCode)
                return Json(new List<User>());

            var json = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<User>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<User>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                users = users.Where(u => u.UserName.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return Json(users);
        }

        public async Task<IActionResult> SendNotification()
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();
            var response = await _httpClient.GetAsync($"api/users?schoolId={schoolId}");
            var users = await response.Content.ReadFromJsonAsync<List<User>>();

            ViewBag.Usuarios = users;
            return View(new NotificationSendViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> SendNotification(NotificationSendViewModel model)
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();
            var notification = new
            {
                title = model.Title,
                content = model.Content,
                date = DateTime.UtcNow,
                isRead = false,
                userID = model.UserID ?? 0,
                schoolID = schoolId.Value
            };

            HttpResponseMessage response;

            if (model.Target == "all")
            {
                response = await _httpClient.PostAsJsonAsync($"api/notifications/send-to-all?schoolId={schoolId}", notification);
            }
            else if (model.Target == "role" && model.RoleID.HasValue)
            {
                response = await _httpClient.PostAsJsonAsync($"api/notifications/send-to-role?roleId={model.RoleID}&schoolId={schoolId}", notification);
            }
            else if (model.Target == "user" && model.UserID.HasValue)
            {
                response = await _httpClient.PostAsJsonAsync("api/notifications", notification);
            }
            else
            {
                ModelState.AddModelError("", "Destino inválido");
                return View(model);
            }

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Notificación enviada correctamente.";
                var users = await _httpClient.GetFromJsonAsync<List<User>>($"api/users?schoolId={schoolId}");
                ViewBag.Usuarios = users;
                return View(new NotificationSendViewModel());
            }

            ModelState.AddModelError("", "Error al enviar notificación.");
            return View(model);
        }
        //--------------------------------------------------------------------------------------------------------------------------------------------------------------------

        //-----Dashboard------------------------------------------------------------------------------------------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();
            var viewModel = new AdminDashboardViewModel();

            async Task<int> GetCount(string endpoint)
            {
                var response = await _httpClient.GetAsync($"api/{endpoint}?schoolId={schoolId}");
                if (!response.IsSuccessStatusCode) return 0;
                var json = await response.Content.ReadAsStringAsync();
                return int.TryParse(json, out int count) ? count : 0;
            }

            viewModel.TotalUsers = await GetCount("users/active-count");
            viewModel.Students = await GetCount("users/active-count-students");
            viewModel.Teachers = await GetCount("users/active-count-teachers");
            viewModel.Parents = await GetCount("users/active-count-parents");

            var courseResponse = await _httpClient.GetAsync($"api/courses?schoolId={schoolId}");
            if (courseResponse.IsSuccessStatusCode)
            {
                var json = await courseResponse.Content.ReadAsStringAsync();
                var courses = JsonSerializer.Deserialize<List<Course>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                viewModel.Courses = courses?.Count ?? 0;
            }

            // ✅ NUEVA LÓGICA: Obtener el nombre del colegio y asignarlo al ViewModel
            string schoolName = "Colegio Desconocido";
            if (schoolId.HasValue)
            {
                var schoolResponse = await _httpClient.GetAsync($"api/schools/{schoolId}");
                if (schoolResponse.IsSuccessStatusCode)
                {
                    var schoolJson = await schoolResponse.Content.ReadAsStringAsync();
                    var school = JsonSerializer.Deserialize<SchoolViewModel>(schoolJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    schoolName = school?.Name ?? "Colegio Desconocido";
                }
            }
            viewModel.SchoolName = schoolName;

            return View("Index", viewModel);
        }

        //*******SALONES******************************************************************

        [HttpGet]
        public async Task<IActionResult> ListClassrooms()
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();

            var response = await _httpClient.GetAsync($"api/classrooms?schoolId={schoolId}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Error al obtener la lista de salones.";
                return View(new List<ClassroomViewModel>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var classrooms = JsonSerializer.Deserialize<List<ClassroomViewModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ClassroomViewModel>();

            return View(classrooms);
        }

        // 🔹 GET: Muestra el formulario para crear un salón
        [HttpGet]
        public IActionResult CreateClassroom()
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");
            return View(new ClassroomViewModel());
        }

        // 🔹 POST: Procesa el formulario para crear un salón
        [HttpPost]
        public async Task<IActionResult> CreateClassroom(ClassroomViewModel model)
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();
            var payload = new
            {
                name = model.Name,
                description = model.Description,
                schoolID = schoolId.Value
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/classrooms", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Salón creado correctamente.";
                return RedirectToAction("ListClassrooms");
            }

            TempData["Error"] = "Error al crear el salón.";
            return View(model);
        }

        // 🔹 GET: Muestra el formulario para editar un salón
        [HttpGet]
        public async Task<IActionResult> EditClassroom(int id)
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();

            var response = await _httpClient.GetAsync($"api/classrooms/{id}?schoolId={schoolId}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Error al cargar los datos del salón.";
                return RedirectToAction("ListClassrooms");
            }

            var json = await response.Content.ReadAsStringAsync();
            var classroom = JsonSerializer.Deserialize<ClassroomViewModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (classroom == null)
            {
                TempData["Error"] = "Salón no encontrado.";
                return RedirectToAction("ListClassrooms");
            }

            return View(classroom);
        }

        // 🔹 POST: Procesa el formulario para actualizar un salón
        [HttpPost]
        public async Task<IActionResult> EditClassroom(ClassroomViewModel model)
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();
            var payload = new
            {
                classroomID = model.ClassroomID,
                name = model.Name,
                description = model.Description,
                schoolID = schoolId.Value
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"api/classrooms/{model.ClassroomID}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Salón actualizado correctamente.";
                return RedirectToAction("ListClassrooms");
            }

            TempData["Error"] = "Error al actualizar el salón.";
            return View(model);
        }

        // 🔹 POST: Elimina un salón
        [HttpPost]
        public async Task<IActionResult> DeleteClassroom(int id)
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

            SetAuthorizationHeader(GetTokenFromSession());
            var response = await _httpClient.DeleteAsync($"api/classrooms/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Salón eliminado correctamente.";
            }
            else
            {
                TempData["Error"] = "Error al eliminar el salón.";
            }

            return RedirectToAction("ListClassrooms");
        }
        // 🔹 GET: Muestra el formulario para asignar un estudiante a un salón
        [HttpGet]
        public async Task<IActionResult> AssignStudentToClassroom()
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

            SetAuthorizationHeader(GetTokenFromSession());
            var schoolId = GetSchoolIdFromSession();

            // Obtiene la lista de estudiantes
            var usersResponse = await _httpClient.GetAsync($"api/users?schoolId={schoolId}");
            if (!usersResponse.IsSuccessStatusCode)
            {
                TempData["Error"] = "Error al obtener la lista de estudiantes.";
                return RedirectToAction("ListUsers");
            }

            var usersJson = await usersResponse.Content.ReadAsStringAsync();
            var allUsers = JsonSerializer.Deserialize<List<User>>(usersJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<User>();
            var students = allUsers.Where(u => u.RoleID == 1).ToList();

            // Obtiene la lista de salones
            var classroomsResponse = await _httpClient.GetAsync($"api/classrooms?schoolId={schoolId}");
            if (!classroomsResponse.IsSuccessStatusCode)
            {
                TempData["Error"] = "Error al obtener la lista de salones.";
                return RedirectToAction("ListClassrooms");
            }

            var classroomsJson = await classroomsResponse.Content.ReadAsStringAsync();
            var classrooms = JsonSerializer.Deserialize<List<ClassroomViewModel>>(classroomsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<ClassroomViewModel>();

            var model = new AssignStudentToClassroomViewModel
            {
                Students = students,
                Classrooms = classrooms
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AssignStudentToClassroom(AssignStudentToClassroomViewModel model)
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

            if (model.SelectedUserId <= 0 || model.SelectedClassroomId <= 0)
            {
                TempData["Error"] = "Debe seleccionar un estudiante y un salón.";
                return RedirectToAction("AssignStudentToClassroom");
            }

            SetAuthorizationHeader(GetTokenFromSession());
            var response = await _httpClient.PutAsync($"api/classrooms/assign/{model.SelectedUserId}?classroomId={model.SelectedClassroomId}", null);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Estudiante asignado al salón correctamente.";
            }
            else
            {
                var errorJson = await response.Content.ReadAsStringAsync();
                var errorMessage = "Error al asignar el estudiante.";
                try
                {
                    using (JsonDocument doc = JsonDocument.Parse(errorJson))
                    {
                        if (doc.RootElement.TryGetProperty("message", out JsonElement messageElement))
                        {
                            errorMessage = messageElement.GetString() ?? errorMessage;
                        }
                    }
                }
                catch (JsonException)
                {
                }
                TempData["Error"] = errorMessage;
            }

            return RedirectToAction("AssignStudentToClassroom");
        }

        // 🔹 GET: Muestra los estudiantes en un salón
        [HttpGet]
        public async Task<IActionResult> ViewStudentsInClassroom(int classroomId)
        {
            if (!IsSessionValid()) return RedirectToAction("Login", "Login");

            SetAuthorizationHeader(GetTokenFromSession());

            // Llamada para obtener la lista de estudiantes
            var response = await _httpClient.GetAsync($"api/classrooms/{classroomId}/students");

            List<User> students = new List<User>();
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                students = JsonSerializer.Deserialize<List<User>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<User>();
            }
            // No se redirige si no hay estudiantes. La lista estará vacía, lo que la vista detectará.

            // Llamada para obtener el nombre del salón
            var classroomResponse = await _httpClient.GetAsync($"api/classrooms/{classroomId}?schoolId={GetSchoolIdFromSession()}");

            string classroomName = "Salón Desconocido";
            if (classroomResponse.IsSuccessStatusCode)
            {
                var classroomJson = await classroomResponse.Content.ReadAsStringAsync();
                var classroom = JsonSerializer.Deserialize<ClassroomViewModel>(classroomJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                classroomName = classroom?.Name ?? classroomName;
            }

            ViewBag.ClassroomName = classroomName;

            return View(students);
        }

        //************************    EVALUACIONES    ***************************************

        [HttpGet]
        public async Task<IActionResult> ListEvaluations()
        {
            var token = GetTokenFromSession();
            var schoolId = GetSchoolIdFromSession();
            var userId = GetUserIdFromSession();

            if (string.IsNullOrEmpty(token) || !schoolId.HasValue || !userId.HasValue)
            {
                return RedirectToAction("Login", "Login");
            }

            SetAuthorizationHeader(token);
            var response = await _httpClient.GetAsync($"api/evaluations?userID={userId.Value}&schoolId={schoolId.Value}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var evaluations = JsonSerializer.Deserialize<List<Evaluation>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return View(evaluations);
            }
            else
            {
                return View(new List<Evaluation>());
            }
        }

        [HttpGet]
        public IActionResult CreateEvaluation()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvaluation(EvaluationViewModel model)
        {
            var token = GetTokenFromSession();
            var schoolId = GetSchoolIdFromSession();
            var userId = GetUserIdFromSession();

            if (!ModelState.IsValid || string.IsNullOrEmpty(token) || !schoolId.HasValue || !userId.HasValue)
            {
                return View(model);
            }

            var newEvaluation = new Evaluation
            {
                Title = model.Title,
                Description = model.Description,
                Date = model.Date,
                CourseID = model.CourseID,
                UserID = userId.Value,
                SchoolID = schoolId.Value
            };

            var json = JsonSerializer.Serialize(newEvaluation);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            SetAuthorizationHeader(token);
            var response = await _httpClient.PostAsync("api/evaluations", content);

            if (response.IsSuccessStatusCode)
            {
                return RedirectToAction("ListEvaluations");
            }
            else
            {
                ModelState.AddModelError("", "Error al crear la evaluación.");
                return View(model);
            }
        }
    }
}