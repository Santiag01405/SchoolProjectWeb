using Microsoft.AspNetCore.Mvc;
using SchoolProjectWeb.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;

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

        //Comienzo CRUD de usuario -----------------------------------------------------------------------------------------------------------
        //Crear usuario
        [HttpGet]
        public IActionResult CreateUser()
        {
            var model = new UserRegisterViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser(UserRegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var payload = new
            {
                userName = model.UserName,
                email = model.Email,
                passwordHash = model.Password,
                roleID = model.RoleID
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/auth/register", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Usuario registrado correctamente.";
                return RedirectToAction("CreateUser");
            }

            TempData["Error"] = "Error al registrar el usuario.";
            return View(model);
        }

        //Obtener usuarios
        [HttpGet]
        public async Task<IActionResult> ListUsers(string search = "")
        {
            var response = await _httpClient.GetAsync("api/users");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Error al obtener usuarios.";
                return View(new List<User>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<User>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<User>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                users = users
                    .Where(u => u.UserName.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return View(users);
        }

        /*  [HttpGet]
          public async Task<IActionResult> ListUsers()
          {
              var response = await _httpClient.GetAsync("api/users");

              if (!response.IsSuccessStatusCode)
              {
                  TempData["Error"] = "Error al obtener usuarios.";
                  return View(new List<User>());
              }

              var json = await response.Content.ReadAsStringAsync();
              var users = JsonSerializer.Deserialize<List<User>>(json, new JsonSerializerOptions
              {
                  PropertyNameCaseInsensitive = true
              }) ?? new List<User>();

              return View(users);
          }*/

        //Editar usuario
        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            var response = await _httpClient.GetAsync($"api/users/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Error al cargar el usuario.";
                return RedirectToAction("ListUsers");
            }

            var json = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<User>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (user == null)
            {
                TempData["Error"] = "Usuario no encontrado.";
                return RedirectToAction("ListUsers");
            }

            var viewModel = new EditUserViewModel
            {
                UserID = user.UserID,
                UserName = user.UserName,
                Email = user.Email,
                RoleID = user.RoleID
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> EditUser(EditUserViewModel model)
        {
            var payload = new
            {
                model.UserID,
                model.UserName,
                model.Email,
                passwordHash = model.Password,
                model.RoleID
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"api/users/{model.UserID}", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Usuario actualizado.";
                return RedirectToAction("ListUsers");
            }

            TempData["Error"] = "Error al actualizar.";
            return View(model);
        }

        //Eliminar usuario
        [HttpPost]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var response = await _httpClient.DeleteAsync($"api/users/{id}");

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Usuario eliminado.";
            }
            else
            {
                TempData["Error"] = "Error al eliminar el usuario.";
            }

            return RedirectToAction("ListUsers");
        }

        //Ver los cursos que enseña un profesor
        public async Task<IActionResult> ViewTaughtCourses(int userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://SchoolProject123.somee.com/api/courses/user/{userId}/taught-courses");

                if (response.IsSuccessStatusCode)
                {
                    var courses = await response.Content.ReadFromJsonAsync<List<TaughtCourseViewModel>>();
                    ViewBag.TeacherID = userId;

                    // Obtener nombre del profesor desde la API de usuarios
                    var userResponse = await _httpClient.GetAsync($"https://SchoolProject123.somee.com/api/users/{userId}");
                    if (userResponse.IsSuccessStatusCode)
                    {
                        var teacher = await userResponse.Content.ReadFromJsonAsync<User>();
                        ViewBag.TeacherName = teacher.UserName;
                    }
                    else
                    {
                        ViewBag.TeacherName = "Nombre no disponible";
                    }

                    return View(courses);
                }

                TempData["Error"] = $"No se encontraron cursos enseñados por el usuario con ID {userId}.";
                return RedirectToAction("ListUsers");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar cursos del profesor: {ex.Message}";
                return RedirectToAction("ListUsers");
            }
        }

        //Fin CRUD de usuario ------------------------------------------------------------------------------------------------------------

        //Comienzo CRUD de cursos ------------------------------------------------------------------------------------------------------------
        [HttpGet]
        public async Task<IActionResult> CreateCourse()
        {
            var teachers = await GetTeachersAsync();
            var model = new CourseViewModel
            {
                Teachers = teachers
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse(CourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Teachers = await GetTeachersAsync();
                return View(model);
            }

            var payload = new
            {
                name = model.Name,
                description = model.Description,
                dayOfWeek = model.DayOfWeek,
                userID = model.UserID
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("api/courses/create", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Curso creado correctamente.";
                return RedirectToAction("CreateCourse");
            }

            TempData["Error"] = "Error al crear el curso.";
            model.Teachers = await GetTeachersAsync();
            return View(model);
        }

        private async Task<List<User>> GetTeachersAsync()
        {
            var response = await _httpClient.GetAsync("api/users");
            if (!response.IsSuccessStatusCode) return new List<User>();

            var json = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<User>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<User>();

            return users.Where(u => u.RoleID == 2).ToList(); // Solo profesores
        }

        [HttpGet]
        public async Task<IActionResult> ListCourses(string? search)
        {
            var response = await _httpClient.GetAsync("api/courses");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Error al obtener cursos.";
                return View(new CourseListViewModel());
            }

            var json = await response.Content.ReadAsStringAsync();
            var courses = JsonSerializer.Deserialize<List<Course>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<Course>();

            var teachers = await GetTeachersAsync();

            var courseViewModels = courses.Select(c =>
            {
                var teacher = teachers.FirstOrDefault(t => t.UserID == c.UserID);
                return new CourseViewModel
                {
                    CourseID = c.CourseID,
                    Name = c.Name,
                    Description = c.Description,
                    DayOfWeek = c.DayOfWeek,
                    UserID = c.UserID,
                    TeacherName = teacher?.UserName ?? "Sin profesor"
                };
            }).ToList();

            // Filtrar si se envió texto de búsqueda
            if (!string.IsNullOrEmpty(search))
            {
                courseViewModels = courseViewModels
                    .Where(c => !string.IsNullOrEmpty(c.Name) && c.Name.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            var model = new CourseListViewModel
            {
                Courses = courseViewModels
            };

            return View(courseViewModels);

        }

        /*[HttpGet]
        public async Task<IActionResult> ListCourses()
        {
            var response = await _httpClient.GetAsync("api/courses");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Error al obtener cursos.";
                return View(new CourseListViewModel());
            }

            var json = await response.Content.ReadAsStringAsync();
            var courses = JsonSerializer.Deserialize<List<Course>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<Course>();

            var teachers = await GetTeachersAsync();

            var courseViewModels = courses.Select(c =>
            {
                var teacher = teachers.FirstOrDefault(t => t.UserID == c.UserID);
                return new CourseViewModel
                {
                    CourseID = c.CourseID,
                    Name = c.Name,
                    Description = c.Description,
                    DayOfWeek = c.DayOfWeek,
                    UserID = c.UserID,
                    TeacherName = teacher?.UserName ?? "Sin profesor"
                };
            }).ToList();

            var model = new CourseListViewModel
            {
                Courses = courseViewModels
            };

            return View(model);
        }*/

        /* [HttpGet]
         public async Task<IActionResult> ListCourses()
         {
             var response = await _httpClient.GetAsync("api/courses");

             if (!response.IsSuccessStatusCode)
             {
                 TempData["Error"] = "Error al obtener cursos.";
                 return View(new CourseListViewModel());
             }

             var json = await response.Content.ReadAsStringAsync();
             var courses = JsonSerializer.Deserialize<List<Course>>(json, new JsonSerializerOptions
             {
                 PropertyNameCaseInsensitive = true
             }) ?? new List<Course>();

             // Mapeo de List<Course> a List<CourseViewModel>
             var courseViewModels = courses.Select(c => new CourseViewModel
             {
                 CourseID = c.CourseID,
                 Name = c.Name,
                 Description = c.Description,
                 DayOfWeek = c.DayOfWeek,
                 UserID = c.UserID
                 // Si querés, podés agregar Teachers o más info aquí
             }).ToList();

             var model = new CourseListViewModel
             {
                 Courses = courseViewModels
             };

             return View(model);
         }*/


        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            var response = await _httpClient.GetAsync($"api/courses/{id}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Error al cargar el curso.";
                return RedirectToAction("ListCourses");
            }

            var json = await response.Content.ReadAsStringAsync();
            var course = JsonSerializer.Deserialize<Course>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (course == null)
            {
                TempData["Error"] = "Curso no encontrado.";
                return RedirectToAction("ListCourses");
            }

            var model = new CourseViewModel
            {
                CourseID = course.CourseID,
                Name = course.Name,
                Description = course.Description,
                DayOfWeek = course.DayOfWeek,
                UserID = course.UserID,
                Teachers = await GetTeachersAsync()
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditCourse(CourseViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Teachers = await GetTeachersAsync();
                return View(model);
            }

            var payload = new
            {
                courseID = model.CourseID,
                name = model.Name,
                description = model.Description,
                dayOfWeek = model.DayOfWeek,
                userID = model.UserID
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"api/courses/{model.CourseID}", content);

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
            var response = await _httpClient.DeleteAsync($"api/courses/{id}");

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
            try
            {
                // Llamada para obtener estudiantes inscritos
                var responseStudents = await _httpClient.GetAsync($"https://SchoolProject123.somee.com/api/enrollments/course/{courseId}/students");

                if (!responseStudents.IsSuccessStatusCode)
                {
                    TempData["Error"] = $"No hay estudiantes inscritos o error al obtener estudiantes del curso con ID {courseId}.";
                    return RedirectToAction("ListCourses");
                }

                var students = await responseStudents.Content.ReadFromJsonAsync<List<StudentViewModel>>();

                // Llamada para obtener detalles del curso (incluye nombre)
                var responseCourse = await _httpClient.GetAsync($"https://SchoolProject123.somee.com/api/courses/{courseId}");

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

        //Ver inscripciones de un curso
        /*public async Task<IActionResult> ViewStudentsInCourse(int courseId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://SchoolProject123.somee.com/api/enrollments/course/{courseId}/students");

                if (response.IsSuccessStatusCode)
                {
                    var students = await response.Content.ReadFromJsonAsync<List<StudentViewModel>>();
                    ViewBag.CourseID = courseId;
                    return View(students);
                }

                TempData["Error"] = $"No hay estudiantes inscritos o error al obtener estudiantes del curso con ID {courseId}.";
                return RedirectToAction("ListCourses");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error al cargar estudiantes: {ex.Message}";
                return RedirectToAction("ListCourses");
            }
        }*/

        //----------------------------------------------------------------------------------------------------------------------------------

        //Crear relacion entre padre e hijo

        [HttpGet]
        public async Task<IActionResult> CreateUserRelationship()
        {
            var response = await _httpClient.GetAsync("api/users");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "No se pudieron cargar los usuarios.";
                return View(new UserRelationshipViewModel());
            }

            var json = await response.Content.ReadAsStringAsync();
            var allUsers = JsonSerializer.Deserialize<List<User>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<User>();

            var model = new UserRelationshipViewModel
            {
                Parents = allUsers.Where(u => u.RoleID == 3).ToList(),    // Padres
                Children = allUsers.Where(u => u.RoleID == 1).ToList()    // Estudiantes
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUserRelationship(UserRelationshipViewModel model)
        {
            var content = new StringContent(JsonSerializer.Serialize(new
            {
                user1ID = model.User1ID,
                user2ID = model.User2ID,
                relationshipType = model.RelationshipType
            }), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/relationships/create", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Relación creada exitosamente.";
                return RedirectToAction("CreateUserRelationship");
            }

            TempData["Error"] = "Error al crear relación.";
            return RedirectToAction("CreateUserRelationship");
        }
        //---------------------------------------------------------------------------------------------------------------------------------


        /*public async Task<IActionResult> ListStudents()
        {
            var response = await _httpClient.GetFromJsonAsync<List<User>>("api/users"); // Ajusta la URL según tu API
            var students = response?.Where(u => u.RoleID == 1).ToList();

            return View(students);
        }*/

        [HttpGet]
        public async Task<IActionResult> ListStudents(string search = "")
        {
            // Obtiene todos los usuarios desde la API
            var response = await _httpClient.GetAsync("api/users");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "Error al obtener usuarios.";
                return View(new List<User>());
            }

            var json = await response.Content.ReadAsStringAsync();
            var allUsers = JsonSerializer.Deserialize<List<User>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<User>();

            // Filtra solo los estudiantes (RoleID == 1)
            var students = allUsers.Where(u => u.RoleID == 1).ToList();

            // Si llegó algo en "search", filtra por UserName
            if (!string.IsNullOrWhiteSpace(search))
            {
                students = students
                    .Where(u => u.UserName.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return View(students);
        }


        /*public async Task<IActionResult> ViewEnrollments(int userId)
        {
            var response = await _httpClient.GetFromJsonAsync<List<EnrollmentViewModel>>($"https://SchoolProject123.somee.com/api/enrollments/user/{userId}");

            ViewBag.UserId = userId;
            return View(response);
        }*/

        public async Task<IActionResult> ViewEnrollments(int userId)
        {
            List<EnrollmentViewModel> enrollments = new();

            try
            {
                var response = await _httpClient.GetAsync($"https://SchoolProject123.somee.com/api/enrollments/user/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    enrollments = await response.Content.ReadFromJsonAsync<List<EnrollmentViewModel>>();
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // No inscripciones encontradas, simplemente se deja la lista vacía
                    TempData["InfoMessage"] = "Este usuario no tiene inscripciones.";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Error al obtener inscripciones: {error}";
                }

                // Obtener datos del usuario (para obtener su nombre)
                var userResponse = await _httpClient.GetAsync($"api/users/{userId}");
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
            ViewBag.UserID = id;

            // Obtener los cursos
            var courses = await _httpClient.GetFromJsonAsync<List<CourseViewModel>>("api/courses");
            ViewBag.Courses = courses;

            // Obtener datos del usuario para mostrar su nombre
            var userResponse = await _httpClient.GetAsync($"api/users/{id}");
            if (userResponse.IsSuccessStatusCode)
            {
                var user = await userResponse.Content.ReadFromJsonAsync<User>(); // o el modelo que uses para usuario
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
            var enrollment = new
            {
                UserID = userId,
                CourseID = courseId
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


        /*[HttpPost]
        public async Task<IActionResult> AssignCourseToUser(int userId, int courseId)
        {
            var enrollment = new
            {
                UserID = userId,
                CourseID = courseId
            };

            var response = await _httpClient.PostAsJsonAsync("api/enrollments", enrollment);

            if (response.IsSuccessStatusCode)
            {
                TempData["SuccessMessage"] = "Usuario inscrito correctamente.";
                return RedirectToAction("ViewEnrollments", new { id = userId });
            }

            var error = await response.Content.ReadAsStringAsync();
            TempData["ErrorMessage"] = $"Error: {error}";
            return RedirectToAction("AssignCourseToUser", new { id = userId });
        }*/

        //Borra inscripcion de usuario
        [HttpPost]
        public async Task<IActionResult> DeleteEnrollment(int enrollmentId, int userId)
        {
            var response = await _httpClient.DeleteAsync($"https://SchoolProject123.somee.com/api/enrollments/{enrollmentId}");

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

        //--------------------------------------------------------------------------------------------------------------------------------------

        //----Notificaciones--------------------------------------------------------------------------------------------------------------------

        [HttpGet]
        public async Task<IActionResult> SearchUsers(string search = "")
        {
            var response = await _httpClient.GetAsync("api/users");

            if (!response.IsSuccessStatusCode)
                return Json(new List<User>());

            var json = await response.Content.ReadAsStringAsync();
            var users = JsonSerializer.Deserialize<List<User>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<User>();

            if (!string.IsNullOrWhiteSpace(search))
            {
                users = users
                    .Where(u => u.UserName.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            return Json(users);
        }

        public async Task<IActionResult> SendNotification()
        {
            var response = await _httpClient.GetAsync("api/users");
            var users = await response.Content.ReadFromJsonAsync<List<User>>();

            ViewBag.Usuarios = users;
            return View(new NotificationSendViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> SendNotification(NotificationSendViewModel model)
        {
            var notification = new
            {
                title = model.Title,
                content = model.Content,
                date = DateTime.UtcNow,
                isRead = false,
                userID = model.UserID ?? 0
            };

            HttpResponseMessage response;

            if (model.Target == "all")
            {
                response = await _httpClient.PostAsJsonAsync("api/notifications/send-to-all", notification);
            }
            else if (model.Target == "role" && model.RoleID.HasValue)
            {
                response = await _httpClient.PostAsJsonAsync($"api/notifications/send-to-role?roleId={model.RoleID}", notification);
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

                // Recargar la vista manualmente con usuarios
                var users = await _httpClient.GetFromJsonAsync<List<User>>("api/users");
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
            var viewModel = new AdminDashboardViewModel();

            async Task<int> GetCount(string endpoint)
            {
                var response = await _httpClient.GetAsync($"api/users/{endpoint}");
                if (!response.IsSuccessStatusCode) return 0;

                var json = await response.Content.ReadAsStringAsync();
                return int.TryParse(json, out int count) ? count : 0;
            }

            viewModel.TotalUsers = await GetCount("active-count");
            viewModel.Students = await GetCount("active-count-students");
            viewModel.Teachers = await GetCount("active-count-teachers");
            viewModel.Parents = await GetCount("active-count-parents");

            // Cursos (puedes tener un endpoint como /api/courses o adaptar según el tuyo)
            var courseResponse = await _httpClient.GetAsync("api/courses");
            if (courseResponse.IsSuccessStatusCode)
            {
                var json = await courseResponse.Content.ReadAsStringAsync();
                var courses = JsonSerializer.Deserialize<List<Course>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                viewModel.Courses = courses?.Count ?? 0;
            }

            return View("Index", viewModel);
        }

    }
}