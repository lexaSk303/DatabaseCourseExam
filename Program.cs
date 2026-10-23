// TODO (задание 5): Добавить пункт меню, выводящий список студентов отдельными методами ADO.NET, EF Core и Dapper.
// TODO (задание 3): Заменить int.Parse на int.TryParse в указанном сценарии; неверный ввод не должен завершать программу.

using DatabaseCourseExam.AdoNet;
using DatabaseCourseExam.Data;
using System.Data;
using DatabaseCourseExam.EntityFramework;
using DatabaseCourseExam.DapperTasks;
using Npgsql; 
using Dapper;
using Microsoft.EntityFrameworkCore;

DataTable? studentsTable = null;

while (true)
{
    Console.WriteLine();
    Console.WriteLine("=== Зачёт по работе с БД ===");
    Console.WriteLine("1. Показать студентов (ADO.NET)");
    Console.WriteLine("2. Изменить возраст студента (ADO.NET)");
    Console.WriteLine("3. Записать студента на курс (ADO.NET)");
    Console.WriteLine("4. Загрузить студентов в DataTable");
    Console.WriteLine("5. Изменить возраст в DataTable");
    Console.WriteLine("6. Показать DataTable");
    Console.WriteLine("7. Сохранить изменения DataTable");
    Console.WriteLine("8. EF Core: показать студентов");
    Console.WriteLine("9. EF Core: изменить возраст студента");
    Console.WriteLine("10. EF Core: показать курсы");
    Console.WriteLine("11. Dapper: показать студентов");
    Console.WriteLine("12. Dapper: изменить возраст");
    Console.WriteLine("13. Dapper: показать курсы");
    Console.WriteLine("14. Dapper: записать студента на курс");
    Console.WriteLine("15. Сравнение: показать студентов (ADO.NET, EF Core, Dapper)"); // задание 5
    Console.WriteLine("0. Выход");
    Console.Write("Выберите действие: ");

    string? choice = Console.ReadLine();

    try
    {
        switch (choice)
        {
            case "1": 
                await AdoNetTasks.ShowStudentsAsync(); 
                break;

            // задание 3
            case "2":
                Console.Write("ID студента: ");
                if (!int.TryParse(Console.ReadLine(), out int studentId)) { Console.WriteLine("Некорректный ID."); break; }

                Console.Write("Новый возраст: ");
                if (!int.TryParse(Console.ReadLine(), out int age) || age is < 14 or > 100)
                {
                    Console.WriteLine("Некорректный возраст");
                    break;
                }
                await AdoNetTasks.UpdateStudentAgeAsync(studentId, age);
                break;
            case "3":
                Console.Write("ID студента: ");
                if (!int.TryParse(Console.ReadLine(), out int enrollmentStudentId)) { Console.WriteLine("Некорректный ID."); break; }

                Console.Write("ID курса: ");
                if (!int.TryParse(Console.ReadLine(), out int courseId)) { Console.WriteLine("Некорректный ID курса."); break; }

                await AdoNetTasks.EnrollStudentAsync(enrollmentStudentId, courseId);
                break;

            case "4":
                studentsTable = await DisconnectedTasks.LoadStudentsAsync();
                Console.WriteLine("Данные загружены в память.");
                DisconnectedTasks.PrintTable(studentsTable);
                break;

            case "5":
                if (studentsTable is null) { Console.WriteLine("Сначала загрузите данные (пункт 4)."); break; }
                Console.Write("ID студента: ");
                if (!int.TryParse(Console.ReadLine(), out int disconnectedStudentId)) { Console.WriteLine("Некорректный ID."); break; }

                Console.Write("Новый возраст: ");
                if (!int.TryParse(Console.ReadLine(), out int disconnectedAge) || disconnectedAge is < 14 or > 100)
                {
                    Console.WriteLine("Возраст должен быть от 14 до 100.");
                    break;
                }
                DisconnectedTasks.ChangeAgeInMemory(studentsTable, disconnectedStudentId, disconnectedAge);
                break;

            case "6":
                if (studentsTable is null) { Console.WriteLine("Сначала загрузите данные (пункт 4)."); break; }
                DisconnectedTasks.PrintTable(studentsTable);
                break;

            case "7":
                if (studentsTable is null) { Console.WriteLine("Сначала загрузите данные (пункт 4)."); break; }
                DisconnectedTasks.SaveChanges(studentsTable);
                break;

            case "8": 
                await EntityFrameworkTasks.ShowStudentsAsync(); 
                break;
            case "9":
                Console.Write("ID студента: ");
                if (!int.TryParse(Console.ReadLine(), out int stId)) { Console.WriteLine("Некорректный ID."); break; }
                Console.Write("Новый возраст: ");
                if (!int.TryParse(Console.ReadLine(), out int newAge) || newAge is < 14 or > 100) { Console.WriteLine("Некорректный возраст."); break; }
                await EntityFrameworkTasks.UpdateStudentAgeAsync(stId, newAge);
                break;
            case "10":
                await EntityFrameworkTasks.ShowCoursesAsync();
                break;
            case "11": 
                await DapperStudentTasks.ShowStudentsAsync();
                break;
            case "12":
                Console.Write("ID студента: ");
                if (!int.TryParse(Console.ReadLine(), out int dStudentId)) { Console.WriteLine("Некорректный ID."); break; }
                Console.Write("Новый возраст: ");
                if (!int.TryParse(Console.ReadLine(), out int dNewAge) || dNewAge is < 14 or > 100) { Console.WriteLine("Некорректный возраст."); break; }
                await DapperStudentTasks.UpdateStudentAgeAsync(dStudentId, dNewAge);
                break;
            case "13": 
                await DapperStudentTasks.ShowCoursesAsync();
                break;
            case "14":
                Console.Write("ID студента: ");
                if (!int.TryParse(Console.ReadLine(), out int dEnrollStudentId)) { Console.WriteLine("Некорректный ID."); break; }
                Console.Write("ID курса: ");
                if (!int.TryParse(Console.ReadLine(), out int dCourseId)) { Console.WriteLine("Некорректный ID курса."); break; }
                await DapperStudentTasks.EnrollStudentAsync(dEnrollStudentId, dCourseId);
                break;

            // задание 5
            case "15":
                await CompareTechnologiesAsync();
                break;

            case "0": return;
            default: Console.WriteLine("Неизвестная команда."); break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Критическая ошибка: {ex.Message}");
    }
}

static async Task CompareTechnologiesAsync()
{
    Console.WriteLine("\n=== Сравнение технологий получения данных (первые 3 студента) ===");

    Console.WriteLine("\n1. EF Core (LINQ to Entities):");
    await using (var efContext = SchoolDbContextFactory.Create())
    {
        var efStudents = await efContext.Students.Take(3).Select(s => s.FirstName).ToListAsync();
        foreach (var name in efStudents) Console.WriteLine($"   - {name}");
    }

    Console.WriteLine("\n2. ADO.NET:");
    await using (var conn = DatabaseConnection.Create())
    {
        await conn.OpenAsync();
        await using var cmd = new NpgsqlCommand("SELECT first_name FROM students ORDER BY id LIMIT 3", conn);
        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            Console.WriteLine($"   - {reader.GetString(0)}");
        }
    }

    // 3. Dapper
    Console.WriteLine("\n3. Dapper:");
    await using (var dapperConn = DatabaseConnection.Create())
    {
        var dapperStudents = await dapperConn.QueryAsync<string>("SELECT first_name FROM students ORDER BY id LIMIT 3");
        foreach (var name in dapperStudents) Console.WriteLine($"   - {name}");
    }
}