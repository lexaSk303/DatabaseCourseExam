using DatabaseCourseExam.Data;
using Dapper;
using DatabaseCourseExam.DapperTasks.DTO;

namespace DatabaseCourseExam.DapperTasks;

public static class DapperStudentTasks
{
    public static async Task ShowStudentsAsync()
    {
        await using var connection = DatabaseConnection.Create();

        const string sql = """
            SELECT
                s.id AS Id,
                s.first_name AS FirstName,
                s.last_name AS LastName,
                s.age AS Age,
                g.name AS GroupName
            FROM students AS s
            JOIN student_groups AS g ON g.id = s.group_id
            ORDER BY s.id;
            """;

        var students = await connection.QueryAsync<StudentDto>(sql);

        foreach (StudentDto student in students)
        {
            Console.WriteLine(
                $"{student.Id}. {student.FirstName} {student.LastName}, " +
                $"возраст: {student.Age}, группа: {student.GroupName}");
        }
    }

    public static async Task UpdateStudentAgeAsync(
        int studentId,
        int newAge)
    {
        await using var connection = DatabaseConnection.Create();

        const string sql = """
            UPDATE students
            SET age = @NewAge
            WHERE id = @StudentId;
            """;

        int affectedRows = await connection.ExecuteAsync(
            sql,
            new
            {
                NewAge = newAge,
                StudentId = studentId
            });

        Console.WriteLine(
            affectedRows > 0
                ? $"Изменено записей: {affectedRows}"
                : "Студент не найден.");
    }

    public static async Task ShowCoursesAsync()
    {
        await using var connection = DatabaseConnection.Create();

        const string sql = """
            SELECT
                id AS Id,
                name AS Name,
                price AS Price,
                available_places AS AvailablePlaces
            FROM courses
            ORDER BY name;
            """;

        var courses = await connection.QueryAsync<CourseDto>(sql);

        foreach (CourseDto course in courses)
        {
            Console.WriteLine(
                $"{course.Name} | Цена: {course.Price:N2} | " +
                $"Свободных мест: {course.AvailablePlaces}");
        }
    }

    // TODO (задание 4): Проверить студента и повторную запись; обновление свободных мест и INSERT должны быть одной транзакцией. = выполнил
    public static async Task EnrollStudentAsync(int studentId, int courseId)
    {
        await using var connection = DatabaseConnection.Create();
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            const string checkSql = """
                SELECT COUNT(1) FROM student_courses 
                WHERE student_id = @StudentId AND course_id = @CourseId;
                """;
            int enrollCount = await connection.ExecuteScalarAsync<int>(checkSql, new { StudentId = studentId, CourseId = courseId }, transaction);

            if (enrollCount > 0)
            {
                Console.WriteLine("Студент уже записан на этот курс.");
                await transaction.RollbackAsync();
                return;
            }
            
            const string updateSql = """
                UPDATE courses
                SET available_places = available_places - 1
                WHERE id = @CourseId AND available_places > 0
                RETURNING available_places;
                """;

            int? remainingPlaces = await connection.QuerySingleOrDefaultAsync<int?>(updateSql, new { CourseId = courseId }, transaction);

            if (remainingPlaces is null)
            {
                throw new InvalidOperationException("Курс не найден или свободных мест нет.");
            }

            const string insertSql = "INSERT INTO student_courses (student_id, course_id) VALUES (@StudentId, @CourseId);";
            await connection.ExecuteAsync(insertSql, new { StudentId = studentId, CourseId = courseId }, transaction);

            // тута фиксация транзакции 
            await transaction.CommitAsync();
            Console.WriteLine($"Студент успешно записан на курс. Осталось мест: {remainingPlaces}");
        }
        catch (Exception ex)
        {
            // нужно при любой ошибке откатываем все изменения, чтобы не было частичного сохранения
            await transaction.RollbackAsync();
            Console.WriteLine($"Операция отменена: {ex.Message}");
        }
    }
}