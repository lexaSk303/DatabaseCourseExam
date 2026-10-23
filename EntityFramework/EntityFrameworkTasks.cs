using DatabaseCourseExam.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;

namespace DatabaseCourseExam.EntityFramework;

public static class EntityFrameworkTasks
{
    // TODO (задание 1): Вывести студентов с названием группы; обеспечить загрузку связанной сущности без обращения к незагруженному свойству.= выполнил
    // TODO (задание 7): Сгруппировать загруженных студентов по группе и вывести количество студентов в каждой группе. = выполнил
    public static async Task ShowStudentsAsync()
    {
        await using var context = SchoolDbContextFactory.Create();

        List<Student> students = await context.Students
            .Include(student => student.Group)
            .OrderBy(student => student.Id)
            .ToListAsync();

        Console.WriteLine("\n=== Список студентов ===");
        foreach (Student student in students)
        {
            string groupName = student.Group?.Name ?? "Группа не назначена";
            Console.WriteLine($"{student.Id}. {student.FirstName} {student.LastName}, возраст: {student.Age}, группа: {groupName}");
        }

        Console.WriteLine("\n=== Количество студентов по группам ===");
        var groupStats = students
            .GroupBy(s => s.Group?.Name ?? "Без группы")
            .Select(g => new { GroupName = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count);

        foreach (var stat in groupStats)
        {
            Console.WriteLine($"{stat.GroupName}: {stat.Count} чел.");
        }
    }

     // TODO (задание 6): Добавить обработку DbUpdateException и объяснить, почему ограничение уникальности должно обеспечиваться БД. = выполнил
     // Ограничения уникальности и целостности должны обеспечиваться на уровне СУБД (PostgreSQL), 
     // а не только в коде C#. это будет гарантировать ACID-свойства
    public static async Task UpdateStudentAgeAsync(int studentId, int newAge)
    {
        await using var context = SchoolDbContextFactory.Create();

        Student? student = await context.Students.FirstOrDefaultAsync(x => x.Id == studentId);

        if (student is null)
        {
            Console.WriteLine("Студент не найден.");
            return;
        }
        student.Age = newAge;
        try
        {
            int affectedRows = await context.SaveChangesAsync();
            Console.WriteLine($"Возраст изменён. Сохранено изменений: {affectedRows}");
        }
        catch (DbUpdateException ex)
        {
            Console.WriteLine($"Ошибка сохранения данных: {ex.InnerException?.Message ?? ex.Message}");
        }
    }

    public static async Task ShowCoursesAsync()
    {
        await using var context = SchoolDbContextFactory.Create();

        var courses = await context.Courses
            .OrderBy(course => course.Name)
            .Select(course => new { course.Name, course.Price, course.AvailablePlaces })
            .ToListAsync();

        foreach (var course in courses)
        {
            Console.WriteLine($"{course.Name} | Цена: {course.Price:N2} | Свободных мест: {course.AvailablePlaces}");
        }
    }
}