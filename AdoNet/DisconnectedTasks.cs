using System.Data;
using DatabaseCourseExam.Data;
using Npgsql;

namespace DatabaseCourseExam.AdoNet;

public static class DisconnectedTasks
{
    public static async Task<DataTable> LoadStudentsAsync()
    {
        var table = new DataTable("Students");

        await using var connection = DatabaseConnection.Create();
        await connection.OpenAsync();

        const string sql = """
            SELECT id, first_name, last_name, age, group_id
            FROM students
            ORDER BY id;
            """;

        await using var command = new NpgsqlCommand(sql, connection);
        using var adapter = new NpgsqlDataAdapter(command);

        adapter.Fill(table);
        table.PrimaryKey = [table.Columns["id"]!];

        return table;
    }

    public static void PrintTable(DataTable table)
    {
        foreach (DataRow row in table.Rows)
        {
            Console.WriteLine(
                $"{row["id"]}: {row["first_name"]} " +
                $"{row["last_name"]}, возраст {row["age"]}");
        }
    }

    // TODO (задание 2): Вывести RowState до и после изменения; объяснить разницу Unchanged и Modified. = выполнил

         // - Unchanged: Строка не изменялась с момента загрузки или последнего AcceptChanges()
         //   DataAdapter проигнорирует её при вызове Update()
         // - Modified: Свойства строки были изменены, DataAdapter при вызове Update() сгенерирует 
         //   SQL-запрос UPDATE, используя исходные Original значения для WHERE, чтобы синхронизировать это изменение с бд

    public static void ChangeAgeInMemory(DataTable table, int studentId, int newAge)
    {
        DataRow? row = table.Rows.Find(studentId);

        if (row is null)
        {
            Console.WriteLine("Студент не найден в DataTable.");
            return;
        }
  
        Console.WriteLine($"Состояние строки до изменения: {row.RowState}");
        row["age"] = newAge;
        Console.WriteLine($"Состояние строки после изменения: {row.RowState}");
    }

    public static int SaveChanges(DataTable table)
    {
        using var connection = DatabaseConnection.Create();
        connection.Open();

        const string sql = "UPDATE students SET age = @age WHERE id = @id;";
        using var command = new NpgsqlCommand(sql, connection);

        command.Parameters.Add("@age", NpgsqlTypes.NpgsqlDbType.Integer, 4, "age");

        var idParam = command.Parameters.Add("@id", NpgsqlTypes.NpgsqlDbType.Integer, 4, "id");
        idParam.SourceVersion = DataRowVersion.Original;

        using var adapter = new NpgsqlDataAdapter();
        adapter.UpdateCommand = command;

        int affectedRows = adapter.Update(table);
        Console.WriteLine($"Сохранено изменений: {affectedRows}");
        return affectedRows;
    }
}